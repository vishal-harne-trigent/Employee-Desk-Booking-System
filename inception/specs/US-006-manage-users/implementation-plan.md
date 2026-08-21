# US-006 — implementation plan

> **The Gate D1 artifact.** The human reads this file, then approves in chat. DEV stamps the approval below; the developer commits the stamp. No code is written before that stamp exists.

|           |                                                                  |
| --------- | ---------------------------------------------------------------- |
| **Story** | `inception/stories/user-stories/US-006-manage-users.md`         |
| **Spec**  | `spec.md`                                                        |
| **Tier**  | Medium                                                           |

## Approval — Gate D1

| Field                | Value           |
| -------------------- | --------------- |
| Status               | approved |
| Approved by          | Vishal Harne \<vharne@degreed.com\> |
| Approved on          | 2026-08-22 |
| Plan commit approved | 1d34f7a6afa1af066b7e67c85e63e3d8176932e1 |

## Steps

Ordered. Test-first per AC: failing test named `... (US-006/AC-##)` before the code that turns it green.

### Step 1 — Application: user admin service

| Field    | Value |
| -------- | ----- |
| Advances | REQ-018..REQ-022, REQ-005, BR-001.10, BR-001.11, BR-001.12 |
| Files    | `Application/Users/AdminUserModels.cs`, `IUserAdminService.cs`, `UserAdminService.cs`; extend `IUserRepository` + `EfUserRepository` (`GetAllUsersAsync`, `GetUserByIdAsync`, `EmailExistsAsync`, `CountActiveAdminsAsync`); register in DI |
| Verify   | `dotnet build EmployeeDeskBooking.sln` — 0 errors |

`CreateUserAsync`, `UpdateUserAsync` (name, email, role), `DeactivateUserAsync`, `ResetPasswordAsync` (returns generated plaintext once). Duplicate email → `DuplicateEmail`. Last active Admin guard on deactivate and Admin→Employee role change → `LastAdminProtected`. Reuse `IPasswordVerifier` + existing `IX_Users_EmailNormalized` unique index.

### Step 2 — API admin user endpoints

| Field    | Value |
| -------- | ----- |
| Advances | AC-01..AC-07 (API), NFR-004 |
| Files    | `Api/Controllers/AdminUsersController.cs`, `Api/Contracts/Admin/AdminUserContracts.cs`; `[Authorize(Roles = Admin)]` |
| Verify   | Swagger shows `GET/POST /api/admin/users`, `PUT /api/admin/users/{id}`, `POST .../deactivate`, `POST .../reset-password` |

Employee JWT → 403. Duplicate email → 409. Last-admin violation → 409.

### Step 3 — Web Manage Users UI (SCR-006)

| Field    | Value |
| -------- | ----- |
| Advances | AC-01..AC-07, SCR-006 ST-01, ST-03, ST-05..ST-09 |
| Files    | `Web/Areas/Admin/Controllers/AdminUsersController.cs`, `Models/AdminUsersViewModel.cs`, `Views/Admin/AdminUsers/Index.cshtml`, CSS |
| Verify   | Signed-in Admin at `/Admin/AdminUsers` — list, add, edit, deactivate, reset-password one-time panel |

New controller (nav already links here). Inline add/edit forms; reset-password result panel (ST-07); last-admin error banner (ST-09).

### Step 4 — Integration tests & traceability

| Field    | Value |
| -------- | ----- |
| Advances | AC-01..AC-07, V-07 |
| Files    | `tests/AdminUsersTests.cs`, `ApiAdminUsersTests.cs`, helpers + `.ac.test.js`; `manifest.json`, `traceability.md` |
| Verify   | `dotnet test` — all tests pass including `... (US-006/AC-##)` |

Two-Admin fixture for AC-07. AC-04 + AC-01 sign-in regression via existing login client. Employee blocked from admin user routes (V-07).

### Step 5 — CI check

| Field    | Value |
| -------- | ----- |
| Advances | Gate D2 readiness |
| Files    | — |
| Verify   | `node tools/aidlc-check.mjs` — OK |

## Rollback

Revert the story PR. No schema change — uses existing `Users` table and unique email index.

## Open questions

| Question | Owner | Blocks |
| -------- | ----- | ------ |
| V-12 password complexity beyond non-empty | PO/security | No — deferred; create accepts admin-provided password; reset generates secure random password |
