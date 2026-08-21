# App architecture — Employee Desk Booking System

> Gate 1 architecture deliverable (advisory). Traces to **BRD-001**, **SRS-001**, **SCR-001 … SCR-007**, **US-001 … US-009**. Once Swashbuckle is live, **`/swagger/v1/swagger.json` is the API contract**.

|                  |                                                                                  |
| ---------------- | -------------------------------------------------------------------------------- |
| **Traces to**    | BRD-001, SRS-001, EPIC-001                                                       |
| **Related**      | `inception/architecture/db-design.md`                                            |

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

The system uses **Layered Architecture** (also called **N-tier architecture**): responsibilities are split into horizontal layers, each layer depends only on the layer **directly below** it, and business rules live in the middle tiers — not in controllers or the database.

| Tier | Project(s) | Responsibility |
| ---- | ---------- | -------------- |
| **Presentation** | `EmployeeDeskBooking.Web`, `EmployeeDeskBooking.Api` | HTTP, routing, auth middleware, Razor views, JSON responses, input binding |
| **Application** | `EmployeeDeskBooking.Application` | Use cases, orchestration, DTOs, validation, business rules (BR-001.*) |
| **Domain** | `EmployeeDeskBooking.Domain` | Entities, enums, domain exceptions — no framework references |
| **Infrastructure** | `EmployeeDeskBooking.Infrastructure` | EF Core `DbContext`, repositories, MailKit, WebPush, password hasher, hosted jobs |
| **Data** | SQL Server (LocalDB in dev) | Persistent storage |

### Dependency rules

```
Presentation  →  Application  →  Domain
                      ↑
               Infrastructure  →  Domain
                      ↓
                    Data (SQL Server)
```

| Rule | Enforcement |
| ---- | ----------- |
| Presentation calls **Application** only | Controllers inject `IBookingService`, not `AppDbContext` |
| Application defines **interfaces**; Infrastructure implements them | `IUserRepository`, `IEmailSender` in Application; implementations in Infrastructure |
| **Domain** has zero upward dependencies | No references to Application, Infrastructure, Web, or Api |
| Infrastructure references Domain + Application contracts | EF entity configs map to Domain types |
| No **layer skipping** | Web/Api must not query EF or SQL directly |
| Shared logic stays in Application | Web and Api are thin adapters over the same services |

Background jobs (`IHostedService`) live in **Infrastructure** but invoke **Application** services — they do not bypass the Application tier.

---

## System context

Employees and Admins use a **server-rendered MVC app** (Razor + Bootstrap 5) aligned to SCR-001 … SCR-007. A companion **Web API** exposes the same domain over REST with **JWT Bearer** auth and **Swashbuckle** OpenAPI docs. Both presentation hosts sit on the same **Application** and **Infrastructure** layers and one **SQL Server** database via **EF Core**.

Outbound email uses **MailKit**; browser push uses **WebPush** with VAPID keys. Background jobs complete past bookings and send day-before reminders.

```
┌──────────────────────────────────────────────────────────────────┐
│  PRESENTATION TIER                                               │
│  ┌─────────────────────────┐  ┌─────────────────────────────┐   │
│  │ EmployeeDeskBooking.Web │  │ EmployeeDeskBooking.Api     │   │
│  │ MVC · Razor · Cookies   │  │ REST · JWT · Swashbuckle    │   │
│  │ SCR-001 … SCR-007       │  │ /api/*                      │   │
│  └───────────┬─────────────┘  └──────────────┬──────────────┘   │
└──────────────┼───────────────────────────────┼──────────────────┘
               │                               │
               └───────────────┬───────────────┘
                               ▼
┌──────────────────────────────────────────────────────────────────┐
│  APPLICATION TIER — services, DTOs, validators, BR-001.* rules   │
└────────────────────────────┬─────────────────────────────────────┘
                             ▼
┌──────────────────────────────────────────────────────────────────┐
│  DOMAIN TIER — User, Desk, Booking, enums (pure .NET)            │
└────────────────────────────▲─────────────────────────────────────┘
                             │
┌────────────────────────────┴─────────────────────────────────────┐
│  INFRASTRUCTURE TIER — EF Core, MailKit, WebPush, hosted jobs    │
└────────────────────────────┬─────────────────────────────────────┘
                             ▼
┌──────────────────────────────────────────────────────────────────┐
│  DATA TIER — SQL Server (LocalDB dev)                            │
└──────────────────────────────────────────────────────────────────┘
         MailKit SMTP · Web Push endpoints (external, via Infrastructure)
```

---

