# BRD-001 — Employee Desk Booking

> Approval = the PO/BA human reviewing + merging this document's PR (Gate 1). No approval headers — GitHub records who approved what.

|                  |                                                                                  |
| ---------------- | -------------------------------------------------------------------------------- |
| **Author**       | BA persona (AI draft) with PO/BA human                                           |
| **Source input** | `inception/product/inputs/2026-08-13-client-discussion.md` (verbatim raw material) |
| **Related**      | EPIC-001 (filled when stories are drafted after design approval)                 |

## 1. Business goal

Provide a web application so employees at a single hybrid office can reserve a specific desk before coming in, and so administrators can oversee and manage bookings across the office. The outcome is predictable desk availability, reduced uncertainty for in-office days, and operational visibility for admins.

## 2. Actors

| Actor    | Description                                      | Needs                                                                 |
| -------- | ------------------------------------------------ | --------------------------------------------------------------------- |
| Employee | Staff member who works hybrid and books a desk   | Sign in, see availability, book one desk per day, view/cancel bookings |
| Admin    | Office administrator                             | Sign in, view all bookings (filtered), cancel bookings on behalf of employees |
| System   | Desk booking application                         | Enforce business rules, roles, booking lifecycle, and date constraints |

## 3. Workflows

1. **Employee books a desk:** Employee signs in → selects a date (today through +30 days, working day, office timezone) → views desks by unique number with availability → selects one available desk → booking is created with status **Confirmed**.
2. **Employee reviews bookings:** Employee opens own bookings list → sees past and future bookings with status → can cancel today or future **Confirmed** bookings → cancelled bookings become **Cancelled**.
3. **Employee changes desk:** Employee cancels the existing booking for that date → books a different available desk for the same date (no direct desk swap on an existing booking).
4. **Admin monitors bookings:** Admin signs in → views all bookings → filters by date and/or status → can cancel a **Confirmed** booking on behalf of an employee for today or a future date.
5. **Booking completes:** When a **Confirmed** booking date passes in office local time without cancellation, status becomes **Completed**.

## 4. Functional requirements

> Each REQ is testable (pass/fail decidable), prioritized MoSCoW, and sourced (input file or named person).

| ID      | Requirement | Priority | Source |
| ------- | ----------- | -------- | ------ |
| REQ-001 | The product is a browser-based web application for desk booking at one office location. | Must | `2026-08-13-client-discussion.md` |
| REQ-002 | A user can sign in with email and password. | Must | `2026-08-13-client-discussion.md` |
| REQ-003 | A signed-in user can sign out. | Must | `2026-08-13-client-discussion.md` |
| REQ-004 | The system assigns each user exactly one role: **Employee** or **Admin**. | Must | `2026-08-13-client-discussion.md` |
| REQ-005 | A user marked deactivated cannot sign in. | Must | `2026-08-13-client-discussion.md` |
| REQ-006 | An Employee can select a booking date from today through 30 calendar days ahead, calculated in the office local timezone. | Must | `2026-08-13-client-discussion.md` |
| REQ-007 | For a selected date, an Employee can view desk availability where each desk is identified by a unique desk number (e.g. A-01, B-02). | Must | `2026-08-13-client-discussion.md`, PO/BA interview |
| REQ-008 | An Employee can book exactly one available desk for one selected date. | Must | `2026-08-13-client-discussion.md` |
| REQ-009 | An Employee can view a list of their own bookings, including past and future dates. | Must | `2026-08-13-client-discussion.md` |
| REQ-010 | An Employee can cancel their own booking for today or a future date; past bookings cannot be cancelled by the Employee. | Must | `2026-08-13-client-discussion.md`, PO/BA interview |
| REQ-011 | An Admin can view all bookings across employees. | Must | `2026-08-13-client-discussion.md` |
| REQ-012 | An Admin can filter all bookings by date. | Must | `2026-08-13-client-discussion.md` |
| REQ-013 | An Admin can filter all bookings by status (**Confirmed**, **Cancelled**, or **Completed**). | Must | `2026-08-13-client-discussion.md`, PO/BA interview |
| REQ-014 | An Admin can cancel an Employee's booking on their behalf for today or a future date; past bookings cannot be cancelled by the Admin. | Must | `2026-08-13-client-discussion.md`, PO/BA interview |

## 5. Non-functional requirements

| ID      | Category    | Requirement (quantified or `TBD (owner)`) | Priority |
| ------- | ----------- | ------------------------------------------- | -------- |
| NFR-001 | Locale/time | All booking dates and the "today" boundary use the office local timezone. | Must |
| NFR-002 | Scope       | The application supports exactly one office location in this release. | Must |
| NFR-003 | Security    | Sign-in credentials are protected in transit (HTTPS in deployed environments). | Must |
| NFR-004 | Usability   | Target device support (desktop-only vs mobile-responsive web): `TBD (owner: PO/client)` | Should |

## 6. Business rules

### BR-001.1 One desk per employee per working day

- **Statement:** When an Employee attempts to create a booking, the system must reject the request if that Employee already has a **Confirmed** booking for the same calendar date (office local timezone).
- **Rationale:** Prevents double-booking and matches hybrid office policy of one seat per person per day.
- **Examples:** Pass — Employee with no booking on 2026-08-20 books desk A-01. Fail — Employee with **Confirmed** booking on 2026-08-20 attempts to book desk B-02 the same date.
- **Affects:** REQ-008

### BR-001.2 Change desk by cancel-then-book

