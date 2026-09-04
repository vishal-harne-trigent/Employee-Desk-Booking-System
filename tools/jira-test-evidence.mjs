// Shared builders for rich Playwright test evidence in Jira (test subtask body).
// Used by tools/aidlc-jira.mjs and triggered after runs from e2e/jira-reporter.js.

import { readFileSync, existsSync, readdirSync } from 'node:fs';
import { join, dirname, isAbsolute } from 'node:path';
import { createRequire } from 'node:module';

const require = createRequire(import.meta.url);
const { uploadScreenshotToJira } = require('../e2e/jira-attachments.js');

const CITATION = /(US-\d{3})\/(AC-\d{2})/;
const ST_CITATION = /SCR-\d{3}\/ST-\d{2}/;
const EDGE = /edge case/i;

function* walkSpecs(suite) {
  for (const spec of suite.specs ?? []) yield spec;
  for (const child of suite.suites ?? []) yield* walkSpecs(child);
}

/** Find e2e/plans/US-###-*.md for a story. */
export function findTestPlan(repoRoot, storyId) {
  const plansDir = join(repoRoot, 'e2e', 'plans');
  if (!existsSync(plansDir)) return null;
  const prefix = `${storyId.toLowerCase()}-`;
  const hit = readdirSync(plansDir).find(
    (f) => f.toLowerCase().startsWith(prefix) && f.endsWith('.md'),
  );
  return hit ? join(plansDir, hit) : null;
}

/** Parse markdown pipe tables under a ## heading. */
export function parsePlanTable(planText, heading) {
  const re = new RegExp(
    `^##\\s+${heading}\\s*$([\\s\\S]*?)(?=^##\\s|\\Z)`,
    'm',
  );
  const section = (planText.match(re)?.[1] ?? '').trim();
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
    if (cells.length === headers.length) {
      const row = {};
      headers.forEach((h, idx) => {
        row[h] = cells[idx];
      });
      rows.push(row);
    }
  }
  return rows;
}

function mapAcLabel(title) {
  const m = CITATION.exec(title ?? '');
  if (m) return m[2];
  if (ST_CITATION.test(title ?? '')) return 'ST-01';
  if (EDGE.test(title ?? '')) return 'Edge';
  return '—';
}

function formatDuration(ms) {
  if (ms == null) return '—';
  if (ms >= 1000) return `${(ms / 1000).toFixed(1)} s`;
  return `${Math.round(ms)} ms`;
}

function formatUtc(iso) {
  if (!iso) return '—';
  try {
    return iso.replace('T', ' ').replace(/\.\d{3}Z$/, ' UTC');
  } catch {
    return iso;
  }
}

function resultLabel(ok, status) {
  if (status === 'skipped') return 'Skipped';
  return ok ? '**Pass**' : '**Fail**';
}

function isImageAttachment(att) {
  return (
    att?.contentType?.startsWith('image/')
    || att?.name === 'screenshot'
    || /\.(png|jpe?g|webp)$/i.test(att?.path ?? att?.name ?? '')
  );
}

/** Resolve Playwright JSON report attachment to a local file or buffer. */
export function extractScreenshotFromReportResult(result, reportDir) {
  if (!result?.attachments?.length) return null;
  const att = result.attachments.find(isImageAttachment);
  if (!att) return null;

  const contentType = att.contentType ?? 'image/png';
  const name = att.name ?? 'screenshot.png';

  if (att.path) {
    const full = isAbsolute(att.path) ? att.path : join(reportDir, att.path);
    if (existsSync(full)) {
      return { path: full, contentType, name };
    }
  }
  if (att.body) {
    const buf =
      typeof att.body === 'string'
        ? Buffer.from(att.body, 'base64')
        : Buffer.from(att.body);
    if (buf.length) return { body: buf, contentType, name };
  }
  return null;
}

function screenshotPathFromResult(result, reportDir) {
  const shot = extractScreenshotFromReportResult(result, reportDir ?? '.');
  return shot?.path?.replace(/\\/g, '/') ?? '';
}

export function jiraAuthConfigured(env = process.env) {
  return (
    !!env.JIRA_EMAIL
    && !!env.JIRA_API_TOKEN
    && env.JIRA_EMAIL !== 'your-email@example.com'
  );
}

/**
 * Upload failure screenshots from playwright-report.json to the story issue (EDBS-38).
 * @returns {Promise<Map<string, { id: string, filename: string, content: string }>>}
 */
export async function uploadFailureScreenshotsFromReport({
  report,
  reportPath,
  env = process.env,
  issueKey,
}) {
  const uploads = new Map();
  if (!report || !issueKey || !jiraAuthConfigured(env)) return uploads;

  const reportDir = dirname(reportPath ?? join(process.cwd(), 'e2e', 'playwright-report.json'));

  for (const suite of report.suites ?? []) {
    for (const spec of walkSpecs(suite)) {
      const results = (spec.tests ?? []).flatMap((t) => t.results ?? []);
      const last = results[results.length - 1];
      const failed =
        !spec.ok || last?.status === 'failed' || last?.status === 'timedOut';
      if (!failed) continue;

      const shot = extractScreenshotFromReportResult(last, reportDir);
      if (!shot) continue;

      const title = spec.title ?? 'unknown';
      const uploaded = await uploadScreenshotToJira({
        env,
        issueKey,
        shot,
        testTitle: title,
      });
      if (uploaded) {
        uploads.set(title, uploaded);
        console.log(
          `[aidlc-jira] Uploaded failure screenshot → ${issueKey}: ${uploaded.filename}`,
        );
      }
    }
  }
  return uploads;
}

