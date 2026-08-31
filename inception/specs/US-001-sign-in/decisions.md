# US-001 — decisions

> Technical choices made while implementing this story, with the reasoning that produced them. A choice that changes the shape of the system is not recorded here. It goes to the Architect as an `ADR-###`.

| ID   | Decision | Rationale | Alternatives rejected |
| ---- | -------- | --------- | --------------------- |
| D-01 | Cookie authentication on Web only | Fastest path for MVC sign-in; API JWT deferred to US-002 | JWT on Web and Api in one story |
| D-02 | `IAuthService` in Application layer | Keeps controllers thin; testable without HTTP | EF queries in `AccountController` |
| D-03 | Generic error for invalid credentials (AC-03) | Prevents account enumeration | Distinct messages per failure reason |
| D-04 | `DbInitializer` seeds dev users when empty | Unblocks local and integration tests without installer | Manual SQL seed scripts |
| D-05 | Post-login stub controllers for Book and Admin | Satisfies routing ACs before US-002/US-004 | Full feature UI in US-001 |

Cite these as `US-001/D-01` outside this folder.
