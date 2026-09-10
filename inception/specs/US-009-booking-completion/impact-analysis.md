# US-009 — impact analysis

|             |                                                                  |
| ----------- | ---------------------------------------------------------------- |
| **Story**   | `inception/stories/user-stories/US-009-booking-completion.md` |
| **Tier**    | Medium                                                           |
| **Updated** | 2026-08-22                                                       |

## Surfaces crossed

| Surface                  | Crossed? | What exactly |
| ------------------------ | -------- | ------------ |
| Contract                 | no       | No new public API or UI routes |
| Persistence              | no       | Updates existing `Bookings` rows (`Status`, `CompletedAt`); no migration |
| Trust                    | no       | Internal system job only |
| Dependency & integration | no       | No new packages |
| Operational              | yes      | `CompletePastBookingsHostedService` daily job on Web |

## Files and callers

| File | Symbol | Change | Callers found |
| ---- | ------ | ------ | ------------- |
| `BookingCompletionService.cs` | `CompletePastBookingsAsync` | new batch transition | hosted service, tests |
| `EfBookingRepository.cs` | query past Confirmed | load candidates | completion service |
| `DependencyInjection.cs` | hosted service registration | Web startup | `Program.cs` |
| `BookingService.cs` | — | **unchanged** | existing controllers |

## Regression risk

| Area | Risk | Why | Covered by |
| ---- | ---- | --- | ---------- |
| Cancel flow | low | Job only targets Confirmed | AC-02 test |
| Today's bookings | medium | Wrong date comparison could complete early | AC-03 test |
| US-003 / US-004 display | low | Status enum already includes Completed | AC-01 + existing list tests |

## Deliberately not touched

- Email or push on completion
- Reminder job (US-007)
- Booking creation or cancellation logic
