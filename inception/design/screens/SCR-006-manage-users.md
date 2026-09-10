# SCR-006 — Manage Users

> Approval = Gate 1 review of this file's PR. A state is "designed" when a component preview renders it and marks it `<!-- @state SCR-006/ST-## -->`.

|                  |                                                      |
| ---------------- | ---------------------------------------------------- |
| **Traces to**    | REQ-018, REQ-019, REQ-020, REQ-021, REQ-022, BR-001.10, BR-001.11, BR-001.12 |
| **Surface**      | `apps/ui` — `/admin/users`                           |
| **Primary user** | Admin                                                |
| **Status**       | draft — awaiting designer review                     |

## Purpose

Let an Admin create and maintain user accounts: email, name, role, activation state, and admin-initiated password reset.

## Layout

```
┌──────────────────────────────────────────────────────────┐
│ [DB] Admin   Desks · Users · All Bookings     Sign out   │
├──────────────────────────────────────────────────────────┤
│ Manage users                          [ + Add user ]       │
│ ┌────────┬───────────────┬──────────┬────────┬────────┐ │
│ │ Name   │ Email         │ Role     │ Status │ Actions│ │
│ ├────────┼───────────────┼──────────┼────────┼────────┤ │
│ │ Jane S │ jane@co.com   │ Employee │ Active │ …      │ │
│ └────────┴───────────────┴──────────┴────────┴────────┘ │
└──────────────────────────────────────────────────────────┘
```

## States

### ST-01 Default

- **When** Admin opens Manage Users with list loaded
- **Shows** User table with role and Active/Inactive status; row actions (Edit, Reset password, Deactivate)
- **Can do** Add user, row actions, navigate, sign out

### ST-02 Loading

- **When** User list fetching
- **Shows** Skeleton rows
- **Can do** Wait

### ST-03 Empty

- **When** No users beyond bootstrap (edge case)
- **Shows** Empty message + Add user
- **Can do** Add user

### ST-04 Error

- **When** Load fails
- **Shows** Error banner + retry
- **Can do** Retry

### ST-05 Add user

- **When** Admin clicks Add user
- **Shows** Form/modal: name, email, role select, initial password, Create / Cancel
- **Can do** Create (unique email BR-001.10) or cancel

### ST-06 Edit user

- **When** Admin clicks Edit
- **Shows** Modal: name, email, role; Save / Cancel
- **Can do** Save or cancel; role change subject to BR-001.11

### ST-07 Reset password result

- **When** Admin completes Reset password (REQ-021, BR-001.12)
- **Shows** One-time display of new password + copy hint; dismiss
- **Can do** Copy password, close (password not shown again)

### ST-08 Deactivate confirm

- **When** Admin clicks Deactivate on active user (not last admin)
- **Shows** Confirm modal
- **Can do** Confirm or cancel

### ST-09 Last admin blocked

- **When** Action would leave zero active Admins (BR-001.11)
- **Shows** Error alert; action rejected
- **Can do** Dismiss; choose different role/deactivation target

## Components

| Component       | Preview                                                       | Notes              |
| --------------- | ------------------------------------------------------------- | ------------------ |
| `manage-users`  | `inception/design/components/manage-users/preview.html`       | All ST-01..ST-09   |

## Interaction and accessibility

- **Keyboard:** Form fields and table actions in logical order; modal trap
- **Focus:** Move to password result panel after reset; warn before dismiss
- **Non-colour signalling:** Role and status as text labels
- **Announcements:** Alert when last-admin rule blocks action

## Structural decisions

| Decision | Rationale | Alternative rejected |
| -------- | --------- | -------------------- |
| Password shown once to Admin | BR-001.12; no email of password | Email temp password |
| Role in create + edit | REQ-022 | Separate role-only screen |
| Reset password separate from edit | Clear audit boundary | Combined edit form |

## Conflicts and open questions

| #   | Conflict / question | Between | Owner | Status |
| --- | ------------------- | ------- | ----- | ------ |
| 1   | Minimum password rules (V-12) | Security / UX copy | PO/security | open |

## Designer handoff

Draw one frame per `ST-##`. Figma wireframes: `tools/EDBS_Wireframes/plugin`.
