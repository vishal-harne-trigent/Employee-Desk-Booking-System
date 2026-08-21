# US-001 — traceability

|             |                                                                  |
| ----------- | ---------------------------------------------------------------- |
| **Story**   | `inception/stories/user-stories/US-001-sign-in.md`               |
| **Updated** | 2026-08-21                                                       |

## Requirement to code

| Req    | File | Symbol / location | Proven by | Status |
| ------ | ---- | ----------------- | --------- | ------ |
| FR-01  | `src/EmployeeDeskBooking.Web/Controllers/AccountController.cs` | `RedirectToRoleHome` | `SignInTests.cs` | implemented |
| FR-02  | `src/EmployeeDeskBooking.Web/Controllers/AccountController.cs` | Admin branch | `SignInTests.cs` | implemented |
| FR-03  | `src/EmployeeDeskBooking.Application/Auth/AuthService.cs` | `SignInAsync` | `SignInTests.cs` | implemented |
| FR-04  | `src/EmployeeDeskBooking.Application/Auth/AuthService.cs` | `DeactivatedAccount` | `SignInTests.cs` | implemented |
| FR-05  | `src/EmployeeDeskBooking.Web/Controllers/AccountController.cs` | `Logout` | `SignInTests.cs` | implemented |
| NFR-01 | `src/EmployeeDeskBooking.Infrastructure/Security/AspNetPasswordVerifier.cs` | `HashPassword` | — | implemented |

## Key symbols

| Symbol | Location |
| ------ | -------- |
| `IAuthService` | `src/EmployeeDeskBooking.Application/Auth/IAuthService.cs` |
| `AuthService` | `src/EmployeeDeskBooking.Application/Auth/AuthService.cs` |
| `DbInitializer` | `src/EmployeeDeskBooking.Infrastructure/DbInitializer.cs` |
