# US-009 — implementation plan

> **The Gate D1 artifact.** The human reads this file and `impact-analysis.md`, then approves in chat. DEV stamps the approval below; the developer commits the stamp. No code is written before that stamp exists.

|           |                                                                  |
| --------- | ---------------------------------------------------------------- |
| **Story** | `inception/stories/user-stories/US-009-booking-completion.md` |
| **Spec**  | `spec.md`                                                        |
| **Tier**  | Complex                                                          |

## Approval — Gate D1

| Field                | Value           |
| -------------------- | --------------- |
| Status               | approved |
| Approved by          | Vishal Harne \<vharne@degreed.com\> |
| Approved on          | 2026-08-22 |
| Plan commit approved | 0a940bb9ec1592a4eb6ec45d6a93afca103c8c4b |

## Steps

Ordered. Test-first per AC: failing test named `... (US-009/AC-##)` before the code that turns it green.

### Step 1 — Application completion service

| Field    | Value |
| -------- | ----- |
| Advances | BR-001.5, AC-01 |
| Files    | `Application/Bookings/IBookingCompletionService.cs`, `BookingCompletionService.cs`; extend `IBookingRepository` with `GetConfirmedBookingsBeforeDateAsync` |
| Verify   | `dotnet build` — selects Confirmed where `BookingDate < officeClock.Today` |

### Step 2 — Repository implementation

| Field    | Value |
| -------- | ----- |
| Advances | AC-01 |
| Files    | `Infrastructure/Bookings/EfBookingRepository.cs` — query + save via existing `SaveChangesAsync` |
| Verify   | Past Confirmed rows loaded; Cancelled/Completed excluded by status filter |

### Step 3 — Hosted job

| Field    | Value |
| -------- | ----- |
| Advances | AC-01 |
| Files    | `Infrastructure/Bookings/CompletePastBookingsHostedService.cs`; register in `DependencyInjection.cs`; enable on Web `Program.cs` (disabled in Testing) |
| Verify   | Job calls `CompletePastBookingsAsync`; idempotent re-run |

Sets `Status = Completed`, `CompletedAt = UtcNow`, `UpdatedAt = UtcNow`.

### Step 4 — Integration tests & traceability

| Field    | Value |
| -------- | ----- |
| Advances | AC-01..AC-03 |
| Files    | `tests/BookingCompletionTests.cs`, `.ac.test.js`; `manifest.json`, `traceability.md` |
| Verify   | `dotnet test` — 3 new tests named `... (US-009/AC-##)` |

Use `TestOfficeClock` / `BookDeskTestClient.FixedToday` for date boundaries.

### Step 5 — CI check

| Field    | Value |
| -------- | ----- |
| Advances | Gate D2 readiness |
| Files    | — |
| Verify   | `node tools/aidlc-check.mjs --write` — OK |

## Rollback

Revert PR. No schema rollback needed.

## Open questions

| Question | Owner | Blocks |
| -------- | ----- | ------ |
| Exact job clock time (~00:05 office local) | PO/client | No — hourly idempotent tick OK for MVP |
