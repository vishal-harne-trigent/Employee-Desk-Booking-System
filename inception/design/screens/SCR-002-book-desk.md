# SCR-002 — Book a Desk

> Approval = Gate 1 review of this file's PR. A state is "designed" when a component preview renders it and marks it `<!-- @state SCR-002/ST-## -->`.

|                  |                                                      |
| ---------------- | ---------------------------------------------------- |
| **Traces to**    | REQ-006, REQ-007, REQ-008, NFR-001, NFR-002          |
| **Surface**      | `apps/ui` — `/book` (Employee home)                  |
| **Primary user** | Employee                                             |
| **Status**       | draft — awaiting designer review                     |

## Purpose

Let an Employee pick a working day within the booking window, see which desks (by unique number) are free, and reserve exactly one desk for that date.

## Layout

Navy top navigation (Desk Booking logo, Desk Availability · My Bookings, avatar, Sign out). Page background muted grey; white cards for date filter and desk table.

```
┌──────────────────────────────────────────────────────────┐
│ [DB] Desk Booking   Desk Availability · My Bookings  JS  │
├──────────────────────────────────────────────────────────┤
│ Desk Availability                                        │
│ ┌ Office date [____]  [ Check availability ] ─────────┐│
│ └───────────────────────────────────────────────────────┘│
│ ┌ Desk ─── Status ───────── Action ────────────────────┐ │
│ │ A-01   ● Available      [ Book desk ]                │ │
│ │ A-02   ● Booked                                      │ │
│ └──────────────────────────────────────────────────────┘ │
└──────────────────────────────────────────────────────────┘
```

## States

### ST-01 Default

- **When** Employee lands on Book Desk; today's date pre-selected (office timezone)
- **Shows** App shell, date picker, prompt to load desks
- **Can do** Change date (today → +30 days, working days only), navigate to My Bookings, sign out

### ST-02 Loading

- **When** Date selected/changed; desk availability is fetching
- **Shows** Skeleton desk grid; date picker disabled briefly
- **Can do** Wait

### ST-03 Desks available

- **When** Desks returned for selected working day
- **Shows** Desk table with unique numbers; Available (pill + Book desk) or Booked (pill, no action)
- **Can do** Book an available desk (opens ST-07), change date

### ST-04 Empty

- **When** All desks booked for selected date
- **Shows** Empty message with icon; suggestion to pick another date
- **Can do** Change date

### ST-05 Error

- **When** Availability request fails
- **Shows** Error banner + retry action
- **Can do** Retry, change date, navigate away

### ST-06 Already booked this date

- **When** Employee already has a **Confirmed** booking for selected date (BR-001.1)
- **Shows** Info banner naming existing desk; desk grid read-only or hidden; link to My Bookings to cancel first (BR-001.2)
- **Can do** Go to My Bookings, pick another date without existing booking

### ST-07 Confirm booking

- **When** Employee clicks Book on an available desk
- **Shows** Modal: desk number, date, Confirm / Cancel
- **Can do** Confirm (creates **Confirmed** booking) or Cancel (returns to ST-03)

## Components

| Component   | Preview                                               | Notes                     |
| ----------- | ----------------------------------------------------- | ------------------------- |
| `book-desk` | `inception/design/components/book-desk/preview.html` | All ST-01..ST-07 states   |

## Interaction and accessibility

- **Keyboard:** Date picker operable; desk cards tab to Book button; modal traps focus
- **Focus:** Return focus to booked desk card on modal cancel
- **Non-colour signalling:** Available/Booked use icon + text label (not green/red alone)
- **Announcements:** Success toast after confirm; loading region `aria-busy`

## Structural decisions

| Decision | Rationale | Alternative rejected |
| -------- | --------- | -------------------- |
| Table for desks | Matches client hi-fi; scannable list without floor-plan map | Card grid — rejected for styling pass |
| Confirm modal before book | Prevents mis-clicks; client SCR-03 is same step as modal, not a route | Separate confirm page |
| Block second booking same day | BR-001.1 — banner + link to cancel | Allow override |

## Conflicts and open questions

| #   | Conflict / question | Between | Owner | Status |
| --- | ------------------- | ------- | ----- | ------ |
| 1   | Public holidays not in BRD — weekend-only enforced in date picker | BR-001.3 / open Q#2 | PO/client | open |
| 3   | Client mockup includes Location column — not in BRD (single office, desk number only) | Client hi-fi / REQ-007 | PO/client | resolved — omitted |
| 4   | Mobile-responsive layout vs desktop-only | NFR-004 | PO/client | open |

## Designer handoff

Draw one frame per `ST-##`. Figma wireframes: `tools/EDBS_Wireframes` plugin.
