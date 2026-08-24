# Security standards

Checked by the Architect persona in design notes and PR review (Gate 2); scanned by the release pipeline (Gate 3).

## Non-negotiables

- **Secrets:** never in code, commits, logs, or docs. Use `appsettings.Development.local.json` (gitignored), user secrets (`EmployeeDeskBooking.Web-dev`), or CI GitHub Secrets. Document key _names_ in `appsettings.Development.local.json.example` — never values
- **Input:** validate at the boundary — model binding on Api/Web, Application services for business rules; reject invalid dates, unknown GUIDs, and out-of-role actions
- **Injection:** DB access through EF Core LINQ only — no string-built SQL; no dynamic code execution
- **AuthN:** Web — HttpOnly cookie, `SameSite=Strict`, secure policy matches request in dev. Api — JWT with validated issuer, audience, signing key, and lifetime
- **AuthZ:** Admin MVC area and `/api/admin/*` require **Admin** role; employees access only their own bookings unless admin. New protected surfaces need a security note in the story PR (ADR when trade-offs are real)
- **Data exposure:** Api returns contract DTOs only — no EF entity serialization; errors leak no internals, connection strings, or stack traces
- **Passwords:** hashed via `IPasswordHasher<User>` (ASP.NET Identity Core); never log or return plaintext passwords
- **Dependencies:** NuGet additions need Architect + human approval; CI must be clean of high/critical vulnerabilities or risk is human-accepted and logged
- **CSRF:** anti-forgery tokens on Web form posts; Api uses JWT (no cookie CSRF on stateless endpoints)
- **CORS:** explicit policy when Api is called from browsers other than same-origin Web — no `*` outside local dev

## EDBS-specific

- SMTP and VAPID private keys in configuration only — verified at startup in Development with a warning when SMTP mode is active but credentials are missing
- Push subscriptions stored per user; unsubscribe on preference change
- No employee self-service password reset — admin-initiated reset only (reduces account-enumeration and token-spray surface)
- Email delivery logs record recipient and status — not message bodies with secrets

## Review prompts (Gate 2 review)

Where does user input enter? Is anything trusted because "it comes from our UI"? Can an Employee call admin routes or another user's booking id? What does this endpoint return that it does not need to? Are reminder/completion jobs idempotent?
