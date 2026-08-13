# SCR-003 — My Bookings

> Approval = Gate 1 review of this file's PR. A state is "designed" when a component preview renders it and marks it `<!-- @state SCR-003/ST-## -->`.

|                  |                                                      |
| ---------------- | ---------------------------------------------------- |
| **Traces to**    | REQ-009, REQ-010                                     |
| **Surface**      | `apps/ui` — `/my-bookings`                           |
| **Primary user** | Employee                                             |
| **Status**       | draft — awaiting designer review                     |

## Purpose

Let an Employee review all their past and future desk bookings and cancel **Confirmed** bookings for today or future dates.

## Layout

```
┌──────────────────────────────────────────────────────────┐
│ EDBS   Book Desk · My Bookings              Sign out     │
├──────────────────────────────────────────────────────────┤
│ My bookings                                              │
│ ┌──────────┬──────────┬─────────────┬─────────┐        │
│ │ Date     │ Desk     │ Status      │ Action  │        │
│ ├──────────┼──────────┼─────────────┼─────────┤        │
│ │ Aug 14   │ A-01     │ ● Confirmed │ Cancel  │        │
│ │ Aug 10   │ B-02     │ ○ Completed │ —       │        │
│ └──────────┴──────────┴─────────────┴─────────┘        │
└──────────────────────────────────────────────────────────┘
```

## States

### ST-01 Default

- **When** Bookings loaded successfully
- **Shows** Table/list sorted by date (future first or chronological — default: nearest date first); status badge with icon + label
- **Can do** Cancel eligible rows (today/future **Confirmed**), navigate, sign out

### ST-02 Loading

- **When** Initial fetch of employee bookings
- **Shows** Skeleton rows
- **Can do** Wait

### ST-03 Empty

- **When** Employee has no bookings ever
- **Shows** Empty state with icon + link to Book Desk
- **Can do** Navigate to Book Desk

### ST-04 Error

- **When** Fetch fails
- **Shows** Error banner + retry
- **Can do** Retry

### ST-05 Cancel confirm

- **When** Employee clicks Cancel on an eligible booking
- **Shows** Modal: date, desk, Confirm cancel / Keep booking
- **Can do** Confirm (status → **Cancelled**) or dismiss

## Components

| Component     | Preview                                                 | Notes                   |
| ------------- | ------------------------------------------------------- | ----------------------- |
| `my-bookings` | `inception/design/components/my-bookings/preview.html` | All ST-01..ST-05 states |

## Interaction and accessibility

- **Keyboard:** Table rows tab to Cancel where shown; modal trap
- **Focus:** Focus moves to modal on open; returns to Cancel on dismiss
- **Non-colour signalling:** Status badges always show text + icon (Confirmed / Cancelled / Completed)
- **Announcements:** Confirm cancellation success via polite live region

## Structural decisions

| Decision | Rationale | Alternative rejected |
| -------- | --------- | -------------------- |
| Table layout | Dense history scan | Card list — harder to compare dates |
| Cancel only on Confirmed + today/future | BR-001.6 | Cancel on Completed |
| No in-row desk change | BR-001.2 cancel-then-book | Edit desk inline |

## Conflicts and open questions

| #   | Conflict / question | Between | Owner | Status |
| --- | ------------------- | ------- | ----- | ------ |
| —   | None blocking structure | — | — | — |

## Designer handoff

Draw one frame per `ST-##`. Figma wireframes: `tools/EDBS_Wireframes` plugin.
