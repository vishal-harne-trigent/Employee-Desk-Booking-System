# App architecture — Employee Desk Booking System

> Gate 1 architecture deliverable (advisory). Traces to **BRD-001**, **SRS-001**, **SCR-001 … SCR-007**, **US-001 … US-009**. Once Swashbuckle is live, **`/swagger/v1/swagger.json` is the API contract**.

|                  |                                                                                  |
| ---------------- | -------------------------------------------------------------------------------- |
| **Traces to**    | BRD-001, SRS-001, EPIC-001                                                       |
| **Related**      | `inception/architecture/db-design.md`, [`technical-specification.md`](technical-specification.md) |

## Technology stack (project standard)

| Layer | Technology | Version |
| ----- | ---------- | ------- |
| Runtime | .NET | 8.0 |
| Web UI | ASP.NET Core MVC, Razor, Bootstrap 5 | 8.0 |
| API | ASP.NET Core Web API, Swashbuckle (OpenAPI) | 8.0 |
| ORM | Entity Framework Core | 8.0.11 |
| Database | Microsoft SQL Server (LocalDB in dev) | — |
| Password hashing | `IPasswordHasher<User>` (ASP.NET Identity Core) | 8.0 |
| Email | MailKit | 4.16+ |
| Push | WebPush (VAPID) | 1.0.12 |
| Auth (Web) | Cookie authentication | — |
| Auth (API) | JWT Bearer | — |

---

## Architectural style: Layered (N-tier)

The system uses **Layered Architecture** with **dual presentation hosts**: **Web MVC** (cookie session) and **REST API** (JWT) both register and call the shared **Application** and **Infrastructure** libraries.

| Tier | Project(s) | Responsibility |
| ---- | ---------- | -------------- |
| **Presentation (UI)** | `EmployeeDeskBooking.Web` | Razor views, cookie session, CSRF, navigation — injects Application services |
| **Presentation (API)** | `EmployeeDeskBooking.Api` | REST endpoints, JWT, Swashbuckle — injects Application services |
| **Application** | `EmployeeDeskBooking.Application` | Use cases, orchestration, DTOs, validation, business rules (BR-001.*) |
| **Domain** | `EmployeeDeskBooking.Domain` | Entities, enums, domain exceptions — no framework references |
| **Infrastructure** | `EmployeeDeskBooking.Infrastructure` | EF Core `DbContext`, repositories, MailKit, WebPush, hosted jobs |
| **Data** | SQL Server (LocalDB in dev) | Persistent storage |

### Dependency rules

```
Web (MVC)  ──►  Application  ──►  Domain
Api (REST) ──►  Application  ──►  Domain
                    ▲
             Infrastructure  ──►  Domain
                    ↓
               SQL Server
```

| Rule | Enforcement |
| ---- | ----------- |
| **Web and Api call Application only** | Controllers inject `IBookingService`, not `AppDbContext` |
| **Both hosts register Infrastructure** | `AddApplication()` + `AddInfrastructure()` in each host's `Program.cs` |
| Application defines **interfaces**; Infrastructure implements them | `IUserRepository`, `IEmailSender` in Application; implementations in Infrastructure |
| **Domain** has zero upward dependencies | No references to Application, Infrastructure, Web, or Api |
| Infrastructure references Domain + Application contracts | EF entity configs map to Domain types |
| No **layer skipping** from Web or Api | Presentation hosts must not query EF or SQL directly |
| Business logic stays in Application | Web and Api are thin adapters |

Background jobs (`IHostedService`) live in **Infrastructure** and invoke **Application** services — registered by whichever host calls `AddInfrastructure(..., enableReminderJob: true, enableCompletionJob: true)` (Web in typical dev; Api may also register them).

---

## System context — EDBS System Architecture

See [`technical-specification.md §2.4`](technical-specification.md) for the canonical diagram and legend.

