# US-003 — traceability

|             |                                                  |
| ----------- | ------------------------------------------------ |
| **Story**   | `inception/stories/user-stories/US-003-my-bookings.md` |
| **Updated** | 2026-08-21                                       |

## Requirement to code

| Req     | File | Symbol / location | Proven by | Status      |
| ------- | ---- | ----------------- | --------- | ----------- |
| REQ-009 | `src/EmployeeDeskBooking.Application/Bookings/BookingService.cs` | `GetMyBookingsAsync` | `tests/EmployeeDeskBooking.Tests/MyBookingsTests.cs` | implemented |
| REQ-010 | `src/EmployeeDeskBooking.Application/Bookings/BookingService.cs` | `CancelBookingAsync` | `tests/EmployeeDeskBooking.Tests/MyBookingsTests.cs` | implemented |
| NFR-004 | `src/EmployeeDeskBooking.Web/Controllers/MyBookingsController.cs` | `[Authorize(Roles = Employee)]` | Integration tests | implemented |

## Key symbols

| Symbol | Location |
| ------ | -------- |
| `GetMyBookingsAsync` | `src/EmployeeDeskBooking.Application/Bookings/IBookingService.cs` |
| `CancelBookingAsync` | `src/EmployeeDeskBooking.Application/Bookings/IBookingService.cs` |
| `MyBookingsController` | `src/EmployeeDeskBooking.Web/Controllers/MyBookingsController.cs` |
