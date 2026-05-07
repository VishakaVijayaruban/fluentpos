# Repository Guidelines

## Project Structure & Module Organization
- `src/server/` contains the ASP.NET Core WebAPI solution (`FluentPOS.sln`) plus shared libraries and feature modules (`Modules.*`, `Shared.*`).
- `src/client/` contains the Angular application (feature modules under `src/app/`, assets in `src/assets/`).
- `docs/` holds architecture and tutorial docs.
- `postman/` includes the API collection.
- `workspace/` provides VS Code workspace settings.

## Build, Test, and Development Commands
- `dotnet build src/server/FluentPOS.sln` builds the server solution.
- `dotnet watch run --project src/server/API` runs the API with hot reload.
- `npm install` in `src/client/` installs client dependencies.
- `npm run start` in `src/client/` serves the Angular app at `http://localhost:4200`.
- `npm run build` in `src/client/` creates a production build.
- `npm test` in `src/client/` runs unit tests (Karma/Jasmine).
- `npm run lint` in `src/client/` runs TSLint.
- `npm run e2e` in `src/client/` runs Protractor end-to-end tests.

## Coding Style & Naming Conventions
- C#: 4-space indentation, PascalCase for types and public members, `I` prefix for interfaces. Style rules are defined in `src/server/stylecop.json` and `src/server/fluentpos.ruleset`.
- TypeScript/Angular: 2-space indentation, camelCase for variables and methods, `*.component.ts` and `*.service.ts` naming patterns.

## Testing Guidelines
- Client unit tests live next to source files as `*.spec.ts` and run via `npm test`.
- End-to-end tests are under `src/client/e2e/` and run via `npm run e2e`.
- Server test projects are not present; add tests when changing core domain or infrastructure logic.

## Commit & Pull Request Guidelines
- Commit messages are present-tense and often use scope prefixes like `NG:` or `API:` (see `git log`).
- Branch naming follows `fluentpos-<issueId>` (example: `fluentpos-70`) and typically targets `master`.
- PRs should reference the related issue, summarize changes, and include UI screenshots when changing the client.

## Configuration & Data
- API configuration lives in `src/server/API/appsettings.json` and `appsettings.Development.json`.
- Update `PersistenceSettings` connection strings before running locally; PostgreSQL is the default provider.
