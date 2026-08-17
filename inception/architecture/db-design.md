# DB design — Employee Desk Booking System

> Gate 1 architecture deliverable (advisory). Traces to **BRD-001**, **SRS-001 §6**. Once TypeORM migrations exist, **migrations are the source of truth** — this document is the starting shape.

|                  |                                                                                  |
| ---------------- | -------------------------------------------------------------------------------- |
| **Traces to**    | BRD-001, SRS-001, REQ-001 … REQ-027, NFR-001 … NFR-006, BR-001.1 … BR-001.16   |
| **Engine**       | PostgreSQL 16+ (recommended default for TypeORM + concurrent booking)          |
| **Office TZ**    | Configured once per deployment (`OFFICE_TIMEZONE`, NFR-001)                     |

## Overview

Single-office desk booking: **users** book **desks** on **calendar dates** with a three-state lifecycle. Master data (users, desks) is admin-maintained. Notification preferences and delivery logs support email and optional browser push.

```
User 1──* Booking *──1 Desk
User 1──0..1 NotificationPreference
Booking 0──* EmailDeliveryLog (audit of send attempts)
Booking 0──0..1 BookingReminder (idempotency for day-before email)
```

---

## Entities

### `users`

Represents an Employee or Admin who can sign in. (REQ-002, REQ-004, REQ-005, REQ-018–REQ-022)

| Column           | Type            | Null | Notes |
| ---------------- | --------------- | ---- | ----- |
| `id`             | UUID            | NO   | Primary key |
| `email`          | VARCHAR(320)    | NO   | Unique, case-insensitive sign-in identifier (BR-001.10) |
| `name`           | VARCHAR(200)    | NO   | Display name |
| `password_hash`  | VARCHAR(255)    | NO   | bcrypt (or argon2) — never store plaintext (REQ-002, V-12 TBD) |
| `role`           | ENUM            | NO   | `employee` \| `admin` (REQ-004) |
| `is_active`      | BOOLEAN         | NO   | Default `true`; `false` = deactivated, cannot sign in (REQ-005, REQ-020) |
| `created_at`     | TIMESTAMPTZ     | NO   | Audit |
| `updated_at`     | TIMESTAMPTZ     | NO   | Audit |

**Indexes / constraints**

- `UNIQUE (lower(email))` — BR-001.10
- Check: at least one active admin must remain (enforced in service layer, BR-001.11 — not a DB check)

**Lifecycle:** Soft operational deactivation via `is_active`; rows are never hard-deleted (booking history integrity).

---

### `desks`

Bookable workspace identified by a unique desk number. (REQ-007, REQ-015–REQ-017)

| Column        | Type         | Null | Notes |
| ------------- | ------------ | ---- | ----- |
| `id`          | UUID         | NO   | Primary key |
| `desk_number` | VARCHAR(32)  | NO   | e.g. `A-01`; unique case-insensitively (BR-001.4, BR-001.8) |
| `status`      | ENUM         | NO   | `active` \| `inactive` (REQ-017) |
| `created_at`  | TIMESTAMPTZ  | NO   | Audit |
| `updated_at`  | TIMESTAMPTZ  | NO   | Audit |

**Indexes / constraints**

- `UNIQUE (lower(desk_number))` — BR-001.8

**Lifecycle:** Deactivate via `status = inactive` (BR-001.7); do not delete desks referenced by bookings.

---

### `bookings`

One employee, one desk, one calendar date, one status. (REQ-008, REQ-009, BR-001.5)

| Column              | Type        | Null | Notes |
| ------------------- | ----------- | ---- | ----- |
| `id`                | UUID        | NO   | Primary key |
| `user_id`           | UUID        | NO   | FK → `users.id` (booking owner) |
| `desk_id`           | UUID        | NO   | FK → `desks.id` |
| `booking_date`      | DATE        | NO   | Office-local calendar date (NFR-001); not UTC midnight ambiguity |
| `status`            | ENUM        | NO   | `confirmed` \| `cancelled` \| `completed` (BR-001.5) |
| `cancelled_at`      | TIMESTAMPTZ | YES  | Set when status → `cancelled` |
| `cancelled_by_id`   | UUID        | YES  | FK → `users.id`; who cancelled (employee self or admin on behalf) |
| `completed_at`      | TIMESTAMPTZ | YES  | Set when status → `completed` |
| `created_at`        | TIMESTAMPTZ | NO   | Audit |
| `updated_at`        | TIMESTAMPTZ | NO   | Audit |

**Indexes / constraints (critical for RISK-004)**

- **Partial unique:** `(user_id, booking_date) WHERE status = 'confirmed'` — BR-001.1 (one confirmed booking per employee per date)
- **Partial unique:** `(desk_id, booking_date) WHERE status = 'confirmed'` — V-04 (one confirmed booking per desk per date)
- Index: `(booking_date, status)` — admin filters (REQ-012, REQ-013)
- Index: `(user_id, booking_date DESC)` — my bookings (REQ-009)

**Lifecycle**

```
confirmed ──cancel──► cancelled
     │
     └── (booking_date < today, office local) ──► completed
```

