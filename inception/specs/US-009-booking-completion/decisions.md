# US-009 — decisions

| Date       | Decision | Rationale | Alternative rejected |
| ---------- | -------- | --------- | -------------------- |
| 2026-08-22 | Daily `IHostedService` on Web (~hourly tick, idempotent) | Matches `app-architecture.md` and US-007 reminder pattern | Complete-on-read in list endpoints |
| 2026-08-22 | `IBookingCompletionService.CompletePastBookingsAsync` callable from tests | Tests invoke with frozen `IOfficeClock` without waiting for scheduler | Timer-only tests |
| 2026-08-22 | Transition when `BookingDate < officeToday` | AC-03: today's Confirmed stays Confirmed until date passes | `BookingDate <= today` |
| 2026-08-22 | Set `CompletedAt` and `UpdatedAt` on transition | Audit trail; column already on entity | Status-only update |
