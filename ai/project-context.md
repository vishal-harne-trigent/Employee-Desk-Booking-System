# Project context — Employee Desk Booking System (EDBS)

**Read this with `ai/AI-DLC.md` before any persona work.** This file is project-owned — tailor it as the product evolves.

| | |
| --- | --- |
| **Product** | Employee Desk Booking System (EDBS) |
| **Client / org** | Trigent |
| **Business goal** | Single-office hybrid desk reservation: employees book desks; admins manage bookings, inventory, and users |
| **Release 1 scope** | Web UI + REST API, email notifications, optional browser push, single office (India Standard Time default) |

---

## Technology stack

| Layer | Choice | Notes |
| ----- | ------ | ----- |
| Runtime | .NET 8 | Solution: `EmployeeDeskBooking.sln` |
| Web UI | ASP.NET Core MVC, Razor, Bootstrap 5 | Cookie auth; project `EmployeeDeskBooking.Web` |
| API | ASP.NET Core Web API, Swashbuckle | JWT Bearer; project `EmployeeDeskBooking.Api` |
| Application | Clean Architecture services | `EmployeeDeskBooking.Application` |
| Domain | Entities, enums, exceptions | `EmployeeDeskBooking.Domain` — no framework refs |
| Infrastructure | EF Core 8, MailKit, WebPush, hosted jobs | `EmployeeDeskBooking.Infrastructure` |
| Database | SQL Server (LocalDB in dev) | Connection string `DefaultConnection` |
| Tests | xUnit + `WebApplicationFactory` | `tests/EmployeeDeskBooking.Tests` |
| CI | GitHub Actions | `.github/workflows/aidlc-check.yml` |
| Issue tracking | Jira (convenience) | Keys `EDBS-###` in manifest; approval stays in GitHub |

---

## Repository layout

```
src/
  EmployeeDeskBooking.Web/          MVC UI (cookie session)
  EmployeeDeskBooking.Api/          REST + Swagger (JWT)
  EmployeeDeskBooking.Application/  Use cases, interfaces, business rules
  EmployeeDeskBooking.Domain/       Entities and enums
  EmployeeDeskBooking.Infrastructure/  EF Core, repos, email, push, jobs
tests/
  EmployeeDeskBooking.Tests/      xUnit integration + unit tests
tools/
  aidlc-check.mjs, aidlc-jira.mjs, aidlc-scaffold.mjs, …
  SendTestEmail/                  Manual SMTP / file-drop test
  RunReminderEmail/               Manual day-before reminder trigger (dev)
inception/
  product/requirements/           BRD-001, SRS-001
  stories/user-stories/           US-001 … US-009
  design/                         SCR specs, tokens, component previews
  architecture/                   TSD, db-design, app-architecture, diagram
knowledge/
  traceability/manifest.json      REQ ↔ US ↔ tests ↔ Jira (generated matrix)
  decisions/                      ADR-###
ai/                               Framework (gates, roles, standards, templates)
```

---

## Architecture (as-built)

**Canonical topology (TSD-001 §2.4):** **Web MVC** (cookie auth) and **REST API** (JWT) are dual presentation hosts. Both call `AddApplication()` and `AddInfrastructure()` in `Program.cs` and inject Application services in controllers. Web does **not** proxy domain operations through Api over HTTP.

```
Browser → Web ──► Application ──► Domain
API clients → Api ──► Application ──► Domain
                        ▲
                 Infrastructure ──► Domain → SQL Server
```

See [`inception/architecture/technical-specification.md`](inception/architecture/technical-specification.md) for diagrams and flows.

**Background jobs** (registered when `ReminderJob:Enabled` / completion job are on; Web host in dev, either host may register):

- `ReminderEmailHostedService` — hourly; sends day-before reminders at office local hour (default 08:00)
- `CompletePastBookingsHostedService` — marks past confirmed bookings as Completed

**External integrations:** SMTP (MailKit), Web Push (VAPID). Dev fallback: `FileDrop` mode writes `.eml` under `App_Data/sent-emails`.

---

## Run locally

**Prerequisites:** .NET 8 SDK, SQL Server LocalDB (Windows) or LocalDB-compatible SQL Server.

```powershell
# Restore and build
dotnet build EmployeeDeskBooking.sln

# Web UI — http://localhost:5198
dotnet run --project src/EmployeeDeskBooking.Web

# API + Swagger — http://localhost:5285/swagger
dotnet run --project src/EmployeeDeskBooking.Api

# All tests
dotnet test EmployeeDeskBooking.sln
```

**Configuration:**

