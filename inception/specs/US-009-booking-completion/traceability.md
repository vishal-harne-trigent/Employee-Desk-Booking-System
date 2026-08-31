# US-009 — traceability

|             |                                                                  |
| ----------- | ---------------------------------------------------------------- |
| **Story**   | `inception/stories/user-stories/US-009-booking-completion.md` |
| **Updated** | 2026-08-31                                                       |

## Requirement to code

| Req    | File | Symbol / location | Proven by | Status |
| ------ | ---- | ----------------- | --------- | ------ |
| FR-01  | `src/EmployeeDeskBooking.Application/Bookings/BookingCompletionService.cs` | `CompletePastBookingsAsync` | `tests/EmployeeDeskBooking.Tests/BookingCompletionTests.cs` | implemented |
| FR-02  | `src/EmployeeDeskBooking.Application/Bookings/BookingCompletionService.cs` | skips Cancelled | `tests/EmployeeDeskBooking.Tests/BookingCompletionTests.cs` | implemented |
| FR-03  | `src/EmployeeDeskBooking.Application/Bookings/BookingCompletionService.cs` | `BookingDate < officeToday` guard | `tests/EmployeeDeskBooking.Tests/BookingCompletionTests.cs` | implemented |
| NFR-01 | `src/EmployeeDeskBooking.Infrastructure/Time/OfficeClock.cs` | office timezone | `tests/EmployeeDeskBooking.Tests/BookingCompletionTests.cs` | implemented |

## Key symbols

| Symbol | Location |
| ------ | -------- |
| `IBookingCompletionService` | `src/EmployeeDeskBooking.Application/Bookings/IBookingCompletionService.cs` |
| `CompletePastBookingsHostedService` | `src/EmployeeDeskBooking.Infrastructure/Bookings/CompletePastBookingsHostedService.cs` |
