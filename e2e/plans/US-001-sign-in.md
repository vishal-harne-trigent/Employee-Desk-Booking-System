# US-001 — Sign in and sign out (browser test plan)

> Review before coding Playwright specs. Generated tests: `e2e/tests/us-001-sign-in.spec.ts`

## Scope

SCR-001 sign-in form — employee and admin routing, error states, sign out.

## Scenarios

| ID | Given | When | Then | AC |
| -- | ----- | ---- | ---- | -- |
| S-01 | Unauthenticated visitor | Opens login | Empty form, Sign in enabled | ST-01 |
| S-02 | Active Employee | Submits valid credentials | Redirect to Desk Availability | AC-01 |
| S-03 | Active Admin | Submits valid credentials | Redirect to All Bookings | AC-02 |
| S-04 | Wrong password | Submits sign-in | Generic alert, no session | AC-03 |
| S-05 | Deactivated account | Submits valid credentials | Deactivated alert, no session | AC-04 |
| S-06 | Signed-in Employee | Clicks Sign out | Login page, protected routes blocked | AC-05 |
| S-07 | Empty fields | Submits sign-in | Validation message, stay on login | Edge |

## Accounts (dev seed)

| Role | Email | Password |
| ---- | ----- | -------- |
| Employee | `vishal_h@trigent.com` | `Password1!` |
| Admin | `admin@trigent.com` | `Password1!` |
| Deactivated | `deactivated@trigent.com` | `Password1!` |
