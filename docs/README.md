# FluentPOS Documentation

Start here if you are new to this repository.

## Read in this order

| # | Doc | What it answers | Time |
|---|---|---|---|
| 1 | **[getting-started.md](getting-started.md)** | How do I install, configure, run, and log in? | 20 min |
| 2 | **[architecture.md](architecture.md)** | How is this put together, and what must I not break? | 25 min |
| 3 | **[users-and-access.md](users-and-access.md)** | Who can do what? How do I add users, roles, tills? | 15 min |
| 4 | **[seed-data.md](seed-data.md)** | What is in the database after a fresh boot? Fixed GUIDs | 10 min |
| 5 | **[api-reference.md](api-reference.md)** | Where is the endpoint I need? | reference |
| 6 | **[testing-guide.md](testing-guide.md)** | How do I run the tests and exercise every feature by hand? | reference |
| 7 | **[deployment.md](deployment.md)** | How do I build, containerise and deploy this safely? | 20 min |
| 8 | **[troubleshooting.md](troubleshooting.md)** | Why is it broken? | reference |

## Also in this repo

| Doc | Purpose |
|---|---|
| [../EPOS_TRANSFORMATION_PLAN.md](../EPOS_TRANSFORMATION_PLAN.md) | Assessment of the original codebase, target architecture, phase-by-phase status, and the sequenced backlog. **Read this to understand why the code looks the way it does.** |
| [../UBIQUITOUS_LANGUAGE.md](../UBIQUITOUS_LANGUAGE.md) | Domain vocabulary — use these words in code and conversation |
| [../CLAUDE.md](../CLAUDE.md) · [../AGENTS.md](../AGENTS.md) | Conventions for AI coding agents working in this repo |
| [../CONTRIBUTING.md](../CONTRIBUTING.md) | Pull request workflow |
| [adding-extended-attribute-tutorial.md](adding-extended-attribute-tutorial.md) | Tutorial: add a custom field to an entity |
| [api-switching-database-provider-tutorial.md](api-switching-database-provider-tutorial.md) | Tutorial: PostgreSQL → MSSQL |

## Quick answers

| Question | Answer |
|---|---|
| Run the API | `dotnet run --project src/server/API` → <http://localhost:5000> |
| Run everything in Docker | `cp .env.example .env`, set `JWT_KEY`, `docker compose up --build` |
| API docs | <http://localhost:5000/swagger> |
| Offline-first till | <http://localhost:5000/pos> |
| Background jobs | <http://localhost:5000/jobs> (⚠ unauthenticated) |
| Health | `/health/live`, `/health/ready` |
| Log in | `POST api/v1/identity/tokens` with `{ email, password }` |
| Default credentials | `superadmin@fluentpos.com` / `123Pa$$word!` |
| Build | `dotnet build src/server/FluentPOS.sln` |
| Test | `dotnet test src/server/FluentPOS.sln` (33 tests) |
| Migrate by hand | `./migrate-database.ps1` |
| Reset the database | `docker compose down -v`, or drop the `fluentpos` database and restart |

## Where things live

```
src/server/API/                    Host + config + the POS PWA (PosClient/)
src/server/Modules/<Name>/         One module: .Core / .Infrastructure / (controllers)
src/server/Shared/                 Shared.Core · Shared.DTOs · Shared.Infrastructure
src/client/                        Angular 12 back-office (legacy; pre-multi-store features only)
docs/                              This documentation
docker-compose.yml                 API + Postgres 16 + Redis 7
migrate-database.ps1               Applies migrations for all nine DbContexts
```

## Documentation conventions

- Payloads and permission names are taken from source. **When these docs and `/swagger` disagree,
  Swagger is right** — it is generated from the code.
- Known gaps and debt are stated explicitly rather than omitted. If something is not
  production-ready, the doc says so.
- Fixed seeded GUIDs are safe to hardcode in scripts and tests; see
  [seed-data.md](seed-data.md#fixed-guids).