export function applyScreenshotUploads(trials, uploads) {
  if (!uploads?.size) return trials;
  return trials.map((t) => {
    const up = uploads.get(t.title);
    if (!up) return t;
    return {
      ...t,
      screenshotFilename: up.filename,
      screenshotUrl: up.content,
      screenshotAttachmentId: up.id,
    };
  });
}

export function buildFailureScreenshotsSection({
  uploads,
  issueKey,
  failedCount,
  authConfigured,
}) {
  if (uploads?.size) {
    const rows = [...uploads.entries()].map(
      ([title, meta]) => `| ${title.replace(/\|/g, '\\|')} | \`${meta.filename}\` |`,
    );
    return (
      `Open **${issueKey} → Attachments** (right panel or issue menu) to view these files:\n\n`
      + '| Failed test | File |\n'
      + '| --- | --- |\n'
      + rows.join('\n')
    );
  }
  if (failedCount > 0 && !authConfigured) {
    return (
      '_Failure screenshot(s) were **not** uploaded — set **JIRA_EMAIL** and **JIRA_API_TOKEN** '
      + 'in repo `.env`, then re-run Playwright or `node tools/aidlc-jira.mjs --story US-001 --tests --apply`._'
    );
  }
  if (failedCount > 0) {
    return (
      '_Playwright reported failed test(s) but no screenshot attachment was found in '
      + '`playwright-report.json` (screenshots are captured only on failure when the browser closes cleanly)._'
    );
  }
  return '_No failures in the latest run — failure screenshots appear here when a test fails._';
}

/** All specs from Playwright JSON report with timing. */
export function trialsFromReport(report, reportPath) {
  const reportDir = reportPath
    ? dirname(reportPath)
    : join(process.cwd(), 'e2e');
  const trials = [];
  let n = 0;
  for (const suite of report.suites ?? []) {
    for (const spec of walkSpecs(suite)) {
      n++;
      const results = (spec.tests ?? []).flatMap((t) => t.results ?? []);
      const last = results[results.length - 1];
      const status = last?.status ?? (spec.ok ? 'passed' : 'failed');
      const failed = !spec.ok || status === 'failed' || status === 'timedOut';
      trials.push({
        trial: n,
        title: spec.title ?? '—',
        ac: mapAcLabel(spec.title),
        started: last?.startTime ?? report.stats?.startTime,
        duration: last?.duration ?? 0,
        ok: spec.ok && status !== 'skipped',
        status,
        browser: 'Chromium',
        file: spec.file ?? '',
        screenshot: failed ? screenshotPathFromResult(last, reportDir) : '',
      });
    }
  }
  return trials;
}

export function buildRunSummary({ passed, failed, skipped, durationMs, baseUrl }) {
  const total = passed + failed + skipped;
  const now = new Date();
  const stamp = now.toISOString().replace('T', ' ').replace(/\.\d{3}Z$/, ' UTC');
  return (
    `**Run:** Playwright E2E — ${stamp}  \n`
    + `**Environment:** \`${baseUrl ?? 'http://localhost:5198'}\` (local dev)  \n`
    + `**Browser:** Chromium  \n`
    + `**Outcome:** ${passed}/${total} scenarios passed · ${failed} failed · ${skipped} skipped  \n`
    + `**Total run time:** ~${formatDuration(durationMs)}`
  );
}

export function buildTestCasesTable(scenarios) {
  if (!scenarios.length) {
    return '_No scenarios table found in e2e plan — add e2e/plans/US-###.md._';
  }
  const rows = scenarios.map((s, i) => {
    const id = s.ID ?? `S-${String(i + 1).padStart(2, '0')}`;
    const tc = id.replace(/^S-/, 'TC-');
    const steps =
      `**Given** ${s.Given ?? '—'} · **When** ${s.When ?? '—'} · **Then** ${s.Then ?? '—'}`;
    const maps = s.AC ?? '—';
    const name = (s.Then ?? s.Given ?? id).replace(/\s+/g, ' ').trim();
    return `| ${tc} | ${name} | ${steps} | ${maps} |`;
  });
  return (
    '| ID | Test case | Steps (Given → When → Then) | Maps to |\n'
    + '| --- | --- | --- | --- |\n'
    + rows.join('\n')
  );
}

export function buildTestDataTable(accounts) {
  if (!accounts.length) {
    return '_No dev seed accounts in plan — add ## Accounts (dev seed) to e2e plan._';
  }
  const rows = accounts.map(
    (a) => `| ${a.Role ?? '—'} | \`${a.Email ?? '—'}\` | \`${a.Password ?? '—'}\` |`,
  );
  return (
    '| Role | Email | Password |\n'
    + '| --- | --- | --- |\n'
    + rows.join('\n')
  );
}

