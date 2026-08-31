# US-002 — decisions

> Technical choices made while implementing this story, with the reasoning that produced them. A choice that changes the shape of the system is not recorded here. It goes to the Architect as an `ADR-###`.

| ID   | Decision | Rationale | Alternatives rejected |
| ---- | -------- | --------- | --------------------- |
| D-01 | `IOfficeClock` abstraction for office-local dates | Testable date boundaries; NFR-001 compliance | `DateTime.UtcNow` everywhere |
| D-02 | Availability + create on shared `IBookingService` | Web and Api stay in sync | Duplicate logic in controllers |
| D-03 | JWT bearer auth added on Api in this story | Enables API integration tests alongside MVC | Defer all Api auth |
| D-04 | Working-day validation rejects Sat/Sun (BR-001.3) | Matches product rules V-02/V-03 | Allow weekend booking |
| D-05 | Optimistic concurrency on desk booking insert | One winner on concurrent book (RISK-004) | Application-level lock only |

Cite these as `US-002/D-01` outside this folder.
