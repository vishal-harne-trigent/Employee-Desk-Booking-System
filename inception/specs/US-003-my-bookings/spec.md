# US-003 — spec (delivery slice)

|           |                                                                  |
| --------- | ---------------------------------------------------------------- |
| **Story** | `inception/stories/user-stories/US-003-my-bookings.md`           |
| **Screen**| `inception/design/screens/SCR-003-my-bookings.md`                |
| **Tier**  | Medium                                                           |

## Functional requirements in scope

| ID      | Summary |
| ------- | ------- |
| REQ-009 | Employee lists all their bookings with date, desk, status |
| REQ-010 | Employee cancels **Confirmed** booking for today or future |

## Non-functional requirements in scope

| ID      | Summary |
| ------- | ------- |
| NFR-004 | Employee-only access on my-bookings routes |

## Acceptance criteria

| AC    | Summary |
| ----- | ------- |
| AC-01 | List shows date, desk number, status (Confirmed / Cancelled / Completed) |
| AC-02 | Cancel today/future Confirmed → Cancelled |
| AC-03 | Past or Completed — no cancel action |
| AC-04 | Empty state with link to Book Desk |

## Out of scope

- Auto **Completed** transition job → US-009 (tests may seed Completed rows)
- Admin cancel → US-004
