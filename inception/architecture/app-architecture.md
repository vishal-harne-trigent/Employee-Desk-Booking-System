# App architecture — Employee Desk Booking System

> Gate 1 architecture deliverable (advisory). Traces to **BRD-001**, **SRS-001**, **SCR-001 … SCR-007**, **US-001 … US-009**. Once OpenAPI and code exist, **`/api-docs-json` is the API source of truth**.

|                  |                                                                                  |
| ---------------- | -------------------------------------------------------------------------------- |
| **Traces to**    | BRD-001, SRS-001, EPIC-001                                                       |
| **Stack**        | Nx monorepo · NestJS API · Angular UI · PostgreSQL · TypeORM                     |
| **Related**      | `inception/architecture/db-design.md`                                            |

## System context

Browser clients (Employee, Admin) talk HTTPS to a single web application: an **Angular SPA** and a **NestJS REST API** backed by **PostgreSQL**. Outbound **SMTP** (or transactional email API) sends booking emails; **Web Push** (VAPID) delivers optional opt-in alerts. Scheduled jobs run inside the API process (NestJS `@nestjs/schedule`) for booking completion and reminder emails.

```
┌─────────────────────────────────────────────────────────────────┐
│                        Browser (HTTPS)                          │
│   Angular SPA (apps/ui) — SCR-001 … SCR-007                     │
└────────────────────────────┬────────────────────────────────────┘
                             │ JSON /api/*
                             │ generated client (libs/api/client)
┌────────────────────────────▼────────────────────────────────────┐
│              NestJS API (apps/api)                              │
│  ┌─────────┐ ┌──────────┐ ┌───────┐ ┌───────┐ ┌──────────────┐  │
│  │  auth   │ │ bookings │ │ desks │ │ users │ │ notifications│  │
│  └─────────┘ └──────────┘ └───────┘ └───────┘ └──────────────┘  │
│  ┌─────────────────────────────────────────────────────────────┐│
│  │ jobs (scheduler): completion + reminder emails              ││
│  └─────────────────────────────────────────────────────────────┘│
└────────────┬───────────────────────────────┬────────────────────┘
             │ TypeORM                        │ SMTP / Web Push
             ▼                                ▼
      PostgreSQL                      Email provider / Push endpoints
```

---

## Repository layout (Nx)

| Path | Responsibility |
| ---- | -------------- |
| `apps/api` | NestJS HTTP API, TypeORM entities, migrations, scheduled jobs |
| `apps/ui` | Angular standalone SPA, lazy feature routes per screen group |
| `libs/api/client` | OpenAPI-generated TypeScript client (never hand-edited) |
| `tools/` | aidlc-check, aidlc-jira, seed scripts |

First story (US-001) scaffolds `apps/api` and `apps/ui` if not present. Update `ai/standards/task-surfaces.md` when the stack is real.

---

## API modules

One NestJS module per business capability. Controllers stay thin; services own rules from BR-001.*.

### `AuthModule` (US-001)

| Concern | Design |
| ------- | ------ |
| Sign-in | `POST /api/auth/login` — email + password → session (REQ-002) |
| Sign-out | `POST /api/auth/logout` — invalidate session (REQ-003) |
| Session | **HTTP-only secure cookie** carrying a signed JWT (or server-side session store). Avoid localStorage for tokens (NFR-003). |
| Current user | `GET /api/auth/me` — role, name, email for routing (Employee → SCR-002, Admin → SCR-004) |
| Errors | Generic message for bad credentials / deactivated account (V-01, SCR-001 ST-03/ST-04) |

**Guards:** `AuthGuard` on all routes except login. `RolesGuard` with `@Roles('admin')` on admin modules.

> Note: Framework seed `api-standards.md` mentions `x-admin-key` for admin routes — **this project uses role-based JWT/cookie auth**, not a static admin key. Update project standards when US-001 lands.

---

### `BookingsModule` (US-002, US-003, US-004, US-009)

