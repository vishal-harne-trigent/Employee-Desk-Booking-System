# Spec index

Every development spec package in this repo. **Check here before creating a new folder** — the capability may already have one, and a change to it is a revision of that package, not a second spec.

| Story | Feature | Tier | Status | Folder |
| ----- | ------- | ---- | ------ | ------ |
| US-001 | Sign in and sign out | Medium | implemented | [`US-001-sign-in/`](US-001-sign-in/) |
| US-002 | Book a desk | Medium | implemented | [`US-002-book-desk/`](US-002-book-desk/) |
| US-003 | View and cancel my bookings | Medium | implemented | [`US-003-my-bookings/`](US-003-my-bookings/) |
| US-004 | Admin view and cancel all bookings | Medium | implemented | [`US-004-admin-bookings/`](US-004-admin-bookings/) |
| US-005 | Admin manage desks | Medium | in delivery | [`US-005-manage-desks/`](US-005-manage-desks/) |

## How to update

- Add a row when you create `inception/specs/US-###-<slug>/` (DEV, at Gate D1)
- Move Status to `implemented` when the story PR merges
- Simple-tier changes own no folder — they record one row in `_change-log.md` instead
