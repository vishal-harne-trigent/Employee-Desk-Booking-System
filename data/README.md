# Development dataset — `dataset.json`

Edit **`data/dataset.json`** to change users, desks, and bookings.

Bookings use `workingDayOffset` from office **today** (Mon–Fri, `Office:TimeZone`):

| Offset | Meaning |
|--------|---------|
| `0` | Today (skipped on weekends) |
| `1` | Next working day |
| `-1` | Previous working day |

Default password: `Password1!` (or `defaultPassword` in the JSON file).

Sign in: `vishal_h@trigent.com` / `Password1!` or `admin@trigent.com` / `Password1!`

---

## Mode 1 — Daily development (database + dataset, data persists)

**Default** when you run with `Development` environment. Uses a **SQLite file** at `data/employeedeskbooking.db` — no LocalDB or SQL Server install required.

- Bookings and changes **survive restarts**
- `data/dataset.json` is loaded **only when the database is empty** (first run)
- To reload from JSON after you have data: `dotnet run --project tools/SeedDatabase -- json --reset`

```powershell
dotnet run --project src/EmployeeDeskBooking.Web
dotnet run --project src/EmployeeDeskBooking.Api
```

Web: http://localhost:5198 — API: http://localhost:5285/swagger

---

## Mode 2 — Share / demo (no database install)

For someone who only has .NET — **no LocalDB, no SQLite file to manage**. Uses in-memory storage loaded from `data/dataset.json` on each startup.

```powershell
dotnet run --project src/EmployeeDeskBooking.Web --launch-profile demo
dotnet run --project src/EmployeeDeskBooking.Api --launch-profile demo
```

Or set the environment once:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Demo"
dotnet run --project src/EmployeeDeskBooking.Web
```

> **Note:** Demo mode does not persist data between runs. Web and API each keep their own in-memory copy (fine for UI walkthroughs; use Mode 1 if both apps must share the same bookings).

SMTP, push notifications, and background jobs are disabled in Demo config.

---

## Seed commands (reload or reset the SQLite file)

```powershell
dotnet run --project tools/SeedDatabase -- json
dotnet run --project tools/SeedDatabase -- json --reset
dotnet run --project tools/SeedDatabase -- minimal
dotnet run --project tools/SeedDatabase -- none
dotnet run --project tools/SeedDatabase -- init
```

---

## Optional — SQL Server / LocalDB

In `appsettings.Development.json` (or a `.local` override):

```json
"Database": { "Provider": "SqlServer" },
"Seed": {
  "Mode": "Json",
  "DatasetPath": "data/dataset.json",
  "JsonOnlyIfEmpty": true,
  "JsonReplaceExisting": false
}
```

Then run `dotnet run --project tools/SeedDatabase -- json --reset` once to load JSON into LocalDB.
