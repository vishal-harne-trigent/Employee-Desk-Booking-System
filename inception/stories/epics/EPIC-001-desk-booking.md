# EPIC-001 — Employee Desk Booking

> Approval = Gate 1 review of the stories PR. This epic groups all user stories for BRD-001 / SRS-001.

|                  |                                                                                  |
| ---------------- | -------------------------------------------------------------------------------- |
| **Traces to**    | BRD-001, SRS-001                                                                 |
| **Goal**         | Hybrid employees book desks; admins oversee bookings, desks, and users           |
| **Stories**      | US-001 … US-009 ([`STORIES-001-desk-booking.md`](STORIES-001-desk-booking.md)) |
| **Delivery plan**| `inception/stories/delivery-plan-EPIC-001.md` (locked 2026-08-27; single sprint) |

## Scope

Single-office web application: authentication, employee booking, admin operations, email notifications, and optional browser push.

## Out of scope

Per BRD-001 §10 (SSO, self-service password reset, multi-office, weekend booking, etc.).

## Delivery order

See **`delivery-plan-EPIC-001.md`** for agent-runtime estimates, phases, and risks. Summary (single sprint — supersedes Sprints 0–4):

1. **Phase A** — Foundation (architecture, CI, exit criteria)  
2. **Phase B** — US-001, US-002  
3. **Phase C** — US-003, US-009  
4. **Phase D** — US-004, US-005, US-006  
5. **Phase E** — US-007, US-008 (MVP complete; ~21–31 h total agent runtime)
