# US-002 — impact analysis

> What this change touches, written **before** it touches anything. Read at Gate D1 next to the plan. Required at Complex tier.

|             |                                                                  |
| ----------- | ---------------------------------------------------------------- |
| **Story**   | `inception/stories/user-stories/US-002-book-desk.md`             |
| **Tier**    | Complex                                                          |
| **Updated** | 2026-08-31                                                       |

## Surfaces crossed

| Surface                  | Crossed? | What exactly                                    |
| ------------------------ | -------- | ----------------------------------------------- |
| Contract                 | yes      | New API `BookingsController` (availability + create); MVC `BookController` |
| Persistence              | yes      | New `Desks`, `Bookings` tables + EF migration |
| Trust                    | yes      | Employee-only book routes; JWT on Api |
| Dependency & integration | no       | Uses existing auth from US-001 |
| Operational              | no       | No background jobs |

## Files and callers

| File | Symbol | Change | Callers found |
| ---- | ------ | ------ | ------------- |
| `BookingService.cs` | `GetAvailabilityAsync`, `CreateBookingAsync` | core booking logic | Web + Api controllers, tests |
| `EfBookingRepository.cs` | queries + insert | persistence | `BookingService` |
| `OfficeClock.cs` | `Today`, working-day checks | date boundaries | `BookingService`, tests |
| `BookController.cs` | availability + confirm actions | MVC UI | SCR-002 views |

## Regression risk

| Area | Risk | Why | Covered by |
| ---- | ---- | --- | ---------- |
| Sign-in routing | low | Book is Employee home from US-001 | existing sign-in tests |
| Concurrent book | medium | Two users same desk/date (RISK-004) | integration tests |
| Date boundaries | medium | Weekend and +30 window edge cases | AC-02 tests |

## Deliberately not touched

- Cancellation flow (US-003)
- Email notifications (US-007)
- Admin desk CRUD (US-005)
