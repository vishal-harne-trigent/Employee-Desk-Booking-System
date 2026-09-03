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
# All e2e tests
npm test

# US-001 sign-in story only
npx playwright test us-001-sign-in.spec.ts

# Watch the browser
npm run test:headed

# Interactive Playwright UI
npm run test:ui
```

US-001 tests sign in through **SCR-001** (email, password, Sign in) — no cookie injection.

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

## Publish to TestLink + Jira

After a run, push cases and pass/fail results to **TestLink**, then update **Jira** test tickets (see `ai/context/testlink-sync.md`):

```powershell
# Once: install tool dependencies
cd tools && npm install && cd ..

cd e2e && npm test && cd ..

# Dry run (default)
node tools/aidlc-testlink.mjs publish

# Write to TestLink + Jira (needs .env credentials)
node tools/aidlc-testlink.mjs publish --apply
```

Jira-only (no TestLink):

```powershell
node tools/aidlc-jira.mjs --story US-001 --tests --report e2e/playwright-report.json --apply
```

## Auth

- **US-001** (`tests/us-001-sign-in.spec.ts`) — signs in via the login form in each test.
- **Smoke** — `beforeEach` UI sign-in for authenticated navigation checks.

## CI

GitHub Actions (`.github/workflows/e2e.yml`) starts SQL Server 2022 in Docker and sets `ConnectionStrings__DefaultConnection` for the Web app — LocalDB is Windows-only and cannot run on `ubuntu-latest`.
