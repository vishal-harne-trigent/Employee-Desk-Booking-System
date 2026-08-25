# US-008 — spec (delivery slice)

|           |                                                                  |
| --------- | ---------------------------------------------------------------- |
| **Story** | `inception/stories/user-stories/US-008-push-notifications.md` |
| **Tier**  | Complex                                                          |

## Functional requirements in scope

| ID      | Summary |
| ------- | ------- |
| REQ-026 | Employee can opt in/out of browser push; default opt-out |
| REQ-027 | Push on Confirmed (book) and Cancelled events when opted in |

## Non-functional requirements in scope

| ID      | Summary |
| ------- | ------- |
| NFR-004 | Role-based access — settings for signed-in Employee |
| NFR-006 | Graceful degradation when push unsupported or permission denied |

## Validation rules in scope

| ID    | Summary |
| ----- | ------- |
| V-14  | Push sent only when `PushOptIn` is true |

## Screen in scope

| ID      | Summary |
| ------- | ------- |
| SCR-007 | Notification Settings — opt-in/out toggle, email info (read-only) |

## Acceptance criteria

| AC    | Summary |
| ----- | ------- |
| AC-01 | Default opt-out — no push on book/cancel |
| AC-02 | Opt in via settings saves preference when subscription registered |
| AC-03 | Push on book and cancel when opted in (includes admin cancel) |
| AC-04 | Opt out stops subsequent push; email unchanged |
| AC-05 | Day-before reminder sends email only — no push |

## Out of scope

- Push for reminder events (BR-001.16)
- Email preference toggles (emails remain mandatory per BR-001.13)
- Production VAPID key deployment (Gate 3 / DevOps)
- Mobile-app or SMS push
