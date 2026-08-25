# SCR-005 — Manage Desks

> Approval = Gate 1 review of this file's PR. A state is "designed" when a component preview renders it and marks it `<!-- @state SCR-005/ST-## -->`.

|                  |                                                      |
| ---------------- | ---------------------------------------------------- |
| **Traces to**    | REQ-015, REQ-016, REQ-017, BR-001.8, BR-001.9        |
| **Surface**      | `apps/ui` — `/admin/desks`                           |
| **Primary user** | Admin                                                |
| **Status**       | draft — awaiting designer review                     |

## Purpose

Let an Admin maintain desk inventory: add desks with unique numbers, edit numbers, and activate or deactivate desks so inactive desks never appear in employee booking.

## Layout

```
┌──────────────────────────────────────────────────────────┐
│ [DB] Admin   Desks · Users · All Bookings     Sign out   │
├──────────────────────────────────────────────────────────┤
│ Manage desks                          [ + Add desk ]     │
│ ┌──────────┬──────────┬──────────────────────────────┐  │
│ │ Desk     │ Status   │ Actions                      │  │
│ ├──────────┼──────────┼──────────────────────────────┤  │
│ │ A-12     │ Active   │ Edit · Deactivate            │  │
│ │ B-03     │ Inactive │ Edit · Activate              │  │
│ └──────────┴──────────┴──────────────────────────────┘  │
└──────────────────────────────────────────────────────────┘
```

## States

### ST-01 Default

- **When** Admin opens Manage Desks with inventory loaded
- **Shows** Desk table (number, Active/Inactive badge, Edit / Activate or Deactivate)
- **Can do** Add desk, edit, activate/deactivate, navigate admin sections, sign out

### ST-02 Loading

- **When** Desk list fetching
- **Shows** Skeleton table rows
- **Can do** Wait

### ST-03 Empty

- **When** No desks in inventory
- **Shows** Empty message + Add desk call to action
- **Can do** Add first desk

### ST-04 Error

- **When** Load fails
- **Shows** Error banner + retry
- **Can do** Retry

### ST-05 Add desk

- **When** Admin clicks Add desk
- **Shows** Modal/form: desk number field, Save / Cancel
- **Can do** Submit unique number or cancel

### ST-06 Edit desk

- **When** Admin clicks Edit on a row
- **Shows** Modal with desk number (editable), Save / Cancel
- **Can do** Save if unique (BR-001.8) or cancel

### ST-07 Deactivate confirm

- **When** Admin clicks Deactivate on an **Active** desk with no blocking future bookings
- **Shows** Confirm modal naming desk
- **Can do** Confirm deactivation or cancel

### ST-08 Deactivate blocked

- **When** Admin attempts deactivate but desk has **Confirmed** bookings today or future (BR-001.9)
- **Shows** Error/info banner listing conflict; deactivate disabled until bookings cleared
- **Can do** Navigate to All Bookings to cancel, or dismiss

## Components

| Component      | Preview                                                      | Notes              |
| -------------- | ------------------------------------------------------------ | ------------------ |
| `manage-desks` | `inception/design/components/manage-desks/preview.html`      | All ST-01..ST-08   |

## Interaction and accessibility

- **Keyboard:** Table actions reachable; modals trap focus
- **Focus:** Return focus to triggering row after modal close
- **Non-colour signalling:** Active/Inactive use label + icon/dot, not colour alone
- **Announcements:** Success/error after save; `role="alert"` on validation errors

## Structural decisions

| Decision | Rationale | Alternative rejected |
| -------- | --------- | -------------------- |
| Admin nav: Desks · Users · All Bookings | Matches client hi-fi; groups admin tools | Bookings-only shell (superseded by BRD expansion) |
| Desk number only (no location field) | Single office; REQ-007 uses desk number only | Floor/zone column from external mockup |
| Block deactivate with future bookings | BR-001.9 default | Silent cascade cancel without confirm |

## Conflicts and open questions

| #   | Conflict / question | Between | Owner | Status |
| --- | ------------------- | ------- | ----- | ------ |
| 1   | Cancel future bookings in same deactivate flow vs block only | BR-001.9 / open Q#6 | PO/client | open — ST-08 shows block |

## Designer handoff

Draw one frame per `ST-##`. Figma wireframes: `tools/EDBS_Wireframes/plugin`.
