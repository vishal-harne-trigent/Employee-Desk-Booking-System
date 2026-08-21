# DB design — Employee Desk Booking System

> Gate 1 architecture deliverable (advisory). Traces to **BRD-001**, **SRS-001 §6**. Once **EF Core migrations** exist, **migrations are the source of truth** — this document is the starting shape.

|                  |                                                                                  |
| ---------------- | -------------------------------------------------------------------------------- |
| **Traces to**    | BRD-001, SRS-001, REQ-001 … REQ-027, NFR-001 … NFR-006, BR-001.1 … BR-001.16   |
| **Engine**       | **Microsoft SQL Server** (LocalDB in dev)                                        |
| **ORM**          | Entity Framework Core 8.0.11                                                     |
| **Access layer** | **Infrastructure tier** — `AppDbContext` and repositories; Presentation never queries SQL directly |
| **Office TZ**    | Configured once per deployment (`Office:TimeZone`, NFR-001)                      |

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

### `Users`

Represents an Employee or Admin who can sign in. (REQ-002, REQ-004, REQ-005, REQ-018–REQ-022)

| Column           | SQL Server type   | Null | Notes |
| ---------------- | ----------------- | ---- | ----- |
| `Id`             | `uniqueidentifier`| NO   | PK, `NEWSEQUENTIALID()` or app-generated GUID |
| `Email`          | `nvarchar(320)`   | NO   | Unique, case-insensitive (BR-001.10) |
| `Name`           | `nvarchar(200)`   | NO   | Display name |
| `PasswordHash`   | `nvarchar(500)`   | NO   | `IPasswordHasher<User>` (ASP.NET Identity Core) — never plaintext |
| `Role`           | `tinyint` / enum  | NO   | `Employee` = 0, `Admin` = 1 (REQ-004) |
| `IsActive`       | `bit`             | NO   | Default `1`; `0` = deactivated (REQ-005, REQ-020) |
| `CreatedAt`      | `datetimeoffset`  | NO   | Audit |
| `UpdatedAt`      | `datetimeoffset`  | NO   | Audit |

**Indexes / constraints**

- Unique index on `Email` with case-insensitive collation (`SQL_Latin1_General_CP1_CI_AS`) **or** persisted computed column `EmailNormalized` + unique index — BR-001.10
- Last active Admin rule enforced in application service (BR-001.11)

**Lifecycle:** Deactivate via `IsActive = 0`; no hard deletes (booking history).

---

### `Desks`

Bookable workspace identified by a unique desk number. (REQ-007, REQ-015–REQ-017)

| Column        | SQL Server type   | Null | Notes |
| ------------- | ----------------- | ---- | ----- |
| `Id`          | `uniqueidentifier`| NO   | PK |
| `DeskNumber`  | `nvarchar(32)`    | NO   | e.g. `A-01`; unique case-insensitively (BR-001.4, BR-001.8) |
| `Status`      | `tinyint` / enum  | NO   | `Active` = 0, `Inactive` = 1 (REQ-017) |
| `CreatedAt`   | `datetimeoffset`  | NO   | Audit |
| `UpdatedAt`   | `datetimeoffset`  | NO   | Audit |

**Indexes / constraints**

- Unique index on normalized desk number (same CI pattern as email) — BR-001.8

**Lifecycle:** Deactivate via `Status = Inactive` (BR-001.7); no deletes when bookings exist.

---

### `Bookings`

One employee, one desk, one calendar date, one status. (REQ-008, REQ-009, BR-001.5)

| Column            | SQL Server type   | Null | Notes |
| ----------------- | ----------------- | ---- | ----- |
| `Id`              | `uniqueidentifier`| NO   | PK |
| `UserId`          | `uniqueidentifier`| NO   | FK → `Users.Id` |
| `DeskId`          | `uniqueidentifier`| NO   | FK → `Desks.Id` |
| `BookingDate`     | `date`            | NO   | Office-local calendar date (NFR-001) |
| `Status`          | `tinyint` / enum  | NO   | `Confirmed`, `Cancelled`, `Completed` |
| `CancelledAt`     | `datetimeoffset`  | YES  | When cancelled |
| `CancelledById`   | `uniqueidentifier`| YES  | FK → `Users.Id` (self or admin) |
| `CompletedAt`     | `datetimeoffset`  | YES  | When completed |
| `CreatedAt`       | `datetimeoffset`  | NO   | Audit |
| `UpdatedAt`       | `datetimeoffset`  | NO   | Audit |

**Filtered unique indexes (critical for RISK-004)**

```sql
-- BR-001.1: one confirmed booking per employee per date
CREATE UNIQUE INDEX IX_Bookings_UserId_BookingDate_Confirmed
ON Bookings (UserId, BookingDate)
WHERE Status = 0;  -- Confirmed

-- V-04: one confirmed booking per desk per date
CREATE UNIQUE INDEX IX_Bookings_DeskId_BookingDate_Confirmed
ON Bookings (DeskId, BookingDate)
WHERE Status = 0;
```

