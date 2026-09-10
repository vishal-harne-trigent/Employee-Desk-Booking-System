# SCR-007 — Notification Settings

> Approval = Gate 1 review of this file's PR. A state is "designed" when a component preview renders it and marks it `<!-- @state SCR-007/ST-## -->`.

|                  |                                                      |
| ---------------- | ---------------------------------------------------- |
| **Traces to**    | REQ-026, REQ-027, NFR-006                            |
| **Surface**      | `apps/ui` — `/settings/notifications`                |
| **Primary user** | Employee (Admin may use same screen)                 |
| **Status**       | draft — awaiting designer review                     |

## Purpose

Let an Employee opt in or out of browser push notifications for booking and cancellation events. Booking emails (confirm, cancel, day-before reminder) are automatic and explained here but not toggled (REQ-023–REQ-025).

## Layout

```
┌──────────────────────────────────────────────────────────┐
│ [DB]   Desk Availability · My Bookings          Sign out │
├──────────────────────────────────────────────────────────┤
│ Notification settings                                    │
│ ┌ Email (automatic) ─────────────────────────────────┐ │
│ │ Confirmation, cancellation, day-before reminder    │ │
│ └──────────────────────────────────────────────────────┘ │
│ ┌ Browser push (optional) ────────────────────────────┐ │
│ │ Status: Disabled / Enabled                           │ │
│ │ [ Enable push ]  [ Disable push ]                    │ │
│ └──────────────────────────────────────────────────────┘ │
│ ← Back to My Bookings                                    │
└──────────────────────────────────────────────────────────┘
```

## States

### ST-01 Default opt-out

- **When** Employee opens settings; push not enabled (default REQ-026)
- **Shows** Email info section; push section shows Disabled; Enable button primary
- **Can do** Enable push (browser permission), navigate back, sign out

### ST-02 Push enabled

- **When** Employee has opted in (REQ-027)
- **Shows** Push status Enabled; Disable button available
- **Can do** Disable push, navigate away

### ST-03 Browser unsupported

- **When** Browser lacks push API or permission permanently denied (NFR-006)
- **Shows** Info message; Enable disabled; email-only note
- **Can do** Read guidance, go back

### ST-04 Error saving

- **When** Save preference fails
- **Shows** Error banner + retry
- **Can do** Retry toggle

### ST-05 Loading

- **When** Loading current preference
- **Shows** Skeleton or disabled controls
- **Can do** Wait

## Components

| Component               | Preview                                                                  | Notes            |
| ----------------------- | ------------------------------------------------------------------------ | ---------------- |
| `notification-settings` | `inception/design/components/notification-settings/preview.html`         | ST-01..ST-05     |

## Interaction and accessibility

- **Keyboard:** Enable/Disable buttons; back link
- **Focus:** Focus on status change announcement after toggle
- **Non-colour signalling:** Enabled/Disabled as text status, not colour alone
- **Announcements:** State change when push toggled

## Structural decisions

| Decision | Rationale | Alternative rejected |
| -------- | --------- | -------------------- |
| Email not toggleable | REQ-023–025 mandatory per BR-001.13 | User opt-out of email |
| Push book/cancel only | REQ-027; BR-001.16 | Push for reminders |
| Linked from My Bookings | Employee discovery without cluttering main nav | Top-level nav item |

## Conflicts and open questions

| #   | Conflict / question | Between | Owner | Status |
| --- | ------------------- | ------- | ----- | ------ |
| 1   | Entry point: nav link vs profile menu | UX | PO/client | open — default link on My Bookings |

## Designer handoff

Draw one frame per `ST-##`. Figma wireframes: `tools/EDBS_Wireframes/plugin`.
