# SRS-001 — Employee Desk Booking System

> Software Requirements Specification derived from approved **BRD-001**. Approval = PO/BA human reviewing + merging this document's PR. GitHub records who approved what.


|                  |                                                                                                                       |
| ---------------- | --------------------------------------------------------------------------------------------------------------------- |
| **Author**       | BA persona (AI draft) with PO/BA human                                                                                |
| **Derived from** | `inception/product/requirements/BRD-001-desk-booking.md` (Gate 1, approved)                                           |
| **Source input** | Same as BRD-001: `2026-08-13-client-discussion.md`, `2026-08-14-admin-provisioning.md`, `2026-08-14-notifications.md` |
| **Related**      | BRD-001, SCR-001 … SCR-007 (design approved), EPIC-001 (when stories are drafted)                                     |




## 1. Introduction



### 1.1 Purpose

This document specifies **what the Employee Desk Booking System software shall do** — behaviour, interfaces, data, and quality attributes — for implementers (development, QA, architecture). It is derived from the approved business requirements in BRD-001 and aligned with approved screen designs (SCR-001 … SCR-007).

### 1.2 Scope

The software product is a **browser-based web application** for a **single hybrid office**. It shall:

- Authenticate users (Employee, Admin) with email and password.
- Let Employees book, view, and cancel desk reservations by date and desk number.
- Let Admins oversee all bookings, manage desk inventory, and manage user accounts.
- Send transactional email notifications and optional browser push notifications per business rules.

Out of scope matches BRD-001 §10 (no SSO, no self-service password reset, no multi-office, no weekend booking, no SMS, etc.).

### 1.3 Definitions and acronyms


| Term                       | Definition                                                                            |
| -------------------------- | ------------------------------------------------------------------------------------- |
| **Employee**               | User role: books and manages own desk reservations.                                   |
| **Admin**                  | User role: full booking oversight plus desk and user administration.                  |
| **Desk**                   | A bookable workspace identified by a unique alphanumeric **desk number** (e.g. A-01). |
| **Booking**                | A reservation linking one Employee, one Desk, and one calendar date, with a status.   |
| **Confirmed**              | Active booking for a future or current working day.                                   |
| **Cancelled**              | Booking voided before use.                                                            |
| **Completed**              | Booking whose date has passed without cancellation.                                   |
| **Active / Inactive desk** | Active desks appear in employee availability; Inactive desks do not.                  |
| **Office local timezone**  | Single timezone used for “today”, date boundaries, and reminder scheduling (NFR-001). |
| **Working day**            | Monday–Friday; Saturday and Sunday are not bookable (BR-001.3).                       |




### 1.4 References


| Document                        | Location                                                                           |
| ------------------------------- | ---------------------------------------------------------------------------------- |
| BRD-001 — Employee Desk Booking | `inception/product/requirements/BRD-001-desk-booking.md`                           |
| Screen specs                    | `inception/design/screens/SCR-001-sign-in.md` … `SCR-007-notification-settings.md` |
| Design tokens                   | `inception/design/tokens.css`                                                      |
| Traceability manifest           | `knowledge/traceability/manifest.json`                                             |




### 1.5 Document overview

Section 2 describes the product context. Section 3 lists functional software requirements by feature area (traced to BRD REQ IDs). Section 4 covers user and external interfaces. Sections 5–8 cover NFRs, data, business rules, and validations. Appendix A maps every BRD requirement to this document and UI screens.

---



## 2. Overall description



### 2.1 Product perspective

The system is a standalone web application (no multi-tenant or multi-site routing in this release). It enforces booking policy server-side; the UI reflects availability and validation outcomes. Background jobs or scheduled tasks may be required for status transitions (**Completed**) and day-before reminder emails.

```
┌─────────────┐     HTTPS      ┌──────────────────────────┐
│   Browser   │ ◄────────────► │  Desk Booking Web App    │
│ (Employee / │                │  (UI + application API)  │
│   Admin)    │                └───────────┬──────────────┘
└─────────────┘                            │
                                           │ SMTP (TBD)
                                           ▼
                               ┌──────────────────────────┐
                               │  Email service           │
                               └──────────────────────────┘
                                           │
                               Browser Push API (opt-in)
```



### 2.2 Product functions


