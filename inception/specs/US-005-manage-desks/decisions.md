# US-005 — decisions

> Technical choices made while implementing this story, with the reasoning that produced them. A choice that changes the shape of the system is not recorded here. It goes to the Architect as an `ADR-###`.

| ID   | Decision | Rationale | Alternatives rejected |
| ---- | -------- | --------- | --------------------- |
| D-01 | Block deactivate when Confirmed today/future bookings exist (V-09) | BR-001.9; avoids cancelling in same flow | Auto-cancel bookings on deactivate |
| D-02 | `IDeskService` separate from `IBookingService` | Single responsibility; desk admin vs booking | Combined service |
| D-03 | Inactive desks filtered in availability query | Employee never sees inactive desks (AC-04) | UI-only hide |
| D-04 | Unique desk number at DB + application layer | Defense in depth for V-08 | UI validation only |

Cite these as `US-005/D-01` outside this folder.