- Cancellation only while `status = confirmed` and `booking_date >= today` (office local) — BR-001.6
- `completed` is terminal; no transition out
- Past `confirmed` rows are moved to `completed` by scheduled job (US-009, SRS-F-070)

---

### `notification_preferences`

Browser push opt-in per user. (REQ-026, REQ-027, NFR-006)

| Column                 | Type        | Null | Notes |
| ---------------------- | ----------- | ---- | ----- |
| `user_id`              | UUID        | NO   | PK + FK → `users.id` (one row per user) |
| `push_opt_in`          | BOOLEAN     | NO   | Default `false` (BR-001.15) |
| `push_subscription`    | JSONB       | YES  | Web Push subscription object when opted in; NULL when opted out |
| `updated_at`           | TIMESTAMPTZ | NO   | Audit |

Employees only in practice; Admins may have a row but push is not required for admin workflows.

---

### `booking_reminders`

Idempotency for day-before reminder emails. (REQ-025, BR-001.14)

| Column        | Type        | Null | Notes |
| ------------- | ----------- | ---- | ----- |
| `booking_id`  | UUID        | NO   | PK + FK → `bookings.id` |
| `sent_at`     | TIMESTAMPTZ | NO   | When reminder was successfully sent |
| `created_at`  | TIMESTAMPTZ | NO   | Audit |

Prevents duplicate reminders if the scheduler runs more than once. Only created on successful send.

---

### `email_delivery_logs`

Operational log for failed (and optionally successful) transactional emails. (NFR-005)

| Column           | Type         | Null | Notes |
| ---------------- | ------------ | ---- | ----- |
| `id`             | UUID         | NO   | Primary key |
| `booking_id`     | UUID         | YES  | FK → `bookings.id` when email relates to a booking |
| `user_id`        | UUID         | YES  | FK → `users.id` (recipient) |
| `email_type`     | ENUM         | NO   | `confirmation` \| `cancellation` \| `reminder` |
| `recipient`      | VARCHAR(320) | NO   | Email address attempted |
| `status`         | ENUM         | NO   | `sent` \| `failed` |
| `error_message`  | TEXT         | YES  | Provider error (no secrets) |
| `created_at`     | TIMESTAMPTZ  | NO   | Audit |

---

## Relationships (plain language)

- One **user** owns many **bookings**; each booking belongs to exactly one user.
- One **desk** appears in many **bookings** over time; each booking references exactly one desk.
- A **booking** is uniquely identified for active reservations by `(user, date)` and `(desk, date)` while `confirmed`.
- One **user** has at most one **notification_preferences** row.
- **booking_reminders** and **email_delivery_logs** hang off bookings for audit and scheduler idempotency.

---

## Keys and business rules enforced in the database

| Rule | Enforcement |
| ---- | ----------- |
| BR-001.1 One desk per employee per day | Partial unique index on `(user_id, booking_date)` where `confirmed` |
| V-04 Desk not double-booked | Partial unique index on `(desk_id, booking_date)` where `confirmed` |
| BR-001.8 Desk number unique | Unique on `lower(desk_number)` |
| BR-001.10 Email unique | Unique on `lower(email)` |
| BR-001.5 Single status | ENUM column + application transitions |
| BR-001.6 Cancel eligibility | Application layer (date + status check) |
| BR-001.9 Deactivate desk with future bookings | Application layer (query before update) |
| BR-001.11 Last admin safeguard | Application layer (count active admins) |

Concurrent create (RISK-004): wrap book in a transaction; rely on partial unique indexes to reject races with `409 Conflict`. Optional `SELECT … FOR UPDATE` on desk row for clearer error messages.

---

## Configuration (not stored in DB)

| Setting | Purpose |
| ------- | ------- |
| `OFFICE_TIMEZONE` | IANA zone, e.g. `Asia/Kolkata` (NFR-001) |
| `REMINDER_SEND_TIME` | Local time for day-before job; default `08:00` (BRD open Q #3) |

Single-office scope: no `office_id` column in this release (NFR-002). Holiday calendar deferred (BRD open Q #2).

---

## Bootstrap and seed data

**First Admin account** (BRD open Q #1): recommend a **one-time seed script** (`tools/seed-admin.mjs` or TypeORM seed) run during deployment that creates the first active Admin if none exists. Alternative: manual SQL insert — rejected for repeatability.

Initial desk inventory: empty; Admin adds desks via US-005 (REQ-015).

---

## Open questions (not invented here)

| # | Question | Owner | Impact on DB |
| - | -------- | ----- | ------------ |
| 1 | First Admin bootstrap mechanism | PO/Architect | Seed script recommended above — confirm in Gate 2 US-001 |
| 2 | Public holiday calendar | PO/client | Future `holidays` table or config; until then Mon–Fri only in app layer |
| 5 | Password complexity (V-12) | PO/security | Validation rules only; column size unchanged |
| 6 | Desk deactivate with future bookings | PO/client | BR-001.9 default: block or cancel-in-same-flow — no schema change either way |

---

## Migration order (suggested)

1. `users`
2. `desks`
3. `bookings`
4. `notification_preferences`
5. `booking_reminders`
6. `email_delivery_logs`
