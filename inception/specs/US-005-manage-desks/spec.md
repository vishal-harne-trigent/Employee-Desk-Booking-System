# US-005 — spec (delivery slice)

|           |                                                                  |
| --------- | ---------------------------------------------------------------- |
| **Story** | `inception/stories/user-stories/US-005-manage-desks.md`         |
| **Screen**| `inception/design/screens/SCR-005-manage-desks.md`              |
| **Tier**  | Medium                                                           |

## Functional requirements in scope

| ID      | Summary |
| ------- | ------- |
| REQ-015 | Admin adds desk with unique number |
| REQ-016 | Admin edits desk number (unique) |
| REQ-017 | Admin activates/deactivates desk; inactive excluded from booking |

## Non-functional requirements in scope

| ID      | Summary |
| ------- | ------- |
| NFR-004 | Admin-only access (V-07) |

## Validation rules in scope

| ID    | Summary |
| ----- | ------- |
| V-08  | Duplicate desk number rejected on add/edit |
| V-09  | Deactivate blocked when Confirmed bookings exist today or future |

## Acceptance criteria

| AC    | Summary |
| ----- | ------- |
| AC-01 | Add desk with unique number → Active, listed |
| AC-02 | Duplicate desk number → validation error (V-08) |
| AC-03 | Edit desk number to another unique value |
| AC-04 | Deactivate desk with no blocking bookings → Inactive, hidden from employee availability |
| AC-05 | Deactivate blocked when Confirmed today/future bookings exist (V-09, ST-08) |

## Out of scope

- Cancel bookings in same deactivate flow (open Q#6 — block only per BR-001.9)
- User management → US-006
- Email notifications on desk changes
