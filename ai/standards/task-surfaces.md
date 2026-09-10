# Project task surfaces

Extends [`ai/context/task-classification.md`](../context/task-classification.md) with the surfaces **this codebase** has. The framework file names the five boundaries every AI-DLC project shares; this one names them in our files and adds what our stack has that the generic list does not.

**This file is project-owned** — not in `ai/framework-lock.json`, so the team edits it freely. Two rules: you may **add** surfaces and **named** Medium carve-outs; you may **not** remove or demote a framework surface (open a `change-request` issue upstream instead).

## Protected paths — always Complex

Any change under these is Complex regardless of diff size:

- `src/EmployeeDeskBooking.Infrastructure/Data/Migrations/**` — schema history
- `.github/workflows/**`, `ai/framework-lock.json` — gate machinery
- `inception/design/tokens.css` — the design system's single source
- `knowledge/traceability/manifest.json` — traceability graph (edit in same PR as linked artifacts)

## API (`src/EmployeeDeskBooking.Api`)

**Complex** — a new controller or route; a new or changed **required** contract field; new JWT claim or auth policy; new `[Authorize]` role combination; Swagger-visible breaking change.

**Medium** — a change inside an existing controller action mapping; a new optional contract field where the Application layer already supports it; a new `[HttpGet]` reusing an existing service method with no schema change.

## Application (`src/EmployeeDeskBooking.Application`)

**Complex** — a new public service interface; a changed business rule (`BR-001.*`) with cross-story impact; new failure reason enum value that changes API contracts.

**Medium** — internal refactor within one service; new repository method supporting an existing use case; email template wording change with no rule change.

## Infrastructure (`src/EmployeeDeskBooking.Infrastructure`)

**Complex** — new or altered EF migration; new external integration (email provider, push); change to hosted job schedule, retry, or idempotency; new `IHostedService`.

**Medium** — repository query optimization with no migration; configuration binding change with same behavior; MailKit/WebPush internals with same public contract.

## Web (`src/EmployeeDeskBooking.Web`)

**Complex** — new area or top-level route; change to cookie auth configuration; shared layout or navigation affecting all roles; new admin screen with new server-side behavior.

**Medium** — view or view-model change within one existing screen; CSS using existing tokens; Razor markup fix with no new Application calls.

## Domain (`src/EmployeeDeskBooking.Domain`)

**Complex** — new entity; new enum value that changes persisted data semantics; removed or renamed property on a persisted entity.

**Medium** — new helper on an entity with no schema impact; documentation-only enum comment.

## Scripts and jobs (`tools/`)

**Complex** — a new script under `tools/`; a change to a job's schedule, retry, or idempotency behavior; any script that reads or writes production data, holds a production credential, or performs a bulk/destructive operation; a change to CLI arguments or exit codes when something else calls it.

**Medium** — internals of an existing script (`SendTestEmail`, `RunReminderEmail`) with the same inputs, outputs, and blast radius.

**Also Complex regardless of location:** rendering unsanitized user HTML in Razor, bypassing Application services to query `AppDbContext` from Web/Api controllers, or storing secrets in committed config.

## Medium carve-outs

Work our stack over-tiers. Each must be *named* — a general "use judgement" clause is not a carve-out:

- Adding a nullable column via EF migration **and** the corresponding optional contract field in the same PR — Medium, not Complex
- A new `[HttpGet]` read endpoint following an existing controller pattern, reusing Application service + response DTO — Medium
- `dotnet ef migrations add` output committed unmodified except for the intended schema change — Medium

## Escalate, do not decide

Surfaces where the persona stops and asks the human even at Medium: adding a NuGet dependency, changing JWT signing keys or cookie policy, any migration that is not additive, and Web→Api refactor boundaries.
