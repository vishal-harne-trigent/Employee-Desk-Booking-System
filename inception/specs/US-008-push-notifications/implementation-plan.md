# US-008 — implementation plan

> **The Gate D1 artifact.** The human reads this file and `impact-analysis.md`, then approves in chat. DEV stamps the approval below; the developer commits the stamp. No code is written before that stamp exists.

|           |                                                                  |
| --------- | ---------------------------------------------------------------- |
| **Story** | `inception/stories/user-stories/US-008-push-notifications.md` |
| **Spec**  | `spec.md`                                                        |
| **Tier**  | Complex                                                          |

## Approval — Gate D1

| Field                | Value           |
| -------------------- | --------------- |
| Status               | approved |
| Approved by          | Vishal Harne \<vharne@degreed.com\> |
| Approved on          | 2026-08-22 |
| Plan commit approved | 1298aa4565344a35d421ac1a1898d5e734b0b79a |

## Steps

Ordered. Test-first per AC: failing test named `... (US-008/AC-##)` before the code that turns it green.

### Step 1 — Domain + persistence

| Field    | Value |
| -------- | ----- |
| Advances | REQ-026 |
| Files    | `Domain/Notifications/NotificationPreference.cs`; EF config; migration `AddNotificationPreferences`; `AppDbContext` |
| Verify   | `dotnet build` — table matches `db-design.md` (`UserId` PK, `PushOptIn` default 0, `PushSubscription` nullable JSON) |

### Step 2 — Application push abstractions

| Field    | Value |
| -------- | ----- |
| Advances | REQ-026, REQ-027, V-14 |
| Files    | `IPushNotificationSender`, `PushNotificationMessage`; `INotificationPreferenceRepository`, `INotificationPreferenceService`; `IBookingPushService`, `BookingPushService` |
| Verify   | `dotnet build` — push service skips send when `PushOptIn` false or subscription null |

### Step 3 — Infrastructure (WebPush + repository)

| Field    | Value |
| -------- | ----- |
| Advances | AC-03 |
| Files    | `WebPushNotificationSender`, `VapidOptions`, `EfNotificationPreferenceRepository`; DI registration; `WebPush` package |
| Verify   | `dotnet build`; `Push:VapidPublicKey`, `Push:VapidPrivateKey`, `Push:Subject` in `appsettings.json` (dev placeholders) |

Tests use `InMemoryPushNotificationSender`.

### Step 4 — Wire booking lifecycle

| Field    | Value |
| -------- | ----- |
| Advances | AC-01, AC-03, AC-04 |
| Files    | Extend `BookingService` — after email, call `IBookingPushService` confirmation/cancellation |
| Verify   | US-007 email tests still pass; push only when opted in |

Admin cancel sends push to the booking employee, not the admin.

### Step 5 — API notification endpoints

| Field    | Value |
| -------- | ----- |
| Advances | AC-02, AC-04 |
| Files    | `Api/Controllers/NotificationsController.cs`; DTOs for preferences + subscription |
| Verify   | `GET` returns opt-in state; `PATCH` opt-out clears subscription; `POST` saves subscription and sets opt-in |

JWT Employee auth per `app-architecture.md`.

### Step 6 — MVC notification settings (SCR-007)

| Field    | Value |
| -------- | ----- |
| Advances | AC-02, NFR-006 |
| Files    | `NotificationSettingsController`, view + view model (ST-01..ST-05); `wwwroot/js/push-settings.js`; link from `MyBookings/Index.cshtml` |
| Verify   | Page shows email info (read-only), push enable/disable; unsupported-browser copy when JS reports no Push API |

Integration tests POST subscription JSON via API (simulating browser grant).

### Step 7 — Integration tests & traceability

| Field    | Value |
| -------- | ----- |
| Advances | AC-01..AC-05 |
| Files    | `PushNotificationTests.cs`, `NotificationSettingsTests.cs`, `ApiNotificationTests.cs`, `.ac.test.js`; extend `ReminderEmailTests` for AC-05 no-push; `manifest.json`, `traceability.md` |
| Verify   | `dotnet test` — all tests pass including `... (US-008/AC-##)` |

### Step 8 — CI check

| Field    | Value |
| -------- | ----- |
| Advances | Gate D2 readiness |
| Files    | — |
| Verify   | `node tools/aidlc-check.mjs --write` — OK |

## Rollback

Revert PR. Run `dotnet ef database update` to prior migration if schema was applied.

## Open questions

| Question | Owner | Blocks |
| -------- | ----- | ------ |
| Production VAPID keys | DevOps / IT | No — dev/test placeholder keys OK |
| SCR-007 entry in top nav vs My Bookings only | PO/client | No — default link on My Bookings per screen spec |
