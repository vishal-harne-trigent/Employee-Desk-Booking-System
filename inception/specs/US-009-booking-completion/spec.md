# US-009 — spec (delivery slice)

|           |                                                                  |
| --------- | ---------------------------------------------------------------- |
| **Story** | `inception/stories/user-stories/US-009-booking-completion.md` |
| **Tier**  | Complex                                                          |

## Functional requirements in scope

| ID      | Summary |
| ------- | ------- |
| REQ-009 | Employee booking lists show accurate status including **Completed** |
| REQ-011 | Admin booking lists show accurate status |
| REQ-013 | Admin status filter includes **Completed** after date passes |

## Business rules in scope

| ID       | Summary |
| -------- | ------- |
| BR-001.5 | Confirmed → Completed after booking date passes (office local) |

## Acceptance criteria

| AC    | Summary |
| ----- | ------- |
| AC-01 | Past Confirmed bookings become Completed when job runs |
| AC-02 | Cancelled bookings are unchanged |
| AC-03 | Today's Confirmed bookings stay Confirmed |

## Out of scope

- New UI or API endpoints
- Notifications on completion
- Cancelling Completed bookings (BR-001.6 — already enforced)
