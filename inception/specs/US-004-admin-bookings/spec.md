# US-004 — spec (delivery slice)

|           |                                                                  |
| --------- | ---------------------------------------------------------------- |
| **Story** | `inception/stories/user-stories/US-004-admin-bookings.md`        |
| **Screen**| `inception/design/screens/SCR-004-admin-bookings.md`             |
| **Tier**  | Medium                                                           |

## Functional requirements in scope

| ID      | Summary |
| ------- | ------- |
| REQ-011 | Admin views all employee bookings |
| REQ-012 | Filter bookings by date |
| REQ-013 | Filter bookings by status |
| REQ-014 | Admin cancels Confirmed today/future booking on employee's behalf |

## Non-functional requirements in scope

| ID      | Summary |
| ------- | ------- |
| NFR-004 | Admin-only access (V-07) |

## Acceptance criteria

| AC    | Summary |
| ----- | ------- |
| AC-01 | All bookings listed with date, desk, employee, status |
| AC-02 | Date filter narrows results |
| AC-03 | Status filter narrows results |
| AC-04 | Admin cancel → Cancelled |

## Out of scope

- Desk management → US-005
- User management → US-006
