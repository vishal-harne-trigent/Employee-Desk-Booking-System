# US-008 — impact analysis

|             |                                                                  |
| ----------- | ---------------------------------------------------------------- |
| **Story**   | `inception/stories/user-stories/US-008-push-notifications.md` |
| **Tier**    | Complex                                                          |
| **Updated** | 2026-08-22                                                       |

## Surfaces crossed

| Surface                  | Crossed? | What exactly |
| ------------------------ | -------- | ------------ |
| Contract                 | yes      | New API routes `GET/PATCH /api/notifications/preferences`, `POST /api/notifications/push-subscription`; new MVC `/Settings/Notifications` |
| Persistence              | yes      | New `NotificationPreferences` table (`PushOptIn`, `PushSubscription` JSON) + EF migration |
| Trust                    | yes      | JWT Employee + cookie auth Employee; subscription JSON stored per user; VAPID keys in config |
| Dependency & integration | yes      | `WebPush` NuGet (VAPID); browser Push API from Razor page JavaScript |
| Operational              | no       | No new hosted job; push is synchronous side effect of book/cancel |

## Files and callers

| File | Symbol | Change | Callers found |
| ---- | ------ | ------ | ------------- |
| `BookingService.cs` | `CreateBookingAsync`, `CancelConfirmedBookingAsync` | invoke push after email | `BookController`, `BookingsController`, `MyBookingsController`, admin controllers, tests |
| `ReminderEmailService.cs` | `ProcessDueRemindersAsync` | **unchanged** — email only | `ReminderEmailHostedService`, tests |
| `Program.cs` (Web) | routes | notification settings page | app startup |
| `DependencyInjection.cs` | push sender + preference repo | WebPush + repositories | Web, Api, tests |
| `MyBookings/Index.cshtml` | — | link to notification settings | Employee nav |

## Regression risk

| Area | Risk | Why | Covered by |
| ---- | ---- | --- | ---------- |
| Book / cancel | medium | New post-save push call | Errors caught; booking commits (same pattern as US-007 email) |
| Email flow | low | Push is additive | US-007 tests still pass |
| Reminder job | low | Must not call push | AC-05 test on `ReminderEmailTests` |
| Existing tests | low | DI registration | Full `dotnet test` (79 tests baseline) |

## Deliberately not touched

- Email templates and `EmailDeliveryLogs` behaviour (US-007)
- `CompletePastBookingsHostedService` (US-009)
- Admin-only screens beyond shared settings access note in SCR-007
