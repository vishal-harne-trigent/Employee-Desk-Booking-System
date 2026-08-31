# US-007 — Booking email notifications

> Technical expansion of US-007. Story defines business need; this defines code behaviour.

|                   |                                                                  |
| ----------------- | ---------------------------------------------------------------- |
| **Story**         | `inception/stories/user-stories/US-007-booking-emails.md`       |
| **Traces to**     | REQ-023, REQ-024, REQ-025, NFR-005, BR-001.13, BR-001.14, BR-001.16, V-13 |
| **Screen**        | none                                                             |
| **Covering ADRs** | none                                                             |
| **Tier**          | Complex                                                          |
| **Status**        | implemented                                                      |
| **Updated**       | 2026-08-31                                                       |

## Problem

Employees must receive mandatory email when a booking is Confirmed or Cancelled, and a day-before reminder for future Confirmed working-day bookings. Email bodies must include desk number and date. SMTP failures must be logged without blocking the booking transaction.

## Functional requirements

| ID    | Requirement | Priority | Serves | Status |
| ----- | ----------- | -------- | ------ | ------ |
| FR-01 | Confirmation email sent when booking becomes Confirmed | Must | AC-01 | implemented |
| FR-02 | Cancellation email sent when booking becomes Cancelled (employee or admin) | Must | AC-02 | implemented |
| FR-03 | Email body includes desk number and booking date (V-13) | Must | AC-03 | implemented |
| FR-04 | One day-before reminder for Confirmed future working-day bookings; skip same-day, Cancelled, and Completed | Must | AC-04 | implemented |
| FR-05 | SMTP and delivery failures logged to `EmailDeliveryLogs` | Must | AC-05 | implemented |

## Non-functional requirements

| ID     | Requirement | Serves |
| ------ | ----------- | ------ |
| NFR-01 | Log email delivery failures for operations; never log passwords | NFR-005 |

## Technical constraints

- MailKit SMTP via `IEmailSender`; `InMemoryEmailSender` in tests
- Booking commits even if email send fails; errors caught and logged
- Reminder job via `ReminderEmailHostedService` using office-local dates

## Out of scope

- Browser push notifications → US-008
- User opt-out of mandatory emails
- SMTP production deployment config (Gate 3 / DevOps)
