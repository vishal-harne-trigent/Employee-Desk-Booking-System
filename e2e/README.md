# E2E — Playwright (AI-DLC browser test layer)

Browser tests for **Employee Desk Booking System**. Test placement is recorded by `testDir` in this folder's `playwright.config.ts`.

## Prerequisites

- .NET 8 SDK + SQL Server LocalDB (same as Web app)
- Node.js 20+

## Setup

```powershell
cd e2e
npm install
npx playwright install chromium
```

If `npm install` fails with an authentication error, your machine may be pointing at a private registry — the `e2e/.npmrc` file forces the public npm registry for this folder.

## Run

```powershell
# From e2e/ — starts Web on http://localhost:5198 if not already running
npm test

# Watch the browser (headed — every test signs in through the login form)
npm run test:headed

# Or: $env:E2E_HEADED = "1"; npm test

# Interactive Playwright UI
npm run test:ui
```

Every test signs in through **SCR-001** (email, password, Sign in button). Cookie injection and `storageState` are not used.

Override base URL or accounts:

```powershell
$env:E2E_BASE_URL = "http://localhost:5198"
$env:E2E_EMPLOYEE_EMAIL = "vishal_h@trigent.com"
$env:E2E_ADMIN_EMAIL = "admin@trigent.com"
$env:E2E_DEACTIVATED_EMAIL = "deactivated@trigent.com"
$env:E2E_PASSWORD = "Password1!"
npm test
```

## Workflow (AI-DLC QA)

1. Write `e2e/plans/US-###.md` from `ai/templates/test-plan.md` — get PR review **before** coding tests.
2. Generate specs in `e2e/tests/` with titles citing `US-###/AC-##`.
3. Add spec paths to `knowledge/traceability/manifest.json` → story `tests[]`.
4. Run `node tools/aidlc-check.mjs` before the story PR merges.

## Reports

- HTML: `e2e/playwright-report/`
- JSON (for `aidlc-qa-coverage.mjs`): `e2e/playwright-report.json`

## Auth

Sign-in is performed in each test (or `beforeEach` for smoke specs) via `signInViaUi()` in `src/login.helpers.ts` — fill email, fill password, click **Sign in**, wait for redirect.
