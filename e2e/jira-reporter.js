// Posts enriched Playwright run results to Playwright Results for Jira (Orbit A webhook).
// Set PLAYWRIGHT_JIRA_* + JIRA_ISSUE_ID in repo-root .env
// Optional: JIRA_STORY_ID + JIRA_* API creds → updates EDBS-95 test evidence subtask

const { execFileSync } = require('node:child_process');
const path = require('node:path');
const { buildWebhookPayload } = require('./jira-webhook-payload');
const {
  extractScreenshot,
  screenshotToBase64,
  uploadScreenshotToJira,
} = require('./jira-attachments');

/** @typedef {import('@playwright/test/reporter').FullConfig} FullConfig */
/** @typedef {import('@playwright/test/reporter').Suite} Suite */
/** @typedef {import('@playwright/test/reporter').FullResult} FullResult */

class JiraReporter {
  constructor(options = {}) {
    this.issueId = options.issueId ?? process.env.JIRA_ISSUE_ID;
    this.webhookUrl = process.env.PLAYWRIGHT_JIRA_WEBHOOK_URL;
    this.token = process.env.PLAYWRIGHT_JIRA_TOKEN;
    /** @type {Suite | undefined} */
    this.suite = undefined;
    this.startTime = 0;
  }

  /** @param {FullConfig} _config @param {Suite} suite */
  onBegin(_config, suite) {
    this.suite = suite;
    this.startTime = Date.now();
  }

  /** @param {FullResult} _result */
  async onEnd(_result) {
    if (this.webhookUrl && this.token) {
      await this.sendWebhook();
    }
    await this.syncTestEvidenceSubtask();
  }

  async sendWebhook() {
    if (!parseIssueId(this.issueId)) {
      console.warn('\n[Jira Reporter] No JIRA_ISSUE_ID — skipping webhook (EDBS-38 → 10037).');
      return;
    }

    const payload = buildWebhookPayload({
      suite: this.suite,
      startTime: this.startTime,
      env: process.env,
    });

    await enrichFailedTestScreenshots(payload, this.suite, process.env);

    console.log('\n[Jira Reporter] Sending enriched webhook to issue:', payload.issueId);
    console.log(
      `[Jira Reporter] ${payload.summary.passed}/${payload.summary.total} passed · `
        + `${payload.details.testCases.length} plan scenarios · `
        + `${payload.details.trials.length} trials`,
    );

    try {
      const response = await fetch(this.webhookUrl, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload),
      });
      const body = await response.text();
      if (!response.ok) {
        console.error(
          `[Jira Reporter] Webhook returned ${response.status}: ${body.slice(0, 500)}`,
        );
        return;
      }
      console.log('[Jira Reporter] Webhook OK:', body || '(empty)');
    } catch (error) {
      console.error('[Jira Reporter] Webhook failed:', error);
    }
  }

  async syncTestEvidenceSubtask() {
    const storyId = process.env.JIRA_STORY_ID;
    const hasApi =
      storyId
      && process.env.JIRA_EMAIL
      && process.env.JIRA_API_TOKEN
      && process.env.JIRA_EMAIL !== 'your-email@example.com';

    if (!hasApi) {
      console.log(
        '[Jira Reporter] Skipping test evidence subtask — set JIRA_STORY_ID, JIRA_EMAIL, JIRA_API_TOKEN',
      );
      return;
    }

    const repoRoot = path.resolve(__dirname, '..');
    const reportPath = path.join(__dirname, 'playwright-report.json');

    console.log(
      `[Jira Reporter] Updating ${storyId} test evidence on subtask + parent ${process.env.JIRA_ISSUE_KEY ?? 'story'}…`,
    );

    try {
      execFileSync(
        process.execPath,
        [
          'tools/aidlc-jira.mjs',
          '--story',
          storyId,
          '--tests',
          '--report',
          reportPath,
          '--apply',
        ],
        { cwd: repoRoot, stdio: 'inherit', env: process.env },
      );
    } catch {
      console.error('[Jira Reporter] Test evidence subtask update failed.');
    }
  }
}

function parseIssueId(raw) {
  if (raw === undefined || raw === '') return undefined;
  const n = Number(raw);
  return Number.isFinite(n) ? n : undefined;
}

/** Attach screenshots to failed tests before webhook POST. */
async function enrichFailedTestScreenshots(payload, suite, env) {
  if (!suite) return;

  const issueKey = env.JIRA_ISSUE_KEY;
  const hasUpload =
    issueKey
    && env.JIRA_BASE_URL
    && env.JIRA_EMAIL
    && env.JIRA_API_TOKEN
    && env.JIRA_EMAIL !== 'your-email@example.com';

  /** @type {Map<string, import('@playwright/test/reporter').TestResult | undefined>} */
  const outcomeByTitle = new Map();

  /** @param {import('@playwright/test/reporter').Suite} s */
  function collectOutcomes(s) {
    for (const test of s.tests) {
      const outcome = test.results?.[test.results.length - 1];
      outcomeByTitle.set(test.title, outcome);
    }
    for (const child of s.suites) collectOutcomes(child);
  }
  collectOutcomes(suite);

  let screenshotCount = 0;

  for (const row of payload.tests) {
    if (row.status !== 'failed') continue;

    const rawTitle = row.title.replace(/^\[TC-\d+\]\s*/, '');
    const outcome = outcomeByTitle.get(rawTitle);
    const shot = extractScreenshot(outcome);
    if (!shot) continue;

    const base64 = screenshotToBase64(shot);
    if (base64) {
      row.screenshot = base64;
      screenshotCount++;
    }

    if (hasUpload) {
      const uploaded = await uploadScreenshotToJira({
        env,
        issueKey,
        shot,
        testTitle: rawTitle,
      });
      if (uploaded) {
        row.screenshotUrl = uploaded.content;
        row.screenshotAttachmentId = uploaded.id;
        row.screenshotFilename = uploaded.filename;
        console.log(
          `[Jira Reporter] Uploaded screenshot → ${issueKey}: ${uploaded.filename}`,
        );
      }
    }
  }

  for (const trial of payload.details.trials) {
    const match = payload.tests.find((t) => t.title === trial.testCase);
    if (match?.screenshot) trial.screenshot = match.screenshot;
    if (match?.screenshotUrl) trial.screenshotUrl = match.screenshotUrl;
  }

  if (screenshotCount > 0) {
    console.log(`[Jira Reporter] Included ${screenshotCount} failure screenshot(s) in payload.`);
  }
}

module.exports = JiraReporter;