- **Statement:** When an Employee wants a different desk on a date they already booked, the system must require cancellation of the existing **Confirmed** booking before a new booking for that date can be created.
- **Rationale:** Client chose explicit cancel-then-book over in-place desk changes.
- **Examples:** Pass — Employee cancels A-01 for Tuesday, then books B-02 for Tuesday. Fail — Employee attempts to change A-01 to B-02 on the same booking record without cancelling.
- **Affects:** REQ-008, REQ-010

### BR-001.3 Working-day booking window

- **Statement:** When an Employee or Admin selects or creates a booking date, the system must allow only Monday–Friday dates; Saturday and Sunday are not bookable.
- **Rationale:** Hybrid office operates on standard working days; weekends are out of scope for booking.
- **Examples:** Pass — booking created for a Wednesday. Fail — booking attempted for a Saturday within the +30-day window.
- **Affects:** REQ-006, REQ-008

### BR-001.4 Unique desk numbers

- **Statement:** When desks are presented for booking, each desk must display a unique identifier in the form of an alphanumeric desk number (e.g. A-01, B-02, C-05).
- **Rationale:** Employees choose a specific desk; labels must be unambiguous.
- **Examples:** Pass — availability list shows "A-01" and "B-02" as distinct selectable desks. Fail — two desks share the same displayed number.
- **Affects:** REQ-007, REQ-008

### BR-001.5 Booking status lifecycle

- **Statement:** Every booking must be in exactly one status: **Confirmed** (active future or current-day reservation), **Cancelled** (voided before use), or **Completed** (the booking date has passed without cancellation).
- **Rationale:** Admin filtering and reporting depend on a shared status vocabulary agreed with the client.
- **Examples:** Pass — past **Confirmed** booking automatically shown as **Completed** after the date. Fail — booking remains **Confirmed** indefinitely after the date passes.
- **Affects:** REQ-009, REQ-011, REQ-012, REQ-013

### BR-001.6 Cancellation eligibility

- **Statement:** When a user (Employee or Admin) attempts to cancel a booking, the system must allow cancellation only if the booking date is today or in the future (office local timezone) and the current status is **Confirmed**; past-date **Confirmed** or **Completed** bookings cannot be cancelled.
- **Rationale:** Aligns employee and admin cancellation rules from client clarification.
- **Examples:** Pass — Admin cancels Employee's booking for tomorrow. Fail — Employee cancels a booking dated yesterday.
- **Affects:** REQ-010, REQ-014

## 7. Validations

| Validation | Rule | Related |
| ---------- | ---- | ------- |
| V-01 | Sign-in rejected for unknown credentials or deactivated account | REQ-002, REQ-005 |
| V-02 | Selected date must be ≥ today and ≤ today + 30 days (office local timezone) | REQ-006 |
| V-03 | Selected date must be a working day (Mon–Fri) | BR-001.3 |
| V-04 | Selected desk must be available (not **Confirmed** by another user) for that date | REQ-008 |
| V-05 | Employee must not already hold a **Confirmed** booking for the same date | BR-001.1 |
| V-06 | Cancellation only on **Confirmed** bookings for today or future dates | BR-001.6 |
| V-07 | Admin-only actions require **Admin** role | REQ-004, REQ-011–REQ-014 |

## 8. Constraints

- Single office location only (no multi-site routing or selection).
- Email/password authentication only; no SSO or social login in this release.
- Desk inventory and user accounts must exist before booking is possible — provisioning mechanism is an open question (see §11).
- Company public holidays are not yet defined in scope — until resolved, only weekend exclusion (BR-001.3) is guaranteed.

## 9. Risks

| ID       | Risk | Likelihood | Impact | Mitigation |
| -------- | ---- | ---------- | ------ | ---------- |
| RISK-001 | Desk and user provisioning approach undecided blocks end-to-end delivery. | High | High | Resolve open question #1 before Gate 2; Architect may propose options in parallel. |
| RISK-002 | Holiday calendar undefined — employees may book on company holidays. | Medium | Medium | Resolve open question #2; interim Mon–Fri rule documented in BR-001.3. |
| RISK-003 | No self-service password reset increases IT support load. | Medium | Low | Document as out of scope; revisit if client priority changes. |
| RISK-004 | Concurrent booking of the same desk could cause double-booking without proper locking. | Low | High | Address in architecture/delivery (not a BA design decision). |

## 10. Out of scope

- Forgot-password / self-service password reset (client confirmed for current stage).
- Email or push notifications for booking confirmation or cancellation (not yet discussed — tracked as open question, not assumed in).
- In-app admin screens for managing desk inventory or user accounts (client listed admin responsibility but provided no functional requirements — provisioning open question).
- Booking more than one desk per employee per day.
- In-place desk swap without cancellation.
- Multi-office or multi-location support.
- Weekend desk booking (Saturday/Sunday).
- Visitor desk booking on behalf of others by Employees (one desk per employee per day only).

## 11. Open questions

| #   | Question | Owner | Status |
| --- | -------- | ----- | ------ |
| 1   | How are desk inventory and user accounts created and maintained — in-app admin features vs one-time/operational setup outside the app? | PO/client | Open |
| 2   | How is the company holiday calendar defined and maintained so working-day rules exclude public holidays? | PO/client | Open |
| 3   | Should the system send email (or other) notifications on booking confirmation or cancellation? | PO/client | Open |
| 4   | Must the web UI support mobile browsers in this release, or desktop-only? | PO/client | Open |
