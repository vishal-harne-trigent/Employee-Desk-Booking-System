# US-002 — Book a desk

> Technical expansion of US-002. Story defines business need; this defines code behaviour.

|                   |                                                                  |
| ----------------- | ---------------------------------------------------------------- |
| **Story**         | `inception/stories/user-stories/US-002-book-desk.md`             |
| **Traces to**     | REQ-006, REQ-007, REQ-008, NFR-001, NFR-002, NFR-004, BR-001.1, BR-001.2, BR-001.3, BR-001.4, BR-001.7 |
| **Screen**        | SCR-002                                                          |
| **Covering ADRs** | none                                                             |
| **Tier**          | Complex                                                          |
| **Status**        | implemented                                                      |
| **Updated**       | 2026-08-31                                                       |

## Problem

Signed-in Employees need to pick a working day within the booking window, see which active desks are available, and create one Confirmed booking per day. Invalid dates, inactive desks, and double-booking must be rejected.

## Functional requirements

| ID    | Requirement | Priority | Serves | Status |
| ----- | ----------- | -------- | ------ | ------ |
| FR-01 | Valid date within today→+30 working-day window loads desk availability | Must | AC-01 | implemented |
| FR-02 | Dates before today, after +30, or on weekends are rejected | Must | AC-02 | implemented |
| FR-03 | Each active desk shows unique desk number and available/booked state | Must | AC-03 | implemented |
| FR-04 | Confirming an available desk creates a Confirmed booking for that employee, desk, and date | Must | AC-04 | implemented |
| FR-05 | Employee with an existing Confirmed booking on the date cannot book another desk | Must | AC-05 | implemented |
| FR-06 | Inactive desks and desks already Confirmed for another user are not bookable | Must | AC-06 | implemented |

## Non-functional requirements

| ID     | Requirement | Serves |
| ------ | ----------- | ------ |
| NFR-01 | Office local timezone for “today” and date boundaries | NFR-001 |
| NFR-02 | Availability query completes within acceptable response time | NFR-002 |
| NFR-03 | Book desk routes restricted to signed-in Employee role | NFR-004 |

## Technical constraints

- `IBookingService` in Application; Web and Api controllers delegate to it
- `IOfficeClock` supplies office-local dates for validation and queries
- One Confirmed booking per employee per calendar day (BR-001.1)

## Out of scope

- Cancel / change desk same day → US-003
- My Bookings list → US-003
- Admin desk management → US-005