export function buildTrialsTable(trials) {
  if (!trials.length) {
    return '_No execution rows — run Playwright first to generate playwright-report.json._';
  }
  const rows = trials.map((t) => {
    let shot = '—';
    if (t.screenshotFilename) {
      shot = `**${t.screenshotFilename}** (issue Attachments)`;
    } else if (t.screenshot) {
      shot = `\`${t.screenshot}\` (local — re-sync with Jira API creds to upload)`;
    } else if (!t.ok && t.status !== 'skipped') {
      shot = '_none in report_';
    }
    return `| ${t.trial} | ${t.title} | ${t.ac} | ${formatUtc(t.started)} | ${formatDuration(t.duration)} | ${resultLabel(t.ok, t.status)} | ${t.browser} | ${shot} |`;
  });
  return (
    '| Trial | Test case | AC | Started (UTC) | Duration | Result | Browser | Screenshot |\n'
    + '| --- | --- | --- | --- | --- | --- | --- | --- |\n'
    + rows.join('\n')
  );
}

export function buildAcSummaryTable(storyId, acBlocks, reportIndex, scenarios) {
  const primaryTest = new Map();
  for (const s of scenarios) {
    const ac = s.AC;
    if (ac && ac.startsWith('AC-')) {
      primaryTest.set(ac, s.ID?.replace(/^S-/, 'TC-') ?? '—');
    }
  }

  const rows = acBlocks.map((ac) => {
    let outcome = 'Not yet run';
    const key = `${storyId}/${ac.id}`;
    const hit = reportIndex?.get(key);
    if (hit?.outcome === 'pass') outcome = '**Pass**';
    else if (hit?.outcome === 'fail') outcome = '**Fail**';
    else if (hit) outcome = 'Not yet run';
    else outcome = 'Not yet automated';
    const tc = primaryTest.get(ac.id) ?? '—';
    return `| ${ac.id} | ${ac.title} | ${outcome} | ${tc} |`;
  });
  if (!rows.length) return '_No acceptance criteria in story file._';
  return (
    '| AC | Criterion | Outcome | Primary test |\n'
    + '| --- | --- | --- | --- |\n'
    + rows.join('\n')
  );
}

export function loadPlaywrightReport(reportPath) {
  if (!reportPath || !existsSync(reportPath)) return null;
  return JSON.parse(readFileSync(reportPath, 'utf8'));
}

export function statsFromReport(report, reportPath) {
  const trials = trialsFromReport(report, reportPath);
  let passed = 0;
  let failed = 0;
  let skipped = 0;
  for (const t of trials) {
    if (t.status === 'skipped') skipped++;
    else if (t.ok) passed++;
    else failed++;
  }
  const durationMs = report.stats?.duration ?? trials.reduce((s, t) => s + t.duration, 0);
  return { passed, failed, skipped, durationMs, trials };
}

export function buildRichEvidence({
  storyId,
  storyText,
  acBlocks,
  reportPath,
  reportIndex,
  repoRoot,
  baseUrl,
  ciRunUrl,
  artifactUrl,
  planUrl,
  specPath,
  screenshotUploads,
  storyIssueKey,
}) {
  const planPath = findTestPlan(repoRoot, storyId);
  const planText = planPath ? readFileSync(planPath, 'utf8') : '';
  const scenarios = parsePlanTable(planText, 'Scenarios');
  const accounts = parsePlanTable(planText, 'Accounts \\(dev seed\\)');

  const report = loadPlaywrightReport(reportPath);
  const stats = report
    ? statsFromReport(report, reportPath)
    : { passed: 0, failed: 0, skipped: 0, durationMs: 0, trials: [] };

  const trialsWithShots = applyScreenshotUploads(stats.trials, screenshotUploads);
  const acSummary = buildAcSummaryTable(storyId, acBlocks, reportIndex, scenarios);
  const issueKey = storyIssueKey ?? process.env.JIRA_ISSUE_KEY ?? 'parent story';

  return {
    RUN_SUMMARY: buildRunSummary({ ...stats, baseUrl }),
    TEST_CASES_TABLE: buildTestCasesTable(scenarios),
    TEST_DATA_TABLE: buildTestDataTable(accounts),
    TEST_TRIALS_TABLE: buildTrialsTable(trialsWithShots),
    FAILURE_SCREENSHOTS: buildFailureScreenshotsSection({
      uploads: screenshotUploads,
      issueKey,
      failedCount: stats.failed,
      authConfigured: jiraAuthConfigured(),
    }),
    AC_SUMMARY_TABLE: acSummary,
    PLAN_URL: planUrl ?? (planPath ? planPath.replace(/\\/g, '/') : '—'),
    SPEC_PATH: specPath ?? `e2e/tests/${storyId.toLowerCase()}-*.spec.ts`,
    CI_RUN_URL: ciRunUrl ?? '—',
    STORY_ID: storyId,
    ARTIFACT_URL: artifactUrl ?? '—',
  };
}
