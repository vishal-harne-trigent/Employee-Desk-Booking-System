# US-005 — implementation plan

> **The Gate D1 artifact.** The human reads this file, then approves in chat. DEV stamps the approval below; the developer commits the stamp. No code is written before that stamp exists.

|           |                                                                  |
| --------- | ---------------------------------------------------------------- |
| **Story** | `inception/stories/user-stories/US-005-manage-desks.md`         |
| **Spec**  | `spec.md`                                                        |
| **Tier**  | Medium                                                           |

## Approval — Gate D1

| Field                | Value           |
| -------------------- | --------------- |
| Status               | approved |
| Approved by          | Vishal Harne \<vharne@degreed.com\> |
| Approved on          | 2026-08-22 |
| Plan commit approved | aa6427587961727f2d9ee2187f11a05c04fd25ae |

## Steps

Ordered. Test-first per AC: failing test named `... (US-005/AC-##)` before the code that turns it green.

### Step 1 — Application: desk inventory service

| Field    | Value |
| -------- | ----- |
| Advances | REQ-015, REQ-016, REQ-017, BR-001.7, BR-001.8, BR-001.9 |
| Files    | `Application/Desks/DeskModels.cs`, `IDeskService.cs`, `DeskService.cs`; `IDeskRepository.cs`, `Infrastructure/Desks/EfDeskRepository.cs`; extend `IBookingRepository` with `HasConfirmedBookingsForDeskOnOrAfterAsync`; register in DI |
| Verify   | `dotnet build EmployeeDeskBooking.sln` — 0 errors |

`GetAllDesksAsync()` — all desks ordered by number. `CreateDeskAsync(deskNumber)` — Active, normalized uppercase trim; duplicate → `DuplicateDeskNumber`. `UpdateDeskNumberAsync(id, deskNumber)` — unique check excluding self. `DeactivateDeskAsync(id)` — blocked if Confirmed bookings on `IOfficeClock.Today` or later; else Inactive. `ActivateDeskAsync(id)` — Active. Reuse existing `IX_Desks_DeskNumberNormalized` unique index for V-08.

### Step 2 — API admin desk endpoints

| Field    | Value |
| -------- | ----- |
| Advances | AC-01..AC-05 (API), NFR-004 |
| Files    | `Api/Controllers/AdminDesksController.cs`, `Api/Contracts/Admin/AdminDeskContracts.cs`; `[Authorize(Roles = Admin)]` |
| Verify   | Swagger shows `GET/POST /api/admin/desks`, `PUT /api/admin/desks/{id}`, `POST .../deactivate`, `POST .../activate` |

Employee JWT → 403. Duplicate → 409. Deactivate blocked → 409 with clear detail (V-09).

### Step 3 — Web Manage Desks UI (SCR-005)

| Field    | Value |
| -------- | ----- |
| Advances | AC-01..AC-05, SCR-005 ST-01, ST-03, ST-05..ST-08 |
| Files    | `Web/Areas/Admin/Controllers/AdminDesksController.cs`, `Models/AdminDesksViewModel.cs`, `Views/Admin/AdminDesks/Index.cshtml`, CSS |
| Verify   | Signed-in Admin at `/Admin/AdminDesks` — list, add form, edit, activate/deactivate with confirm |

New controller (nav already links here). Inline add/edit panels and deactivate confirm (same POST + antiforgery pattern as US-004). Empty inventory shows ST-03 message. Blocked deactivate shows ST-08 banner.

### Step 4 — Integration tests & traceability

| Field    | Value |
| -------- | ----- |
| Advances | AC-01..AC-05, V-07, V-08, V-09 |
| Files    | `tests/AdminDesksTests.cs`, `ApiAdminDesksTests.cs`, helpers + `.ac.test.js`; `manifest.json`, `traceability.md` |
| Verify   | `dotnet test` — all tests pass including `... (US-005/AC-##)` |

AC-04: after deactivate, employee availability API/page excludes desk. AC-05: seed Confirmed future booking on desk, assert deactivate fails. Employee cannot access admin desk routes (V-07).

### Step 5 — CI check

| Field    | Value |
| -------- | ----- |
| Advances | Gate D2 readiness |
| Files    | — |
| Verify   | `node tools/aidlc-check.mjs` — OK |

## Rollback

Revert the story PR. No schema change — uses existing `Desks` table and unique index.

## Open questions

| Question | Owner | Blocks |
| -------- | ----- | ------ |
| — | — | — |
