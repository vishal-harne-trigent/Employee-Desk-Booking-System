# US-001 — impact analysis

> What this change touches, written **before** it touches anything. Read at Gate D1 next to the plan. Required at Complex tier.

|             |                                                                  |
| ----------- | ---------------------------------------------------------------- |
| **Story**   | `inception/stories/user-stories/US-001-sign-in.md`               |
| **Tier**    | Complex                                                          |
| **Updated** | 2026-08-31                                                       |

## Surfaces crossed

| Surface                  | Crossed? | What exactly                                    |
| ------------------------ | -------- | ----------------------------------------------- |
| Contract                 | no       | Cookie auth only on Web; no public API auth endpoints in this slice |
| Persistence              | yes      | New `Users` table, EF migration, `DbInitializer` seed |
| Trust                    | yes      | Password verification, session cookies, role-based routing, deactivated-account guard |
| Dependency & integration | no       | ASP.NET Core Identity primitives only (`IPasswordHasher`) |
| Operational              | no       | No background jobs |

## Files and callers

| File | Symbol | Change | Callers found |
| ---- | ------ | ------ | ------------- |
| `AuthService.cs` | `SignInAsync` | credential check + role return | `AccountController`, tests |
| `AccountController.cs` | `Login`, `Logout` | cookie sign-in/out | default route, views |
| `AppDbContext.cs` | `Users` DbSet | new entity | repositories, initializer |
| `Program.cs` | cookie auth middleware | session configuration | all Web controllers |

## Regression risk

| Area | Risk | Why | Covered by |
| ---- | ---- | --- | ---------- |
| Default routing | medium | Unauthenticated users must land on login | `SignInTests.cs` |
| Role routing | medium | Wrong home breaks Employee/Admin journeys | AC-01, AC-02 tests |
| Password storage | high | Plaintext or weak hashing | NFR-01 via `AspNetPasswordVerifier` |

## Deliberately not touched

- JWT bearer auth on `EmployeeDeskBooking.Api`
- Book desk or admin booking business logic (stub destinations only)
- User administration (US-006)
