# US-007 — traceability

|             |                                                  |
| ----------- | ------------------------------------------------ |
| **Story**   | `inception/stories/user-stories/US-007-booking-emails.md` |
| **Updated** | 2026-08-22                                       |

## Requirement to code

| Req     | File | Symbol / location | Proven by | Status   |
| ------- | ---- | ----------------- | --------- | -------- |
| REQ-023 | `src/EmployeeDeskBooking.Application/Notifications/BookingEmailService.cs` | confirmation send | `tests/EmployeeDeskBooking.Tests/BookingEmailTests.cs` | implemented |
| REQ-024 | `src/EmployeeDeskBooking.Application/Notifications/BookingEmailService.cs` | cancellation send | `tests/EmployeeDeskBooking.Tests/BookingEmailTests.cs` | implemented |
| REQ-025 | `src/EmployeeDeskBooking.Application/Notifications/ReminderEmailService.cs` | day-before reminder | `tests/EmployeeDeskBooking.Tests/ReminderEmailTests.cs` | implemented |
| NFR-005 | `src/EmployeeDeskBooking.Infrastructure/Notifications/EfNotificationRepositories.cs` | failure logging | `tests/EmployeeDeskBooking.Tests/BookingEmailTests.cs` | implemented |

## Key symbols

| Symbol | Location |
| ------ | -------- |
| `IBookingEmailService` | `src/EmployeeDeskBooking.Application/Notifications/IBookingEmailService.cs` |
| `IReminderEmailService` | `src/EmployeeDeskBooking.Application/Notifications/IBookingEmailService.cs` |
| `IEmailSender` | `src/EmployeeDeskBooking.Application/Notifications/IEmailSender.cs` |
| `ReminderEmailHostedService` | `src/EmployeeDeskBooking.Infrastructure/Notifications/ReminderEmailHostedService.cs` |
