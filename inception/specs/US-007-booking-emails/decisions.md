# US-007 — decisions

| Date       | Decision | Rationale | Alternative rejected |
| ---------- | -------- | --------- | -------------------- |
| 2026-08-22 | MailKit for SMTP (`IEmailSender`) | Matches `app-architecture.md`; widely used in .NET | SendGrid SDK (extra vendor lock-in for MVP) |
| 2026-08-22 | `InMemoryEmailSender` in tests | QA notes: mock/capture mail without SMTP | Real SMTP in CI |
| 2026-08-22 | Booking commits even if email fails; log to `EmailDeliveryLogs` | AC-05 requires failure logging; blocking booking on SMTP outage harms core flow | Roll back booking on email failure |
| 2026-08-22 | Static HTML email bodies in Application layer | V-13 needs desk + date only; no Razor host in Application | Razor templates (deferred) |
| 2026-08-22 | Reminder job exposed as `IReminderEmailService.ProcessDueRemindersAsync` + thin `IHostedService` | Tests invoke job with frozen `IOfficeClock` without waiting for scheduler | Timer-only tests |
| 2026-08-22 | Reminder window: bookings with `BookingDate == officeToday.AddDays(1)` where tomorrow is a working day | BR-001.14 calendar day before; skip same-day (`BookingDate == today`) | Send at fixed hour only without date filter |
