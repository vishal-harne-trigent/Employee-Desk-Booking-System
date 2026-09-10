# US-006 — Admin manage users

> Approval = Gate 1 review of this file's PR. Delivery = the story PR (`feat/US-006-manage-users`) merging with every AC proven by a test named `... (US-006/AC-##)`.

|                |                                         |
| -------------- | --------------------------------------- |
| **Epic**       | EPIC-001                                |
| **Traces to**  | REQ-001, REQ-003, REQ-004, REQ-005, REQ-018, REQ-019, REQ-020, REQ-021, REQ-022, NFR-004, BR-001.10, BR-001.11, BR-001.12 |
| **Priority**   | Must                                    |
| **Estimate**   | 8 pts (AI draft — humans re-estimate)   |
| **Depends on** | US-001                                  |

## Story

As an **Admin**
I want to create and maintain user accounts and roles
So that the right people can sign in and use the system with the correct permissions.

## Acceptance criteria

### AC-01 Create user

- **Given** a signed-in Admin
- **When** they create a user with email, name, role (**Employee** or **Admin**), and initial password
- **Then** the account is created and can sign in (REQ-018)

### AC-02 Reject duplicate email

- **Given** an email already assigned to a user
- **When** the Admin creates or edits a user to that email
- **Then** the save is rejected (BR-001.10, V-10)

### AC-03 Edit user name and email

- **Given** an existing user
- **When** the Admin updates name and/or email (unique)
- **Then** the profile is saved (REQ-019)

### AC-04 Deactivate user

- **Given** an active user who is not the last active Admin
- **When** the Admin deactivates the account
- **Then** the user cannot sign in (REQ-020, REQ-005)

### AC-05 Reset password

- **Given** an existing user
- **When** the Admin resets the password
- **Then** a new password is set and displayed once to the Admin for copy (REQ-021, BR-001.12)

### AC-06 Change role

- **Given** an existing user
- **When** the Admin changes role between **Employee** and **Admin**
- **Then** the role is updated on next sign-in (REQ-022)

### AC-07 Protect last Admin

- **Given** only one active **Admin** remains
- **When** the Admin attempts to deactivate that account or change its role to Employee
- **Then** the action is rejected (BR-001.11, V-11, SCR-006 ST-09)

## Edge cases

- Password complexity (V-12): `TBD (owner: PO/security)` — enforce minimum once decided.
- First Admin bootstrap (open question #1) is out of this story — seed/installer concern.

## UI

Served by **SCR-006 — Manage Users**. Password reset modal shows one-time password with copy encouragement.

## QA notes

Two-Admin fixture for AC-07; deactivated user for sign-in regression with US-001.

## API impacts

User CRUD, deactivate, password reset endpoints — TBD in architecture.
