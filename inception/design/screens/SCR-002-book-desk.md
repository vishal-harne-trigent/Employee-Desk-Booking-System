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

```
┌──────────────────────────────────────────────────────────┐
│ EDBS   Book Desk · My Bookings              Sign out     │
├──────────────────────────────────────────────────────────┤
│ Book a desk for: [ date picker ▼ ]  (office timezone)    │
│                                                          │
│  ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐       │
│  │ A-01    │ │ A-02    │ │ B-01    │ │ B-02    │       │
│  │ ✓ Avail │ │ ✗ Booked│ │ ✓ Avail │ │ ✓ Avail │       │
│  │ [Book]  │ │         │ │ [Book]  │ │ [Book]  │       │
│  └─────────┘ └─────────┘ └─────────┘ └─────────┘       │
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
- **Shows** Desk cards with unique numbers; Available (icon + label + Book) or Booked (icon + label, no action)
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
| Card grid for desks | Scannable desk numbers at a glance | Floor plan — no REQ for map |
| Confirm modal before book | Prevents mis-clicks on adjacent desks | Instant book |
| Block second booking same day | BR-001.1 — banner + link to cancel | Allow override |

## Conflicts and open questions

| #   | Conflict / question | Between | Owner | Status |
| --- | ------------------- | ------- | ----- | ------ |
| 1   | Public holidays not in BRD — weekend-only enforced in date picker | BR-001.3 / open Q#2 | PO/client | open |
| 2   | Mobile-responsive layout vs desktop-only | NFR-004 | PO/client | open |

## Designer handoff

Draw one frame per `ST-##`. Figma wireframes: `tools/EDBS_Wireframes` plugin.
