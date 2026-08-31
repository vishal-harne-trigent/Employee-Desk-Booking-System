# US-003 — traceability

|             |                                                                  |
| ----------- | ---------------------------------------------------------------- |
| **Story**   | `inception/stories/user-stories/US-003-my-bookings.md`           |
| **Updated** | 2026-08-31                                                       |

## Requirement to code

| Req    | File | Symbol / location | Proven by | Status |
| ------ | ---- | ----------------- | --------- | ------ |
| FR-01  | `src/EmployeeDeskBooking.Application/Bookings/BookingService.cs` | `GetMyBookingsAsync` | `tests/EmployeeDeskBooking.Tests/MyBookingsTests.cs` | implemented |
| FR-02  | `src/EmployeeDeskBooking.Application/Bookings/BookingService.cs` | `CancelBookingAsync` | `tests/EmployeeDeskBooking.Tests/MyBookingsTests.cs` | implemented |
| FR-03  | `src/EmployeeDeskBooking.Web/Views/MyBookings/Index.cshtml` | cancel action visibility | `tests/EmployeeDeskBooking.Tests/MyBookingsTests.cs` | implemented |
| FR-04  | `src/EmployeeDeskBooking.Web/Views/MyBookings/Index.cshtml` | empty state | `tests/EmployeeDeskBooking.Tests/MyBookingsTests.cs` | implemented |
| NFR-01 | `src/EmployeeDeskBooking.Web/Controllers/MyBookingsController.cs` | `[Authorize(Roles = Employee)]` | `tests/EmployeeDeskBooking.Tests/MyBookingsTests.cs` | implemented |

## Key symbols

| Symbol | Location |
| ------ | -------- |
| `GetMyBookingsAsync` | `src/EmployeeDeskBooking.Application/Bookings/IBookingService.cs` |
| `CancelBookingAsync` | `src/EmployeeDeskBooking.Application/Bookings/IBookingService.cs` |
| `MyBookingsController` | `src/EmployeeDeskBooking.Web/Controllers/MyBookingsController.cs` |
