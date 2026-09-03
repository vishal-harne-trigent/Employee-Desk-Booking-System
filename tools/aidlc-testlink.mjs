#!/usr/bin/env node
// aidlc-testlink — push Playwright cases/results to TestLink, then Jira test tickets.
//
//   node tools/aidlc-testlink.mjs publish --report e2e/playwright-report.json
//   node tools/aidlc-testlink.mjs publish --report e2e/playwright-report.json --apply
//   node tools/aidlc-testlink.mjs push-cases --specs e2e/tests
//
// Rules: ai/context/testlink-sync.md
// Jira updates go through tools/aidlc-jira.mjs (never ad-hoc API calls).
import { readFileSync, readdirSync, existsSync } from 'node:fs';
import { join, relative } from 'node:path';
import { execFileSync } from 'node:child_process';
import { coverageFromReport } from './aidlc-qa-coverage.mjs';
import {
  addCaseToPlan,
  createTestLinkClient,
  ensureBuild,
  reportResult,
  resolveProjectId,
  resolveTestPlanId,
  resolveTestSuiteId,
  upsertTestCase,
} from './testlink-client.mjs';

const REPO = process.cwd();
const args = process.argv.slice(2);
const APPLY = args.includes('--apply');
const flag = (name, fallback = null) => {
  const i = args.indexOf(`--${name}`);
  return i === -1 ? fallback : args[i + 1];
};
const die = (m) => {
  console.error(`aidlc-testlink: ${m}`);
  process.exit(1);
};

