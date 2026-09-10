# API standards

Applies to `EmployeeDeskBooking.Api`. The OpenAPI document at **`/swagger/v1/swagger.json`** (Swashbuckle) **is** the contract.

## Routing

- All routes under `/api`; lowercase path segments
- Resources plural: `/api/bookings`, `/api/admin/users`, `/api/admin/desks`
- Admin surface under `/api/admin/*` — always `[Authorize(Roles = "Admin")]`
- Employee + admin shared routes: `[Authorize(Roles = "Employee,Admin")]`
- Auth endpoints: `/api/auth/login`, `/api/auth/me`
- Health: `/api/health`
- Versioning: none for release 1; breaking changes need an ADR

## Request and response shapes

- Contract types live in `EmployeeDeskBooking.Api/Contracts/` — never expose Domain entities directly
- Request bodies: explicit properties; use `[FromBody]`, `[FromQuery]`, `[FromRoute]` as appropriate
- Responses: typed DTOs with `[ProducesResponseType]` on every action
- Date fields: `DateOnly` for booking dates (office calendar days)
- IDs: `Guid` for entity identifiers

## Validation

- Validate at the controller edge before calling Application services
- Model binding failures → ASP.NET default `400` with validation problem details
- Business-rule failures from Application → map to explicit status codes (see below)
- Reject unknown or out-of-range inputs; do not silently coerce invalid dates or IDs

## Status codes

| Code | Use |
| ---- | --- |
| `200` | Successful read or update returning a body |
| `201` | Resource created (`CreateBooking`, etc.) |
| `204` | Successful action with no body (delete, cancel where applicable) |
| `400` | Model binding / format errors |
| `401` | Missing or invalid JWT |
| `403` | Authenticated but wrong role |
| `404` | Referenced desk, user, or booking not found |
| `409` | Conflict (desk already booked, duplicate constraint) |
| `422` | Domain rejection — invalid booking date, outside window, inactive desk |
| `500` | Never intentional — unhandled exception |

## Error body

- Use `Problem()` / RFC 7807 `ProblemDetails` via `ControllerBase.Problem`
- Include a short `title` and human-readable `detail`; no stack traces or connection strings in responses
- Reuse message helpers where they exist (e.g. `BookingApiMessages`)

## Auth

- JWT Bearer on all endpoints except `/api/auth/login` and `/api/health`
- Claims: user id, name, email, role — read via `ClaimTypes.NameIdentifier` / custom helpers in controllers
- No API-key header pattern; admin is role-based JWT, not a separate key

## Pagination and collections

- Current release: bounded lists (bookings by date, desk inventory) — no cursor pagination required
- If an unbounded collection is added later: document `page`/`pageSize` with a max and cite an ADR

## Documentation

- Every new or changed endpoint updates Swashbuckle-visible types and `[ProducesResponseType]` attributes
- After API changes, verify `/swagger` reflects the new contract before merging
