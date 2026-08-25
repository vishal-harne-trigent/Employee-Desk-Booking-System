# US-007 — impact analysis

|             |                                                  |
| ----------- | ------------------------------------------------ |
| **Story**   | `inception/stories/user-stories/US-007-booking-emails.md` |
| **Tier**    | Complex                                          |
| **Updated** | 2026-08-22                                       |

## Surfaces crossed

| Surface                  | Crossed? | What exactly                                    |
| ------------------------ | -------- | ----------------------------------------------- |
| Contract                 | no       | No new public API routes; emails are side effects of existing book/cancel |
| Persistence              | yes      | New `EmailDeliveryLogs`, `BookingReminders` tables + EF migration |
| Trust                    | yes      | Sends to user account email; logs must not contain passwords (NFR-005) |
| Dependency & integration | yes      | MailKit SMTP client (`MailKit` package)         |
| Operational              | yes      | `ReminderEmailHostedService` daily job (~08:00 office local, configurable) |

## Files and callers

| File | Symbol | Change | Callers found |
| ---- | ------ | ------ | ------------- |
| `BookingService.cs` | `CreateBookingAsync` | invoke confirmation email after save | `BookController`, `BookingsController`, tests |
| `BookingService.cs` | `CancelConfirmedBookingAsync` | invoke cancellation email after save | `MyBookingsController`, `AdminBookingsController`, Api controllers, tests |
| `Program.cs` (Web) | DI + hosted service | register email + reminder job | app startup |
| `DependencyInjection.cs` (Infrastructure) | email sender | MailKit + repositories | Web, Api, tests |

## Regression risk

| Area | Risk | Why | Covered by |
| ---- | ---- | --- | ---------- |
| Book / cancel | medium | New post-save email call could throw and break flow | Booking still commits; email errors caught and logged (decisions.md) |
| Reminder job | medium | Duplicate sends without idempotency | `BookingReminders` + AC-04 tests |
| Existing tests | low | DI registration changes | Full `dotnet test` suite (70 tests) |

## Deliberately not touched

- Push notification preferences (US-008 / `NotificationPreferences` table)
- `CompletePastBookingsHostedService` (US-009)
- Email content branding beyond desk + date (V-13 minimum)
