# US-001 — Sign in and sign out (browser test plan)

> Review before coding Playwright specs. Generated tests: `e2e/tests/us-001-sign-in.spec.ts`

## Scope

SCR-001 sign-in form — employee and admin routing, error states, sign out. One scenario per story acceptance criterion (AC-01..AC-05).

## Scenarios

| ID | Given | When | Then | AC |
| -- | ----- | ---- | ---- | -- |
| S-01 | Active Employee | Submits valid credentials | Redirect to Desk Availability | AC-01 |
| S-02 | Active Admin | Submits valid credentials | Redirect to All Bookings | AC-02 |
| S-03 | Wrong password | Submits sign-in | Generic alert, no session | AC-03 |
| S-04 | Deactivated account | Submits valid credentials | Deactivated alert, no session | AC-04 |
| S-05 | Signed-in Employee | Clicks Sign out | Login page, protected routes blocked | AC-05 |

## Accounts (dev seed)

| Role | Email | Password |
| ---- | ----- | -------- |
| Employee | `vishal_h@trigent.com` | `Password1!` |
| Admin | `admin@trigent.com` | `Password1!` |
| Deactivated | `deactivated@trigent.com` | `Password1!` |
