# TestLink sync — binding rules for QA

TestLink is a **parallel test-management view** for clients and test managers who do not read the repository. It is **not an approval surface** — same boundary as Jira ([`jira-sync.md`](jira-sync.md)).

## What TestLink is for here

| Purpose | Detail |
| ------- | ------ |
| **Test case catalogue** | Playwright specs citing `US-###/AC-##` are pushed as TestLink test cases |
| **Execution evidence** | Pass/fail from `e2e/playwright-report.json` after a real run |
| **Jira visibility** | After TestLink publish, `aidlc-jira.mjs --tests --report` updates Jira test subtasks with the same outcomes |

GitHub + CI remain canonical. TestLink and Jira are convenience mirrors.

## The flow

```
Playwright run  →  e2e/playwright-report.json
       ↓
aidlc-testlink publish  →  TestLink (cases + results)
       ↓
aidlc-jira --tests --report  →  Jira test tickets (Pass / Fail / Not yet automated)
```

**One command** runs the full chain:

```powershell
cd e2e
npm test
cd ..
node tools/aidlc-testlink.mjs publish --apply
```

Dry run is the default (no `--apply`).

## Rules

- **Results only from real runs.** Never hand-type Pass in TestLink or Jira.
- **Test names carry the criterion** — `US-001/AC-03` in the Playwright title, same as unit/API tests.
- **Jira writes go through `aidlc-jira.mjs` only** — not the TestLink UI, not ad-hoc API calls.
- **Missing TestLink must not break CI.** Publish steps are optional (secrets-gated in GitHub Actions).

## Configuration

Add to `.env` (copy from `.env.example`):

| Variable | Meaning |
| -------- | ------- |
| `TESTLINK_API_URL` | Full XML-RPC URL, e.g. `https://host/testlink/lib/api/xmlrpc/v1/xmlrpc.php` |
| `TESTLINK_DEV_KEY` | Developer key from your TestLink user profile |
| `TESTLINK_PROJECT` | Project name (default: `JIRA_PROJECT_KEY` / `EDBS`) |
| `TESTLINK_PLAN` | Test plan name (default: `EPIC-001 MVP`) |
| `TESTLINK_SUITE` | Suite for Playwright cases (default: `Playwright E2E`) |
| `TESTLINK_BUILD` | Build label (default: `e2e-YYYY-MM-DD-<git-sha>`) |
| `TESTLINK_AUTHOR` | TestLink login for case author |
| `TESTLINK_PLATFORM_ID` | Required when your test plan uses platforms |

Create the **project**, **test plan**, and **test suite** in TestLink before the first `--apply`.

### Linking TestLink ↔ Jira (optional)

If your TestLink instance has the **Jira integration** plugin, map requirement keys (`EDBS-38`, etc.) on test cases in TestLink admin. The repo tool links Jira through `aidlc-jira.mjs` using manifest `jira` fields — you do not need the plugin for Jira result sync.

## Commands

```powershell
# Install tool dependencies (once)
cd tools && npm install && cd ..

# Push cases only (no run required)
node tools/aidlc-testlink.mjs push-cases

# Full publish after a Playwright run
node tools/aidlc-testlink.mjs publish --report e2e/playwright-report.json --apply

# Jira only (no TestLink) — after Playwright run
node tools/aidlc-jira.mjs --story US-001 --tests --report e2e/playwright-report.json --apply
```

## Who may write

| Persona | TestLink | Jira test tickets |
| ------- | -------- | ----------------- |
| `/qa` | Yes | Yes (via `aidlc-jira.mjs`) |
| `/manager` | Read / orchestrate | Yes |
| Others | Route to `/qa` | Route to `/qa` or `/manager` |
