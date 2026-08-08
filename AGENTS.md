# Repository Guidelines

Conventions for AI coding agents and new contributors. Human-facing documentation lives in
[`docs/`](docs/README.md); see [CLAUDE.md](CLAUDE.md) for the fuller architecture briefing.

## Project Structure & Module Organization
- `src/server/` contains the ASP.NET Core 10 WebAPI solution (`FluentPOS.sln`) plus shared libraries
  and feature modules (`Modules.*`, `Shared.*`).
- `src/server/API/` is the host (`Bootstrapper.csproj`); `src/server/API/PosClient/` is the
  offline-first POS PWA served at `/pos`.
- `src/client/` contains the Angular 12 back-office application (legacy — covers only the
  pre-multi-store feature set).
- `docs/` holds the documentation set; start at `docs/README.md`.
- `postman/` includes an API collection (predates the multi-store work; prefer Swagger).
- `workspace/` provides VS Code workspace settings.

Modules: Identity, Organizations, Catalog, People, Sales, Inventory, Purchasing, Reporting.
`Modules.Accounting.*` is an empty shell — csproj files with no source, not referenced by the host.

## Build, Test, and Development Commands
- `dotnet build src/server/FluentPOS.sln` builds the server solution (0 errors; ~85 known StyleCop
  warnings).
- `dotnet test src/server/FluentPOS.sln` runs the 33 unit tests.
- `dotnet run --project src/server/API` runs the API; `dotnet watch run --project src/server/API` adds
  hot reload.
- `./migrate-database.ps1` applies migrations for all nine DbContexts (needed when
  `PersistenceSettings.MigrateOnStartup` is false).
- `docker compose up --build` runs API + PostgreSQL 16 + Redis 7 (`cp .env.example .env` first and set
  `JWT_KEY`).
- `npm install` in `src/client/` installs client dependencies (needs Node 14/16).
- `npm run start` in `src/client/` serves the Angular app at `http://localhost:4200`.
- `npm run build` in `src/client/` creates a production build.
- `npm test` in `src/client/` runs unit tests (Karma/Jasmine).
- `npm run lint` in `src/client/` runs TSLint.
- `npm run e2e` in `src/client/` runs Protractor end-to-end tests.

## Architecture Rules (non-negotiable)
- Modules **must not** reference each other's projects or write to each other's tables. Cross-module
  access goes through interfaces in `Shared.Core` or MediatR notifications; shared types live in
  `Shared.DTOs`.
- Every store-scoped entity **must** implement `IMustHaveStore`. `ModuleDbContext` applies a global
  query filter and auto-stamps the store on insert — omitting the marker is a tenancy leak.
- Catalog-ish entities that POS nodes sync **must** implement `ISyncTracked`.
- Controllers are `internal sealed` (discovered by `InternalControllerFeatureProvider`) and do nothing
  but `Mediator.Send(...)`.
- Background work goes through `IJobService` (Hangfire), never `Task.Run`.
- Events are in-process only — there is **no transactional outbox**. Do not assume a handler ran.
- Never persist `DateTime.Now` (PostgreSQL `timestamptz` rejects `Kind=Local`); never `OrderBy` after
  `ProjectTo`.

## Coding Style & Naming Conventions
- C#: 4-space indentation, PascalCase for types and public members, `I` prefix for interfaces, the
  FluentPOS copyright header on every file. Style rules are defined in `src/server/stylecop.json` and
  `src/server/fluentpos.ruleset` (`TreatWarningsAsErrors` is off).
- TypeScript/Angular: 2-space indentation, camelCase for variables and methods, `*.component.ts` and
  `*.service.ts` naming patterns.

## Testing Guidelines
- Server unit tests live in `Modules.<Name>.Core.Tests/` (Sales, Purchasing, Reporting) and
  `Shared.Infrastructure.Tests/`, mirroring the source folder structure. xUnit + FakeItEasy; class
  names `<ThingUnderTest>Should`, method names continuing the sentence.
- Add tests when changing core domain or infrastructure logic.
- **There is no integration-test project.** Nothing covers HTTP + permissions + EF query filters
  together, so tenancy changes must be verified manually against the scenarios in
  `docs/testing-guide.md` until that gap is closed.
- Client unit tests live next to source files as `*.spec.ts` and run via `npm test`; end-to-end tests
  are under `src/client/e2e/` and run via `npm run e2e`.

## Commit & Pull Request Guidelines
- Commit messages are present-tense and use scope prefixes — `API:`, `NG:`, `docs:` (see `git log`).
- Branch naming follows `fluentpos-<issueId>` (example: `fluentpos-70`) targeting `master`.
  Transformation-phase work uses `Phase_NN`.
- Run `dotnet build` and `dotnet test` before opening a PR.
- PRs should reference the related issue, summarize changes, and include UI screenshots when changing
  a client.
- Update `docs/` alongside behaviour changes, and record phase progress in
  `EPOS_TRANSFORMATION_PLAN.md`.

## Configuration & Data
- API configuration lives in `src/server/API/appsettings.json` and `appsettings.Development.json`.
  Any key is overridable by environment variable using `__` for nesting (e.g.
  `PersistenceSettings__ConnectionStrings__postgres`).
- Update `PersistenceSettings` connection strings before running locally; PostgreSQL is the default
  and the only provider exercised by CI.
- `MigrateOnStartup` and `SeedOnStartup` default to `true` — the database is created, migrated and
  seeded on first run. Both must be `false` when running more than one replica.
- Seeded logins (password `123Pa$$word!`): `superadmin@fluentpos.com` (head office),
  `staff@fluentpos.com` (Store One), `franchisee@fluentpos.com` (Northern Franchise org). Of the six
  seeded roles, only SuperAdmin, Staff and Manager receive permissions.
- Fixed seeded GUIDs for organizations, stores, terminals, VAT rates, the walk-in customer and the
  sample supplier are documented in `docs/seed-data.md` and defined in
  `Shared.Core/Constants/OrganizationConstants.cs`.
- Never commit secrets. `JwtSettings.Key` in `appsettings.json` is a sample value and must be replaced
  outside local development.
