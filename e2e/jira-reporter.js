// playwright-jira-reporter.js — matches "Playwright to Jira Config" app webhook format.
// Set PLAYWRIGHT_JIRA_WEBHOOK_URL, PLAYWRIGHT_JIRA_TOKEN, JIRA_ISSUE_ID in repo-root .env
// EDBS-38 → JIRA_ISSUE_ID=10037 (numeric id, not the key)

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
    if (!this.webhookUrl || !this.token) {
      return;
    }

    const issueId = parseIssueId(this.issueId);
    if (issueId === undefined) {
      console.warn(
        '\n[Jira Reporter] No JIRA_ISSUE_ID provided. Skipping Jira update.',
      );
      console.warn('[Jira Reporter] EDBS-38 → use JIRA_ISSUE_ID=10037');
      return;
    }

    const duration = Date.now() - this.startTime;
    const stats = { passed: 0, failed: 0, skipped: 0, duration };
    /** @type {{ title: string; status: string; duration: number }[]} */
    const tests = [];

    /** @param {Suite} suite */
    const traverse = (suite) => {
      for (const test of suite.tests) {
        const outcome = test.results[test.results.length - 1];
        if (!outcome) continue;

        let mappedStatus = 'skipped';
        if (outcome.status === 'passed') {
          stats.passed++;
          mappedStatus = 'passed';
        } else if (
          outcome.status === 'failed' ||
          outcome.status === 'timedOut' ||
          outcome.status === 'interrupted'
        ) {
          stats.failed++;
          mappedStatus = 'failed';
        } else {
          stats.skipped++;
        }

        tests.push({
          title: test.title,
          status: mappedStatus,
          duration: outcome.duration ?? 0,
        });
      }
      for (const child of suite.suites) traverse(child);
    };

    if (this.suite) traverse(this.suite);

    const overallStatus = stats.failed > 0 ? 'failed' : 'passed';
    const payload = {
      token: this.token,
      issueId,
      status: overallStatus,
      summary: stats,
      tests,
    };

    console.log('\n[Jira Reporter] Sending report to Jira issue:', issueId);
    console.log('[Jira Reporter] Payload:', JSON.stringify(payload, null, 2));

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
      console.log('[Jira Reporter] Successfully sent to Jira.');
      if (body) console.log('[Jira Reporter] Response:', body);
    } catch (error) {
      console.error('[Jira Reporter] Failed to send to Jira:', error);
    }
  }
}

/** App expects numeric issue id (e.g. 10037), not key (EDBS-38). */
function parseIssueId(raw) {
  if (raw === undefined || raw === '') return undefined;
  const n = Number(raw);
  return Number.isFinite(n) ? n : undefined;
}

module.exports = JiraReporter;