```
┌─────────────────── Clients ───────────────────┐
│  Browser (MVC UI · Cookie)   API consumers (JWT)│
└─────────┬─────────────────────────┬─────────────┘
          ▼                         ▼
┌───────── Presentation ────────────────────────┐
│  EDBS.Web (MVC·Razor·BS5)   EDBS.Api (REST)  │
│  sw.js · Push SW            Swagger           │
└─────────┬───────────────────────┬─────────────┘
          │                       │
          └───────────┬───────────┘
                      ▼ Internal flow
          EDBS.Application + EDBS.Infrastructure
                      │
                      ▼
          EDBS.Domain · Entities · Enums
                      │
                      ▼
              SQL Server (EDBS)
          ┌───────────┴───────────┐
          ▼                       ▼
    SMTP (email)          FCM / Web Push
```

| Client | Auth | Entry host | Domain path |
| ------ | ---- | ---------- | ----------- |
| Browser | Cookie | `EmployeeDeskBooking.Web` | Web → Application → Infrastructure |
| API consumer | JWT Bearer | `EmployeeDeskBooking.Api` | Api → Application → Infrastructure |

Outbound email uses **MailKit (SMTP)**; browser push uses **WebPush (VAPID)**. Hosted jobs register with Infrastructure on the host that enables them (typically Web for local dev).

---

## Solution layout (N-tier mapping)

```
EmployeeDeskBooking.sln
src/
  EmployeeDeskBooking.Domain/           ← Domain tier
  EmployeeDeskBooking.Application/      ← Application tier
  EmployeeDeskBooking.Infrastructure/   ← Infrastructure tier (+ DI registration)
  EmployeeDeskBooking.Web/              ← Presentation (MVC) — registers Application + Infrastructure
  EmployeeDeskBooking.Api/              ← Presentation (REST) — registers Application + Infrastructure
tests/
  EmployeeDeskBooking.Tests/            Unit + integration tests
tools/                                  aidlc-check, aidlc-jira, seed utilities
```

**US-001** scaffolds the solution, enforces project references per the dependency rules above, and adds the first EF Core migration. Update `ai/standards/task-surfaces.md` and coding standards to replace the AI-DLC NestJS/Angular seed with this .NET N-tier layout.

---

## Layer responsibilities

| Tier | Project | Owns | Must not |
| ---- | ------- | ---- | -------- |
| **Presentation (UI)** | `Web` | MVC + Razor + BS5, `sw.js` push service worker, cookie auth, injects Application services | Query `DbContext` or SQL directly; embed BR-001.* rules |
| **Presentation (API)** | `Api` | REST + Swagger, push subscribe API, JWT, injects Application services | Query `DbContext` or SQL directly; embed BR-001.* rules |
| **Application** | `Application` | `IBookingService`, `IAuthService`, DTOs, validators, `IOfficeClock`, BR-001.* | Reference EF, MailKit, or ASP.NET |
| **Domain** | `Domain` | `User`, `Desk`, `Booking`, enums, domain exceptions | Reference any other project |
| **Infrastructure** | `Infrastructure` | `AppDbContext`, EF migrations, repository implementations, MailKit, WebPush, `IHostedService` jobs | Expose data access to Presentation |

Web and Api are **thin presentation adapters** — both delegate to Application services; business logic is not duplicated between hosts.

---

## Authentication and authorization

### Web (US-001) — Cookie session + Application services

| Concern | Design |
| ------- | ------ |
| Sign-in | `AccountController.Login` POST → `IAuthService.SignInAsync` → cookie session with role claim |
| Sign-out | `AccountController.Logout` POST — clear auth cookie |
| Cookie | ASP.NET Core Cookie Authentication middleware; `HttpOnly`, `Secure`, `SameSite=Strict` in prod (NFR-003) |
| Routing after login | Employee → `/Desks/Availability` (SCR-002); Admin → `/Admin/AdminBookings` (SCR-004) — US-001 AC-01/02 |
| Deactivated user | Sign-in rejected with deactivated message (SCR-001 ST-04, REQ-005) |
| Generic error | Invalid credentials use same message (V-01) |

**Authorization:** `[Authorize(Roles = "Employee,Admin")]` on employee flows; `[Authorize(Roles = "Admin")]` on Admin area. Employee-facing nav uses `asp-area=""` so routes resolve from Admin pages.

### API — JWT Bearer

| Concern | Design |
| ------- | ------ |
| Token issue | `POST /api/auth/login` returns JWT |
| Validation | JWT Bearer middleware on all `/api/*` except login |
| Claims | `sub`, `email`, `name`, `role` (`Employee` \| `Admin`) |
| Admin endpoints | `[Authorize(Roles = "Admin")]` on `/api/admin/*` |

