# US-003 — My bookings

> Technical expansion of US-003. Story defines business need; this defines code behaviour.

|                   |                                                                  |
| ----------------- | ---------------------------------------------------------------- |
| **Story**         | `inception/stories/user-stories/US-003-my-bookings.md`           |
| **Traces to**     | REQ-009, REQ-010, NFR-004, BR-001.5, BR-001.6                    |
| **Screen**        | SCR-003                                                          |
| **Covering ADRs** | none                                                             |
| **Tier**          | Medium                                                           |
| **Status**        | implemented                                                      |
| **Updated**       | 2026-08-31                                                       |

## Problem

Employees need to see all their bookings with date, desk, and status, and cancel Confirmed bookings for today or future working days. Past and Completed bookings must not offer cancel.

## Functional requirements

| ID    | Requirement | Priority | Serves | Status |
| ----- | ----------- | -------- | ------ | ------ |
| FR-01 | Employee sees all their bookings with date, desk number, and status (Confirmed / Cancelled / Completed) | Must | AC-01 | implemented |
| FR-02 | Cancelling a Confirmed booking dated today or future sets status to Cancelled | Must | AC-02 | implemented |
| FR-03 | Past-dated or Completed bookings show no cancel action | Must | AC-03 | implemented |
| FR-04 | Employee with no bookings sees empty state with link to Book Desk | Must | AC-04 | implemented |

## Non-functional requirements

| ID     | Requirement | Serves |
| ------ | ----------- | ------ |
| NFR-01 | My bookings routes restricted to signed-in Employee role | NFR-004 |

## Technical constraints

- Reuses `IBookingService` from US-002 for list and cancel operations
- Cancel uses same office-local date rules as booking (BR-001.6)

## Out of scope

- Auto **Completed** transition job → US-009 (tests may seed Completed rows)
- Admin cancel → US-004
