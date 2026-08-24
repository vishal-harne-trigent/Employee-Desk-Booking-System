# Testing standards — Employee Desk Booking System

## Policy (current development phase)

**Automated tests for user stories are not required right now.**

- DEV does **not** create or run unit/integration tests when implementing `US-###` stories.
- Evidence for a story PR in this phase: **manual verification** (steps in the PR description) + human review — not `dotnet test` output tied to ACs.
- Existing tests in `tests/EmployeeDeskBooking.Tests/` remain in the repo but are **not** expanded for new story delivery until this policy is lifted.
- When the PO re-enables testing, restore the rules in §Target state below and use `feat/US-###-<slug>` delivery branches again.

See also: `ai/project-context.md` §Delivery policy.

## Stack (when tests are used)

| Level | Tool | Where |
| ----- | ---- | ----- |
| Integration (Web) | xUnit + `WebApplicationFactory` | `tests/EmployeeDeskBooking.Tests/*Tests.cs` |
| Integration (API) | xUnit + `CustomApiApplicationFactory` | `tests/EmployeeDeskBooking.Tests/Api*Tests.cs` |
| Unit | xUnit | Next to helpers or in `tests/` |

Run: `dotnet test tests/EmployeeDeskBooking.Tests`

## Target state (when story testing is re-enabled)

- Test the **requirement**: every test cites `US-### / AC-##` in the name; assert observable behavior, not internals.
- Per story: positive cases from AC, then negative, then boundary — or written justification.
- Deterministic tests only — no sleeps, real SMTP, or wall-clock deps; use fakes and `Testing` environment.
- Test names read as specs: `[Fact] public async Task Admin_can_add_desk_with_location_US_005_AC_03()`
- A red test is a finding: never deleted, skipped, or loosened to pass.
- `aidlc-check` verifies active tests cite each manifest AC on `feat/US-###-*` branches.

## Bug reports (always)

Reproducible or it doesn't exist: steps, expected (AC or issue ref), actual, environment. Regression tests on bug fixes are **recommended** but not mandatory in the current deferred-testing phase unless the PO says otherwise.