JWT supports OpenAPI consumers and automated tests. Web MVC uses cookie auth and Application services directly (not HTTP to Api).

---

## Application services (by feature)

### Auth — US-001

- `IAuthService` — login, logout, current user
- `IUserStore` / repository via EF

### Bookings — US-002, US-003, US-004, US-009

| Service method | Stories | Notes |
| -------------- | ------- | ----- |
| `GetAvailabilityAsync(date)` | US-002 | Active desks + booked flag |
| `CreateBookingAsync(deskId, date)` | US-002 | Transaction; 409 on unique violation |
| `GetMyBookingsAsync()` | US-003 | |
| `CancelBookingAsync(id, cancelledBy)` | US-003, US-004 | BR-001.6 eligibility |
| `GetAllBookingsAsync(filters)` | US-004 | Admin date/status filters |
| `CompletePastBookingsAsync()` | US-009 | Called by hosted service |

Raises domain events or calls `INotificationService` on confirm/cancel.

### Desks — US-005

- CRUD + activate/deactivate; BR-001.9 guard before deactivate

### Users — US-006

- CRUD, deactivate, **reactivate** (Web MVC), reset password on dedicated page (new + confirm — BR-001.12), last-admin check (BR-001.11)
- Hash new passwords with `IPasswordHasher<User>`

### Notifications — US-007, US-008

| Component | Technology |
| --------- | ---------- |
| Transactional email | `IEmailSender` → MailKit SMTP |
| Templates | Razor email templates or static HTML with desk + date (V-13) |
| Failure logging | `EmailDeliveryLogs` table (NFR-005) |
| Push preferences | MVC `NotificationSettingsController` (SCR-007) |
| Push delivery | `WebPush` library; subscription JSON in DB |
| Reminders | Hosted service; idempotent via `BookingReminders` |

---

## Web API surface (Swashbuckle)

Base path: `/api`. Document with Swashbuckle at `/swagger`.

| Area | Example routes | Auth |
| ---- | -------------- | ---- |
| Auth | `POST /api/auth/login` | Anonymous |
| Bookings | `GET /api/bookings/availability`, `POST /api/bookings`, `GET /api/bookings/mine`, `POST /api/bookings/{id}/cancel` | JWT, Employee |
| Admin bookings | `GET /api/admin/bookings`, `POST /api/admin/bookings/{id}/cancel` | JWT, Admin |
| Admin desks | `GET/POST/PATCH /api/admin/desks` | JWT, Admin |
| Admin users | `GET/POST/PATCH /api/admin/users`, `POST …/reset-password` | JWT, Admin |
| Notifications | `GET/PATCH /api/notifications/preferences`, `POST /api/notifications/push-subscription` | JWT, Employee |

HTTP status: `200/201` success, `400` validation, `401/403` auth, `409` booking conflict, `422` domain rejection.

MVC controllers call Application services directly; keep Api routes in sync for OpenAPI consumers and integration tests.

---

## MVC UI (Razor + Bootstrap 5)

Controllers and views map to approved screens. Apply navy/green tokens from `inception/design/tokens.css` via site CSS; use Desk Booking logo asset.

| Area / route | Screen | Role |
| ------------ | ------ | ---- |
| `/Account/Login` | SCR-001 | Anonymous |
| `/Desks/Availability` | SCR-002 | Employee, Admin |
| `/MyBookings` | SCR-003 | Employee, Admin |
| `/Admin/AdminBookings` | SCR-004 | Admin |
| `/Admin/AdminDesks` | SCR-005 | Admin |
| `/Admin/AdminUsers` | SCR-006 | Admin |
| `/Admin/AdminUsers/ResetPassword` | SCR-006 (reset) | Admin |
| `/Settings/Notifications` | SCR-007 | Employee, Admin |

Shared `_Layout.cshtml`: role-based nav (Employee vs Admin per wireframes). View models per ST-## states (loading, empty, error).

Responsive vs desktop-only: **TBD (NFR-004)** — Bootstrap 5 grid defaults to responsive.

---

## Background jobs (`IHostedService`)

