# US-001 — Sign in and sign out

> Approval = Gate 1 review of this file's PR. Delivery = the story PR (`feat/US-001-sign-in`) merging with every AC proven by a test named `... (US-001/AC-##)`.

|                |                                         |
| -------------- | --------------------------------------- |
| **Epic**       | EPIC-001                                |
| **Traces to**  | REQ-001, REQ-002, REQ-003, REQ-004, REQ-005, NFR-003, NFR-004 |
| **Priority**   | Must                                    |
| **Estimate**   | 5 pts (AI draft — humans re-estimate)   |
| **Depends on** | —                                       |

## Story

As an **Employee or Admin**
I want to sign in with my email and password and sign out when done
So that only authorised users access desk booking features.

## Acceptance criteria

### AC-01 Employee lands on Book Desk after sign-in

- **Given** an active Employee account with valid credentials
- **When** the user submits the sign-in form
- **Then** the user is authenticated and routed to the Book Desk screen (SCR-002)

### AC-02 Admin lands on All Bookings after sign-in

- **Given** an active Admin account with valid credentials
- **When** the user submits the sign-in form
- **Then** the user is authenticated and routed to the Admin All Bookings screen (SCR-004)

### AC-03 Invalid credentials rejected

- **Given** unknown email or wrong password
- **When** the user submits sign-in
- **Then** sign-in fails with a generic error message and no session is created (SCR-001 ST-03)

### AC-04 Deactivated account rejected

- **Given** a user account marked deactivated
- **When** the user attempts to sign in with correct credentials
- **Then** sign-in is rejected with a deactivated-account message (SCR-001 ST-04)

### AC-05 Sign out ends session

- **Given** a signed-in user on any authenticated screen
- **When** the user chooses Sign out
- **Then** the session ends and the user is returned to the sign-in screen

## Edge cases

- Empty email or password: client validation prevents submit or server rejects with same generic error as AC-03.
- Double submit while loading: button disabled (SCR-001 ST-02).

## UI

Served by **SCR-001 — Sign In** (`inception/design/screens/SCR-001-sign-in.md`). States ST-01 Default through ST-04 Deactivated account. Keyboard: Tab order email → password → Sign in; errors use `role="alert"`.

## QA notes

Seed at least one Employee and one Admin for routing tests. Deactivated user fixture for AC-04.

## API impacts

Auth endpoints (sign-in, sign-out, session) — to be defined in architecture; validated at delivery against OpenAPI.
