# US-004 — impact analysis

> What this change touches, written **before** it touches anything. Read at Gate D1 next to the plan. Required at Complex tier.

|             |                                                                  |
| ----------- | ---------------------------------------------------------------- |
| **Story**   | `inception/stories/user-stories/US-004-admin-bookings.md`        |
| **Tier**    | Complex                                                          |
| **Updated** | 2026-08-31                                                       |

## Surfaces crossed

| Surface                  | Crossed? | What exactly                                    |
| ------------------------ | -------- | ----------------------------------------------- |
| Contract                 | yes      | Admin Api list/filter/cancel endpoints; MVC Admin area views |
| Persistence              | no       | Reads and updates existing `Bookings` rows only |
| Trust                    | yes      | Admin-only area and Api routes (V-07) |
| Dependency & integration | no       | Extends `IBookingService` from US-002 |
| Operational              | no       | No background jobs |

## Files and callers

| File | Symbol | Change | Callers found |
| ---- | ------ | ------ | ------------- |
| `BookingService.cs` | `GetAllBookingsAsync`, `AdminCancelBookingAsync` | admin list + cancel | Admin Web/Api controllers, tests |
| `AdminBookingsController.cs` | list, filter, cancel actions | MVC admin UI | SCR-004 views |
| `EfBookingRepository.cs` | filtered queries | persistence | `BookingService` |

## Regression risk

| Area | Risk | Why | Covered by |
| ---- | ---- | --- | ---------- |
| Employee cancel | low | Separate code path | US-003 tests unchanged |
| Admin auth | high | Data leak if Employee reaches admin list | V-07 integration tests |
| Filter combinations | medium | Empty results vs errors | AC-02, AC-03 tests |

## Deliberately not touched

- Desk CRUD (US-005)
- User administration (US-006)
- Email on admin cancel (US-007 adds side effect later)
