# SCR-004 — Admin Bookings

> Approval = Gate 1 review of this file's PR. A state is "designed" when a component preview renders it and marks it `<!-- @state SCR-004/ST-## -->`.

|                  |                                                      |
| ---------------- | ---------------------------------------------------- |
| **Traces to**    | REQ-011, REQ-012, REQ-013, REQ-014                   |
| **Surface**      | `apps/ui` — `/admin/bookings`                        |
| **Primary user** | Admin                                                |
| **Status**       | draft — awaiting designer review                     |

## Purpose

Let an Admin view every employee booking, filter by date and status, and cancel **Confirmed** bookings on an employee's behalf for today or future dates.

## Layout

```
┌──────────────────────────────────────────────────────────┐
│ [DB] Admin   Desks · Users · All Bookings     Sign out     │
├──────────────────────────────────────────────────────────┤
│ Filters: [ Date ▼ ]  [ Status: All ▼ ]   [ Apply ]      │
│ ┌──────────┬───────────┬──────────┬─────────┬────────┐ │
│ │ Date     │ Employee  │ Desk     │ Status  │ Action │ │
│ ├──────────┼───────────┼──────────┼─────────┼────────┤ │
│ │ Aug 14   │ jane@…    │ A-01     │ Confirmed│ Cancel │ │
│ └──────────┴───────────┴──────────┴─────────┴────────┘ │
└──────────────────────────────────────────────────────────┘
```

## States

### ST-01 Default

- **When** Admin opens All Bookings; unfiltered or default filter
- **Shows** Full booking list with employee email, desk, date, status badge
- **Can do** Set filters, cancel eligible bookings, sign out

### ST-02 Loading

- **When** Bookings list fetching
- **Shows** Skeleton table
- **Can do** Wait

### ST-03 Empty filter results

- **When** Filters applied but no rows match
- **Shows** Empty message + clear filters action
- **Can do** Clear filters

### ST-04 Error

- **When** Load fails
- **Shows** Error banner + retry
- **Can do** Retry

### ST-05 Filters applied

- **When** Admin applied date and/or status filter (REQ-012, REQ-013)
- **Shows** Active filter chips + filtered rows; count summary optional
- **Can do** Adjust filters, cancel eligible rows

### ST-06 Cancel on behalf confirm

- **When** Admin clicks Cancel on eligible row
- **Shows** Modal naming employee, date, desk; Confirm / Keep
- **Can do** Confirm (**Cancelled**) or dismiss

## Components

| Component         | Preview                                                     | Notes                   |
| ----------------- | ----------------------------------------------------------- | ----------------------- |
| `admin-bookings`  | `inception/design/components/admin-bookings/preview.html`   | All ST-01..ST-06 states |

## Interaction and accessibility

- **Keyboard:** Filter controls → Apply → table → Cancel buttons; modal trap
- **Focus:** Visible focus on filter Apply and row actions
- **Non-colour signalling:** Status badges with icon + label; filter state not colour-only
- **Announcements:** Filter result count announced when Apply clicked

## Structural decisions

| Decision | Rationale | Alternative rejected |
| -------- | --------- | -------------------- |
| Admin nav: Desks · Users · All Bookings | Full admin toolset per BRD REQ-015..022 | Bookings-only shell |
| Explicit Apply on filters | Clear fetch boundary for server-filtered list | Instant filter on every keystroke |
| Cancel only today/future Confirmed | BR-001.6 parity with employee rules | Admin can cancel Completed |

## Conflicts and open questions

| #   | Conflict / question | Between | Owner | Status |
| --- | ------------------- | ------- | ----- | ------ |
| 1   | Deactivate desk with future bookings flow | BR-001.9 / open Q#6 | PO/client | open |

## Designer handoff

Draw one frame per `ST-##`. Figma wireframes: `tools/EDBS_Wireframes` plugin.
