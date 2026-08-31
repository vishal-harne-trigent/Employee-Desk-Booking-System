# US-006 — Manage users

> Technical expansion of US-006. Story defines business need; this defines code behaviour.

|                   |                                                                  |
| ----------------- | ---------------------------------------------------------------- |
| **Story**         | `inception/stories/user-stories/US-006-manage-users.md`          |
| **Traces to**     | REQ-018, REQ-019, REQ-020, REQ-021, REQ-022, REQ-005, NFR-004, V-10, V-11 |
| **Screen**        | SCR-006                                                          |
| **Covering ADRs** | none                                                             |
| **Tier**          | Complex                                                          |
| **Status**        | implemented                                                      |
| **Updated**       | 2026-08-31                                                       |

## Problem

Admins need to create users, edit name and email, deactivate accounts, reset passwords (shown once), and change roles. Deactivated users cannot sign in. The last active Admin cannot be deactivated or demoted.

## Functional requirements

| ID    | Requirement | Priority | Serves | Status |
| ----- | ----------- | -------- | ------ | ------ |
| FR-01 | Admin creates user with email, name, role, and password; user can sign in | Must | AC-01 | implemented |
| FR-02 | Duplicate email on create or edit returns validation error (V-10) | Must | AC-02 | implemented |
| FR-03 | Admin edits user name and email | Must | AC-03 | implemented |
| FR-04 | Deactivated user cannot sign in | Must | AC-04 | implemented |
| FR-05 | Password reset generates new password shown once to Admin | Must | AC-05 | implemented |
| FR-06 | Admin changes user role between Employee and Admin | Must | AC-06 | implemented |
| FR-07 | Last active Admin cannot be deactivated or demoted (V-11) | Must | AC-07 | implemented |

## Non-functional requirements

| ID     | Requirement | Serves |
| ------ | ----------- | ------ |
| NFR-01 | Manage users routes restricted to Admin role (V-07) | NFR-004 |

## Technical constraints

- `IUserAdminService` in Application; reuses `IAuthService` deactivated check from US-001
- Passwords hashed via same verifier as sign-in (NFR-003)

## Out of scope

- Password complexity rule V-12 (non-empty admin-set password on create)
- First Admin bootstrap / installer seed
- User reactivation
