# US-007 — Send booking email notifications

> Approval = Gate 1 review of this file's PR. Delivery = the story PR (`feat/US-007-booking-emails`) merging with every AC proven by a test named `... (US-007/AC-##)`.

|                |                                         |
| -------------- | --------------------------------------- |
| **Epic**       | EPIC-001                                |
| **Traces to**  | REQ-023, REQ-024, REQ-025, NFR-005, BR-001.13, BR-001.14, BR-001.16 |
| **Priority**   | Must                                    |
| **Estimate**   | 8 pts (AI draft — humans re-estimate)   |
| **Depends on** | US-002, US-003, US-004                  |

## Story

As an **Employee** (booking owner)
I want email when I book, cancel, or have a desk reserved for tomorrow
So that I have reliable confirmation without opting in.

## Acceptance criteria

### AC-01 Confirmation email on book

- **Given** a booking transitions to **Confirmed**
- **When** the transaction completes
- **Then** a confirmation email is sent to the booking owner's account email (REQ-023, BR-001.13)

### AC-02 Cancellation email on cancel

- **Given** a booking transitions to **Cancelled** (employee or admin initiated)
- **When** the cancellation completes
- **Then** a cancellation email is sent to the booking owner (REQ-024, BR-001.13)

### AC-03 Email content includes desk and date

- **Given** any booking confirmation or cancellation email
- **When** the email is rendered
- **Then** it includes the desk number and booking date (V-13)

### AC-04 Day-before reminder

- **Given** a **Confirmed** booking on a future working day (Mon–Fri)
- **When** the previous calendar day arrives in office local timezone
- **Then** one reminder email is sent; no reminder for same-day bookings or **Cancelled**/**Completed** bookings (REQ-025, BR-001.14)

### AC-05 Log delivery failures

- **Given** email send fails (SMTP error, invalid address)
- **When** the failure occurs
- **Then** the failure is logged for operations follow-up (NFR-005)

## Edge cases

- Reminder send time: `TBD (owner: PO/client)` — default 08:00 office local proposed in BRD.
- SMTP/sender domain open question #7 — configure at deploy time.
- No user opt-out of mandatory emails (BRD §10).

## QA notes

Use test mail catcher or mock SMTP. Scheduler test for reminder job with frozen clock.

## API impacts

Email service integration; scheduled reminder job — no UI screen.