| Endpoint group | Role | Stories |
| -------------- | ---- | ------- |
| `GET /api/bookings/availability?date=` | Employee | US-002 — active desks + booked flag for date |
| `POST /api/bookings` | Employee | US-002 — create confirmed booking |
| `GET /api/bookings/mine` | Employee | US-003 — own history |
| `DELETE /api/bookings/:id` or `POST …/cancel` | Employee | US-003 — cancel own |
| `GET /api/admin/bookings?date=&status=` | Admin | US-004 — list + filters |
| `POST /api/admin/bookings/:id/cancel` | Admin | US-004 — cancel on behalf |

**Domain rules in `BookingsService`:** date window (V-02), Mon–Fri (V-03), one confirmed per user/desk (V-04, V-05), inactive desk rejection (BR-001.7), cancel eligibility (BR-001.6). Book inside a DB transaction; map unique violations to **409 Conflict** (RISK-004).

**Events:** emit `BookingConfirmed` / `BookingCancelled` domain events consumed by `NotificationsModule` (US-007, US-008).

---

### `DesksModule` (US-005)

| Endpoint | Role | Notes |
| -------- | ---- | ----- |
| `GET /api/admin/desks` | Admin | List all desks |
| `POST /api/admin/desks` | Admin | Add desk (V-08) |
| `PATCH /api/admin/desks/:id` | Admin | Edit number, activate/deactivate |
| `POST /api/admin/desks/:id/deactivate` | Admin | Optional dedicated flow for BR-001.9 cancel-in-same-flow |

---

### `UsersModule` (US-006)

| Endpoint | Role | Notes |
| -------- | ---- | ----- |
| `GET /api/admin/users` | Admin | List users |
| `POST /api/admin/users` | Admin | Create with initial password (REQ-018) |
| `PATCH /api/admin/users/:id` | Admin | Edit name/email (V-10) |
| `POST /api/admin/users/:id/deactivate` | Admin | BR-001.11 last-admin check |
| `POST /api/admin/users/:id/reset-password` | Admin | Returns one-time plaintext to Admin only (BR-001.12) — never log password |

---

### `NotificationsModule` (US-007, US-008)

| Concern | Design |
| ------- | ------ |
| Email | `EmailService` — SMTP via `@nestjs/config`; templates for confirm/cancel/reminder (REQ-023–025, V-13) |
| Failure log | Write `email_delivery_logs` on failure (NFR-005) |
| Push opt-in | `GET/PATCH /api/notifications/preferences` (SCR-007) |
| Push subscribe | `POST /api/notifications/push-subscription` — store JSON in `notification_preferences` |
| Push send | `PushService` using web-push + VAPID keys; only when `push_opt_in` (V-14, BR-001.15) |
| Reminders | No push for reminders (BR-001.16) |

Listens to booking events; email is mandatory on confirm/cancel (BR-001.13).

---

### `JobsModule` (US-009, US-007)

| Job | Schedule | Behaviour |
| --- | -------- | --------- |
| `CompletePastBookingsJob` | Daily ~00:05 office local | `confirmed` where `booking_date < today` → `completed` (SRS-F-070) |
| `SendReminderEmailsJob` | Daily 08:00 office local (TBD) | For tomorrow's working-day `confirmed` bookings, send reminder if not in `booking_reminders` (BR-001.14) |

Use `@nestjs/schedule` with timezone-aware cron (`OFFICE_TIMEZONE`). Jobs must be idempotent.

---

## UI architecture (Angular)

Feature-first lazy routes aligned to screens:

| Route prefix | Screen | User |
| ------------ | ------ | ---- |
| `/sign-in` | SCR-001 | All (unauthenticated) |
| `/book` | SCR-002 | Employee |
| `/my-bookings` | SCR-003 | Employee |
| `/admin/bookings` | SCR-004 | Admin |
| `/admin/desks` | SCR-005 | Admin |
| `/admin/users` | SCR-006 | Admin |
| `/settings/notifications` | SCR-007 | Employee |

- **`core/`** — auth facade, session state (signals), HTTP interceptors (cookie credentials)
- **`shared/`** — layout shell, nav, design tokens from `inception/design/tokens.css`
- **`features/*/`** — one folder per route; components match SCR states (ST-01 …)
- API access only through **generated client** + thin facade services (coding standards)

Post-login routing: Employee → `/book`; Admin → `/admin/bookings` (US-001 AC-01/AC-02).

