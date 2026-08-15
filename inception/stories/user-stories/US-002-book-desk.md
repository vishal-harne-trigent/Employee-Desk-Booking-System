# US-002 — Book a desk

> Approval = Gate 1 review of this file's PR. Delivery = the story PR (`feat/US-002-book-desk`) merging with every AC proven by a test named `... (US-002/AC-##)`.

|                |                                         |
| -------------- | --------------------------------------- |
| **Epic**       | EPIC-001                                |
| **Traces to**  | REQ-001, REQ-003, REQ-006, REQ-007, REQ-008, NFR-001, NFR-002, BR-001.1, BR-001.2, BR-001.3, BR-001.4, BR-001.7 |
| **Priority**   | Must                                    |
| **Estimate**   | 8 pts (AI draft — humans re-estimate)   |
| **Depends on** | US-001                                  |

## Story

As an **Employee**
I want to pick a working day and book one available desk by its desk number
So that I have a confirmed seat before I come to the office.

## Acceptance criteria

### AC-01 Select a date within the booking window

- **Given** a signed-in Employee on Book Desk
- **When** they choose a date from today through 30 calendar days ahead (office local timezone)
- **Then** the system loads desk availability for that date

### AC-02 Reject invalid dates

- **Given** a signed-in Employee
- **When** they select a date before today, after today+30, or on Saturday/Sunday
- **Then** the system rejects the date and does not show bookable availability (BR-001.3, V-02, V-03)

### AC-03 View active desks and availability

- **Given** a valid working-day date
- **When** availability loads
- **Then** each **Active** desk shows its unique desk number and whether it is available or booked (SCR-002 ST-03)

### AC-04 Book one available desk

- **Given** the Employee has no **Confirmed** booking for that date and a desk is available
- **When** they confirm booking that desk
- **Then** a **Confirmed** booking is created for that Employee, desk, and date (BR-001.4)

### AC-05 Reject double booking same day

- **Given** the Employee already has a **Confirmed** booking on the selected date
- **When** they attempt to book another desk for the same date
- **Then** the request is rejected (BR-001.1, V-05)

### AC-06 Inactive or taken desks not bookable

- **Given** a desk is **Inactive** or already **Confirmed** for another user on that date
- **When** the Employee attempts to book it
- **Then** the request is rejected (BR-001.7, V-04)

## Edge cases

- Change desk same day: must cancel existing booking first (BR-001.2) — covered in US-003 + re-book flow.
- Concurrent book of same desk: one succeeds; other fails (RISK-004).

## UI

Served by **SCR-002 — Book a Desk**. Table layout for desk availability; states include loading, empty date, success confirmation, and validation errors.

## QA notes

Fixtures: mix of Active/Inactive desks; pre-seeded booking on one desk for availability display.

## API impacts

Booking create and availability query endpoints — TBD in architecture.
