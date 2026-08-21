# US-002 — spec (delivery slice)

> Medium tier: story + SCR-002 + architecture docs are authoritative; this file satisfies `aidlc-check` check 16.

|           |                                                                  |
| --------- | ---------------------------------------------------------------- |
| **Story** | `inception/stories/user-stories/US-002-book-desk.md`             |
| **Screen**| `inception/design/screens/SCR-002-book-desk.md`                  |
| **Tier**  | Medium                                                           |

## Functional requirements in scope

| ID      | Summary |
| ------- | ------- |
| REQ-006 | Employee selects a date within the booking window (today → +30, working days) |
| REQ-007 | Active desks listed by unique desk number with availability |
| REQ-008 | Employee books one available desk → Confirmed booking |

## Non-functional requirements in scope

| ID      | Summary |
| ------- | ------- |
| NFR-001 | Office local timezone for “today” and date boundaries |
| NFR-002 | Availability loads within acceptable response time (integration-tested) |
| NFR-004 | Role-based access — Employee only on book desk routes |

## Acceptance criteria

| AC    | Summary |
| ----- | ------- |
| AC-01 | Valid date loads desk availability |
| AC-02 | Invalid/weekend/out-of-window dates rejected |
| AC-03 | Active desks show number + available/booked |
| AC-04 | Confirm booking creates Confirmed record |
| AC-05 | One booking per employee per day enforced |
| AC-06 | Inactive or taken desks rejected |

## Out of scope (later stories)

- Cancel / change desk same day → US-003
- My Bookings list → US-003
- Admin desk management → US-005
