# US-006 — decisions

> Technical choices made while implementing this story, with the reasoning that produced them. A choice that changes the shape of the system is not recorded here. It goes to the Architect as an `ADR-###`.

| ID   | Decision | Rationale | Alternatives rejected |
| ---- | -------- | --------- | --------------------- |
| D-01 | `IUserAdminService` separate from `IAuthService` | Admin operations vs authentication | Single user service |
| D-02 | Generated reset password shown once in UI flash/temp | AC-05; never stored plaintext | Email reset link to user |
| D-03 | Last active Admin guard on deactivate and demote (V-11) | Prevents admin lockout | Allow demote with warning |
| D-04 | Reuse `AspNetPasswordVerifier` for admin-set passwords | Consistent with US-001 NFR-003 | Separate hash scheme |

Cite these as `US-006/D-01` outside this folder.
