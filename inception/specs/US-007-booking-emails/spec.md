# US-007 — spec (delivery slice)

|           |                                                                  |
| --------- | ---------------------------------------------------------------- |
| **Story** | `inception/stories/user-stories/US-007-booking-emails.md`       |
| **Tier**  | Complex                                                          |

## Functional requirements in scope

| ID      | Summary |
| ------- | ------- |
| REQ-023 | Confirmation email when booking becomes Confirmed |
| REQ-024 | Cancellation email when booking becomes Cancelled |
| REQ-025 | Day-before reminder for Confirmed future working-day bookings |

## Non-functional requirements in scope

| ID      | Summary |
| ------- | ------- |
| NFR-005 | Log email delivery failures for operations |

## Validation rules in scope

| ID    | Summary |
| ----- | ------- |
| V-13  | Emails include desk number and booking date |

## Acceptance criteria

| AC    | Summary |
| ----- | ------- |
| AC-01 | Confirmation email on successful book |
| AC-02 | Cancellation email on employee or admin cancel |
| AC-03 | Email body includes desk number and date |
| AC-04 | One reminder on calendar day before booking; skip same-day / Cancelled / Completed |
| AC-05 | SMTP failures logged to `EmailDeliveryLogs` |

## Out of scope

- Browser push notifications → US-008 (SCR-007)
- User opt-out of mandatory emails
- SMTP production deployment config (Gate 3 / DevOps)
