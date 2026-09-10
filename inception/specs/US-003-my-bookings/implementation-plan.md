# US-003 — implementation plan

> **The Gate D1 artifact.** The human reads this file, then approves in chat. DEV stamps the approval below; the developer commits the stamp. No code is written before that stamp exists.

|           |                                                                  |
| --------- | ---------------------------------------------------------------- |
| **Story** | `inception/stories/user-stories/US-003-my-bookings.md`           |
| **Spec**  | `spec.md`                                                        |
| **Tier**  | Medium                                                           |

## Approval — Gate D1

| Field                | Value           |
| -------------------- | --------------- |
| Status               | approved |
| Approved by          | Vishal Harne \<vharne@degreed.com\> |
| Approved on          | 2026-08-21 |
| Plan commit approved | 3d935f2b5a3cd5c86248d73ac9793425db9bb903 |

## Steps

Ordered. Test-first per AC: failing test named `... (US-003/AC-##)` before the code that turns it green.

### Step 1 — Application: list & cancel

| Field    | Value |
| -------- | ----- |
| Advances | REQ-009, REQ-010, BR-001.6 |
| Files    | `Application/Bookings/IBookingService.cs`, `BookingService.cs`, `MyBookingModels.cs`, `CancelBookingFailureReason.cs`; `IBookingRepository.cs`, `Infrastructure/Bookings/EfBookingRepository.cs` |
| Verify   | `dotnet build EmployeeDeskBooking.sln` — 0 errors |

`GetMyBookingsAsync(userId)` returns date, desk number, status. `CancelBookingAsync(userId, bookingId, cancelledById)` sets **Cancelled** only for own **Confirmed** bookings on today or future (office local). Past or **Completed** → reject.

### Step 2 — API endpoints

| Field    | Value |
| -------- | ----- |
| Advances | AC-01..AC-03 (API) |
| Files    | `Api/Controllers/BookingsController.cs` — `GET /api/bookings/mine`, `POST /api/bookings/{id}/cancel`; extend `Contracts/Bookings/*`, `BookingApiMessages.cs` |
| Verify   | Swagger lists mine + cancel routes |

HTTP: `200` success, `404` not found, `409` not cancellable.

### Step 3 — Web My Bookings UI (SCR-003)

| Field    | Value |
| -------- | ----- |
| Advances | AC-01..AC-04, SCR-003 ST-01..ST-05 |
| Files    | `Web/Controllers/MyBookingsController.cs`, `Models/MyBookingsViewModel.cs`, `Views/MyBookings/Index.cshtml`, CSS tweaks |
| Verify   | Signed-in Employee at `/MyBookings/Index` — table, cancel modal, empty state |

Nav link in `_AppNav.cshtml` already points to My Bookings — wire the controller.

### Step 4 — Integration tests & traceability

| Field    | Value |
| -------- | ----- |
| Advances | AC-01..AC-04 |
| Files    | `tests/MyBookingsTests.cs`, `ApiMyBookingsTests.cs`, test helpers + `.ac.test.js`; `manifest.json`, `traceability.md` |
| Verify   | `dotnet test` — all tests pass including `... (US-003/AC-##)` |

Seed **Confirmed**, **Cancelled**, and **Completed** bookings in test factories for date-range coverage.

### Step 5 — CI check

| Field    | Value |
| -------- | ----- |
| Advances | Gate D2 readiness |
| Files    | — |
| Verify   | `node tools/aidlc-check.mjs` — OK |

## Rollback

Revert the story PR. No schema change — cancel updates existing rows only.

## Open questions

| Question | Owner | Blocks |
| -------- | ----- | ------ |
| — | — | — |
