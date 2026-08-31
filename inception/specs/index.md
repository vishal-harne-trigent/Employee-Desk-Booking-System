# Spec index

Every development spec package in this repo. **Check here before creating a new folder** — the capability may already have one, and a change to it is a revision of that package, not a second spec.

| Story | Feature | Tier | Status | Folder |
| ----- | ------- | ---- | ------ | ------ |
| US-001 | Sign in and sign out | Complex | implemented | [`inception/specs/US-001-sign-in/`](US-001-sign-in/) |
| US-002 | Book a desk | Complex | implemented | [`inception/specs/US-002-book-desk/`](US-002-book-desk/) |
| US-003 | My bookings | Medium | implemented | [`inception/specs/US-003-my-bookings/`](US-003-my-bookings/) |
| US-004 | Admin all bookings | Complex | implemented | [`inception/specs/US-004-admin-bookings/`](US-004-admin-bookings/) |
| US-005 | Manage desks | Complex | implemented | [`inception/specs/US-005-manage-desks/`](US-005-manage-desks/) |
| US-006 | Manage users | Complex | implemented | [`inception/specs/US-006-manage-users/`](US-006-manage-users/) |
| US-007 | Booking email notifications | Complex | implemented | [`inception/specs/US-007-booking-emails/`](US-007-booking-emails/) |
| US-008 | Push notifications | Complex | implemented | [`inception/specs/US-008-push-notifications/`](US-008-push-notifications/) |
| US-009 | Auto-complete past bookings | Medium | implemented | [`inception/specs/US-009-booking-completion/`](US-009-booking-completion/) |

## How to update

- Add a row when you create `inception/specs/US-###-<slug>/` (DEV, at Gate D1)
- Move Status to `implemented` when the story PR merges
- Simple-tier changes own no folder — they record one row in `_change-log.md` instead
