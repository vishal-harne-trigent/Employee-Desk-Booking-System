# US-008 — traceability

|             |                                                                  |
| ----------- | ---------------------------------------------------------------- |
| **Story**   | `inception/stories/user-stories/US-008-push-notifications.md`   |
| **Updated** | 2026-08-31                                                       |

## Requirement to code

| Req    | File | Symbol / location | Proven by | Status |
| ------ | ---- | ----------------- | --------- | ------ |
| FR-01  | `src/EmployeeDeskBooking.Application/Notifications/BookingPushService.cs` | opt-out default | `tests/EmployeeDeskBooking.Tests/PushNotificationTests.cs` | implemented |
| FR-02  | `src/EmployeeDeskBooking.Application/Notifications/NotificationPreferenceService.cs` | opt in + subscription | `tests/EmployeeDeskBooking.Tests/NotificationSettingsTests.cs` | implemented |
| FR-03  | `src/EmployeeDeskBooking.Application/Notifications/BookingPushService.cs` | push on book/cancel | `tests/EmployeeDeskBooking.Tests/PushNotificationTests.cs` | implemented |
| FR-04  | `src/EmployeeDeskBooking.Application/Notifications/NotificationPreferenceService.cs` | opt out | `tests/EmployeeDeskBooking.Tests/NotificationSettingsTests.cs` | implemented |
| FR-05  | `src/EmployeeDeskBooking.Application/Notifications/ReminderEmailService.cs` | no push on reminder | `tests/EmployeeDeskBooking.Tests/ReminderEmailTests.cs` | implemented |
| NFR-01 | `src/EmployeeDeskBooking.Web/Controllers/NotificationSettingsController.cs` | Employee-only settings | `tests/EmployeeDeskBooking.Tests/NotificationSettingsTests.cs` | implemented |
| NFR-02 | `src/EmployeeDeskBooking.Web/Views/NotificationSettings/Index.cshtml` | unsupported browser copy | `tests/EmployeeDeskBooking.Tests/NotificationSettingsTests.cs` | implemented |

## Key symbols

| Symbol | Location |
| ------ | -------- |
| `IBookingPushService` | `src/EmployeeDeskBooking.Application/Notifications/PushNotificationContracts.cs` |
| `INotificationPreferenceService` | `src/EmployeeDeskBooking.Application/Notifications/PushNotificationContracts.cs` |
| `IPushNotificationSender` | `src/EmployeeDeskBooking.Application/Notifications/PushNotificationContracts.cs` |
| `WebPushNotificationSender` | `src/EmployeeDeskBooking.Infrastructure/Notifications/WebPushNotificationSender.cs` |
| `NotificationsController` | `src/EmployeeDeskBooking.Api/Controllers/NotificationsController.cs` |
| `NotificationSettingsController` | `src/EmployeeDeskBooking.Web/Controllers/NotificationSettingsController.cs` |
