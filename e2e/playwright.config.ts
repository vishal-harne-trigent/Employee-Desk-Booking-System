import { defineConfig, devices } from '@playwright/test';
import path from 'node:path';

const repoRoot = path.resolve(__dirname, '..');
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
  ],
  use: {
    baseURL: process.env.E2E_BASE_URL ?? 'http://localhost:5198',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
  },
  webServer: {
    command: `dotnet run --project "${webProject}" --launch-profile http`,
    url: 'http://localhost:5198',
    reuseExistingServer: !process.env.CI,
    timeout: 180_000,
    cwd: repoRoot,
  },
  globalSetup: path.join(__dirname, 'src', 'seed.setup.ts'),
  projects: [
    {
      name: 'employee-chromium',
      use: {
        ...devices['Desktop Chrome'],
        storageState: path.join(__dirname, '.auth', 'employee.json'),
      },
    },
    {
      name: 'admin-chromium',
      use: {
        ...devices['Desktop Chrome'],
        storageState: path.join(__dirname, '.auth', 'admin.json'),
      },
    },
  ],
});
