# US-001 — Sign in and sign out

> Technical expansion of US-001. Story defines business need; this defines code behaviour.

|                   |                                                                  |
| ----------------- | ---------------------------------------------------------------- |
| **Story**         | `inception/stories/user-stories/US-001-sign-in.md`               |
| **Traces to**     | REQ-001, REQ-002, REQ-003, REQ-004, REQ-005, NFR-003, NFR-004    |
| **Screen**        | SCR-001                                                          |
| **Covering ADRs** | none                                                             |
| **Tier**          | Medium                                                           |
| **Status**        | implemented                                                      |
| **Updated**       | 2026-08-21                                                       |

## Problem

The scaffold has no authentication. Users must sign in with email and password, receive a cookie session, and sign out. Role determines post-login destination.

## Functional requirements

| ID    | Requirement | Priority | Serves | Status |
| ----- | ----------- | -------- | ------ | ------ |
| FR-01 | Active Employee with valid credentials is routed to Book Desk after sign-in | Must | AC-01 | implemented |
| FR-02 | Active Admin with valid credentials is routed to All Bookings after sign-in | Must | AC-02 | implemented |
| FR-03 | Unknown email or wrong password shows generic error; no session created | Must | AC-03 | implemented |
| FR-04 | Deactivated account shows deactivated message; no session created | Must | AC-04 | implemented |
| FR-05 | Sign out clears session and returns to sign-in | Must | AC-05 | implemented |

## Non-functional requirements

| ID     | Requirement | Serves |
| ------ | ----------- | ------ |
| NFR-01 | Passwords hashed via `IPasswordHasher<User>`; never logged | NFR-003 |

## Technical constraints

- Cookie authentication on `EmployeeDeskBooking.Web` only (API auth deferred)
- Presentation calls `IAuthService`; no direct EF access from controllers
- `DbInitializer` seeds dev users when database is empty

## Out of scope

- JWT / API sign-in (US-002 catch-up)
- Forgot password
- Full Book Desk / Admin bookings UI (stub pages only)
