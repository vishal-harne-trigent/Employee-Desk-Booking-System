# US-009 — Complete past bookings automatically

> Approval = Gate 1 review of this file's PR. Delivery = the story PR (`feat/US-009-booking-completion`) merging with every AC proven by a test named `... (US-009/AC-##)`.

|                |                                         |
| -------------- | --------------------------------------- |
| **Epic**       | EPIC-001                                |
| **Traces to**  | REQ-009, REQ-011, REQ-013, BR-001.5     |
| **Priority**   | Must                                    |
| **Estimate**   | 3 pts (AI draft — humans re-estimate)   |
| **Depends on** | US-002                                  |

## Story

As the **system**
I want past **Confirmed** bookings to become **Completed** automatically
So that booking history and admin filters reflect accurate status after the desk date passes.

## Acceptance criteria

### AC-01 Transition after booking date

- **Given** a booking in **Confirmed** status whose date is before today in office local timezone
- **When** the completion job runs (scheduled daily or on read — implementation choice)
- **Then** the booking status becomes **Completed** (BR-001.5)

### AC-02 Cancelled bookings unchanged

- **Given** a booking already **Cancelled**
- **When** the completion job runs
- **Then** status remains **Cancelled**

### AC-03 Today’s Confirmed bookings stay Confirmed

- **Given** a **Confirmed** booking dated today (office local)
- **When** the completion job runs before end of day
- **Then** status remains **Confirmed** until the date has passed

## Edge cases

- Timezone boundary at midnight office local — job must use configured office timezone (NFR-001).

## QA notes

Freeze clock tests across timezone. Verify US-003 and US-004 display **Completed** after job runs.

## API impacts

Background scheduler or batch update — no UI.
