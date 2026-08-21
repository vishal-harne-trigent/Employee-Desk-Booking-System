# US-005 — traceability

|             |                                                  |
| ----------- | ------------------------------------------------ |
| **Story**   | `inception/stories/user-stories/US-005-manage-desks.md` |
| **Updated** | 2026-08-22                                       |

## Requirement to code

| Req     | File | Symbol / location | Proven by | Status      |
| ------- | ---- | ----------------- | --------- | ----------- |
| REQ-015 | `src/EmployeeDeskBooking.Application/Desks/DeskService.cs` | `CreateDeskAsync` | `tests/EmployeeDeskBooking.Tests/AdminDesksTests.cs` | implemented |
| REQ-016 | `src/EmployeeDeskBooking.Application/Desks/DeskService.cs` | `UpdateDeskNumberAsync` | `tests/EmployeeDeskBooking.Tests/AdminDesksTests.cs` | implemented |
| REQ-017 | `src/EmployeeDeskBooking.Application/Desks/DeskService.cs` | `DeactivateDeskAsync`, `ActivateDeskAsync` | `tests/EmployeeDeskBooking.Tests/AdminDesksTests.cs` | implemented |
| NFR-004 | `src/EmployeeDeskBooking.Web/Areas/Admin/Controllers/AdminDesksController.cs` | `[Authorize(Roles = Admin)]` | `AdminDesksTests.cs` V-07 | implemented |

## Key symbols

| Symbol | Location |
| ------ | -------- |
| `IDeskService` | `src/EmployeeDeskBooking.Application/Desks/IDeskService.cs` |
| `AdminDesksController` (Web) | `src/EmployeeDeskBooking.Web/Areas/Admin/Controllers/AdminDesksController.cs` |
| `AdminDesksController` (Api) | `src/EmployeeDeskBooking.Api/Controllers/AdminDesksController.cs` |
