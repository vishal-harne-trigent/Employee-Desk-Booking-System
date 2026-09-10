# US-006 — traceability

|             |                                                                  |
| ----------- | ---------------------------------------------------------------- |
| **Story**   | `inception/stories/user-stories/US-006-manage-users.md`          |
| **Updated** | 2026-08-31                                                       |

## Requirement to code

| Req    | File | Symbol / location | Proven by | Status |
| ------ | ---- | ----------------- | --------- | ------ |
| FR-01  | `src/EmployeeDeskBooking.Application/Users/UserAdminService.cs` | `CreateUserAsync` | `tests/EmployeeDeskBooking.Tests/AdminUsersTests.cs` | implemented |
| FR-02  | `src/EmployeeDeskBooking.Application/Users/UserAdminService.cs` | duplicate email guard | `tests/EmployeeDeskBooking.Tests/AdminUsersTests.cs` | implemented |
| FR-03  | `src/EmployeeDeskBooking.Application/Users/UserAdminService.cs` | `UpdateUserAsync` | `tests/EmployeeDeskBooking.Tests/AdminUsersTests.cs` | implemented |
| FR-04  | `src/EmployeeDeskBooking.Application/Auth/AuthService.cs` | deactivated check | `tests/EmployeeDeskBooking.Tests/AdminUsersTests.cs` | implemented |
| FR-05  | `src/EmployeeDeskBooking.Application/Users/UserAdminService.cs` | `ResetPasswordAsync` | `tests/EmployeeDeskBooking.Tests/AdminUsersTests.cs` | implemented |
| FR-06  | `src/EmployeeDeskBooking.Application/Users/UserAdminService.cs` | role update | `tests/EmployeeDeskBooking.Tests/AdminUsersTests.cs` | implemented |
| FR-07  | `src/EmployeeDeskBooking.Application/Users/UserAdminService.cs` | last-admin guard | `tests/EmployeeDeskBooking.Tests/AdminUsersTests.cs` | implemented |
| NFR-01 | `src/EmployeeDeskBooking.Web/Areas/Admin/Controllers/AdminUsersController.cs` | `[Authorize(Roles = Admin)]` | `tests/EmployeeDeskBooking.Tests/AdminUsersTests.cs` | implemented |

## Key symbols

| Symbol | Location |
| ------ | -------- |
| `IUserAdminService` | `src/EmployeeDeskBooking.Application/Users/IUserAdminService.cs` |
| `AdminUsersController` (Web) | `src/EmployeeDeskBooking.Web/Areas/Admin/Controllers/AdminUsersController.cs` |
| `AdminUsersController` (Api) | `src/EmployeeDeskBooking.Api/Controllers/AdminUsersController.cs` |
