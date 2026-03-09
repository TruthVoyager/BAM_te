# Stargate API

REST API for the **Astronaut Career Tracking System (ACTS)**. It keeps a master list of People and their Astronaut Duties (rank, title, start/end dates). People are uniquely identified by name; duties are updated from an external service.

**Tech stack:** .NET 8, ASP.NET Core, Entity Framework Core (SQLite), MediatR (CQRS), Swagger/OpenAPI.

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

---

## Setup and run

### 1. Restore and build

From the **api** folder:

```bash
cd tech_exercise_v.0.0.4/tech_exercise/package/exercise1/api
dotnet restore
dotnet build
```

### 2. Configure the database (optional)

The default connection string in `appsettings.json` points to a SQLite file in the api folder:

- **Default:** `Data Source=<path-to-api-folder>\starbase.db`

To use a different path or file name, edit `appsettings.json` (or override in `appsettings.Development.json`):

```json
"ConnectionStrings": {
  "StarbaseApiDatabase": "Data Source=starbase.db"
}
```

Using a relative path like `starbase.db` creates the file in the API’s output directory when the app runs.

### 3. Apply migrations (create/update the database)

Run from the **api** project directory:

```bash
dotnet ef database update
```

This creates or updates the SQLite database using migrations in `Business/Migrations/`. Ensure no other process (e.g. the running API) has the database file open if you see locking errors.

### 4. Run the API

From the **api** folder:

```bash
dotnet run
```

By default the API listens on the URLs shown in the console (e.g. `http://localhost:5204` or `https://localhost:7xxx`). Check `Properties/launchSettings.json` for the exact ports.

- **Swagger UI:** open `https://localhost:<port>/swagger` (or the HTTP URL) in a browser to explore and call endpoints.

---

## Running tests

Tests live in the **StargateAPI.Tests** project (sibling of `api`). Run them from the **exercise1** folder or from the test project folder.

**If you get build errors** (e.g. "Could not copy ... StargateAPI.exe ... file is being used by another process"): **stop the running API** (the StargateAPI process). The test run builds the API project first; it cannot overwrite the exe while the API is running.

### Run tests (no coverage)

From the **exercise1** folder:

```bash
dotnet test StargateAPI.Tests/StargateAPI.Tests.csproj
```

From the **StargateAPI.Tests** folder:

```bash
dotnet test
```

### Run tests with code coverage

To get a coverage report and meet the >50% requirement, use the **coverlet.runsettings** file so that Program, Migrations, and middleware are excluded from coverage:

From the **exercise1** folder:

```bash
dotnet test StargateAPI.Tests/StargateAPI.Tests.csproj --settings StargateAPI.Tests/coverlet.runsettings --collect "XPlat Code Coverage" --results-directory TestResults
```

From the **StargateAPI.Tests** folder:

```bash
dotnet test --settings coverlet.runsettings --collect "XPlat Code Coverage" --results-directory TestResults
```

Coverage output is written under `TestResults/<run-id>/coverage.cobertura.xml`. You can open this with tools like ReportGenerator or your IDE’s coverage viewer. Without the runsettings file, coverage will appear lower because Program, migrations, and middleware are included.

---

## Updating the database (migrations)

After changing entities or the `StargateContext` model:

1. **Add a new migration** (from the **api** folder):

   ```bash
   dotnet ef migrations add <MigrationName> --output-dir Business/Migrations
   ```

   Example:

   ```bash
   dotnet ef migrations add AddNewTable --output-dir Business/Migrations
   ```

2. **Apply migrations** to the database:

   ```bash
   dotnet ef database update
   ```

3. **Revert** to a specific migration (if needed):

   ```bash
   dotnet ef database update <PreviousMigrationName>
   ```

4. **Remove the last migration** (only if not yet applied):

   ```bash
   dotnet ef migrations remove
   ```

Stop the API before running migrations so the database file is not locked.

---

## API endpoints

| Method | Path | Description |
|--------|------|-------------|
| GET | `/Person` | Get all people |
| GET | `/Person/{name}` | Get a person by name (with astronaut summary) |
| POST | `/Person` | Create a person (body: plain text name) |
| PUT | `/Person/{currentName}` | Update a person’s name (body: new name) |
| GET | `/AstronautDuty/{name}` | Get astronaut duties for a person by name |
| POST | `/AstronautDuty` | Add an astronaut duty (body: JSON with name, rankId, dutyTitle, dutyStartDate) |
| GET | `/Rank` | Get all ranks (for dropdowns / UI) |

Responses use a common shape: `success`, `message`, `responseCode`, plus endpoint-specific data (e.g. `person`, `astronautDuties`, `id`).

---

## Project structure

- **`Controllers/`** – REST endpoints; delegate to MediatR and use process logging.
- **`Business/Commands/`** – Create/update operations (e.g. CreatePerson, UpdatePerson, CreateAstronautDuty) and their handlers/preprocessors.
- **`Business/Queries/`** – Read operations (GetPeople, GetPersonByName, GetAstronautDutiesByName) and handlers.
- **`Business/Data/`** – EF entities (Person, AstronautDetail, AstronautDuty, Rank, ProcessLog) and `StargateContext`.
- **`Business/Migrations/`** – EF Core migrations.
- **`Business/Services/`** – `IProcessLogService` / `ProcessLogService` for success and exception logging to the database.
- **`Middleware/`** – `ProcessLoggingMiddleware` logs successful (2xx) requests.

---

## Process logging

- **Successes:** 2xx responses are logged by middleware (path, method, status code) and stored in the `ProcessLog` table.
- **Exceptions:** Controllers catch errors, log them via `IProcessLogService.LogExceptionAsync`, and return a safe response. Logs include message, path, method, status code, and optional stack trace.
- Logs are stored in the database in the `ProcessLog` table (see migration `AddProcessLog`).

---

## Troubleshooting

- **“Database file locked” / build fails copying exe:** Stop the running API (and any other process using the DB or the API’s output folder), then run `dotnet build` or `dotnet ef database update` again.
- **Tests fail with NullReferenceException in controller:** Exception-path tests set `HttpContext` on the controller; ensure you’re not reusing a controller instance that has lost its context.
- **Coverage below 50%:** Use `--settings StargateAPI.Tests/coverlet.runsettings` when running tests so Program, Migrations, and middleware are excluded.
