# US-002 — traceability

|             |                                                                  |
| ----------- | ---------------------------------------------------------------- |
| **Story**   | `inception/stories/user-stories/US-002-book-desk.md`             |
| **Updated** | 2026-08-31                                                       |

## Requirement to code

| Req    | File | Symbol / location | Proven by | Status |
| ------ | ---- | ----------------- | --------- | ------ |
| FR-01  | `src/EmployeeDeskBooking.Application/Bookings/BookingService.cs` | date validation + `GetAvailabilityAsync` | `tests/EmployeeDeskBooking.Tests/BookDeskTests.cs` | implemented |
| FR-02  | `src/EmployeeDeskBooking.Application/Bookings/BookingService.cs` | `ValidateBookingDate` | `tests/EmployeeDeskBooking.Tests/BookDeskTests.cs`, `ApiBookingTests.cs` | implemented |
| FR-03  | `src/EmployeeDeskBooking.Application/Bookings/BookingService.cs` | `GetAvailabilityAsync` | `tests/EmployeeDeskBooking.Tests/BookDeskTests.cs` | implemented |
| FR-04  | `src/EmployeeDeskBooking.Application/Bookings/BookingService.cs` | `CreateBookingAsync` | `tests/EmployeeDeskBooking.Tests/BookDeskTests.cs` | implemented |
| FR-05  | `src/EmployeeDeskBooking.Application/Bookings/BookingService.cs` | one-per-day guard | `tests/EmployeeDeskBooking.Tests/BookDeskTests.cs` | implemented |
| FR-06  | `src/EmployeeDeskBooking.Application/Bookings/BookingService.cs` | inactive/taken guard | `tests/EmployeeDeskBooking.Tests/BookDeskTests.cs` | implemented |
| NFR-01 | `src/EmployeeDeskBooking.Infrastructure/Time/OfficeClock.cs` | office timezone | `tests/EmployeeDeskBooking.Tests/BookDeskTests.cs` | implemented |
| NFR-02 | `src/EmployeeDeskBooking.Application/Bookings/BookingService.cs` | availability query | `tests/EmployeeDeskBooking.Tests/BookDeskTests.cs` | implemented |
| NFR-03 | `src/EmployeeDeskBooking.Web/Controllers/BookController.cs` | `[Authorize(Roles = Employee)]` | `tests/EmployeeDeskBooking.Tests/SignInTests.cs` | implemented |

## Key symbols

| Symbol | Location |
| ------ | -------- |
| `IBookingService` | `src/EmployeeDeskBooking.Application/Bookings/IBookingService.cs` |
| `BookingService` | `src/EmployeeDeskBooking.Application/Bookings/BookingService.cs` |
| `IOfficeClock` | `src/EmployeeDeskBooking.Application/Time/IOfficeClock.cs` |
| `BookingsController` | `src/EmployeeDeskBooking.Api/Controllers/BookingsController.cs` |
| `BookController` | `src/EmployeeDeskBooking.Web/Controllers/BookController.cs` |
