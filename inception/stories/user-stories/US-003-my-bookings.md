# US-003 — View and cancel my bookings

> Approval = Gate 1 review of this file's PR. Delivery = the story PR (`feat/US-003-my-bookings`) merging with every AC proven by a test named `... (US-003/AC-##)`.

|                |                                         |
| -------------- | --------------------------------------- |
| **Epic**       | EPIC-001                                |
| **Traces to**  | REQ-001, REQ-003, REQ-009, REQ-010, NFR-004, BR-001.5, BR-001.6 |
| **Priority**   | Must                                    |
| **Estimate**   | 5 pts (AI draft — humans re-estimate)   |
| **Depends on** | US-001, US-002                          |

## Story

As an **Employee**
I want to see all my desk bookings and cancel upcoming ones
So that I can manage my hybrid schedule and free a desk if plans change.

## Acceptance criteria

### AC-01 List my bookings

- **Given** a signed-in Employee with past and future bookings
- **When** they open My Bookings
- **Then** they see their bookings with date, desk number, and status (**Confirmed**, **Cancelled**, or **Completed**) (SCR-003 ST-01)

### AC-02 Cancel a Confirmed booking for today or future

- **Given** a **Confirmed** booking dated today or in the future (office local timezone)
- **When** the Employee confirms cancellation
- **Then** the booking status becomes **Cancelled** (BR-001.6)

### AC-03 Cannot cancel past bookings

- **Given** a booking dated before today or status **Completed**
- **When** the Employee views My Bookings
- **Then** no cancel action is offered (V-06)

### AC-04 Empty state

- **Given** an Employee with no bookings
- **When** they open My Bookings
- **Then** an empty state is shown with a path to Book Desk (SCR-003 ST-03)

## Edge cases

- Cancel confirmation modal before status change (SCR-003 ST-05).
- **Completed** status displayed for past dates after US-009 job runs.

## UI

Served by **SCR-003 — My Bookings**. Status badges use icon + label, not colour alone.

## QA notes

Seed bookings in Confirmed, Cancelled, and Completed states across date ranges.

## API impacts

List employee bookings and cancel booking endpoints — TBD in architecture.