Responsive vs desktop-only: **TBD (NFR-004)** — default to responsive layout using existing tokens.

---

## Cross-cutting concerns

| Topic | Approach |
| ----- | -------- |
| **Validation** | class-validator DTOs at API edge; whitelist + forbid unknown fields |
| **Errors** | Global exception filter → `{ statusCode, message, error }`; domain rejections → 422/409 |
| **AuthZ** | Role guard on admin paths; employees cannot call `/api/admin/*` |
| **Time** | `OfficeClock` service — all “today” and date parsing use `OFFICE_TIMEZONE` (NFR-001) |
| **Logging** | nestjs-pino structured logs; no passwords or reset tokens in logs (RISK-005) |
| **Config** | `@nestjs/config` + Joi validation schema in `apps/api` |
| **OpenAPI** | Swagger plugin on DTOs; CI regenerates `libs/api/client` on API changes |
| **Security** | HTTPS in prod; bcrypt passwords; CSRF consideration if cookie session (same-site strict) |

---

## Key flows

### F1 — Employee books a desk (US-002)

1. UI: pick date → `GET /api/bookings/availability?date=`
2. UI: pick desk → `POST /api/bookings { deskId, date }`
3. API: validate date/desk/user rules → transaction insert → partial unique indexes guard races
4. On success: emit `BookingConfirmed` → email + optional push
5. UI: success state SCR-002 ST-05

### F2 — Admin cancels on behalf (US-004)

1. `POST /api/admin/bookings/:id/cancel`
2. Set `status=cancelled`, `cancelled_by_id=admin`
3. Emit `BookingCancelled` → email + optional push to booking owner

### F3 — Day-before reminder (US-007)

1. Cron at 08:00 office local
2. Find `confirmed` bookings where `booking_date = tomorrow` and tomorrow is Mon–Fri
3. Skip if `booking_reminders` row exists
4. Send email; record success in `booking_reminders` + log

---

## Deployment shape (high level)

| Environment | Components |
| ----------- | ---------- |
| Dev | Docker Compose: PostgreSQL + API + UI dev server |
| Prod | TBD by DevOps (Gate 3) — API + UI static host + managed PostgreSQL; SMTP from IT (open Q #7) |

---

## Story → module map (delivery order)

| Sprint | Stories | Primary modules |
| ------ | ------- | --------------- |
| 1 | US-001, US-002 | auth, bookings (partial), scaffold |
| 2 | US-003, US-009 | bookings, jobs |
| 3 | US-004, US-005, US-006 | bookings (admin), desks, users |
| 4 | US-007, US-008 | notifications, jobs (reminders) |

---

## Decisions recorded here (no separate ADR)

| Decision | Choice | Rationale |
| -------- | ------ | --------- |
| Stack | NestJS + Angular + PostgreSQL | Matches AI-DLC reference standards; team scaffolds in US-001 |
| Admin auth | Role in JWT/cookie, not `x-admin-key` | REQ-004 role model; admins sign in like employees |
| Session transport | HTTP-only cookie | SPA-friendly, NFR-003 |
| Double-booking | DB partial unique indexes + transaction | RISK-004; fail fast with 409 |
| Scheduler | In-process NestJS cron | Single-office scale; no separate worker needed for MVP |
| First Admin | Seed script (recommended) | BRD open Q #1 — repeatable deploys |

---

## Open questions (unchanged from BRD)

| # | Question | Owner |
| - | -------- | ----- |
| 1 | Confirm first-Admin bootstrap (seed vs manual) | PO/Architect |
| 2 | Holiday calendar | PO/client |
| 3 | Reminder send time (default 08:00 local) | PO/client |
| 4 | Mobile-responsive vs desktop-only UI | PO/client |
| 5 | Password policy | PO/security |
| 6 | Desk deactivate UX (block vs cancel-in-flow) | PO/client |
| 7 | SMTP provider and sender domain | PO/IT |

---

## What happens when code exists

| Inception doc | Superseded by |
| ------------- | ------------- |
| Entity columns | TypeORM migrations in `apps/api` |
| REST endpoints | OpenAPI at `/api-docs-json` |
| UI routes | `apps/ui` route config |

Keep this document as history; do not duplicate changes in prose after Gate 2 starts.
