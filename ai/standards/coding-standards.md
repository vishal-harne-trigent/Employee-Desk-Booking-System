# Coding standards

Applies to all code under `src/` and `tests/`. Personas load this when implementing or reviewing code (Gate 2).

## General (all C#)

- **.NET 8**, nullable reference types enabled; avoid `null!` unless justified with a comment
- Small, single-purpose types and methods; SOLID, DRY, KISS — but no speculative abstraction
- Names say what; comments say why; comment density matches surrounding code
- No dead code, no commented-out code in commits
- Use `CancellationToken` on async public APIs; pass it through to EF Core and `IEmailSender`
- Prefer **primary constructors** and `sealed` classes where the codebase already does (services, controllers)
- Build and test via **`dotnet`** — never hand-edit generated migration designer files except through `dotnet ef`
- All tasks: `dotnet build EmployeeDeskBooking.sln`, `dotnet test EmployeeDeskBooking.sln`

## Layering (Clean Architecture)

| Project | May reference | Must not |
| ------- | ------------- | -------- |
| `EmployeeDeskBooking.Domain` | — | Application, Infrastructure, Web, Api |
| `EmployeeDeskBooking.Application` | Domain | Infrastructure, Web, Api, EF Core |
| `EmployeeDeskBooking.Infrastructure` | Domain, Application (interfaces) | Web, Api |
| `EmployeeDeskBooking.Api` | Application | Infrastructure internals (`AppDbContext` in controllers) |
| `EmployeeDeskBooking.Web` | Application *(as-built)* | Direct EF / SQL *(target: HTTP client to Api only)* |

- **Application** defines interfaces (`IBookingRepository`, `IEmailSender`); **Infrastructure** implements them
- Business rules live in **Application** services (`BookingService`, `ReminderEmailService`); Web and Api are thin adapters
- **Domain** holds entities, enums, and domain-specific exceptions only — no ASP.NET or EF attributes

## Application (`EmployeeDeskBooking.Application`)

- One service per capability area (`IBookingService`, `IAuthService`, …) with a single `*Service` implementation
- Return **result types** or nullable outcomes for expected failures (e.g. `CreateBookingResult` with failure reason) — do not throw for business-rule rejections unless the codebase already uses exceptions for that path
- Repository interfaces stay in Application; query shapes use domain types or small DTO records in Application
- No `IConfiguration`, `HttpContext`, or `DbContext` in Application services

## Infrastructure (`EmployeeDeskBooking.Infrastructure`)

- EF Core: entity configuration in `Data/*Configuration.cs`; schema changes **only** through migrations (`dotnet ef migrations add`)
- Repositories: `Ef*Repository` classes implementing Application interfaces; use LINQ — no string-built SQL
- Cross-cutting: email (`MailKitEmailSender`, `FileDropEmailSender`), push (`WebPushNotificationSender`), time (`OfficeClock`)
- Hosted jobs (`IHostedService`) invoke Application services — do not duplicate business logic in jobs
- Register services in `DependencyInjection.cs`; options via `IOptions<T>` bound from configuration sections

## API (`EmployeeDeskBooking.Api`)

- Controllers under `Controllers/`; routes prefixed `api/`; resources plural (`api/bookings`, `api/admin/desks`)
- **Thin controllers:** extract user id from claims → call Application service → map to contract response
- Request/response types in `Contracts/` — separate from Domain entities
- Auth: JWT Bearer; `[Authorize(Roles = "Employee,Admin")]` or `[Authorize(Roles = "Admin")]` on admin routes
- Domain rejections → `Problem()` with appropriate status (`422`, `409`, `404`); see `api-standards.md`
- Swagger annotations via `[ProducesResponseType]`; OpenAPI at `/swagger/v1/swagger.json` is the contract

## Web (`EmployeeDeskBooking.Web`)

- MVC areas: `Areas/Admin/` for admin screens; shared views under `Views/`
- View models in `Models/` or `Areas/Admin/Models/` — no domain entities passed to Razor views
- Cookie authentication; anti-forgery on form posts
- Styling: Bootstrap 5 + project CSS (`wwwroot/css/edbs.css`); design tokens in `inception/design/tokens.css`
- Keep controllers thin: validate model state → delegate to Application service → return view or redirect

## Domain (`EmployeeDeskBooking.Domain`)

- Entities are POCOs; status enums as `: byte` where existing pattern applies
- No framework references in the `.csproj`
- Enums and constants that encode business vocabulary (`BookingStatus`, `UserRole`, `EmailType`)
