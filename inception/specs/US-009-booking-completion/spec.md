# US-009 — Auto-complete past bookings

> Technical expansion of US-009. Story defines business need; this defines code behaviour.

|                   |                                                                  |
| ----------------- | ---------------------------------------------------------------- |
| **Story**         | `inception/stories/user-stories/US-009-booking-completion.md`   |
| **Traces to**     | REQ-009, REQ-011, REQ-013, BR-001.5, NFR-001                     |
| **Screen**        | none                                                             |
| **Covering ADRs** | none                                                             |
| **Tier**          | Medium                                                           |
| **Status**        | implemented                                                      |
| **Updated**       | 2026-08-31                                                       |

## Problem

Confirmed bookings whose date is before today in office local timezone must transition to Completed when the completion job runs. Cancelled bookings are unchanged. Today's Confirmed bookings remain Confirmed until the date passes.

## Functional requirements

| ID    | Requirement | Priority | Serves | Status |
| ----- | ----------- | -------- | ------ | ------ |
| FR-01 | Past Confirmed bookings become Completed when job runs | Must | AC-01 | implemented |
| FR-02 | Cancelled bookings remain Cancelled when job runs | Must | AC-02 | implemented |
| FR-03 | Today's Confirmed bookings stay Confirmed until booking date passes | Must | AC-03 | implemented |

## Non-functional requirements

| ID     | Requirement | Serves |
| ------ | ----------- | ------ |
| NFR-01 | Job uses office local timezone for date comparison | NFR-001 |

## Technical constraints

- `IBookingCompletionService.CompletePastBookingsAsync` callable from tests with frozen `IOfficeClock`
- Daily `CompletePastBookingsHostedService` on Web; idempotent transitions
- No new UI or API endpoints

## Out of scope

- Notifications on completion
- Cancelling Completed bookings (BR-001.6 — already enforced)
