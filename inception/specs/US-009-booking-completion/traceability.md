# US-009 — traceability

|             |                                                                  |
| ----------- | ---------------------------------------------------------------- |
| **Story**   | `inception/stories/user-stories/US-009-booking-completion.md` |
| **Updated** | 2026-08-22                                                       |

## Requirement to code

| Req     | File | Symbol / location | Proven by | Status   |
| ------- | ---- | ----------------- | --------- | -------- |
| REQ-009 | `src/EmployeeDeskBooking.Application/Bookings/BookingCompletionService.cs` | status transition | `tests/EmployeeDeskBooking.Tests/BookingCompletionTests.cs` | implemented |
| REQ-011 | `src/EmployeeDeskBooking.Application/Bookings/BookingCompletionService.cs` | status transition | `tests/EmployeeDeskBooking.Tests/BookingCompletionTests.cs` | implemented |
| REQ-013 | `src/EmployeeDeskBooking.Application/Bookings/BookingCompletionService.cs` | Completed filter support | `tests/EmployeeDeskBooking.Tests/BookingCompletionTests.cs` | implemented |

## Key symbols

| Symbol | Location |
| ------ | -------- |
| `IBookingCompletionService` | `src/EmployeeDeskBooking.Application/Bookings/IBookingCompletionService.cs` |
| `CompletePastBookingsHostedService` | `src/EmployeeDeskBooking.Infrastructure/Bookings/CompletePastBookingsHostedService.cs` |