- Shared settings: `appsettings.json` per host
- Development overrides: `appsettings.Development.json`
- Local secrets (gitignored): `appsettings.Development.local.json` or user secrets id `EmployeeDeskBooking.Web-dev`
- SMTP credentials: `Smtp:Username` / `Smtp:Password` (or `Email:*`); without credentials, use `Smtp:Mode` = `FileDrop`
- Office timezone: `Office:TimeZone` (default `India Standard Time`)
- Database is migrated and seeded on startup in Development (`InitializeDatabaseAsync`)

**Dev utilities:**

```powershell
dotnet run --project tools/SendTestEmail/SendTestEmail.csproj [recipient@email.com]
dotnet run --project tools/RunReminderEmail/RunReminderEmail.csproj   # bypasses 08:00 window
```

---

## Domain essentials

| Concept | Rule |
| ------- | ---- |
| Booking window | Today through +30 calendar days; Mon–Fri only (office timezone) |
| Booking status | `Confirmed` → `Cancelled` or `Completed` |
| One desk per user per day | Enforced in Application layer |
| Notifications | Email on confirm/cancel + day-before reminder; push optional on book/cancel only |
| Roles | `Employee`, `Admin` |
| Password reset | Admin-initiated only (no employee self-service forgot-password) |

Business rules are numbered **BR-001.\*** in architecture docs and implemented in `EmployeeDeskBooking.Application`.

---

## AI-DLC artifacts for this product

| Artifact | Location | IDs |
| -------- | -------- | --- |
| Requirements | `inception/product/requirements/` | BRD-001, SRS-001, REQ-001 … |
| User stories | `inception/stories/user-stories/` | US-001 … US-009 |
| **Consolidated stories** | `inception/stories/STORIES-001-desk-booking.md` | All stories in one document |
| Screens | `inception/design/screens/` | SCR-001 … SCR-007 |
| Architecture | `inception/architecture/` | TSD-001, db-design, app-architecture |
| Traceability | `knowledge/traceability/manifest.json` | Links REQ, US, tests, Jira |
| Decisions | `knowledge/decisions/` | ADR-001 … |

**Jira mapping (examples):** US-001 → EDBS-38, US-002 → EDBS-39, … (see manifest `stories` entries).

**Before opening a PR:** `node tools/aidlc-check.mjs`

---

## Testing conventions

- Test project: `tests/EmployeeDeskBooking.Tests/EmployeeDeskBooking.Tests.csproj`
- Integration tests use `CustomWebApplicationFactory` / `CustomApiApplicationFactory` with environment `Testing` and in-memory or test DB patterns
- Tests named after acceptance criteria: `*.ac.test.js` (Vitest-style naming on C# tests) and `*Tests.cs`
- Email/push fakes: `InMemoryEmailSender`, `InMemoryPushNotificationSender`, `TestOfficeClock`
- Run a single test class: `dotnet test --filter "FullyQualifiedName~ReminderEmailTests"`

### Testing delivery phase

The full as-built branch includes AC-linked tests for US-001 … US-009. **Current scaffold delivery may ship story PRs without new automated AC tests** when the PO defers test authoring for that sprint. Manual verification and `dotnet build` still apply. Before Gate 3 release, restore AC coverage per `ai/standards/testing-standards.md` and update `knowledge/traceability/manifest.json`.

---

## Standards — tailoring status

| File | Status |
| ---- | ------ |
| `ai/standards/coding-standards.md` | Tailored — .NET 8 / Clean Architecture |
| `ai/standards/api-standards.md` | Tailored — ASP.NET Core Web API + Swashbuckle |
| `ai/standards/testing-standards.md` | Tailored — xUnit + `WebApplicationFactory` |
| `ai/standards/security-standards.md` | Tailored — cookie + JWT, EF Core, MailKit/VAPID |
| `ai/standards/task-surfaces.md` | Tailored — EDBS project paths and tiers |
| `ai/standards/git-standards.md` | Tailored — scopes `web`, `api`, `application`, … |

---

## Persona shortcuts

| Need | Command |
| ---- | ------- |
| Start / route | `/aidlc` |
| Requirements | `/ba` |
| Screens / design system | `/ux` |
| Architecture / PR review | `/architect` |
| Implement a story | `/dev` |
| Tests / bugs | `/qa` |
| CI / release | `/devops` |
| Status / delivery plan | `/manager` |

Approvals are **GitHub PR reviews**, never chat text (except Gate D1 implementation plan in chat — see `ai/gates/delivery.md`).

---

## Known open items

- **Branch protection** — `aidlc-check` as required status may not be enforced on current GitHub plan (convention until configured)
- **Gate 3 test restore** — deferred AC tests on scaffold branches must be re-linked before production release

---

## Source-of-truth hierarchy (when docs conflict)

1. EF Core migrations (`src/EmployeeDeskBooking.Infrastructure/Data/Migrations/`)
2. OpenAPI / Swagger on Api host (`/swagger/v1/swagger.json`)
3. `inception/architecture/technical-specification.md` and companion architecture docs
