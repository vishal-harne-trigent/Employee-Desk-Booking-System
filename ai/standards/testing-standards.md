# Testing standards

## Levels and placement

| Level | Tool | Where |
| ----- | ---- | ----- |
| Integration (Web MVC) | xUnit + `WebApplicationFactory` | `tests/EmployeeDeskBooking.Tests/*Tests.cs` |
| Integration (REST API) | xUnit + `WebApplicationFactory` | `tests/EmployeeDeskBooking.Tests/Api*Tests.cs` |
| Unit (pure logic) | xUnit | `tests/EmployeeDeskBooking.Tests/*Tests.cs` (e.g. template/formatters) |
| AC traceability | DisplayName + method name | `*.ac.test.js` companion files **or** `[Fact(DisplayName = "... (US-###/AC-##)")]` on C# tests |

Run via **`dotnet test EmployeeDeskBooking.sln`** only. Filter: `dotnet test --filter "FullyQualifiedName~ReminderEmailTests"`.

## Test infrastructure

- **`CustomWebApplicationFactory`** — Web host, environment `Testing`, in-memory EF database, seeded users
- **`CustomApiApplicationFactory`** — Api host with the same testing substitutions
- **Fakes:** `InMemoryEmailSender`, `InMemoryPushNotificationSender`, `TestOfficeClock` — no real SMTP, push, or wall-clock in tests
- **`BookDeskTestClient`** / **`ApiTestClient`** — HTTP helpers for end-to-end flows through the host
- Reset state between tests via factory helpers (`ResetBookingsAsync`, scoped `AppDbContext`)

## Rules

- Test the **requirement**: every TC cites `US-### / AC-##`; tests assert observable behavior, not implementation internals
- Per story: positive cases from AC, then negative, then boundary — all three classes or a written justification
- Deterministic tests only — no sleeps, real network, or wall-clock deps; use fakes and fixed dates (`BookDeskTestClient.FixedToday`)
- Test names read as specs: `[Fact(DisplayName = "Day-before reminder for confirmed future booking (US-007/AC-04)")]`
- A red test is a finding: never deleted, skipped, or loosened to pass. Fix code or (with BA/human approval) fix the requirement
- Coverage: every AC ≥ 1 TC before a story is _done_
- **The AC citation is a label, not the proof.** `aidlc-check` confirms an active test named `US-###/AC-##` exists and passes — the proof is the assertion. Write the test first and watch it fail; a reviewer who cannot map an assertion to the AC treats that as a finding

## Naming conventions

- Test class: `{Feature}Tests` or `Api{Feature}Tests`
- AC-linked companion files: `{Feature}Tests.ac.test.js` (Vitest-style naming for traceability tooling; body may be a stub pointing at the C# test)
- Method names: descriptive snake or PascalCase matching existing files (`Day_before_reminder_US_007_AC_04`)

## Bug reports

Reproducible or it does not exist: steps, expected (AC ref), actual, environment, severity. Filed to DEV; regression TC added on fix citing the issue.
