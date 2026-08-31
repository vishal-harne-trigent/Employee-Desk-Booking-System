# US-004 — decisions

> Technical choices made while implementing this story, with the reasoning that produced them. A choice that changes the shape of the system is not recorded here. It goes to the Architect as an `ADR-###`.

| ID   | Decision | Rationale | Alternatives rejected |
| ---- | -------- | --------- | --------------------- |
| D-01 | Separate `AdminCancelBookingAsync` from employee cancel | Clear audit intent; same validation rules | Single cancel method with role flag |
| D-02 | Optional date and status query filters on one list endpoint | Matches SCR-004 filter UX | Separate endpoints per filter |
| D-03 | Admin area under `Areas/Admin` | Isolates Admin authorization policy | Mixed controllers with role checks only |
| D-04 | Reuse stub `AdminBookingsController` from US-001 | Incremental delivery on existing route | New controller name |

Cite these as `US-004/D-01` outside this folder.
