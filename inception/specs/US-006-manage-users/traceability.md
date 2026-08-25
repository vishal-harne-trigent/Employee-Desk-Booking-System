# US-006 — traceability

|             |                                                  |
| ----------- | ------------------------------------------------ |
| **Story**   | `inception/stories/user-stories/US-006-manage-users.md` |
| **Updated** | 2026-08-22                                       |

## Requirement to code

| Req     | File | Symbol / location | Proven by | Status      |
| ------- | ---- | ----------------- | --------- | ----------- |
| REQ-018 | `src/EmployeeDeskBooking.Application/Users/UserAdminService.cs` | `CreateUserAsync` | `tests/EmployeeDeskBooking.Tests/AdminUsersTests.cs` | implemented |
| REQ-019 | `src/EmployeeDeskBooking.Application/Users/UserAdminService.cs` | `UpdateUserAsync` | `tests/EmployeeDeskBooking.Tests/AdminUsersTests.cs` | implemented |
| REQ-020 | `src/EmployeeDeskBooking.Application/Users/UserAdminService.cs` | `DeactivateUserAsync` | `tests/EmployeeDeskBooking.Tests/AdminUsersTests.cs` | implemented |
| REQ-021 | `src/EmployeeDeskBooking.Application/Users/UserAdminService.cs` | `ResetPasswordAsync` | `tests/EmployeeDeskBooking.Tests/AdminUsersTests.cs` | implemented |
| REQ-022 | `src/EmployeeDeskBooking.Application/Users/UserAdminService.cs` | `UpdateUserAsync` (role) | `tests/EmployeeDeskBooking.Tests/AdminUsersTests.cs` | implemented |
| REQ-005 | `src/EmployeeDeskBooking.Application/Auth/AuthService.cs` | deactivated check | `AdminUsersTests.cs` AC-04 | implemented |
| NFR-004 | `src/EmployeeDeskBooking.Web/Areas/Admin/Controllers/AdminUsersController.cs` | `[Authorize(Roles = Admin)]` | `AdminUsersTests.cs` V-07 | implemented |

## Key symbols

| Symbol | Location |
| ------ | -------- |
| `IUserAdminService` | `src/EmployeeDeskBooking.Application/Users/IUserAdminService.cs` |
| `AdminUsersController` (Web) | `src/EmployeeDeskBooking.Web/Areas/Admin/Controllers/AdminUsersController.cs` |
| `AdminUsersController` (Api) | `src/EmployeeDeskBooking.Api/Controllers/AdminUsersController.cs` |
