# SCR-001 — Sign In

> Approval = Gate 1 review of this file's PR. A state is "designed" when a component preview renders it and marks it `<!-- @state SCR-001/ST-## -->`.

|                  |                                                      |
| ---------------- | ---------------------------------------------------- |
| **Traces to**    | REQ-002, REQ-003, REQ-004, REQ-005, NFR-003          |
| **Surface**      | `apps/ui` — `/login`                                 |
| **Primary user** | Employee, Admin                                      |
| **Status**       | draft — awaiting designer review                     |

## Purpose

Let a user authenticate with email and password before accessing desk booking. Success routes Employee to Book Desk; Admin to Admin Bookings. Deactivated accounts and invalid credentials are rejected with clear feedback.

## Layout

Navy full-bleed login canvas; centred white card with Desk Booking logo, “Welcome back” heading, email/password fields, green primary button.

```
┌──────────────────────────────────────────────┐
│  (navy background)                           │
│       ┌────────────────────────────┐         │
│       │ [DB]  Welcome back         │         │
│       │ Email    [_______________] │         │
│       │ Password [_______________] │         │
│       │      [ Sign in ]           │         │
│       │ (error banner when needed) │         │
│       └────────────────────────────┘         │
└──────────────────────────────────────────────┘
```

## States

### ST-01 Default

- **When** User opens the app unauthenticated
- **Shows** Empty email and password fields, Sign in button enabled
- **Can do** Enter credentials, submit, Tab through fields

### ST-02 Loading

- **When** User submitted valid-form credentials; server is verifying
- **Shows** Fields disabled, Sign in button shows loading indicator
- **Can do** Wait (no duplicate submit)

### ST-03 Error — invalid credentials

- **When** Server rejects unknown email or wrong password (V-01)
- **Shows** Error banner with icon + text; fields retain values except password cleared
- **Can do** Correct credentials and retry

### ST-04 Error — deactivated account

- **When** Authenticated user record is deactivated (REQ-005)
- **Shows** Error banner explaining account is deactivated; no route into app
- **Can do** Contact administrator (copy TBD by PO); retry not permitted

## Components

| Component | Preview                                           | Notes                         |
| --------- | ------------------------------------------------- | ----------------------------- |
| `sign-in` | `inception/design/components/sign-in/preview.html` | All ST-01..ST-04 states     |

## Interaction and accessibility

- **Keyboard:** Email → Password → Sign in; Enter submits from password field
- **Focus:** Visible `--focus-ring` on inputs and button
- **Non-colour signalling:** Errors use icon + message text, not red border alone
- **Announcements:** Error banner uses `role="alert"` on state change

## Structural decisions

| Decision | Rationale | Alternative rejected |
| -------- | --------- | -------------------- |
| Single centered card on navy canvas | Matches client hi-fi; focuses task before auth | Split marketing + login — out of scope |
| No forgot-password link | REQ out of scope per BRD §10 | Link that goes nowhere |
| Role-based redirect after login | REQ-004 — one role per user | Role picker on login |

## Conflicts and open questions

| #   | Conflict / question | Between | Owner | Status |
| --- | ------------------- | ------- | ----- | ------ |
| 1   | Deactivated-account support message copy | UX / PO | PO/client | open |

## Designer handoff

Draw one frame per `ST-##`. Import tokens from `inception/design/tokens.json`. Figma wireframes: run plugin in `tools/EDBS_Wireframes`.
