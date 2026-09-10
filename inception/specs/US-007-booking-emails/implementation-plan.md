# US-007 — implementation plan

> **The Gate D1 artifact.** The human reads this file and `impact-analysis.md`, then approves in chat. DEV stamps the approval below; the developer commits the stamp. No code is written before that stamp exists.

|           |                                                                  |
| --------- | ---------------------------------------------------------------- |
| **Story** | `inception/stories/user-stories/US-007-booking-emails.md`     |
| **Spec**  | `spec.md`                                                        |
| **Tier**  | Complex                                                          |

## Approval — Gate D1

| Field                | Value           |
| -------------------- | --------------- |
| Status               | approved |
| Approved by          | Vishal Harne \<vharne@degreed.com\> |
| Approved on          | 2026-08-22 |
| Plan commit approved | 356265a617d034c0dd3187d9f88bd6280b1a42cd |

## Steps

Ordered. Test-first per AC: failing test named `... (US-007/AC-##)` before the code that turns it green.

### Step 1 — Domain + persistence

| Field    | Value |
| -------- | ----- |
| Advances | REQ-025, NFR-005 |
| Files    | `Domain/Notifications/EmailType.cs`, `EmailDeliveryStatus.cs`; entities `EmailDeliveryLog`, `BookingReminder`; EF configs; migration `AddBookingEmailNotifications`; `AppDbContext` |
| Verify   | `dotnet build` — migration adds `EmailDeliveryLogs`, `BookingReminders` |

### Step 2 — Application email abstractions

| Field    | Value |
| -------- | ----- |
| Advances | AC-03, NFR-005 |
| Files    | `Application/Notifications/IEmailSender.cs`, `BookingEmailMessage.cs`, `IBookingEmailService.cs`, `BookingEmailService.cs`; `IEmailDeliveryLogRepository`, `IBookingReminderRepository` |
| Verify   | `dotnet build` — 0 errors |

HTML templates include desk number + booking date (V-13). Log every send attempt (Sent/Failed).

### Step 3 — Infrastructure (MailKit + repositories)

| Field    | Value |
| -------- | ----- |
| Advances | AC-01, AC-02, AC-05 |
| Files    | `Infrastructure/Notifications/MailKitEmailSender.cs`, `EmailOptions.cs`, `EfEmailDeliveryLogRepository.cs`, `EfBookingReminderRepository.cs`; extend `IBookingRepository` with booking+user+desk projection for emails; DI registration |
| Verify   | `dotnet build`; dev config section `Email:*` in `appsettings.json` |

Tests use `InMemoryEmailSender` (captures messages, can simulate failure).

### Step 4 — Wire booking lifecycle

| Field    | Value |
| -------- | ----- |
| Advances | AC-01, AC-02 |
| Files    | Extend `BookingService` — after successful create/cancel call `IBookingEmailService` |
| Verify   | Existing book/cancel tests still pass |

### Step 5 — Day-before reminder job

| Field    | Value |
| -------- | ----- |
| Advances | AC-04 |
| Files    | `Application/Notifications/IReminderEmailService.cs`, `ReminderEmailService.cs`; `Infrastructure/Notifications/ReminderEmailHostedService.cs`; register on Web `Program.cs` |
| Verify   | Job selects Confirmed bookings for tomorrow; skips if `BookingReminders` row exists; inserts row on success |

Configurable `Email:ReminderHourLocal` (default 8). Tests call `ProcessDueRemindersAsync` directly with `TestOfficeClock`.

### Step 6 — Integration tests & traceability

| Field    | Value |
| -------- | ----- |
| Advances | AC-01..AC-05 |
| Files    | `tests/BookingEmailTests.cs`, `ReminderEmailTests.cs`, `.ac.test.js`; `InMemoryEmailSender`; `manifest.json`, `traceability.md` |
| Verify   | `dotnet test` — all tests pass including `... (US-007/AC-##)` |

Extend `BookDeskTestClient` / booking tests for email assertions where appropriate.

### Step 7 — CI check

| Field    | Value |
| -------- | ----- |
| Advances | Gate D2 readiness |
| Files    | — |
| Verify   | `node tools/aidlc-check.mjs` — OK |

## Rollback

Revert PR. Run `dotnet ef database update` to prior migration if schema was applied.

## Open questions

| Question | Owner | Blocks |
| -------- | ----- | ------ |
| Production SMTP host/credentials | DevOps / IT (open Q#7) | No — disabled/fake sender OK in dev and tests |
| Exact reminder clock time | PO/client (TBD) | No — default 08:00 local via config |
