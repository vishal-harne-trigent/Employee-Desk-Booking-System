# US-001 — implementation plan

> **The Gate D1 artifact.** The human reads this file, then approves in chat. DEV stamps the approval below; the developer commits the stamp. No code is written before that stamp exists.

|           |                                                                  |
| --------- | ---------------------------------------------------------------- |
| **Story** | `inception/stories/user-stories/US-001-sign-in.md`               |
| **Spec**  | `spec.md`                                                        |
| **Tier**  | Complex                                                          |

## Approval — Gate D1

| Field                | Value           |
| -------------------- | --------------- |
| Status               | approved |
| Approved by          | Vishal Harne \<vharne@degreed.com\> |
| Approved on          | 2026-08-21 |
| Plan commit approved | 2926e6be5a64ffa8dcc955fea5bc1adbd4cdd86c |

## Steps

Ordered. Test-first per AC: failing test named `... (US-001/AC-##)` before the code that turns it green.

### Step 1 — Domain & persistence (Users)

| Field    | Value |
| -------- | ----- |
| Advances | REQ-002, REQ-004, REQ-005 |
| Files    | `src/EmployeeDeskBooking.Domain/Users/User.cs`, `UserRole.cs`; `Infrastructure/Data/UserConfiguration.cs`, `AppDbContext.cs`; migration `InitialUsers`; `DbInitializer.cs` |
| Verify   | `dotnet ef migrations add InitialUsers --project src/EmployeeDeskBooking.Infrastructure --startup-project src/EmployeeDeskBooking.Web` — succeeds |

Seed dev accounts via `DbInitializer` when no users exist (architecture Q#1): Employee `vishal_h@trigent.com`, Admin `admin@trigent.com`, password `Password1!` (V-12).

### Step 2 — Application auth layer

| Field    | Value |
| -------- | ----- |
| Advances | REQ-002, REQ-003, REQ-005, NFR-003 |
| Files    | `Application/Auth/*`, `Application/Users/IUserRepository.cs`, `Application/Security/IPasswordVerifier.cs`, `Infrastructure/Users/EfUserRepository.cs`, `Infrastructure/Security/AspNetPasswordVerifier.cs`, `DependencyInjection.cs` (both tiers) |
| Verify   | `dotnet build EmployeeDeskBooking.sln` — 0 errors |

### Step 3 — Web sign-in UI (SCR-001)

| Field    | Value |
| -------- | ----- |
| Advances | AC-01..AC-04, SCR-001 ST-01..ST-04 |
| Files    | `Web/Controllers/AccountController.cs`, `Models/LoginViewModel.cs`, `Views/Account/Login.cshtml`, `Views/Shared/_LoginLayout.cshtml`, `wwwroot/css/site.css` (login styling), cookie auth in `Program.cs` |
| Verify   | Manual: login page renders at `/Account/Login` |

Default route becomes sign-in (`{controller=Account}/{action=Login}`). Post-login routing: Employee → `/Book`, Admin → `/Admin/AdminBookings`.

### Step 4 — Post-login stub destinations & sign-out

| Field    | Value |
| -------- | ----- |
| Advances | AC-01, AC-02, AC-05 |
| Files    | `Web/Controllers/BookController.cs`, `Areas/Admin/Controllers/AdminBookingsController.cs` (minimal authorized stubs); logout via GET `/Account/Logout` |
| Verify   | Signed-in Employee reaches `/Book`; Admin reaches `/Admin/AdminBookings`; logout returns to login |

Stubs are placeholders until US-002/US-004 — one `[Authorize]` action + minimal view each.

### Step 5 — Integration tests & traceability

| Field    | Value |
| -------- | ----- |
| Advances | AC-01..AC-05 |
| Files    | `tests/EmployeeDeskBooking.Tests/SignInTests.cs`, `CustomWebApplicationFactory.cs`, `appsettings.Testing.json`; `knowledge/traceability/manifest.json`; `inception/specs/US-001-sign-in/traceability.md` |
| Verify   | `dotnet test EmployeeDeskBooking.sln` — all tests pass including `... (US-001/AC-##)` |

### Step 6 — CI check

| Field    | Value |
| -------- | ----- |
| Advances | Gate D2 readiness |
| Files    | — |
| Verify   | `node tools/aidlc-check.mjs` — OK |

## Rollback

Revert the story PR. Drop the `EmployeeDeskBooking` LocalDB database if migrations were applied locally.

## Open questions

| Question | Owner | Blocks |
| -------- | ----- | ------ |
| — | — | — |
