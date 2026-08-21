# US-004 — implementation plan

> **The Gate D1 artifact.** The human reads this file, then approves in chat. DEV stamps the approval below; the developer commits the stamp. No code is written before that stamp exists.

|           |                                                                  |
| --------- | ---------------------------------------------------------------- |
| **Story** | `inception/stories/user-stories/US-004-admin-bookings.md`        |
| **Spec**  | `spec.md`                                                        |
| **Tier**  | Medium                                                           |

## Approval — Gate D1

| Field                | Value           |
| -------------------- | --------------- |
| Status               | approved |
| Approved by          | Vishal Harne \<vharne@degreed.com\> |
| Approved on          | 2026-08-21 |
| Plan commit approved | 5668066176411fe2607837b4998910f1d4f5c5d8 |

## Steps

Ordered. Test-first per AC: failing test named `... (US-004/AC-##)` before the code that turns it green.

### Step 1 — Application: admin list & cancel

| Field    | Value |
| -------- | ----- |
| Advances | REQ-011, REQ-012, REQ-013, REQ-014, BR-001.6 |
| Files    | `Application/Bookings/AdminBookingModels.cs`; extend `IBookingService`, `BookingService`; extend `IBookingRepository`, `EfBookingRepository` |
| Verify   | `dotnet build EmployeeDeskBooking.sln` — 0 errors |

`GetAllBookingsAsync(AdminBookingFilters?)` returns date, desk, employee email/name, status, `CanCancel`. `AdminCancelBookingAsync(bookingId, adminId)` cancels any employee's **Confirmed** today/future booking (same date rule as US-003).

### Step 2 — API admin endpoints

| Field    | Value |
| -------- | ----- |
| Advances | AC-01..AC-04 (API) |
| Files    | `Api/Controllers/AdminBookingsController.cs`, `Api/Contracts/Admin/AdminBookingContracts.cs`; `[Authorize(Roles = Admin)]` |
| Verify   | Swagger shows `GET /api/admin/bookings`, `POST /api/admin/bookings/{id}/cancel` |

Query params: optional `date`, `status`. Employee JWT → 403.

### Step 3 — Web Admin All Bookings UI (SCR-004)

| Field    | Value |
| -------- | ----- |
| Advances | AC-01..AC-04, SCR-004 ST-01..ST-04 |
| Files    | `Web/Areas/Admin/Controllers/AdminBookingsController.cs`, `Models/AdminBookingsViewModel.cs`, `Views/Admin/AdminBookings/Index.cshtml`, nav/CSS |
| Verify   | Signed-in Admin at `/Admin/AdminBookings` — filter form, table, admin cancel |

Replace US-001 stub view. Empty filter results show clear-filters message (ST-03).

### Step 4 — Integration tests & traceability

| Field    | Value |
| -------- | ----- |
| Advances | AC-01..AC-04, V-07 |
| Files    | `tests/AdminBookingsTests.cs`, `ApiAdminBookingsTests.cs`, helpers + `.ac.test.js`; `manifest.json`, `traceability.md` |
| Verify   | `dotnet test` — all tests pass including `... (US-004/AC-##)` |

Seed multi-employee bookings; assert Employee cannot access admin routes.

### Step 5 — CI check

| Field    | Value |
| -------- | ----- |
| Advances | Gate D2 readiness |
| Files    | — |
| Verify   | `node tools/aidlc-check.mjs` — OK |

## Rollback

Revert the story PR. No schema change.

## Open questions

| Question | Owner | Blocks |
| -------- | ----- | ------ |
| — | — | — |
