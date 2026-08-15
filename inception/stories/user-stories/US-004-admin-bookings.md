# US-004 — Admin view and cancel all bookings

> Approval = Gate 1 review of this file's PR. Delivery = the story PR (`feat/US-004-admin-bookings`) merging with every AC proven by a test named `... (US-004/AC-##)`.

|                |                                         |
| -------------- | --------------------------------------- |
| **Epic**       | EPIC-001                                |
| **Traces to**  | REQ-001, REQ-003, REQ-011, REQ-012, REQ-013, REQ-014, NFR-004, BR-001.6 |
| **Priority**   | Must                                    |
| **Estimate**   | 5 pts (AI draft — humans re-estimate)   |
| **Depends on** | US-001                                  |

## Story

As an **Admin**
I want to view and filter all employee bookings and cancel on their behalf when needed
So that I can support staff and manage office utilisation.

## Acceptance criteria

### AC-01 View all bookings

- **Given** a signed-in Admin
- **When** they open All Bookings
- **Then** they see bookings across all employees with date, desk, employee, and status (SCR-004 ST-01)

### AC-02 Filter by date

- **Given** the All Bookings list
- **When** the Admin applies a date filter
- **Then** only bookings matching that date are shown (REQ-012)

### AC-03 Filter by status

- **Given** the All Bookings list
- **When** the Admin filters by **Confirmed**, **Cancelled**, or **Completed**
- **Then** only bookings with that status are shown (REQ-013)

### AC-04 Cancel on behalf of employee

- **Given** a **Confirmed** booking for today or a future date
- **When** the Admin confirms cancel on behalf of the employee
- **Then** the booking becomes **Cancelled** (REQ-014, BR-001.6)

## Edge cases

- Admin cannot cancel past or **Completed** bookings — same rule as Employee.
- Empty filter results show appropriate empty state (SCR-004 ST-03).

## UI

Served by **SCR-004 — All Bookings**. Admin nav: Desks · Users · All Bookings.

## QA notes

Multi-employee seed data; verify Admin role required (V-07).

## API impacts

Admin list/filter/cancel booking endpoints — TBD in architecture.
