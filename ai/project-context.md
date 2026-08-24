# Project context — Employee Desk Booking System

> Project-owned. Personas read this before delivery work alongside `ai/AI-DLC.md`.

## Product

Single-office **Employee Desk Booking System (EDBS)**: hybrid employees reserve desks; admins manage bookings, desk inventory (with location), and users. Web MVC + REST API on .NET 8, SQL Server, EF Core.

## Stack

| Layer | Technology |
| ----- | ---------- |
| Runtime | .NET 8 |
| Web UI | ASP.NET Core MVC, Razor, Bootstrap 5 |
| API | ASP.NET Core Web API, JWT, Swashbuckle |
| Data | EF Core 8, SQL Server (LocalDB in dev) |
| Tests (when used) | xUnit, `WebApplicationFactory` in `tests/EmployeeDeskBooking.Tests` |

## Repository layout

```
src/
  EmployeeDeskBooking.Domain/
  EmployeeDeskBooking.Application/
  EmployeeDeskBooking.Infrastructure/
  EmployeeDeskBooking.Web/          # cookie auth UI
  EmployeeDeskBooking.Api/          # JWT REST API
tests/EmployeeDeskBooking.Tests/
inception/                          # BRD, SRS, architecture, stories, specs
knowledge/traceability/manifest.json
```

## Delivery policy (current phase)

**Story unit/integration tests are deferred.** During this development phase:

- DEV does **not** create or run automated tests for user stories (`US-###`).
- Story PRs ship **code only**; manual verification and review are the evidence for now.
- Do **not** add `US-###/AC-##`-citing tests to the manifest for new story work unless the PO explicitly re-enables the testing gate.
- **Bug fixes** may still include regression tests when the team chooses — not required for every story change in this phase.

### Branch naming (important for CI)

The framework’s `aidlc-check` **requires** AC-citing tests on branches named `feat/US-###-*`.

For story work **without** tests in this phase, use a non-story branch name (e.g. `develop`, `feat/desk-location`, `fix/admin-nav`) — **not** `feat/US-005-manage-desks`.

Re-enable the full AC→test gate when the PO signs off (update `ai/standards/testing-standards.md` §Policy and restore `feat/US-###-*` delivery branches).

## Key references

| Doc | Path |
| --- | ---- |
| BRD | `inception/product/requirements/BRD-001-desk-booking.md` |
| SRS | `inception/product/requirements/SRS-001-desk-booking.md` |
| TSD | `inception/architecture/technical-specification.md` |
| Dev accounts (local seed) | Admin `admin@trigent.com` / Employee `vishal_h@trigent.com` — password `Password1!` |

## Local run

```bash
dotnet run --project src/EmployeeDeskBooking.Web    # http://localhost:5198
dotnet run --project src/EmployeeDeskBooking.Api     # Swagger http://localhost:5285/swagger
```
