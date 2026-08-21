# US-008 — traceability

|             |                                                                  |
| ----------- | ---------------------------------------------------------------- |
| **Story**   | `inception/stories/user-stories/US-008-push-notifications.md` |
| **Updated** | 2026-08-22                                                       |

## Requirement to code

| Req     | File | Symbol / location | Proven by | Status   |
| ------- | ---- | ----------------- | --------- | -------- |
| REQ-026 | `src/EmployeeDeskBooking.Application/Notifications/NotificationPreferenceService.cs` | opt in/out | `tests/EmployeeDeskBooking.Tests/NotificationSettingsTests.cs` | implemented |
| REQ-027 | `src/EmployeeDeskBooking.Application/Notifications/BookingPushService.cs` | push on book/cancel | `tests/EmployeeDeskBooking.Tests/PushNotificationTests.cs` | implemented |
| NFR-004 | `src/EmployeeDeskBooking.Web/Controllers/NotificationSettingsController.cs` | Employee-only settings | `tests/EmployeeDeskBooking.Tests/NotificationSettingsTests.cs` | implemented |
| NFR-006 | `src/EmployeeDeskBooking.Web/Views/NotificationSettings/Index.cshtml` | unsupported browser copy | `tests/EmployeeDeskBooking.Tests/NotificationSettingsTests.cs` | implemented |

## Key symbols

| Symbol | Location |
| ------ | -------- |
| `IBookingPushService` | `src/EmployeeDeskBooking.Application/Notifications/PushNotificationContracts.cs` |
| `INotificationPreferenceService` | `src/EmployeeDeskBooking.Application/Notifications/PushNotificationContracts.cs` |
| `IPushNotificationSender` | `src/EmployeeDeskBooking.Application/Notifications/PushNotificationContracts.cs` |
| `WebPushNotificationSender` | `src/EmployeeDeskBooking.Infrastructure/Notifications/WebPushNotificationSender.cs` |
| `NotificationsController` | `src/EmployeeDeskBooking.Api/Controllers/NotificationsController.cs` |
| `NotificationSettingsController` | `src/EmployeeDeskBooking.Web/Controllers/NotificationSettingsController.cs` |
