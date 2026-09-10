# US-008 — Push notifications

> Technical expansion of US-008. Story defines business need; this defines code behaviour.

|                   |                                                                  |
| ----------------- | ---------------------------------------------------------------- |
| **Story**         | `inception/stories/user-stories/US-008-push-notifications.md`   |
| **Traces to**     | REQ-026, REQ-027, NFR-004, NFR-006, BR-001.15, BR-001.16, V-14 |
| **Screen**        | SCR-007                                                          |
| **Covering ADRs** | none                                                             |
| **Tier**          | Complex                                                          |
| **Status**        | implemented                                                      |
| **Updated**       | 2026-08-31                                                       |

## Problem

Employees may opt in to browser push notifications for book and cancel events; default is opt-out. Push is sent only when opted in and a subscription is stored. Reminder events send email only. Unsupported browsers degrade gracefully.

## Functional requirements

| ID    | Requirement | Priority | Serves | Status |
| ----- | ----------- | -------- | ------ | ------ |
| FR-01 | Default opt-out — no push on book or cancel until user opts in | Must | AC-01 | implemented |
| FR-02 | Opt in via Notification Settings saves preference when browser subscription is registered | Must | AC-02 | implemented |
| FR-03 | Push delivered on Confirmed and Cancelled when opted in (includes admin cancel) | Must | AC-03 | implemented |
| FR-04 | Opt out stops subsequent push; email behaviour unchanged | Must | AC-04 | implemented |
| FR-05 | Day-before reminder sends email only — no push regardless of preference | Must | AC-05 | implemented |

## Non-functional requirements

| ID     | Requirement | Serves |
| ------ | ----------- | ------ |
| NFR-01 | Notification settings restricted to signed-in Employee | NFR-004 |
| NFR-02 | Graceful degradation when push unsupported or permission denied | NFR-006 |

## Technical constraints

- WebPush (VAPID) via `WebPush` NuGet; `InMemoryPushNotificationSender` in tests
- `NotificationPreferences` table stores `PushOptIn` and subscription JSON
- Booking commits even if push send fails (parity with US-007 email)

## Out of scope

- Push for reminder events (BR-001.16)
- Email preference toggles (emails remain mandatory per BR-001.13)
- Production VAPID key deployment (Gate 3 / DevOps)
- Mobile-app or SMS push
