# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

### Server (ASP.NET Core)
```bash
# Build the solution
dotnet build src/server/FluentPOS.sln

# Run API with hot reload (from repo root)
dotnet watch run --project src/server/API

# Add EF migration for a module (run from the Infrastructure project directory)
dotnet ef migrations add <MigrationName> --context <ModuleName>DbContext
```

### Client (Angular)
```bash
# From src/client/
npm install
npm run start        # dev server at http://localhost:4200
npm run build        # production build
npm test             # unit tests (Karma/Jasmine)
npm run lint         # TSLint
npm run e2e          # Protractor end-to-end tests
```

### Local Configuration
Before running, update `src/server/API/appsettings.json`:
- `PersistenceSettings.ConnectionStrings.postgres` — default: `Host=localhost;Database=fluentpos;Username=postgres;Password=root`
- Database migrations and seed data run automatically on startup.
- Default credentials: `superadmin@fluentpos.com / 123Pa$$word!` and `staff@fluentpos.com / 123Pa$$word!`

## Architecture

FluentPOS is a **modular monolith** — a single deployable API composed of isolated feature modules, each following Clean Architecture (Onion/Hexagonal).

### Module Structure

Every module under `src/server/Modules/<Name>/` has three projects:

| Project | Purpose |
|---|---|
| `Modules.<Name>.Core` | Domain entities, MediatR commands/queries/events, FluentValidation validators, AutoMapper profiles, `I<Name>DbContext` interface |
| `Modules.<Name>.Infrastructure` | EF Core `DbContext`, migrations, seeders, services implementing Core interfaces |
| `Modules.<Name>` | ASP.NET Controllers, module registration extension (`Add<Name>Module`) |

### How Modules Are Wired

`src/server/API/Startup.cs` calls `Add<Name>Module(_config)` for each module in `ConfigureServices`. Each module's `ModuleExtensions.cs` registers its own controllers (via `IMvcBuilder`) and services. Shared infrastructure (`AddSharedInfrastructure`) sets up middleware, Swagger, JWT, CORS, and global exception handling — all modules inherit this.

### Cross-Module Communication Rules

- Modules **cannot directly reference each other's projects** or modify each other's database tables.
- Cross-cutting concerns are handled via **interfaces** in `Shared.Core` and **domain events** via MediatR (`INotification` / `INotificationHandler`).
- Shared DTOs live in `Shared.DTOs` and are the only safe way to pass data across module boundaries.

### Shared Libraries

- `Shared.Core` — base entity types (`BaseEntity`, `AuditableEntity`), shared interfaces (`ICurrentUserService`, etc.), extension methods, pagination helpers.
- `Shared.Infrastructure` — middleware pipeline, JWT auth, Swagger, AutoMapper, Serilog, global exception handling, EF audit interceptors.
- `Shared.DTOs` — request/response DTOs shared across modules and the API surface.

### Feature Pattern (CQRS + MediatR)

Within each module's Core project, features follow this layout:
```
Features/<Entity>/
  Commands/         # IRequest<T> command records + IRequestHandler
  Commands/Validators/  # FluentValidation AbstractValidator<TCommand>
  Events/           # INotification domain events + INotificationHandler
  Queries/          # IRequest<T> query records + IRequestHandler
  Queries/Validators/
```

### Angular Client

The Angular app (`src/client/`) mirrors the module structure under `src/app/modules/` (auth, admin, home, pos). Core services, guards, and interceptors are in `src/app/core/`. JWT tokens are obtained from `api/identity/tokens` and attached by an HTTP interceptor.

## Code Style

- **C#**: StyleCop analyzers enforced via `src/server/stylecop.json` and `src/server/fluentpos.ruleset`. 4-space indent, PascalCase for types and public members, `I` prefix for interfaces.
- **TypeScript/Angular**: 2-space indent, camelCase for variables and methods, `*.component.ts` / `*.service.ts` naming.
- **Commit messages**: Present-tense with scope prefix — e.g., `API: add product search endpoint`, `NG: fix cart total calculation`.
- **Branch naming**: `fluentpos-<issueId>` targeting `master`.
