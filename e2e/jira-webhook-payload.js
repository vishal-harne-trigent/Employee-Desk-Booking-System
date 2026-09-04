// Builds an enriched webhook payload for Playwright Results for Jira (Orbit A).
// Core fields match the app contract; extra summary/test/details fields are additive.

const fs = require('node:fs');
const path = require('node:path');

const CITATION = /(US-\d{3})\/(AC-\d{2})/;
const ST_CITATION = /SCR-\d{3}\/ST-\d{2}/;
const EDGE = /edge case/i;

function mapAc(title) {
  const m = CITATION.exec(title ?? '');
  if (m) return m[2];
  if (ST_CITATION.test(title ?? '')) return 'ST-01';
  if (EDGE.test(title ?? '')) return 'Edge';
  return null;
}

function mapStory(title) {
  return CITATION.exec(title ?? '')?.[1] ?? null;
}

function parsePlanScenarios(repoRoot, storyId) {
  const plansDir = path.join(repoRoot, 'e2e', 'plans');
  if (!fs.existsSync(plansDir)) return [];
  const prefix = `${storyId.toLowerCase()}-`;
  const file = fs
    .readdirSync(plansDir)
    .find((f) => f.toLowerCase().startsWith(prefix) && f.endsWith('.md'));
  if (!file) return [];

  const text = fs.readFileSync(path.join(plansDir, file), 'utf8');
  const section = (text.match(/^##\s+Scenarios\s*$([\s\S]*?)(?=^##\s|\Z)/m)?.[1] ?? '').trim();
  const lines = section.split('\n').filter((l) => /^\s*\|.*\|\s*$/.test(l));
  if (lines.length < 2) return [];

  const headers = lines[0]
    .replace(/^\||\|$/g, '')
    .split('|')
    .map((c) => c.trim());
  const rows = [];
  for (let i = 2; i < lines.length; i++) {
    const cells = lines[i]
      .replace(/^\||\|$/g, '')
      .split('|')
      .map((c) => c.trim());
    if (cells.length !== headers.length) continue;
    const row = {};
    headers.forEach((h, idx) => {
      row[h] = cells[idx];
    });
    rows.push(row);
  }
  return rows;
}

function parsePlanAccounts(repoRoot, storyId) {
  const plansDir = path.join(repoRoot, 'e2e', 'plans');
  if (!fs.existsSync(plansDir)) return [];
  const prefix = `${storyId.toLowerCase()}-`;
  const file = fs
    .readdirSync(plansDir)
    .find((f) => f.toLowerCase().startsWith(prefix) && f.endsWith('.md'));
  if (!file) return [];

  const text = fs.readFileSync(path.join(plansDir, file), 'utf8');
  const section = (
    text.match(/^##\s+Accounts \(dev seed\)\s*$([\s\S]*?)(?=^##\s|\Z)/m)?.[1] ?? ''
  ).trim();
  const lines = section.split('\n').filter((l) => /^\s*\|.*\|\s*$/.test(l));
  if (lines.length < 2) return [];

  const headers = lines[0]
    .replace(/^\||\|$/g, '')
    .split('|')
    .map((c) => c.trim());
  const rows = [];
  for (let i = 2; i < lines.length; i++) {
    const cells = lines[i]
      .replace(/^\||\|$/g, '')
      .split('|')
      .map((c) => c.trim());
    if (cells.length !== headers.length) continue;
    const row = {};
    headers.forEach((h, idx) => {
      row[h] = cells[idx];
    });
    rows.push(row);
  }
  return rows;
}

function scenarioIdForAc(scenarios, ac) {
  if (!ac) return null;
  const hit = scenarios.find((s) => s.AC === ac);
  return hit?.ID?.replace(/^S-/, 'TC-') ?? null;
}

function mapStatus(outcome) {
  if (!outcome) return 'skipped';
  if (outcome.status === 'passed') return 'passed';
  if (
    outcome.status === 'failed'
    || outcome.status === 'timedOut'
    || outcome.status === 'interrupted'
  ) {
    return 'failed';
  }
  return 'skipped';
}

function errorMessage(outcome) {
  const msg = outcome?.error?.message;
  if (!msg) return undefined;
  return msg.split('\n')[0].slice(0, 300);
}

function projectName(test) {
  let suite = test.parent;
  while (suite) {
    if (typeof suite.project === 'function') {
      const proj = suite.project();
      if (proj?.name) return proj.name;
    }
    suite = suite.parent;
  }
  return 'chromium';
}

function specFile(test) {
  const file = test.location?.file;
  if (!file) return undefined;
  return file.replace(/\\/g, '/').replace(/^.*\/e2e\//, 'e2e/');
}

/**
 * @param {object} opts
 * @param {import('@playwright/test/reporter').Suite | undefined} opts.suite
 * @param {number} opts.startTime
 * @param {Record<string, string | undefined>} opts.env
 */
function buildWebhookPayload({ suite, startTime, env }) {
  const repoRoot = path.resolve(__dirname, '..');
  const storyId = env.JIRA_STORY_ID ?? 'US-001';
  const scenarios = parsePlanScenarios(repoRoot, storyId);
  const accounts = parsePlanAccounts(repoRoot, storyId);

  const stats = { passed: 0, failed: 0, skipped: 0, duration: 0 };
  /** @type {object[]} */
  const tests = [];
  /** @type {Map<string, { outcome: string, test: string }>} */
  const acOutcomes = new Map();

  /** @param {import('@playwright/test/reporter').Suite} s */
  function traverse(s) {
    for (const test of s.tests) {
      const results = test.results ?? [];
      const outcome = results[results.length - 1];
      const status = mapStatus(outcome);
      const ac = mapAc(test.title);
      const story = mapStory(test.title) ?? storyId;
      const tcId = scenarioIdForAc(scenarios, ac);
      const duration = outcome?.duration ?? 0;
      const browser = projectName(test);
      const file = specFile(test);

      if (status === 'passed') stats.passed++;
      else if (status === 'failed') stats.failed++;
      else stats.skipped++;

      const displayTitle = tcId ? `[${tcId}] ${test.title}` : test.title;

      const row = {
        title: displayTitle,
        status,
        duration,
        ac: ac ?? undefined,
        story,
        browser,
        file,
        project: browser,
        retries: Math.max(0, results.length - 1),
      };
      const err = errorMessage(outcome);
      if (err) row.error = err;
      if (outcome?.startTime) row.startedAt = outcome.startTime;

      tests.push(row);

      if (ac && story) {
        acOutcomes.set(`${story}/${ac}`, {
          outcome: status === 'passed' ? 'pass' : status === 'failed' ? 'fail' : 'skip',
          test: displayTitle,
        });
      }
    }
    for (const child of s.suites) traverse(child);
  }

  if (suite) traverse(suite);

  stats.duration = Date.now() - startTime;
  const total = stats.passed + stats.failed + stats.skipped;
  const overallStatus = stats.failed > 0 ? 'failed' : 'passed';
  const baseUrl = env.E2E_BASE_URL ?? 'http://localhost:5198';

  const testCases = scenarios.map((s) => ({
    id: s.ID?.replace(/^S-/, 'TC-') ?? s.ID,
    given: s.Given,
    when: s.When,
    then: s.Then,
    mapsTo: s.AC,
  }));

  const acSummary = scenarios
    .filter((s) => s.AC?.startsWith('AC-'))
    .map((s) => {
      const key = `${storyId}/${s.AC}`;
      const hit = acOutcomes.get(key);
      return {
        ac: s.AC,
        criterion: s.Then,
        outcome: hit?.outcome ?? 'not_run',
        primaryTest: s.ID?.replace(/^S-/, 'TC-'),
      };
    });

  const issueId = Number(env.JIRA_ISSUE_ID);
  const payload = {
    token: env.PLAYWRIGHT_JIRA_TOKEN,
    issueId: Number.isFinite(issueId) ? issueId : env.JIRA_ISSUE_ID,
    status: overallStatus,
    summary: {
      ...stats,
      total,
      browser: tests[0]?.browser ?? 'chromium',
      browsers: [...new Set(tests.map((t) => t.browser).filter(Boolean))],
      environment: baseUrl,
      runAt: new Date().toISOString(),
      storyId,
      issueKey: env.JIRA_ISSUE_KEY ?? undefined,
      projectKey: env.JIRA_PROJECT_KEY ?? undefined,
      spec: env.JIRA_E2E_SPEC ?? 'e2e/tests/us-001-sign-in.spec.ts',
      plan: `e2e/plans/${storyId.toLowerCase()}-sign-in.md`,
      runner: 'Playwright',
      ci: env.CI === 'true' || env.CI === '1',
    },
    tests,
    details: {
      testCases,
      testData: accounts.map((a) => ({
        role: a.Role,
        email: a.Email?.replace(/`/g, ''),
        password: a.Password?.replace(/`/g, ''),
      })),
      acSummary,
      trials: tests.map((t, i) => ({
        trial: i + 1,
        testCase: t.title,
        ac: t.ac,
        startedAt: t.startedAt,
        duration: t.duration,
        result: t.status,
        browser: t.browser,
        error: t.error,
        screenshot: t.screenshot,
        screenshotUrl: t.screenshotUrl,
      })),
    },
  };

  return payload;
}

module.exports = { buildWebhookPayload };
