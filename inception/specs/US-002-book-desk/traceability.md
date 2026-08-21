# US-002 — traceability

|             |                                                  |
| ----------- | ------------------------------------------------ |
| **Story**   | `inception/stories/user-stories/US-002-book-desk.md` |
| **Updated** | 2026-08-21                                       |

## Requirement to code

| Req     | File | Symbol / location | Proven by | Status      |
| ------- | ---- | ----------------- | --------- | ----------- |
| REQ-006 | `src/EmployeeDeskBooking.Application/Bookings/BookingService.cs` | Date validation | `tests/EmployeeDeskBooking.Tests/BookDeskTests.cs`, `ApiBookingTests.cs` | implemented |
| REQ-007 | `src/EmployeeDeskBooking.Application/Bookings/BookingService.cs` | `GetAvailabilityAsync` | `tests/EmployeeDeskBooking.Tests/BookDeskTests.cs` | implemented |
| REQ-008 | `src/EmployeeDeskBooking.Application/Bookings/BookingService.cs` | `CreateBookingAsync` | `tests/EmployeeDeskBooking.Tests/BookDeskTests.cs` | implemented |
| NFR-001 | `src/EmployeeDeskBooking.Infrastructure/Time/OfficeClock.cs` | Office timezone | `tests/EmployeeDeskBooking.Tests/BookDeskTests.cs` | implemented |
| NFR-002 | `src/EmployeeDeskBooking.Application/Bookings/BookingService.cs` | Availability query | Integration tests | implemented |
| NFR-004 | `src/EmployeeDeskBooking.Web/Controllers/BookController.cs` | `[Authorize(Roles = Employee)]` | `tests/EmployeeDeskBooking.Tests/SignInTests.cs` | implemented |

## Key symbols

| Symbol | Location |
| ------ | -------- |
| `IBookingService` | `src/EmployeeDeskBooking.Application/Bookings/IBookingService.cs` |
| `BookingService` | `src/EmployeeDeskBooking.Application/Bookings/BookingService.cs` |
| `IOfficeClock` | `src/EmployeeDeskBooking.Application/Time/IOfficeClock.cs` |
| `BookingsController` | `src/EmployeeDeskBooking.Api/Controllers/BookingsController.cs` |
| `BookController` | `src/EmployeeDeskBooking.Web/Controllers/BookController.cs` |
