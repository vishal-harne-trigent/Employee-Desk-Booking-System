# US-005 — Manage desks

> Technical expansion of US-005. Story defines business need; this defines code behaviour.

|                   |                                                                  |
| ----------------- | ---------------------------------------------------------------- |
| **Story**         | `inception/stories/user-stories/US-005-manage-desks.md`          |
| **Traces to**     | REQ-015, REQ-016, REQ-017, NFR-004, V-08, V-09, BR-001.9         |
| **Screen**        | SCR-005                                                          |
| **Covering ADRs** | none                                                             |
| **Tier**          | Complex                                                          |
| **Status**        | implemented                                                      |
| **Updated**       | 2026-08-31                                                       |

## Problem

Admins need to add desks with unique numbers, edit desk numbers, and activate or deactivate desks. Inactive desks must be excluded from employee availability. Deactivation is blocked when Confirmed bookings exist for today or future dates.

## Functional requirements

| ID    | Requirement | Priority | Serves | Status |
| ----- | ----------- | -------- | ------ | ------ |
| FR-01 | Admin adds desk with unique number; desk is Active and listed | Must | AC-01 | implemented |
| FR-02 | Duplicate desk number on add or edit returns validation error (V-08) | Must | AC-02 | implemented |
| FR-03 | Admin edits desk number to another unique value | Must | AC-03 | implemented |
| FR-04 | Deactivate desk with no blocking bookings sets Inactive and hides from availability | Must | AC-04 | implemented |
| FR-05 | Deactivate blocked when Confirmed today/future bookings exist (V-09) | Must | AC-05 | implemented |

## Non-functional requirements

| ID     | Requirement | Serves |
| ------ | ----------- | ------ |
| NFR-01 | Manage desks routes restricted to Admin role (V-07) | NFR-004 |

## Technical constraints

- `IDeskService` in Application; inactive desks excluded in `GetAvailabilityAsync` (US-002)
- Unique desk number enforced at persistence layer

## Out of scope

- Cancel bookings in same deactivate flow (block only per BR-001.9)
- User management → US-006
- Email notifications on desk changes
