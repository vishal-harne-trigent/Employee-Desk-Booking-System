# US-004 — Admin all bookings

> Technical expansion of US-004. Story defines business need; this defines code behaviour.

|                   |                                                                  |
| ----------------- | ---------------------------------------------------------------- |
| **Story**         | `inception/stories/user-stories/US-004-admin-bookings.md`        |
| **Traces to**     | REQ-011, REQ-012, REQ-013, REQ-014, NFR-004, BR-001.6            |
| **Screen**        | SCR-004                                                          |
| **Covering ADRs** | none                                                             |
| **Tier**          | Complex                                                          |
| **Status**        | implemented                                                      |
| **Updated**       | 2026-08-31                                                       |

## Problem

Admins need to view all employee bookings with date, desk, employee, and status; filter by date and status; and cancel Confirmed bookings on an employee's behalf for today or future dates.

## Functional requirements

| ID    | Requirement | Priority | Serves | Status |
| ----- | ----------- | -------- | ------ | ------ |
| FR-01 | Admin sees all bookings with date, desk, employee, and status | Must | AC-01 | implemented |
| FR-02 | Date filter narrows the booking list | Must | AC-02 | implemented |
| FR-03 | Status filter (Confirmed / Cancelled / Completed) narrows the list | Must | AC-03 | implemented |
| FR-04 | Admin cancel of Confirmed today/future booking sets status to Cancelled | Must | AC-04 | implemented |

## Non-functional requirements

| ID     | Requirement | Serves |
| ------ | ----------- | ------ |
| NFR-01 | Admin all-bookings routes restricted to Admin role (V-07) | NFR-004 |

## Technical constraints

- Admin area MVC + matching Api controllers share `IBookingService`
- Same cancel date rules as employee cancel (BR-001.6)

## Out of scope

- Desk management → US-005
- User management → US-006
