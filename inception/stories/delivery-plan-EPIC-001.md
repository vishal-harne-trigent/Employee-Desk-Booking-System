# Delivery plan — EPIC-001 Employee Desk Booking

> **Baseline locked** by delivery manager review on **2026-08-17**. This file records the agreed sprint grouping, story-point estimates, and hours mapping. Live sprint boards, assignees, and day-to-day tracking stay in Jira (see ADR-002). Revisions require an explicit change — not silent drift.

|                  |                                                                                  |
| ---------------- | -------------------------------------------------------------------------------- |
| **Epic**         | EPIC-001                                                                         |
| **Traces to**    | BRD-001, SRS-001, US-001 … US-009                                                |
| **Locked by**    | Delivery manager (human approval in conversation, 2026-08-17)                    |
| **Capacity assumption** | ~40 person-hours per two-week sprint (part-time review/approval); ~80 h full-time |

## Story point → hours mapping

| Story Points | Approximate Hours       |
| ------------ | ----------------------- |
| **1 SP**     | 2–4 Hours               |
| **2 SP**     | 4–8 Hours               |
| **3 SP**     | 1 Day (6–8 Hours)       |
| **5 SP**     | 2–3 Days (12–20 Hours)  |
| **8 SP**     | 4–5 Days (24–40 Hours)  |
| **13 SP**    | 1–2 Weeks (40–80 Hours) |

Story points on each user story are AI drafts from Gate 1; hours below apply this mapping.

## Per-story effort

| Story | Title | SP | Hours |
| ----- | ----- | -- | ----- |
| US-001 | Sign in / sign out | 5 | 12–20 |
| US-002 | Book a desk | 8 | 24–40 |
| US-003 | My bookings | 5 | 12–20 |
| US-004 | Admin all bookings | 5 | 12–20 |
| US-005 | Admin manage desks | 5 | 12–20 |
| US-006 | Admin manage users | 8 | 24–40 |
| US-007 | Booking email notifications | 8 | 24–40 |
| US-008 | Browser push preferences | 5 | 12–20 |
| US-009 | Auto-complete past bookings | 3 | 6–8 |
| **Stories subtotal** | | **52 SP** | **138–228 h** |

## Sprint 0 — Foundation (fixed)

Sprint 0 is **mandatory** before any user story delivery begins.

| Work | SP | Hours | Owner |
| ---- | -- | ----- | ----- |
| Database + app architecture (Gate 1 design docs) | 5 | 12–20 | Architect |
| CI / repo scaffold | 3 | 6–8 | DevOps |
| Open-question decisions (first Admin bootstrap, password policy) | 2 | 4–8 | PO + Architect |
| **Sprint 0 subtotal** | **10 SP** | **22–36 h** | |

**Exit criteria**

- Architecture PR merged (db design + app architecture)
- First Admin account bootstrap approach decided (BRD open Q #1)
- Password minimum length/complexity decided (BRD open Q #5)
- Minimal CI runs `aidlc-check` and test-runner shell

## Sprint schedule (two-week sprints)

| Sprint | Stories | SP | Hours | Cumulative hours |
| ------ | ------- | -- | ----- | ---------------- |
| **0 — Foundation** | Architecture, CI, decisions | 10 | 22–36 | 22–36 |
| **1 — Auth + booking core** | US-001, US-002 | 13 | 36–60 | 58–96 |
| **2 — Employee wrap-up** | US-003, US-009 | 8 | 18–28 | 76–124 |
| **3 — Admin** | US-004, US-005, US-006 | 18 | 48–80 | 124–204 |
| **4 — Notifications (MVP)** | US-007, US-008 | 13 | 36–60 | 160–264 |

**Grand total:** 62 SP · **160–264 person-hours** (~212 h midpoint)

**Calendar (indicative):** ~10 weeks at 40 h/fortnight; ~5–6 weeks at 80 h/fortnight.

### Sprint 1 detail

- **US-001** must merge before **US-002** starts (auth + app shell).
- **US-002** is the largest employee-facing story (8 SP).

### Sprint 2 detail

- **US-003** depends on US-001 and US-002.
- **US-009** (background completion job) can start once US-002 merges.

### Sprint 3 detail

- **US-004**, **US-005**, **US-006** all depend on US-001 only; may run in parallel after Sprint 1.
- Resolve BRD open Q **#6** (deactivate desk with future bookings) before or during US-005.

### Sprint 4 detail

- **US-007** depends on US-002, US-003, US-004 (booking events exist).
- Resolve BRD open Q **#7** (SMTP sender/domain) before US-007 merge; Q **#3** default 08:00 office local is acceptable interim.
- **US-008** can run in parallel with US-007 after employee flows exist.

## Risks

| Risk | Impact | Mitigation | Owner |
| ---- | ------ | ---------- | ----- |
| SMTP not provisioned (BRD Q #7) | Blocks US-007 | Dev mail catcher in Sprints 1–2; prod SMTP by Sprint 4 | PO / IT |
| First Admin bootstrap undecided (BRD Q #1) | Blocks realistic auth/user tests | Decide in Sprint 0; seed script is default | PO / Architect |
| Holiday calendar undefined (BRD Q #2) | Bookings on company holidays | Ship Mon–Fri rule; calendar as change request | PO / client |
| Mobile vs desktop (BRD Q #4) | UI rework if decided late | Default desktop-first responsive; confirm Sprint 0 | PO / client |
| Sprint 3 overload (18 SP) | Slippage | Move US-006 to Sprint 4 if needed | Delivery manager |

## Shareable summary

> **Employee Desk Booking — EPIC-001 baseline**  
> 9 stories · 52 SP · 44 acceptance criteria · 7 screens  
> **160–264 h** total (incl. fixed Sprint 0 foundation)  
> **MVP:** end of Sprint 4 (notifications)  
> **Next:** Sprint 0 — `/architect` for system design PR

## Related

- Epic: `inception/stories/epics/EPIC-001-desk-booking.md`
- Requirements: `inception/product/requirements/BRD-001-desk-booking.md`, `SRS-001-desk-booking.md`
- Traceability: `knowledge/traceability/manifest.json`
