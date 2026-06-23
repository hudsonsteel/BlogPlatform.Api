# BlogPlatform.Api

[![CI](https://github.com/hudsonsteel/BlogPlatform.Api/actions/workflows/ci.yml/badge.svg)](https://github.com/hudsonsteel/BlogPlatform.Api/actions/workflows/ci.yml)

A RESTful API for a simple blogging platform. Backend coding challenge submission.

## Overview

The API manages blog posts and their associated comments, exposing four endpoints:

| Method | Route | Purpose |
|---|---|---|
| `GET` | `/api/posts` | List all blog posts with comment count |
| `POST` | `/api/posts` | Create a new blog post |
| `GET` | `/api/posts/{id}` | Get a post with its content and comments |
| `POST` | `/api/posts/{id}/comments` | Add a comment to a post |
| `GET` | `/health` | Service health check |

## Tech stack

- **.NET 10** / **C# 14**
- **ASP.NET Core** with Controllers
- **Entity Framework Core 10** + **SQLite**
- **FluentValidation** for request validation
- **Swagger / OpenAPI** for interactive API documentation
- **Serilog** for structured logging
- **Global exception handling middleware** (`IExceptionHandler`) with ProblemDetails (RFC 7807)
- **xUnit + FluentAssertions + Moq** for unit tests
- **GitHub Actions** for CI
- **Docker** for containerization

## Architecture

Clean Architecture with strict dependency direction:

```
Presentation  →  Application  →  Domain
       ↓                ↑
Infrastructure  ────────┘
```

| Layer | Responsibility |
|---|---|
| `BlogPlatform.Domain` | Entities, value objects, domain rules. Zero framework dependencies. |
| `BlogPlatform.Application` | Use cases, DTOs, validators, interfaces. |
| `BlogPlatform.Infrastructure` | EF Core DbContext, repositories, migrations. |
| `BlogPlatform.Presentation` | Controllers, middleware, Program.cs, Swagger. |
| `BlogPlatform.Tests.Unit` | xUnit tests for use cases and domain logic. |

## Patterns & Principles

| Pattern | How it's applied |
|---|---|
| **Clean Architecture** | Strict dependency direction (Presentation → Application → Domain), Infrastructure plugged in via interfaces. |
| **Thin controllers** | Controllers contain only routing and `HandleResult` translation. Zero business logic, zero validation, zero `try/catch`. All work happens in use cases. |
| **Result pattern** | Use cases return `Result<T>` for expected outcomes (validation failure, not found, conflict). Exceptions are reserved for truly unexpected failures and handled by the global middleware. |
| **Guard clauses / Early return** | Validation runs first; invalid input returns immediately. No nested `if/else` ladders. |
| **Repository + Unit of Work** | Application defines interfaces; Infrastructure provides the EF Core implementations. Transactions are committed explicitly via UoW, not per-method. |
| **Dependency Inversion (SOLID)** | All Infrastructure dependencies flow inward through interfaces defined in Application. |
| **Single Responsibility** | One use case per workflow. No service classes that grow into "god objects". |
| **DRY** | Shared `ApiControllerBase` with `HandleResult` helper, base `Entity` (Id, CreatedAt), `Directory.Build.props` for MSBuild config, `.editorconfig` for code style. |
| **Global exception handling middleware** | `IExceptionHandler` returns ProblemDetails (RFC 7807) with correlation ID. Logged through Serilog. |

## How to run

### Option 1 — Local (.NET SDK)

Requires .NET 10 SDK.

```bash
dotnet restore
dotnet build
dotnet run --project src/BlogPlatform.Presentation
```

Open Swagger at `http://localhost:5xxx/swagger` (the exact port is printed at startup).

### Option 2 — Docker

```bash
docker compose up --build
```

Coming in Phase 5. See "Next Steps" below.

## How to test

```bash
dotnet test
```

## Architecture decisions

| Decision | Why |
|---|---|
| Clean Architecture (4 projects) | Predictable dependency direction, easy to test, mirrors enterprise patterns. |
| SQLite | Zero configuration for the reviewer to run. EF Core migrations handle schema. |
| Result pattern (no throw for expected failures) | Distinguishes between *expected* outcomes (validation, not found) and *unexpected* errors (caught by middleware). |
| FluentValidation | Explicit, testable, separated from DTOs. |
| Controllers (not Minimal APIs) | Better fit for the enterprise context this API mirrors; cleaner separation for unit tests. |
| Given-When-Then tests | Tests read as specifications. Matches the pattern used in production projects. |

## AI workflow

This project was developed using Claude Code with a custom setup under `.claude/`:

- `.claude/CLAUDE.md` — project context (architecture, conventions, build commands).
- `.claude/commands/` — playbooks for recurring tasks (`add-endpoint.md`, `add-use-case.md`, `add-test.md`).

The setup ensures consistent code generation aligned with the architectural decisions above.

## Next steps

Given more time, the following would be added:

- **Authentication & Authorization** (JWT + refresh tokens)
- **Pagination** (cursor-based for `/api/posts`)
- **Integration tests** with `WebApplicationFactory` and an in-memory SQLite database
- **Caching** (Redis or in-memory for hot reads)
- **Rate limiting** on write endpoints
- **Full observability** (OpenTelemetry, structured logs, metrics export)
- **Frontend SPA** (React + TypeScript) — see my [Nosso Bairro project](https://nosso-bairro.online) for a full-stack reference (.NET 10 backend + React PWA frontend + Azure Container Apps + Cloudflare)
- **Branch protection** — `Require status checks to pass before merging` enforcing this CI workflow

## Git workflow

- `main`: protected branch, production-clean. Direct pushes are blocked; only merged via PR from `develop`.
- `develop`: integration branch. Feature branches merge here first.
- `feature/*`: per logical chunk of work (setup, posts endpoints, comments endpoints, tests, docs).
- All commits follow [Conventional Commits](https://www.conventionalcommits.org/).

## License

MIT. See [LICENSE](./LICENSE).
