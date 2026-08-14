# Client discussion — initial requirements (verbatim)

**Date:** 2026-08-13  
**Source:** Client discussion, captured by PO/BA human via BA persona interview

---

## Purpose

Web app for a hybrid office where employees book a desk before coming in. Single office location.

## Users

- **Employee** — book and manage own desks
- **Admin** — manage desks, users, and all bookings

## Core functional requirements

### Authentication

- Sign in / sign out with email and password
- Two roles: Employee and Admin
- Deactivated users cannot sign in

### Employee

- Pick a date (today through +30 days) and see desk availability
- Book one available desk for one date
- View own bookings (past and future)
- Cancel future bookings

### Admin

- View all bookings (filter by date/status)
- Cancel bookings on behalf of employees

---

## Clarifications from PO/BA interview (same session)

| Topic | Decision |
| ----- | -------- |
| Desk and user account provisioning | **Not decided** (in-app admin vs external setup) |
| One desk per employee per day | **Yes** |
| Employee cancellation | **Today and future** (not past) |
| Working days | **Mon–Fri only** (weekends not bookable) |
| Booking statuses | **Confirmed**, **Cancelled**, **Completed** (client agreed) |
| Password reset | **Out of scope** for current stage |
| Admin cancellation | **Today and future only** (not past) |
| Company holidays | **Not decided** (how holidays are maintained) |
| Desk identity | **Unique desk numbers** (e.g. A-01, B-02, C-05); employee picks a specific desk |
| Date/time basis | **Office local timezone** |
| Change desk on same day | **Cancel first, then book again** (no in-place swap) |
| Email / notifications | **Not yet discussed with client** |
