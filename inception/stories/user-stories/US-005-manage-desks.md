# US-005 — Admin manage desks

> Approval = Gate 1 review of this file's PR. Delivery = the story PR (`feat/US-005-manage-desks`) merging with every AC proven by a test named `... (US-005/AC-##)`.

|                |                                         |
| -------------- | --------------------------------------- |
| **Epic**       | EPIC-001                                |
| **Traces to**  | REQ-001, REQ-003, REQ-004, REQ-015, REQ-016, REQ-017, NFR-004, BR-001.7, BR-001.8, BR-001.9 |
| **Priority**   | Must                                    |
| **Estimate**   | 5 pts (AI draft — humans re-estimate)   |
| **Depends on** | US-001                                  |

## Story

As an **Admin**
I want to add, edit, and activate or deactivate desks in the office inventory
So that employees only book desks that are actually available to use.

## Acceptance criteria

### AC-01 Add a desk with unique number

- **Given** a signed-in Admin on Manage Desks
- **When** they add a desk with a desk number not already in use
- **Then** the desk is created as **Active** and appears in the list (REQ-015, BR-001.8)

### AC-02 Reject duplicate desk number

- **Given** desk number A-01 already exists
- **When** the Admin adds or edits another desk to A-01
- **Then** the save is rejected with a validation error (V-08)

### AC-03 Edit desk number

- **Given** an existing **Active** desk
- **When** the Admin edits its desk number to another unique value
- **Then** the desk number is updated (REQ-016)

### AC-04 Deactivate desk

- **Given** a desk with no **Confirmed** bookings for today or future dates
- **When** the Admin deactivates it
- **Then** the desk becomes **Inactive** and no longer appears in employee availability (REQ-017, BR-001.7)

### AC-05 Block deactivate with future bookings

- **Given** a desk with one or more **Confirmed** bookings for today or future dates
- **When** the Admin attempts to deactivate without cancelling those bookings
- **Then** deactivation is blocked with a clear message (BR-001.9, V-09, SCR-005 ST-08)

## Edge cases

- Reactivating an **Inactive** desk returns it to employee availability.
- Open question #6: cancel-in-same-flow vs block-only — default block per BR-001.9.

## UI

Served by **SCR-005 — Manage Desks**. Add/edit modals and deactivate confirmation.

## QA notes

Desk with and without future bookings for AC-05.

## API impacts

Desk CRUD and activate/deactivate endpoints — TBD in architecture.
