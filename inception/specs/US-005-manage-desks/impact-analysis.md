# US-005 — impact analysis

> What this change touches, written **before** it touches anything. Read at Gate D1 next to the plan. Required at Complex tier.

|             |                                                                  |
| ----------- | ---------------------------------------------------------------- |
| **Story**   | `inception/stories/user-stories/US-005-manage-desks.md`          |
| **Tier**    | Complex                                                          |
| **Updated** | 2026-08-31                                                       |

## Surfaces crossed

| Surface                  | Crossed? | What exactly                                    |
| ------------------------ | -------- | ----------------------------------------------- |
| Contract                 | yes      | Admin Api desk CRUD; MVC SCR-005 manage-desks UI |
| Persistence              | yes      | `Desks` entity updates; unique index on desk number |
| Trust                    | yes      | Admin-only routes (V-07) |
| Dependency & integration | no       | Availability query in US-002 reads desk status |
| Operational              | no       | No background jobs |

## Files and callers

| File | Symbol | Change | Callers found |
| ---- | ------ | ------ | ------------- |
| `DeskService.cs` | CRUD + activate/deactivate | desk admin logic | Admin controllers, tests |
| `BookingService.cs` | `GetAvailabilityAsync` | excludes Inactive desks | `BookController`, tests |
| `EfDeskRepository.cs` | persistence | desk queries | `DeskService` |

## Regression risk

| Area | Risk | Why | Covered by |
| ---- | ---- | --- | ---------- |
| Book desk availability | medium | Deactivated desk must disappear from list | US-002 + AC-04 tests |
| Deactivate with bookings | medium | Must not orphan Confirmed bookings | AC-05 / V-09 tests |
| Duplicate numbers | low | Unique constraint | AC-02 tests |

## Deliberately not touched

- Booking cancellation on deactivate (block only)
- User management (US-006)
