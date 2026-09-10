# US-006 — impact analysis

> What this change touches, written **before** it touches anything. Read at Gate D1 next to the plan. Required at Complex tier.

|             |                                                                  |
| ----------- | ---------------------------------------------------------------- |
| **Story**   | `inception/stories/user-stories/US-006-manage-users.md`          |
| **Tier**    | Complex                                                          |
| **Updated** | 2026-08-31                                                       |

## Surfaces crossed

| Surface                  | Crossed? | What exactly                                    |
| ------------------------ | -------- | ----------------------------------------------- |
| Contract                 | yes      | Admin Api user CRUD + password reset; MVC SCR-006 |
| Persistence              | yes      | `Users` table updates; unique email index |
| Trust                    | yes      | Admin-only routes; password hashing; last-admin guard (V-11) |
| Dependency & integration | no       | Extends US-001 auth model |
| Operational              | no       | No background jobs |

## Files and callers

| File | Symbol | Change | Callers found |
| ---- | ------ | ------ | ------------- |
| `UserAdminService.cs` | create/edit/deactivate/reset/role | user admin logic | Admin controllers, tests |
| `AuthService.cs` | deactivated check | blocks sign-in | `AccountController`, tests |
| `EfUserRepository.cs` | user persistence | CRUD | `UserAdminService`, `AuthService` |

## Regression risk

| Area | Risk | Why | Covered by |
| ---- | ---- | --- | ---------- |
| Sign-in | medium | Deactivate must block login (REQ-005) | AC-04 + US-001 tests |
| Last admin | high | Lockout if last Admin demoted | AC-07 / V-11 tests |
| Password reset | medium | One-time display; secure generation | AC-05 tests |

## Deliberately not touched

- Self-service password change
- User reactivation flow
- Booking or desk admin (US-004, US-005)
