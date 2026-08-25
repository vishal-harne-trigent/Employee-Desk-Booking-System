# US-006 — spec (delivery slice)

|           |                                                                  |
| --------- | ---------------------------------------------------------------- |
| **Story** | `inception/stories/user-stories/US-006-manage-users.md`         |
| **Screen**| `inception/design/screens/SCR-006-manage-users.md`              |
| **Tier**  | Medium                                                           |

## Functional requirements in scope

| ID      | Summary |
| ------- | ------- |
| REQ-018 | Admin creates user with email, name, role, initial password |
| REQ-019 | Admin edits user name and email (unique) |
| REQ-020 | Admin deactivates user |
| REQ-021 | Admin resets password; shown once to Admin |
| REQ-022 | Admin changes user role (Employee ↔ Admin) |
| REQ-005 | Deactivated user cannot sign in |

## Non-functional requirements in scope

| ID      | Summary |
| ------- | ------- |
| NFR-004 | Admin-only access (V-07) |

## Validation rules in scope

| ID    | Summary |
| ----- | ------- |
| V-10  | Duplicate email rejected on create/edit |
| V-11  | Last active Admin cannot be deactivated or demoted |

## Acceptance criteria

| AC    | Summary |
| ----- | ------- |
| AC-01 | Create user → can sign in |
| AC-02 | Duplicate email → validation error (V-10) |
| AC-03 | Edit name and email |
| AC-04 | Deactivate user → cannot sign in |
| AC-05 | Reset password → one-time display to Admin |
| AC-06 | Change role Employee ↔ Admin |
| AC-07 | Block deactivate/demote of last active Admin (V-11, ST-09) |

## Out of scope

- Password complexity rule V-12 (TBD by PO/security) — use non-empty admin-set password on create; generated reset password uses secure random string
- First Admin bootstrap / installer seed
- User reactivation (no AC — deactivate only)