## Solution layout (N-tier mapping)

```
EmployeeDeskBooking.sln
src/
  EmployeeDeskBooking.Domain/           ← Domain tier
  EmployeeDeskBooking.Application/      ← Application tier
  EmployeeDeskBooking.Infrastructure/   ← Infrastructure tier (+ DI registration)
  EmployeeDeskBooking.Web/              ← Presentation tier (MVC)
  EmployeeDeskBooking.Api/              ← Presentation tier (REST)
tests/
  EmployeeDeskBooking.Tests/            Unit + integration tests
tools/                                  aidlc-check, aidlc-jira, seed utilities
```

**US-001** scaffolds the solution, enforces project references per the dependency rules above, and adds the first EF Core migration. Update `ai/standards/task-surfaces.md` and coding standards to replace the AI-DLC NestJS/Angular seed with this .NET N-tier layout.

---

## Layer responsibilities

| Tier | Project | Owns | Must not |
| ---- | ------- | ---- | -------- |
| **Presentation** | `Web` | MVC controllers, Razor views, cookie auth, `[Authorize]`, anti-forgery | Call `DbContext`, embed BR-001.* rules |
| **Presentation** | `Api` | REST controllers, JWT, Swashbuckle, Problem Details | Call `DbContext`, embed BR-001.* rules |
| **Application** | `Application` | `IBookingService`, `IAuthService`, DTOs, validators, `IOfficeClock`, BR-001.* | Reference EF, MailKit, or ASP.NET |
| **Domain** | `Domain` | `User`, `Desk`, `Booking`, enums, domain exceptions | Reference any other project |
| **Infrastructure** | `Infrastructure` | `AppDbContext`, EF migrations, repository implementations, MailKit, WebPush, `IHostedService` jobs | Expose data access to Presentation |

Web and Api are **thin presentation adapters** — both delegate to the same Application services; no duplicated business logic.

---

## Authentication and authorization

### Web (US-001) — Cookie

| Concern | Design |
| ------- | ------ |
| Sign-in | `AccountController.Login` POST — validate email/password via `IAuthService` (REQ-002) |
| Sign-out | `AccountController.Logout` POST — clear auth cookie (REQ-003) |
| Cookie | ASP.NET Core Cookie Authentication middleware; `HttpOnly`, `Secure`, `SameSite=Strict` in prod (NFR-003) |
| Password verify | `IPasswordHasher<User>.VerifyHashedPassword` |
| Routing after login | Employee → `Book/Index` (SCR-002); Admin → `AdminBookings/Index` (SCR-004) — US-001 AC-01/02 |
| Deactivated user | Reject at login (SCR-001 ST-04, REQ-005) |
| Generic error | Invalid credentials + deactivated use same message (V-01) |

**Authorization:** `[Authorize(Roles = "Admin")]` on admin controllers/areas.

### API — JWT Bearer

| Concern | Design |
| ------- | ------ |
| Token issue | `POST /api/auth/login` returns JWT (for API clients / AJAX if used) |
| Validation | JWT Bearer middleware on all `/api/*` except login |
| Claims | `sub`, `email`, `role` (`Employee` \| `Admin`) |
| Admin endpoints | `[Authorize(Roles = "Admin")]` on `/api/admin/*` |

MVC remains the primary UI; JWT supports OpenAPI consumers and progressive enhancement (e.g. push subscription POST from Razor page JavaScript).

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

- CRUD, deactivate, reset password (return plaintext once to Admin — BR-001.12), last-admin check (BR-001.11)
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

MVC actions may mirror these operations server-side without calling the HTTP API (direct Application layer) — keep Api in sync for contract tests.

---

## MVC UI (Razor + Bootstrap 5)

Controllers and views map to approved screens. Apply navy/green tokens from `inception/design/tokens.css` via site CSS; use Desk Booking logo asset.

| Area / route | Screen | Role |
| ------------ | ------ | ---- |
| `/Account/Login` | SCR-001 | Anonymous |
| `/Book` | SCR-002 | Employee |
| `/MyBookings` | SCR-003 | Employee |
| `/Admin/Bookings` | SCR-004 | Admin |
| `/Admin/Desks` | SCR-005 | Admin |
| `/Admin/Users` | SCR-006 | Admin |
| `/Settings/Notifications` | SCR-007 | Employee |

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

1. MVC: `BookController` GET — date picker → `GetAvailabilityAsync`
2. POST selected desk → `CreateBookingAsync` inside transaction
3. On success → MailKit confirmation + optional WebPush
4. Redirect to success view (SCR-002 ST-05)

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
