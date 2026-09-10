# US-005 — traceability

|             |                                                                  |
| ----------- | ---------------------------------------------------------------- |
| **Story**   | `inception/stories/user-stories/US-005-manage-desks.md`          |
| **Updated** | 2026-08-31                                                       |

## Requirement to code

| Req    | File | Symbol / location | Proven by | Status |
| ------ | ---- | ----------------- | --------- | ------ |
| FR-01  | `src/EmployeeDeskBooking.Application/Desks/DeskService.cs` | `CreateDeskAsync` | `tests/EmployeeDeskBooking.Tests/AdminDesksTests.cs` | implemented |
| FR-02  | `src/EmployeeDeskBooking.Application/Desks/DeskService.cs` | duplicate number guard | `tests/EmployeeDeskBooking.Tests/AdminDesksTests.cs` | implemented |
| FR-03  | `src/EmployeeDeskBooking.Application/Desks/DeskService.cs` | `UpdateDeskNumberAsync` | `tests/EmployeeDeskBooking.Tests/AdminDesksTests.cs` | implemented |
| FR-04  | `src/EmployeeDeskBooking.Application/Desks/DeskService.cs` | `DeactivateDeskAsync`, `ActivateDeskAsync` | `tests/EmployeeDeskBooking.Tests/AdminDesksTests.cs` | implemented |
| FR-05  | `src/EmployeeDeskBooking.Application/Desks/DeskService.cs` | blocking-bookings check | `tests/EmployeeDeskBooking.Tests/AdminDesksTests.cs` | implemented |
| NFR-01 | `src/EmployeeDeskBooking.Web/Areas/Admin/Controllers/AdminDesksController.cs` | `[Authorize(Roles = Admin)]` | `tests/EmployeeDeskBooking.Tests/AdminDesksTests.cs` | implemented |

## Key symbols

| Symbol | Location |
| ------ | -------- |
| `IDeskService` | `src/EmployeeDeskBooking.Application/Desks/IDeskService.cs` |
| `AdminDesksController` (Web) | `src/EmployeeDeskBooking.Web/Areas/Admin/Controllers/AdminDesksController.cs` |
| `AdminDesksController` (Api) | `src/EmployeeDeskBooking.Api/Controllers/AdminDesksController.cs` |