| #    | Function                                                   |
| ---- | ---------------------------------------------------------- |
| F-01 | User authentication and session management                 |
| F-02 | Employee desk availability and booking                     |
| F-03 | Employee booking history and self-service cancellation     |
| F-04 | Admin booking registry, filter, and cancel-on-behalf       |
| F-05 | Admin desk inventory (CRUD, activate/deactivate)           |
| F-06 | Admin user provisioning (CRUD, role, password reset)       |
| F-07 | Transactional email (confirm, cancel, day-before reminder) |
| F-08 | Optional browser push (book/cancel, opt-in)                |




### 2.3 User classes and characteristics


| User class | Characteristics                                                   | System access                                       |
| ---------- | ----------------------------------------------------------------- | --------------------------------------------------- |
| Employee   | Hybrid worker; books one desk per working day                     | SCR-001, SCR-002, SCR-003, SCR-007                  |
| Admin      | Office administrator                                              | All Employee screens plus SCR-004, SCR-005, SCR-006 |
| System     | Automated processes (status completion, reminders, notifications) | Background scheduling; no UI                        |


After sign-in, **Employee** users land on desk booking (SCR-002); **Admin** users land on admin bookings (SCR-004) per approved design.

### 2.4 Operating environment


| Item      | Requirement                                                                               |
| --------- | ----------------------------------------------------------------------------------------- |
| Client    | Modern web browser; mobile-responsive vs desktop-only: `TBD (owner: PO/client)` (NFR-004) |
| Transport | HTTPS in deployed environments (NFR-003)                                                  |
| Time      | Single office local timezone for all date logic (NFR-001)                                 |
| Email     | Outbound SMTP or transactional email provider: `TBD (owner: PO/IT)` (open question #7)    |
| Push      | W3C Push API where supported; graceful degradation (NFR-006)                              |




### 2.5 Design and implementation constraints

- Single office location only (NFR-002).
- Email/password authentication only; no SSO in this release.
- Desk and user master data maintained in-app by Admins (no external HR sync in scope).
- First Admin bootstrap mechanism: `TBD (owner: PO/Architect)` (open question #1).
- Public holiday calendar not defined; only Mon–Fri exclusion is guaranteed until open question #2 is resolved.
- Concurrent booking of the same desk must be prevented (RISK-004 — architecture/delivery concern).



### 2.6 Assumptions and dependencies

- Each user has a unique, valid email address used for sign-in and notifications.
- Office local timezone is configured once for the deployment.
- Email delivery failures are logged for operations (NFR-005); retry policy is an implementation detail.
- Browser push requires user permission; unsupported browsers rely on email only.

---



## 3. System features



### 3.1 Authentication and session (F-01)


| ID        | Software requirement                                                                                                                             | Priority | Traces to              |
| --------- | ------------------------------------------------------------------------------------------------------------------------------------------------ | -------- | ---------------------- |
| SRS-F-001 | The application shall provide a sign-in screen accepting email and password and shall establish an authenticated session on success.             | Must     | REQ-002, SCR-001       |
| SRS-F-002 | The application shall reject sign-in for invalid credentials with a generic error (no account enumeration).                                      | Must     | REQ-002, SCR-001 ST-03 |
| SRS-F-003 | The application shall reject sign-in for **deactivated** user accounts.                                                                          | Must     | REQ-005, SCR-001 ST-04 |
| SRS-F-004 | The application shall provide sign-out, terminating the session.                                                                                 | Must     | REQ-003                |
| SRS-F-005 | The application shall store exactly one role per user (**Employee** or **Admin**) and shall enforce role-based access on every protected action. | Must     | REQ-004                |




### 3.2 Employee desk booking (F-02, F-03)


| ID        | Software requirement                                                                                                                                            | Priority | Traces to                     |
| --------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------- | ----------------------------- |
| SRS-F-010 | The application shall allow an Employee to select a booking date from **today** through **today + 30 calendar days**, evaluated in the office local timezone.   | Must     | REQ-006, SCR-002              |
| SRS-F-011 | The application shall reject dates outside that window and non-working days (Sat/Sun).                                                                          | Must     | REQ-006, BR-001.3, V-02, V-03 |
| SRS-F-012 | For a valid selected date, the application shall display **Active** desks with unique desk numbers and availability (available vs booked).                      | Must     | REQ-007, SCR-002              |
| SRS-F-013 | The application shall allow an Employee to create a booking assigning exactly one available desk to themselves for one date, with initial status **Confirmed**. | Must     | REQ-008, SCR-002              |
| SRS-F-014 | The application shall reject booking if the Employee already has a **Confirmed** booking for that date (BR-001.1).                                              | Must     | REQ-008, V-05                 |
| SRS-F-015 | The application shall reject booking if the desk is already **Confirmed** for another user on that date (V-04).                                                 | Must     | REQ-008                       |
| SRS-F-016 | The application shall reject booking against **Inactive** desks (BR-001.7).                                                                                     | Must     | REQ-007, REQ-008              |
| SRS-F-017 | The application shall not support in-place desk change; changing desk requires cancel-then-book (BR-001.2).                                                     | Must     | REQ-008, REQ-010              |
| SRS-F-018 | The application shall list the signed-in Employee's bookings (past and future) with status.                                                                     | Must     | REQ-009, SCR-003              |
| SRS-F-019 | The application shall allow an Employee to cancel their own **Confirmed** booking for today or a future date; past dates are not cancellable.                   | Must     | REQ-010, SCR-003, BR-001.6    |
| SRS-F-020 | On cancellation, the application shall set booking status to **Cancelled**.                                                                                     | Must     | REQ-010, BR-001.5             |




### 3.3 Admin booking oversight (F-04)


| ID        | Software requirement                                                                                                           | Priority | Traces to                  |
| --------- | ------------------------------------------------------------------------------------------------------------------------------ | -------- | -------------------------- |
| SRS-F-030 | The application shall allow an Admin to view all employees' bookings.                                                          | Must     | REQ-011, SCR-004           |
| SRS-F-031 | The application shall allow filtering bookings by date.                                                                        | Must     | REQ-012, SCR-004           |
| SRS-F-032 | The application shall allow filtering bookings by status (**Confirmed**, **Cancelled**, **Completed**).                        | Must     | REQ-013, SCR-004           |
| SRS-F-033 | The application shall allow an Admin to cancel an Employee's **Confirmed** booking for today or a future date on their behalf. | Must     | REQ-014, SCR-004, BR-001.6 |




### 3.4 Admin desk management (F-05)


| ID        | Software requirement                                                                                                                                                         | Priority | Traces to                    |
| --------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------- | ---------------------------- |
| SRS-F-040 | The application shall allow an Admin to add a desk with a unique desk number.                                                                                                | Must     | REQ-015, SCR-005             |
| SRS-F-041 | The application shall allow an Admin to edit a desk number subject to uniqueness (BR-001.8).                                                                                 | Must     | REQ-016, SCR-005             |
| SRS-F-042 | The application shall allow an Admin to set a desk **Active** or **Inactive**.                                                                                               | Must     | REQ-017, SCR-005             |
| SRS-F-043 | **Inactive** desks shall be excluded from employee availability and new bookings (BR-001.7).                                                                                 | Must     | REQ-017                      |
| SRS-F-044 | The application shall block deactivation when the desk has **Confirmed** bookings for today or future dates unless those bookings are cancelled in the same flow (BR-001.9). | Must     | REQ-017, SCR-005 ST-08, V-09 |




### 3.5 Admin user management (F-06)


| ID        | Software requirement                                                                                                                                     | Priority | Traces to                             |
| --------- | -------------------------------------------------------------------------------------------------------------------------------------------------------- | -------- | ------------------------------------- |
| SRS-F-050 | The application shall allow an Admin to create a user with email, name, role, and Admin-set initial password.                                            | Must     | REQ-018, SCR-006                      |
| SRS-F-051 | The application shall allow an Admin to edit a user's name and email with unique email validation (BR-001.10).                                           | Must     | REQ-019, SCR-006                      |
| SRS-F-052 | The application shall allow an Admin to deactivate a user; deactivated users cannot sign in.                                                             | Must     | REQ-020, REQ-005, SCR-006             |
| SRS-F-053 | The application shall allow an Admin to reset a user's password; the new password shall be shown once to the Admin (not emailed by default) (BR-001.12). | Must     | REQ-021, SCR-006                      |
| SRS-F-054 | The application shall allow an Admin to assign or change role between **Employee** and **Admin**.                                                        | Must     | REQ-022, SCR-006                      |
| SRS-F-055 | The application shall prevent deactivation or role change that would leave zero active **Admin** users (BR-001.11).                                      | Must     | REQ-020, REQ-022, SCR-006 ST-09, V-11 |




### 3.6 Notifications (F-07, F-08)


| ID        | Software requirement                                                                                                                                                                                                   | Priority | Traces to        |
| --------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------- | ---------------- |
| SRS-F-060 | On transition to **Confirmed**, the system shall send a confirmation email to the booking owner's account email (mandatory, no opt-out) (BR-001.13).                                                                   | Must     | REQ-023          |
| SRS-F-061 | On transition to **Cancelled**, the system shall send a cancellation email to the booking owner (BR-001.13).                                                                                                           | Must     | REQ-024          |
| SRS-F-062 | Emails shall include desk number and booking date (V-13).                                                                                                                                                              | Must     | REQ-023–REQ-025  |
| SRS-F-063 | For each **Confirmed** booking on a future **working day**, the system shall send one reminder email on the previous calendar day (office local timezone); default send time `TBD` — proposed 08:00 local (BR-001.14). | Must     | REQ-025          |
| SRS-F-064 | Reminder emails shall not be sent for **Cancelled**, **Completed**, or same-day bookings.                                                                                                                              | Must     | BR-001.14        |
| SRS-F-065 | The application shall provide notification settings for Employees to opt in/out of browser push; default is opt-out (BR-001.15).                                                                                       | Must     | REQ-026, SCR-007 |
| SRS-F-066 | When opted in, the system shall send browser push on **Confirmed** (book) and **Cancelled** for that Employee's bookings (V-014).                                                                                      | Must     | REQ-027          |
| SRS-F-067 | Day-before reminders shall be email only; no browser push for reminders (BR-001.16).                                                                                                                                   | Must     | REQ-025, REQ-027 |
| SRS-F-068 | Failed email sends shall be logged for operational follow-up (NFR-005).                                                                                                                                                | Must     | NFR-005          |




### 3.7 Booking lifecycle automation (F-01 extension)


| ID        | Software requirement                                                                                                                                    | Priority | Traces to |
| --------- | ------------------------------------------------------------------------------------------------------------------------------------------------------- | -------- | --------- |
| SRS-F-070 | The system shall transition **Confirmed** bookings to **Completed** after the booking date passes in office local time without cancellation (BR-001.5). | Must     | BR-001.5  |
| SRS-F-071 | Each booking shall be in exactly one status: **Confirmed**, **Cancelled**, or **Completed**.                                                            | Must     | BR-001.5  |


---



## 4. External interface requirements



### 4.1 User interfaces


| Screen  | Purpose                                   | Primary user    | Spec                                                        |
| ------- | ----------------------------------------- | --------------- | ----------------------------------------------------------- |
| SCR-001 | Sign in                                   | Employee, Admin | `inception/design/screens/SCR-001-sign-in.md`               |
| SCR-002 | Book a desk (availability + reserve)      | Employee        | `inception/design/screens/SCR-002-book-desk.md`             |
| SCR-003 | My bookings (list + cancel)               | Employee        | `inception/design/screens/SCR-003-my-bookings.md`           |
| SCR-004 | All bookings (admin list, filter, cancel) | Admin           | `inception/design/screens/SCR-004-admin-bookings.md`        |
| SCR-005 | Manage desks                              | Admin           | `inception/design/screens/SCR-005-manage-desks.md`          |
| SCR-006 | Manage users                              | Admin           | `inception/design/screens/SCR-006-manage-users.md`          |
| SCR-007 | Notification settings (push opt-in)       | Employee        | `inception/design/screens/SCR-007-notification-settings.md` |


Navigation: Employees use **Desk Availability · My Bookings**; Admins use **Desks · Users · All Bookings**. Approved visual design uses navy/green tokens and Desk Booking logo (`inception/design/assets/desk-booking-logo.png`).

### 4.2 Software interfaces


| Interface        | Description                                                  | Owner                       |
| ---------------- | ------------------------------------------------------------ | --------------------------- |
| Email (SMTP/API) | Outbound transactional mail for confirm, cancel, reminder    | `TBD (owner: PO/IT)`        |
| Web Push         | Browser push subscription and delivery for opt-in Employees  | Implementation per NFR-006  |
| Persistence      | Application database for users, desks, bookings, preferences | Architect (Gate 1 parallel) |




### 4.3 Communications interfaces


| Channel      | Events                                         | Recipients           | Mandatory                              |
| ------------ | ---------------------------------------------- | -------------------- | -------------------------------------- |
| Email        | Book confirmed, cancelled, day-before reminder | Booking owner        | Yes (except delivery failure handling) |
| Browser push | Book confirmed, cancelled                      | Opt-in Employee only | No                                     |


---



## 5. Non-functional requirements

Canonical definitions: **BRD-001 §5** (NFR-001 … NFR-006). The table below adds implementation notes only — it does **not** redefine BRD requirements.


| BRD reference     | Category      | Implementation note                                               | Priority |
| ----------------- | ------------- | ----------------------------------------------------------------- | -------- |
| BRD-001 / NFR-001 | Locale/time   | All booking dates and “today” boundary use office local timezone. | Must     |
| BRD-001 / NFR-002 | Scope         | Exactly one office location in this release.                      | Must     |
| BRD-001 / NFR-003 | Security      | Credentials protected in transit (HTTPS when deployed).           | Must     |
| BRD-001 / NFR-004 | Usability     | Desktop-only vs mobile-responsive: `TBD (owner: PO/client)`.      | Should   |
| BRD-001 / NFR-005 | Notifications | Transactional emails sent reliably; failures logged.              | Must     |
| BRD-001 / NFR-006 | Notifications | Push requires opt-in; unsupported browsers degrade to email only. | Must     |


---



## 6. Data requirements



### 6.1 Logical data model


| Entity                     | Key attributes                                                              | Relationships                                           |
| -------------------------- | --------------------------------------------------------------------------- | ------------------------------------------------------- |
| **User**                   | id, email (unique), name, password hash, role (Employee/Admin), active flag | Owns many Bookings; has optional NotificationPreference |
| **Desk**                   | id, desk number (unique), status (Active/Inactive)                          | Referenced by Bookings                                  |
| **Booking**                | id, user id, desk id, date (office local calendar date), status, timestamps | One user + one desk + one date per booking record       |
| **NotificationPreference** | user id, browser push opt-in flag, push subscription payload (if opted in)  | One per Employee user                                   |




### 6.2 Enumerations and lifecycles

**Booking status**

```
Confirmed ──cancel──► Cancelled
     │
     └── (date passes, office local) ──► Completed
```

**Desk status:** `Active` | `Inactive`

**User:** `active` | `deactivated` (deactivated ⇒ cannot sign in)

---



## 7. Business rules

Canonical statements and examples remain in **BRD-001 §6**. Implementers shall enforce:


| Rule ID   | Summary                                                  | Affects (REQ)    |
| --------- | -------------------------------------------------------- | ---------------- |
| BR-001.1  | One **Confirmed** booking per Employee per date          | REQ-008          |
| BR-001.2  | Change desk via cancel-then-book only                    | REQ-008, REQ-010 |
| BR-001.3  | Bookable dates Mon–Fri only                              | REQ-006, REQ-008 |
| BR-001.4  | Unique desk numbers                                      | REQ-007, REQ-008 |
| BR-001.5  | Status lifecycle Confirmed / Cancelled / Completed       | REQ-009–REQ-013  |
| BR-001.6  | Cancel only **Confirmed**, today or future               | REQ-010, REQ-014 |
| BR-001.7  | Inactive desks excluded from booking                     | REQ-007, REQ-017 |
| BR-001.8  | Desk number unique on add/edit                           | REQ-015, REQ-016 |
| BR-001.9  | Block desk deactivate with future **Confirmed** bookings | REQ-017          |
| BR-001.10 | User email unique                                        | REQ-018, REQ-019 |
| BR-001.11 | Cannot remove last active Admin                          | REQ-020, REQ-022 |
| BR-001.12 | Password reset shown once to Admin                       | REQ-021          |
| BR-001.13 | Mandatory booking emails                                 | REQ-023, REQ-024 |
| BR-001.14 | Day-before reminder email (working days)                 | REQ-025          |
| BR-001.15 | Browser push opt-in only                                 | REQ-026, REQ-027 |
| BR-001.16 | No push for reminders                                    | REQ-025, REQ-027 |


---



## 8. Validation rules


| ID   | Validation                                                      | Related                             |
| ---- | --------------------------------------------------------------- | ----------------------------------- |
| V-01 | Sign-in rejected for unknown credentials or deactivated account | REQ-002, REQ-005                    |
| V-02 | Date ≥ today and ≤ today + 30 (office TZ)                       | REQ-006                             |
| V-03 | Date is Mon–Fri                                                 | BR-001.3                            |
| V-04 | Desk available (not **Confirmed** by another user)              | REQ-008                             |
| V-05 | Employee has no other **Confirmed** booking same date           | BR-001.1                            |
| V-06 | Cancel only **Confirmed**, today or future                      | BR-001.6                            |
| V-07 | Admin-only actions require Admin role                           | REQ-004, REQ-011–022                |
| V-08 | Desk number unique on add/edit                                  | REQ-015, REQ-016                    |
| V-09 | Cannot deactivate desk with unresolved future bookings          | REQ-017                             |
| V-10 | User email unique on create/edit                                | REQ-018, REQ-019                    |
| V-11 | Cannot remove last active Admin                                 | REQ-020, REQ-022                    |
| V-12 | Password meets policy                                           | REQ-021, `TBD (owner: PO/security)` |
| V-13 | Emails include desk number and date                             | REQ-023–REQ-025                     |
| V-14 | Push only when opt-in                                           | REQ-026, REQ-027                    |


---



## 9. Open items


| #   | Item                                                               | Owner        | Status                  |
| --- | ------------------------------------------------------------------ | ------------ | ----------------------- |
| 1   | First Admin bootstrap (seed vs installer vs manual DB)             | PO/Architect | Open                    |
| 2   | Company holiday calendar                                           | PO/client    | Open                    |
| 3   | Day-before reminder send time (default 08:00 office local)         | PO/client    | Open                    |
| 4   | Mobile-responsive vs desktop-only UI                               | PO/client    | Open                    |
| 5   | Password complexity (V-12)                                         | PO/security  | Open                    |
| 6   | Desk deactivate with future bookings — block vs cancel-in-one-step | PO/client    | Open — default BR-001.9 |
| 7   | SMTP sender domain / service                                       | PO/IT        | Open                    |


---



## Appendix A — Requirement traceability


| BRD reference               | SRS coverage                    | UI screen                  |
| --------------------------- | ------------------------------- | -------------------------- |
| BRD-001 / REQ-001           | §2.1, §4.1                      | All SCR                    |
| BRD-001 / REQ-002           | SRS-F-001, SRS-F-002            | SCR-001                    |
| BRD-001 / REQ-003           | SRS-F-004                       | All authenticated SCR      |
| BRD-001 / REQ-004           | SRS-F-005                       | SCR-001, SCR-005, SCR-006  |
| BRD-001 / REQ-005           | SRS-F-003, SRS-F-052            | SCR-001, SCR-006           |
| BRD-001 / REQ-006           | SRS-F-010, SRS-F-011            | SCR-002                    |
| BRD-001 / REQ-007           | SRS-F-012, SRS-F-016            | SCR-002                    |
| BRD-001 / REQ-008           | SRS-F-013 … SRS-F-017           | SCR-002                    |
| BRD-001 / REQ-009           | SRS-F-018                       | SCR-003                    |
| BRD-001 / REQ-010           | SRS-F-019, SRS-F-020            | SCR-003                    |
| BRD-001 / REQ-011           | SRS-F-030                       | SCR-004                    |
| BRD-001 / REQ-012           | SRS-F-031                       | SCR-004                    |
| BRD-001 / REQ-013           | SRS-F-032                       | SCR-004                    |
| BRD-001 / REQ-014           | SRS-F-033                       | SCR-004                    |
| BRD-001 / REQ-015           | SRS-F-040                       | SCR-005                    |
| BRD-001 / REQ-016           | SRS-F-041                       | SCR-005                    |
| BRD-001 / REQ-017           | SRS-F-042 … SRS-F-044           | SCR-005                    |
| BRD-001 / REQ-018           | SRS-F-050                       | SCR-006                    |
| BRD-001 / REQ-019           | SRS-F-051                       | SCR-006                    |
| BRD-001 / REQ-020           | SRS-F-052, SRS-F-055            | SCR-006                    |
| BRD-001 / REQ-021           | SRS-F-053                       | SCR-006                    |
| BRD-001 / REQ-022           | SRS-F-054, SRS-F-055            | SCR-006                    |
| BRD-001 / REQ-023           | SRS-F-060, SRS-F-062            | (email — no dedicated SCR) |
| BRD-001 / REQ-024           | SRS-F-061, SRS-F-062            | (email)                    |
| BRD-001 / REQ-025           | SRS-F-063, SRS-F-064, SRS-F-067 | (email / scheduler)        |
| BRD-001 / REQ-026           | SRS-F-065                       | SCR-007                    |
| BRD-001 / REQ-027           | SRS-F-066, SRS-F-067            | SCR-007                    |
| BRD-001 / NFR-001 … NFR-006 | §5                              | Per manifest               |


