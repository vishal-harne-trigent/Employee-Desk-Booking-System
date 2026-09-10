import { defineConfig, devices } from '@playwright/test';
import fs from 'node:fs';
import path from 'node:path';

const repoRoot = path.resolve(__dirname, '..');

/** Load repo-root `.env` when vars are not already set (local runs). */
function loadEnvFile(filePath: string) {
  try {
    for (const line of fs.readFileSync(filePath, 'utf8').split('\n')) {
      const trimmed = line.trim();
      if (!trimmed || trimmed.startsWith('#')) continue;
      const eq = trimmed.indexOf('=');
      if (eq === -1) continue;
      const key = trimmed.slice(0, eq).trim();
      const value = trimmed.slice(eq + 1).trim();
      if (process.env[key] === undefined) process.env[key] = value;
    }
  } catch {
    // no .env — rely on shell / CI secrets
  }
}

loadEnvFile(path.join(repoRoot, '.env'));

const jiraReporterEnabled =
  !!process.env.PLAYWRIGHT_JIRA_WEBHOOK_URL && !!process.env.PLAYWRIGHT_JIRA_TOKEN;
const webProject = path.join(
  repoRoot,
  'src',
  'EmployeeDeskBooking.Web',
  'EmployeeDeskBooking.Web.csproj',
);

export default defineConfig({
  testDir: './tests',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: [
    ['list'],
    ['html', { open: 'never', outputFolder: 'playwright-report' }],
    ['json', { outputFile: 'playwright-report.json' }],
    ...(jiraReporterEnabled ? [['./jira-reporter.js'] as const] : []),
  ],
  use: {
    baseURL: process.env.E2E_BASE_URL ?? 'http://localhost:5198',
    headless: process.env.E2E_HEADED !== '1',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    video: process.env.E2E_HEADED === '1' ? 'on' : 'off',
  },
  webServer: {
    command: `dotnet run --project "${webProject}" --launch-profile http`,
    url: 'http://localhost:5198',
    reuseExistingServer: !process.env.CI,
    timeout: 180_000,
    cwd: repoRoot,
  },
  projects: [
    {
      name: 'us-001-chromium',
      testMatch: /us-001-sign-in\.spec\.ts/,
      use: { ...devices['Desktop Chrome'] },
    },
    {
      name: 'employee-chromium',
      testIgnore: /us-001-sign-in\.spec\.ts/,
      grepInvert: /\(US-001\/AC-02\)/,
      use: { ...devices['Desktop Chrome'] },
    },
    {
      name: 'admin-chromium',
      testIgnore: /us-001-sign-in\.spec\.ts/,
      grep: /\(US-001\/AC-02\)/,
      use: { ...devices['Desktop Chrome'] },
    },
  ],
});
