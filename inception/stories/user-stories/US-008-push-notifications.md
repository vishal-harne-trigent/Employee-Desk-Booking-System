# US-008 — Browser push notification preferences

> Approval = Gate 1 review of this file's PR. Delivery = the story PR (`feat/US-008-push-notifications`) merging with every AC proven by a test named `... (US-008/AC-##)`.

|                |                                         |
| -------------- | --------------------------------------- |
| **Epic**       | EPIC-001                                |
| **Traces to**  | REQ-001, REQ-003, REQ-026, REQ-027, NFR-004, NFR-006, BR-001.15, BR-001.16 |
| **Priority**   | Must                                    |
| **Estimate**   | 5 pts (AI draft — humans re-estimate)   |
| **Depends on** | US-001, US-002, US-003                  |

## Story

As an **Employee**
I want to optionally enable browser push alerts when I book or cancel a desk
So that I get instant feedback without relying on email alone.

## Acceptance criteria

### AC-01 Default opt-out

- **Given** a new or existing Employee who has not opted in
- **When** they book or cancel a desk
- **Then** no browser push is sent (BR-001.15, REQ-026)

### AC-02 Opt in via settings

- **Given** a signed-in Employee on Notification Settings
- **When** they enable browser push and grant browser permission
- **Then** their preference is saved as opted-in (REQ-026, SCR-007 ST-02)

### AC-03 Push on book and cancel when opted in

- **Given** an Employee opted in to push
- **When** their booking becomes **Confirmed** or **Cancelled**
- **Then** a browser push notification is delivered (REQ-027, V-14)

### AC-04 Opt out stops push

- **Given** an opted-in Employee
- **When** they disable browser push in settings
- **Then** subsequent book/cancel events send email only (BR-001.15)

### AC-05 No push for reminders

- **Given** any Employee regardless of push preference
- **When** a day-before reminder fires
- **Then** only email is sent — no browser push (BR-001.16)

## Edge cases

- Browser denies permission: show graceful message; email still sent (NFR-006).
- Unsupported browser: degrade to email-only with explanatory copy.

## UI

Served by **SCR-007 — Notification Settings**. Toggle for push opt-in; saved confirmation state.

## QA notes

Test with supported browser and mocked Push API. Verify admin-initiated cancel still triggers push to employee when opted in.

## API impacts

Push subscription storage and send on booking events — TBD in architecture.
