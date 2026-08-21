# US-002 — implementation plan

> **The Gate D1 artifact.** The human reads this file, then approves in chat. DEV stamps the approval below; the developer commits the stamp. No code is written before that stamp exists.

|           |                                                                  |
| --------- | ---------------------------------------------------------------- |
| **Story** | `inception/stories/user-stories/US-002-book-desk.md`             |
| **Spec**  | — (Medium tier; story + SCR-002 + architecture docs)             |
| **Tier**  | Medium                                                           |

## Approval — Gate D1

| Field                | Value           |
| -------------------- | --------------- |
| Status               | approved |
| Approved by          | Vishal Harne \<vharne@degreed.com\> |
| Approved on          | 2026-08-21 |
| Plan commit approved | e3022b59aa34f1c14e4cc17ee2793504d0ece930 |

## Steps

Ordered. Test-first per AC: failing test named `... (US-002/AC-##)` before the code that turns it green.

### Step 1 — Domain & persistence (Desks, Bookings)

| Field    | Value |
| -------- | ----- |
| Advances | REQ-007, REQ-008, BR-001.1, BR-001.7 |
| Files    | `Domain/Desks/Desk.cs`, `DeskStatus.cs`; `Domain/Bookings/Booking.cs`, `BookingStatus.cs`; `Infrastructure/Data/DeskConfiguration.cs`, `BookingConfiguration.cs`, `AppDbContext.cs`; migration `AddDesksAndBookings` with filtered unique indexes per `db-design.md` |
| Verify   | `dotnet ef migrations add AddDesksAndBookings --project src/EmployeeDeskBooking.Infrastructure --startup-project src/EmployeeDeskBooking.Web` — succeeds |

### Step 2 — Application booking layer & office clock

| Field    | Value |
| -------- | ----- |
| Advances | REQ-006, REQ-008, NFR-001, BR-001.3, BR-001.4 |
| Files    | `Application/Bookings/*` (`IBookingService`, `BookingService`, models, validation); `Application/Time/IOfficeClock.cs`; `Infrastructure/Bookings/EfBookingRepository.cs`, `Infrastructure/Time/OfficeClock.cs`; DI registration |
| Verify   | `dotnet build EmployeeDeskBooking.sln` — 0 errors |

Date rules: today through +30 calendar days, office local timezone (`Office:TimeZone` = `India Standard Time`); reject weekends (V-02, V-03).

### Step 3 — Seed desks & extend DbInitializer

| Field    | Value |
| -------- | ----- |
| Advances | REQ-007, QA fixtures |
| Files    | `Infrastructure/DbInitializer.cs` — seed Active desks `A-01`…`A-05` and one Inactive `B-99` when empty |
| Verify   | Fresh DB migrate + seed shows 6 desks |

### Step 4 — API booking endpoints

| Field    | Value |
| -------- | ----- |
| Advances | AC-01..AC-06 (API surface) |
| Files    | `Api/Controllers/BookingsController.cs`, `Api/Contracts/Bookings/*`, `Api/Bookings/BookingApiMessages.cs`; `[Authorize]` on routes |
| Verify   | Swagger shows `GET /api/bookings/availability?date=` and `POST /api/bookings` |

HTTP: `200/201` success, `400` invalid date, `409` double-book or desk taken, `422` inactive desk.

### Step 5 — Web Book Desk UI (SCR-002)

| Field    | Value |
| -------- | ----- |
| Advances | AC-01..AC-06, SCR-002 ST-01..ST-07 |
| Files    | `Web/Controllers/BookController.cs`, `Models/BookIndexViewModel.cs`, `Views/Book/Index.cshtml`, `Views/Shared/_AppNav.cshtml`, nav/CSS updates per SCR-002 |
| Verify   | Signed-in Employee at `/Book/Index` — date picker, desk table, book flow |

States covered: loading, desks available, empty date, validation errors, already-booked banner (ST-06), confirm modal (ST-07).

### Step 6 — Integration tests & traceability

| Field    | Value |
| -------- | ----- |
| Advances | AC-01..AC-06 |
| Files    | `tests/BookDeskTests.cs`, `BookDeskTestClient.cs`, `ApiBookingTests.cs`, `TestOfficeClock.cs`; extend `CustomWebApplicationFactory` / `CustomApiApplicationFactory` with desk/booking seed; `.ac.test.js` companions; `manifest.json`, `traceability.md` |
| Verify   | `dotnet test EmployeeDeskBooking.sln` — all tests pass including `... (US-002/AC-##)` |

### Step 7 — CI check

| Field    | Value |
| -------- | ----- |
| Advances | Gate D2 readiness |
| Files    | — |
| Verify   | `node tools/aidlc-check.mjs` — OK |

## Rollback

Revert the story PR. Drop/re-migrate LocalDB if `AddDesksAndBookings` was applied locally.

## Open questions

| Question | Owner | Blocks |
| -------- | ----- | ------ |
| — | — | — |