Additional indexes: `(BookingDate, Status)` for admin filters; `(UserId, BookingDate DESC)` for my bookings.

**Lifecycle**

```
Confirmed ──cancel──► Cancelled
     │
     └── (BookingDate < today, office local) ──► Completed
```

---

### `NotificationPreferences`

Browser push opt-in per user. (REQ-026, REQ-027, NFR-006)

| Column              | SQL Server type   | Null | Notes |
| ------------------- | ----------------- | ---- | ----- |
| `UserId`            | `uniqueidentifier`| NO   | PK + FK → `Users.Id` |
| `PushOptIn`         | `bit`             | NO   | Default `0` (BR-001.15) |
| `PushSubscription`  | `nvarchar(max)`   | YES  | JSON Web Push subscription; NULL when opted out |
| `UpdatedAt`         | `datetimeoffset`  | NO   | Audit |

---

### `BookingReminders`

Idempotency for day-before reminder emails. (REQ-025, BR-001.14)

| Column       | SQL Server type   | Null | Notes |
| ------------ | ----------------- | ---- | ----- |
| `BookingId`  | `uniqueidentifier`| NO   | PK + FK → `Bookings.Id` |
| `SentAt`     | `datetimeoffset`  | NO   | Successful send timestamp |
| `CreatedAt`  | `datetimeoffset`  | NO   | Audit |

---

### `EmailDeliveryLogs`

Operational log for transactional email attempts. (NFR-005)

| Column          | SQL Server type   | Null | Notes |
| --------------- | ----------------- | ---- | ----- |
| `Id`            | `uniqueidentifier`| NO   | PK |
| `BookingId`     | `uniqueidentifier`| YES  | FK → `Bookings.Id` |
| `UserId`        | `uniqueidentifier`| YES  | FK → `Users.Id` |
| `EmailType`     | `tinyint` / enum  | NO   | Confirmation, Cancellation, Reminder |
| `Recipient`     | `nvarchar(320)`   | NO   | Address attempted |
| `Status`        | `tinyint` / enum  | NO   | Sent, Failed |
| `ErrorMessage`  | `nvarchar(max)`   | YES  | Provider error (no secrets) |
| `CreatedAt`     | `datetimeoffset`  | NO   | Audit |

---

## EF Core mapping notes

- Fluent API configures filtered unique indexes (`HasIndex(...).HasFilter(...)`) in `BookingConfiguration`.
- Enums stored as `tinyint` or mapped with `.HasConversion<int>()`.
- `PushSubscription` validated as JSON in application layer before persist.
- Connection string: `Server=(localdb)\mssqllocaldb;Database=EmployeeDeskBooking;...` in dev.

---

## Keys and business rules

| Rule | Enforcement |
| ---- | ----------- |
| BR-001.1 One booking per employee per day | Filtered unique index on `(UserId, BookingDate)` |
| V-04 Desk not double-booked | Filtered unique index on `(DeskId, BookingDate)` |
| BR-001.8 / BR-001.10 Uniqueness | CI collation or normalized computed columns |
| BR-001.6, BR-001.9, BR-001.11 | Application services + EF transactions |

Concurrent book (RISK-004): `IDbContextTransaction` + catch `DbUpdateException` on unique violation → HTTP 409.

---

## Configuration (not in DB)

| Setting | Purpose |
| ------- | ------- |
| `Office:TimeZone` | IANA zone e.g. `Asia/Kolkata` (NFR-001) |
| `Reminders:SendTimeLocal` | Default `08:00` (BRD open Q #3) |

---

## Bootstrap and seed data

**First Admin** (BRD Q #1 — **resolved**): `DbInitializer` on app startup/migrate creates the first Admin (and dev Employee accounts) when no users exist, using `IPasswordHasher<User>`. No manual DB step in dev; production bootstrap TBD at Gate 3.

Initial desks: empty until US-005.

---

## Migration order (EF Core)

1. Users  
2. Desks  
3. Bookings (+ filtered unique indexes)  
4. NotificationPreferences  
5. BookingReminders  
6. EmailDeliveryLogs  

Command: `dotnet ef migrations add InitialCreate --project src/EmployeeDeskBooking.Infrastructure --startup-project src/EmployeeDeskBooking.Web`

---

## Resolved decisions (from open questions)

| # | Decision | Resolution |
| - | -------- | ---------- |
| 1 | First Admin bootstrap | **`DbInitializer`** seeds Admin (+ dev Employee) when the database has no users — approved PO/Architect, 2026-08-21 |
| 5 | Password complexity (V-12) | **Min 8 chars**, at least one uppercase, lowercase, digit, and special character — approved PO/security, 2026-08-21 |

## Open questions

| # | Question | Owner |
| - | -------- | ----- |
| 2 | Holiday calendar | PO/client |
| 6 | Desk deactivate UX | PO/client |