const CITATION = /(US-\d{3})\/(AC-\d{2})/g;
const TEST_TITLE = /(?:test|it)\s*\(\s*[`'"]([^`'"]+)[`'"]/g;

function readEnv(name) {
  return process.env[name]?.trim() ?? '';
}

function loadConfig() {
  return {
    apiUrl: readEnv('TESTLINK_API_URL'),
    devKey: readEnv('TESTLINK_DEV_KEY'),
    project: readEnv('TESTLINK_PROJECT') || readEnv('JIRA_PROJECT_KEY') || 'EDBS',
    plan: readEnv('TESTLINK_PLAN') || 'EPIC-001 MVP',
    suite: readEnv('TESTLINK_SUITE') || 'Playwright E2E',
    build: readEnv('TESTLINK_BUILD') || defaultBuildName(),
    author: readEnv('TESTLINK_AUTHOR') || readEnv('JIRA_EMAIL') || 'qa',
    platformId: readEnv('TESTLINK_PLATFORM_ID')
      ? Number(readEnv('TESTLINK_PLATFORM_ID'))
      : null,
    reportPath:
      flag('report', join(REPO, 'e2e', 'playwright-report.json')),
    specsDir: flag('specs', join(REPO, 'e2e', 'tests')),
    runUrl: flag('run-url', lastCiRun()),
  };
}

function defaultBuildName() {
  const sha = process.env.GITHUB_SHA?.slice(0, 7);
  const date = new Date().toISOString().slice(0, 10);
  return sha ? `e2e-${date}-${sha}` : `e2e-${date}-local`;
}

function lastCiRun() {
  try {
    const out = execFileSync(
      'gh',
      ['run', 'list', '--workflow', 'e2e.yml', '--limit', '1', '--json', 'url,conclusion'],
      { encoding: 'utf8' },
    );
    return JSON.parse(out)[0]?.url ?? '—';
  } catch {
    return '—';
  }
}

function discoverFromSpecs(specsDir) {
  const cases = new Map();
  if (!existsSync(specsDir)) {
    return cases;
  }

  for (const file of readdirSync(specsDir).filter((f) => f.endsWith('.spec.ts'))) {
    const text = readFileSync(join(specsDir, file), 'utf8');
    const rel = relative(REPO, join(specsDir, file)).replace(/\\/g, '/');
    for (const match of text.matchAll(TEST_TITLE)) {
      const title = match[1];
      const citation = title.match(/(US-\d{3})\/(AC-\d{2})/);
      if (!citation) {
        continue;
      }
      const key = `${citation[1]}/${citation[2]}`;
      cases.set(key, {
        story: citation[1],
        ac: citation[2],
        title,
        file: rel,
        externalId: key.replace('/', '-'),
      });
    }
  }
  return cases;
}

function discoverFromReport(reportPath) {
  if (!existsSync(reportPath)) {
    die(`report not found: ${reportPath}. Run: cd e2e && npm test`);
  }
  const report = JSON.parse(readFileSync(reportPath, 'utf8'));
  const doc = coverageFromReport(report, {
    runUrl: flag('run-url', lastCiRun()),
  });
  return doc.results.map((r) => ({
    story: r.story,
    ac: r.ac,
    title: r.test,
    file: r.file.replace(/\\/g, '/'),
    externalId: `${r.story}-${r.ac}`.replace('/', '-'),
    outcome: r.outcome,
  }));
}

function storiesFromCases(cases) {
  return [...new Set([...cases].map((c) => c.story ?? c[1]?.story).filter(Boolean))];
}

async function pushCases(client, cfg, cases) {
  const projectId = await resolveProjectId(client, cfg.project);
  const planId = await resolveTestPlanId(client, cfg.project, cfg.plan);
  const suiteId = await resolveTestSuiteId(client, cfg.project, cfg.suite);
  const buildId = await ensureBuild(client, planId, cfg.build);
  const caseIds = new Map();

  for (const item of cases.values ? cases.values() : cases) {
    const externalId = item.externalId ?? `${item.story}-${item.ac}`;
    const summary = [
      `**Story:** ${item.story}`,
      `**AC:** ${item.ac}`,
      `**Playwright:** \`${item.title}\``,
      `**File:** \`${item.file}\``,
      '',
      '_Synced from the repository by aidlc-testlink.mjs._',
    ].join('\n');

    if (!APPLY) {
      console.log(`[dry-run] upsert case ${externalId} → suite "${cfg.suite}"`);
      continue;
    }

    const caseId = await upsertTestCase(client, {
      projectId,
      suiteId,
      externalId,
      name: `${item.story}/${item.ac} — ${item.title}`,
      summary,
      authorLogin: cfg.author,
    });
    await addCaseToPlan(client, planId, caseId);
    caseIds.set(`${item.story}/${item.ac}`, { caseId, externalId });
    console.log(`TestLink case ${externalId} (id ${caseId})`);
  }

  return { projectId, planId, suiteId, buildId, caseIds };
}

async function reportResults(client, cfg, results, ctx) {
  for (const r of results) {
    const key = `${r.story}/${r.ac}`;
    const status = r.outcome === 'pass' ? 'p' : 'f';
    const notes = [
      `Playwright: ${r.title}`,
      `File: ${r.file}`,
      `CI: ${cfg.runUrl}`,
    ].join('\n');

    if (!APPLY) {
      console.log(`[dry-run] report ${key} → ${status === 'p' ? 'PASS' : 'FAIL'}`);
      continue;
    }

    const mapped = ctx.caseIds.get(key);
    if (!mapped?.caseId) {
      console.warn(`aidlc-testlink: skip result ${key} — case id not found`);
      continue;
    }

    await reportResult(client, {
      caseId: mapped.caseId,
      planId: ctx.planId,
      buildId: ctx.buildId,
      status,
      notes,
      platformId: cfg.platformId,
    });
    console.log(`TestLink result ${key} → ${status === 'p' ? 'PASS' : 'FAIL'}`);
  }
}

function syncJira(cfg, storyIds) {
  const reportArg = ['--report', cfg.reportPath];
  const runUrlArg = cfg.runUrl && cfg.runUrl !== '—' ? ['--run-url', cfg.runUrl] : [];

  for (const storyId of storyIds) {
    const cmd = [
      'node',
      join(REPO, 'tools', 'aidlc-jira.mjs'),
      '--story',
      storyId,
      '--tests',
      ...reportArg,
      ...runUrlArg,
      ...(APPLY ? ['--apply'] : []),
    ];
    console.log(`\n→ Jira sync ${storyId}${APPLY ? '' : ' (dry run)'}`);
    execFileSync(process.execPath, cmd.slice(1), {
      stdio: 'inherit',
      cwd: REPO,
      env: process.env,
    });
  }
}

async function publish(cfg) {
  const specCases = discoverFromSpecs(cfg.specsDir);
  const reportResults_ = discoverFromReport(cfg.reportPath);

  for (const r of reportResults_) {
    specCases.set(`${r.story}/${r.ac}`, r);
  }

  if (specCases.size === 0) {
    die('no US-###/AC-## citations found in specs or report.');
  }

  if (!cfg.apiUrl || !cfg.devKey) {
    console.log(
      'DRY RUN — TestLink credentials not set (TESTLINK_API_URL, TESTLINK_DEV_KEY).\n' +
        'Showing what would be published:\n',
    );
    for (const item of specCases.values()) {
      const outcome = item.outcome ? ` [${item.outcome}]` : '';
      console.log(`  ${item.story}/${item.ac}${outcome} — ${item.title}`);
    }
    console.log(
      `\nStories for Jira: ${[...new Set([...specCases.values()].map((c) => c.story))].join(', ')}`,
    );
    console.log('\nRe-run with credentials and --apply to write to TestLink + Jira.');
    syncJira(cfg, [...new Set([...specCases.values()].map((c) => c.story))]);
    return;
  }

  const client = createTestLinkClient({ apiUrl: cfg.apiUrl, devKey: cfg.devKey });
  const ctx = await pushCases(client, cfg, specCases);
  await reportResults(client, cfg, reportResults_, ctx);
  syncJira(cfg, [...new Set(reportResults_.map((r) => r.story))]);
}

async function pushCasesOnly(cfg) {
  const cases = discoverFromSpecs(cfg.specsDir);
  if (cases.size === 0) {
    die(`no Playwright tests citing US-###/AC-## in ${cfg.specsDir}`);
  }

  if (!cfg.apiUrl || !cfg.devKey) {
    console.log('DRY RUN — set TESTLINK_API_URL and TESTLINK_DEV_KEY, then --apply.');
    for (const item of cases.values()) {
      console.log(`  ${item.story}/${item.ac} — ${item.title}`);
    }
    return;
  }

  const client = createTestLinkClient({ apiUrl: cfg.apiUrl, devKey: cfg.devKey });
  await pushCases(client, cfg, cases);
}

async function main() {
  const cfg = loadConfig();
  const command = args[0];

  if (!command || command === 'help' || command === '--help') {
    console.log(`aidlc-testlink — Playwright → TestLink → Jira

Usage:
  node tools/aidlc-testlink.mjs publish [--report e2e/playwright-report.json] [--apply]
  node tools/aidlc-testlink.mjs push-cases [--specs e2e/tests] [--apply]

Environment (see .env.example):
  TESTLINK_API_URL   e.g. https://testlink.example.com/lib/api/xmlrpc/v1/xmlrpc.php
  TESTLINK_DEV_KEY   Developer key from TestLink user profile
  TESTLINK_PROJECT   TestLink project name (default: JIRA_PROJECT_KEY or EDBS)
  TESTLINK_PLAN      Test plan name (default: EPIC-001 MVP)
  TESTLINK_SUITE     Test suite name (default: Playwright E2E)
  TESTLINK_BUILD     Build name (default: e2e-YYYY-MM-DD-<sha>)
  TESTLINK_AUTHOR    TestLink login for case author
  TESTLINK_PLATFORM_ID  Optional platform id when the plan requires one

Workflow:
  cd e2e && npm test
  node tools/aidlc-testlink.mjs publish --apply

Dry run is the default. Jira updates use tools/aidlc-jira.mjs (ai/context/jira-sync.md).
`);
    return;
  }

  if (command === 'publish') {
    await publish(cfg);
    return;
  }

  if (command === 'push-cases') {
    await pushCasesOnly(cfg);
    return;
  }

  die(`unknown command "${command}". Try: publish | push-cases | help`);
}

main().catch((error) => die(error.message));
