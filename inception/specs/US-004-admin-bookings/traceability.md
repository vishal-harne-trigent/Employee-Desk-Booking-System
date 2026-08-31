# US-004 — traceability

|             |                                                                  |
| ----------- | ---------------------------------------------------------------- |
| **Story**   | `inception/stories/user-stories/US-004-admin-bookings.md`        |
| **Updated** | 2026-08-31                                                       |

## Requirement to code

| Req    | File | Symbol / location | Proven by | Status |
| ------ | ---- | ----------------- | --------- | ------ |
| FR-01  | `src/EmployeeDeskBooking.Application/Bookings/BookingService.cs` | `GetAllBookingsAsync` | `tests/EmployeeDeskBooking.Tests/AdminBookingsTests.cs` | implemented |
| FR-02  | `src/EmployeeDeskBooking.Application/Bookings/BookingService.cs` | date filter parameter | `tests/EmployeeDeskBooking.Tests/AdminBookingsTests.cs` | implemented |
| FR-03  | `src/EmployeeDeskBooking.Application/Bookings/BookingService.cs` | status filter parameter | `tests/EmployeeDeskBooking.Tests/AdminBookingsTests.cs` | implemented |
| FR-04  | `src/EmployeeDeskBooking.Application/Bookings/BookingService.cs` | `AdminCancelBookingAsync` | `tests/EmployeeDeskBooking.Tests/AdminBookingsTests.cs` | implemented |
| NFR-01 | `src/EmployeeDeskBooking.Web/Areas/Admin/Controllers/AdminBookingsController.cs` | `[Authorize(Roles = Admin)]` | `tests/EmployeeDeskBooking.Tests/AdminBookingsTests.cs` | implemented |

## Key symbols

| Symbol | Location |
| ------ | -------- |
| `GetAllBookingsAsync` | `src/EmployeeDeskBooking.Application/Bookings/IBookingService.cs` |
| `AdminCancelBookingAsync` | `src/EmployeeDeskBooking.Application/Bookings/IBookingService.cs` |
| `AdminBookingsController` (Web) | `src/EmployeeDeskBooking.Web/Areas/Admin/Controllers/AdminBookingsController.cs` |
| `AdminBookingsController` (Api) | `src/EmployeeDeskBooking.Api/Controllers/AdminBookingsController.cs` |
