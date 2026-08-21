# US-004 — traceability

|             |                                                  |
| ----------- | ------------------------------------------------ |
| **Story**   | `inception/stories/user-stories/US-004-admin-bookings.md` |
| **Updated** | 2026-08-21                                       |

## Requirement to code

| Req     | File | Symbol / location | Proven by | Status      |
| ------- | ---- | ----------------- | --------- | ----------- |
| REQ-011 | `src/EmployeeDeskBooking.Application/Bookings/BookingService.cs` | `GetAllBookingsAsync` | `tests/EmployeeDeskBooking.Tests/AdminBookingsTests.cs` | implemented |
| REQ-012 | `src/EmployeeDeskBooking.Application/Bookings/BookingService.cs` | `GetAllBookingsAsync` filters | `tests/EmployeeDeskBooking.Tests/AdminBookingsTests.cs` | implemented |
| REQ-013 | `src/EmployeeDeskBooking.Application/Bookings/BookingService.cs` | `GetAllBookingsAsync` filters | `tests/EmployeeDeskBooking.Tests/AdminBookingsTests.cs` | implemented |
| REQ-014 | `src/EmployeeDeskBooking.Application/Bookings/BookingService.cs` | `AdminCancelBookingAsync` | `tests/EmployeeDeskBooking.Tests/AdminBookingsTests.cs` | implemented |
| NFR-004 | `src/EmployeeDeskBooking.Web/Areas/Admin/Controllers/AdminBookingsController.cs` | `[Authorize(Roles = Admin)]` | `AdminBookingsTests.cs` V-07 | implemented |

## Key symbols

| Symbol | Location |
| ------ | -------- |
| `GetAllBookingsAsync` | `src/EmployeeDeskBooking.Application/Bookings/IBookingService.cs` |
| `AdminCancelBookingAsync` | `src/EmployeeDeskBooking.Application/Bookings/IBookingService.cs` |
| `AdminBookingsController` (Web) | `src/EmployeeDeskBooking.Web/Areas/Admin/Controllers/AdminBookingsController.cs` |
| `AdminBookingsController` (Api) | `src/EmployeeDeskBooking.Api/Controllers/AdminBookingsController.cs` |