| Service | Schedule | Story |
| ------- | -------- | ----- |
| `CompletePastBookingsHostedService` | Daily ~00:05 office local | US-009 |
| `ReminderEmailHostedService` | Daily 08:00 office local (TBD) | US-007 |

Use `TimeZoneInfo` with configured `Office:TimeZone`. Jobs idempotent and logged.

---

## Cross-cutting

| Topic | Approach |
| ----- | -------- |
| Validation | Data annotations + FluentValidation in Application |
| Errors | Problem Details (`RFC 7807`) on API; ModelState on MVC |
| Time | `IOfficeClock` — all “today” logic uses office timezone (NFR-001) |
| Logging | `ILogger<T>`; never log passwords or reset tokens (RISK-005) |
| Config | `appsettings.json` + environment; secrets in user secrets / Key Vault |
| CSRF | Anti-forgery tokens on MVC forms |
| HTTPS | Required in non-dev (NFR-003) |

---

## Key flows

### Employee books (US-002)

1. MVC: `DesksController.Availability` GET — date picker → `GetAvailabilityAsync`
2. GET `/Desks/Book?deskId=&date=` → `CreateBookingAsync` inside transaction
3. On success → MailKit confirmation + optional WebPush
4. Re-render availability view with success banner (SCR-002 ST-05)

### Day-before reminder (US-007)

1. `ReminderEmailHostedService` fires at configured local time
2. Query confirmed bookings for tomorrow (Mon–Fri target day)
3. Skip if `BookingReminders` exists; else MailKit send + insert row

---

## Deployment (high level)

| Environment | Components |
| ----------- | ---------- |
| Dev | LocalDB + IIS Express / Kestrel; `dotnet run` on Web + Api |
| Prod | TBD (Gate 3) — Azure App Service or IIS + SQL Server; SMTP from IT (open Q #7) |

---

## Story → delivery map

| Sprint | Stories | Primary work |
| ------ | ------- | ------------ |
| 1 | US-001, US-002 | Solution scaffold, cookie auth, booking create |
| 2 | US-003, US-009 | My bookings, completion job |
| 3 | US-004, US-005, US-006 | Admin areas |
| 4 | US-007, US-008 | MailKit + WebPush + reminder job |

---

## Decisions (no separate ADR)

| Decision | Choice |
| -------- | ------ |
| Architecture | **Layered (N-tier)** — Presentation → Application → Domain; Infrastructure implements Application contracts |
| Stack | .NET 8, MVC + Web API, EF Core, SQL Server — **project standard** |
| UI | Server-rendered Razor (not SPA) — matches SCR wireframes and team stack |
| Dual auth | Cookies for MVC; JWT for API/OpenAPI |
| Passwords | `IPasswordHasher<User>` — do not custom-hash; **V-12:** min 8 chars, upper + lower + digit + special (PO/security approved 2026-08-21) |
| Email / push | MailKit + WebPush as specified |
| Concurrency | SQL Server filtered unique indexes + EF transactions (RISK-004) |
| First Admin | **`DbInitializer`** seeds Admin (+ dev Employee) when no users exist (PO/Architect approved 2026-08-21) |

---

## Resolved decisions (from BRD open questions)

| # | Decision | Resolution |
| - | -------- | ---------- |
| 1 | First Admin bootstrap | **`DbInitializer`** on startup/migrate when no users — PO/Architect, 2026-08-21 |
| 5 | Password policy (V-12) | Min 8 chars; upper, lower, digit, special required — PO/security, 2026-08-21 |

## Open questions (from BRD)

| # | Question | Owner |
| - | -------- | ----- |
| 2 | Holiday calendar | PO/client |
| 3 | Reminder time (default 08:00) | PO/client |
| 4 | Mobile vs desktop | PO/client |
| 6 | Desk deactivate flow | PO/client |
| 7 | SMTP / sender | PO/IT |

---

## When code exists

| Inception doc | Superseded by |
| ------------- | ------------- |
| Tables/columns | EF Core migrations in `Infrastructure` |
| REST shapes | Swashbuckle OpenAPI JSON |
| UI | Razor views + routes in `Web` |

Keep this document as history after Gate 2; do not maintain two sources of truth.
