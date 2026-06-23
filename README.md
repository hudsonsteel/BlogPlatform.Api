# BlogPlatform.Api

[![CI](https://github.com/hudsonsteel/BlogPlatform.Api/actions/workflows/ci.yml/badge.svg)](https://github.com/hudsonsteel/BlogPlatform.Api/actions/workflows/ci.yml)

A RESTful API for a simple blogging platform — manage blog posts and their associated comments. Backend coding challenge submission.

## Overview

| Method | Route | Purpose |
|---|---|---|
| `GET` | `/api/posts` | List all posts (id, title, createdAt, comment count) |
| `POST` | `/api/posts` | Create a new post |
| `GET` | `/api/posts/{id}` | Get a post with its content and comments |
| `POST` | `/api/posts/{id}/comments` | Add a comment to a post |
| `GET` | `/health` | Liveness check |
| `GET` | `/swagger` | Interactive API documentation |

## Quick start

```bash
git clone https://github.com/hudsonsteel/BlogPlatform.Api.git
cd BlogPlatform.Api
dotnet run --project src/BlogPlatform.Presentation
```

Then open `http://localhost:5183/swagger`. The SQLite database is created automatically; migrations run at startup.

To verify the full set of endpoints and edge cases without clicking, import the three files under `/postman/` and run the **BlogPlatform.Api - Integration** collection. **33 assertions** pass against a fresh database.

## Tech stack

- **.NET 10** / **C# 14**
- **ASP.NET Core** with Controllers (thin) + Swagger
- **Entity Framework Core 10** + **SQLite**
- **FluentValidation** for entity-level rules
- **Serilog** (console + rolling file) with request logging
- **Global exception handling middleware** (`IExceptionHandler`) returning RFC 7807 ProblemDetails
- **xUnit + FluentAssertions + Moq** — 36 unit tests
- **GitHub Actions** for CI (build + test on push/PR)
- **Docker** + **docker compose** for containerized runs

## Architecture

Clean Architecture with strict dependency direction:

```
Presentation  →  Application  →  Domain
       ↓                ↑
Infrastructure  ────────┘
```

| Layer | Responsibility |
|---|---|
| `BlogPlatform.Domain` | Entities, validators, `Entity` base with notification pattern, repository interfaces. Only depends on FluentValidation. |
| `BlogPlatform.Application` | Use cases, DTOs, mappers (`*BuilderMap`), query interfaces (`IBlogPostQueries`), `Result<T>` / `Error`. |
| `BlogPlatform.Infrastructure` | EF Core `DbContext`, `EntityConfiguration<T>` base + per-entity configs, repositories, queries, migrations, DI registration. |
| `BlogPlatform.Presentation` | ASP.NET Core controllers, `ApiControllerBase` (`HandleResult`), `GlobalExceptionHandler`, `Program.cs`, Swagger, Serilog. |
| `BlogPlatform.Tests.Unit` | xUnit tests for use cases, mappers, validators and entity behavior. |

### Repository vs Queries (CQRS-lite)

- **Write side (Domain)**: `IBlogPostRepository` works with aggregates only. A single `GetByIdAsync` returns the **full aggregate** — aggregates are atomic by design. `AddAsync(BlogPost)` is the only mutation; children are added through the aggregate root (`post.AddComment(comment)`).
- **Read side (Application)**: `IBlogPostQueries` returns projected DTOs directly from the database. The shape of read models is an Application concern, not a Domain one.

## Patterns & Principles

| Pattern | How it's applied |
|---|---|
| **Clean Architecture** | Strict dependency direction; Infrastructure plugged in through interfaces. |
| **CQRS-lite** | Domain repository for the write side; Application query interfaces for projected reads. |
| **Aggregate Root** | Repository methods load and mutate by aggregate. Children (comments) are accessed through `BlogPost`. |
| **Notification pattern** | The `Entity` base class collects notifications instead of throwing inside constructors. Use cases call `await entity.ThrowIfInvalidAsync(validator, ct)`, an extension method that runs the validator, copies failures into the entity's notifications and throws `DomainValidationException` if any. The global middleware catches it and returns 400 ProblemDetails. |
| **Result pattern** | Use cases return `Result<T>` for expected business outcomes (`NotFound`, `Conflict`). Exceptions are reserved for validation failures (caught by middleware) and truly unexpected errors. |
| **Thin controllers** | Every controller action is 4-7 lines. Inject the use case, call `Handle`, return `HandleResult(result)`. No `if`, no `try`, no business logic, no validation calls. |
| **Mappers (`*BuilderMap`)** | `BlogPostBuilderMap`, `CommentBuilderMap` translate between DTOs and entities. Use cases never `new` an entity directly. |
| **Primary constructors for DI** | Use cases, repositories, handlers all use C# 12+ primary constructors. No boilerplate fields + constructors. |
| **EntityConfiguration<T>** | A base EF Core configuration owns the shared bits (PK, value-generated Id, required `CreatedAt`, ignored notification members). Per-entity configurations declare only their table + entity-specific properties. |
| **Guard clauses / early return** | Validation first, return on first failure. No nested `if/else` ladders. `csharp_prefer_braces = when_multiline` in `.editorconfig`. |
| **Global exception middleware** | `IExceptionHandler` writes through `IProblemDetailsService.TryWriteAsync`. A `CustomizeProblemDetails` callback stamps a fresh GUID `traceId` on every error response. Both 404 (from controller's `Problem()`) and 400/500 (from middleware) produce identical shape. |

## How to run

### Option 1 — Local (.NET SDK)

Requires .NET 10 SDK.

```bash
dotnet restore
dotnet build
dotnet run --project src/BlogPlatform.Presentation
```

Swagger auto-opens at `http://localhost:5183/swagger` (configured in `launchSettings.json`).

### Option 2 — Docker

Requires Docker.

```bash
docker compose up --build
```

The container exposes port `5183`. Visit `http://localhost:5183/swagger`. SQLite data persists in a named Docker volume (`blogplatform-data`); logs in `blogplatform-logs`. To start fresh:

```bash
docker compose down -v
docker compose up --build
```

## How to test

### Unit tests (xUnit)

```bash
dotnet test
```

**36 tests** covering use cases, mappers, validators and entity behavior. All use the Given-When-Then structuring convention. Conventions documented in `.claude/CLAUDE.md`.

### Integration tests (Postman)

The `/postman/` folder contains three files:

- `BlogPlatform.postman_environment.json` — defines `baseUrl` and `createdPostId`
- `BlogPlatform-Endpoints.postman_collection.json` — one request per endpoint for ad-hoc exploration
- `BlogPlatform-Integration.postman_collection.json` — **15 sequenced requests / 33 assertions** proving happy paths, validation failures and not-found scenarios for posts and comments

**To run:** import all three files into a Postman workspace, select the **BlogPlatform Local** environment, then right-click the Integration collection and choose **Run collection**.

Expected: **33 / 33 assertions pass** against a running API.

## Endpoint reference

### `GET /api/posts`

Returns all posts ordered by `createdAt` descending.

```bash
curl http://localhost:5183/api/posts
```

```json
[
  {
    "id": "7eb3393b-7f97-4eb2-9d3b-7eb2475c06ca",
    "title": "Integration test post",
    "createdAt": "2026-06-23T14:27:27.234Z",
    "commentCount": 1
  }
]
```

### `POST /api/posts`

Creates a post. Title (max 200) and Content (max 10,000) are required.

```bash
curl -X POST http://localhost:5183/api/posts \
  -H "Content-Type: application/json" \
  -d '{"title":"My first post","content":"Hello world"}'
```

Success (`201 Created`, with `Location` header pointing to the new resource):

```json
{
  "id": "7eb3393b-7f97-4eb2-9d3b-7eb2475c06ca",
  "title": "My first post",
  "content": "Hello world",
  "createdAt": "2026-06-23T14:27:27.234Z",
  "comments": []
}
```

Validation failure (`400 Bad Request`):

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation failed.",
  "status": 400,
  "detail": "Title is required. Content is required.",
  "traceId": "2bc0cace-acd1-4d4e-8116-66475ccd13e2"
}
```

### `GET /api/posts/{id}`

Returns the post with all its comments (ordered ascending by `createdAt`).

```bash
curl http://localhost:5183/api/posts/7eb3393b-7f97-4eb2-9d3b-7eb2475c06ca
```

```json
{
  "id": "7eb3393b-7f97-4eb2-9d3b-7eb2475c06ca",
  "title": "My first post",
  "content": "Hello world",
  "createdAt": "2026-06-23T14:27:27.234Z",
  "comments": [
    {
      "id": "be03b1f0-94c9-4eed-a3f2-c2531f70b134",
      "author": "Alice",
      "content": "Great post!",
      "createdAt": "2026-06-23T14:30:12.519Z"
    }
  ]
}
```

Not found (`404 Not Found`):

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "NotFound",
  "status": 404,
  "detail": "Post with id '00000000-0000-0000-0000-000000000000' was not found.",
  "traceId": "8b7f9e21-3c4d-4ed8-a912-de8f1c2a3b4d"
}
```

### `POST /api/posts/{id}/comments`

Adds a comment. Author (max 100) and Content (max 2,000) are required.

```bash
curl -X POST http://localhost:5183/api/posts/7eb3393b-7f97-4eb2-9d3b-7eb2475c06ca/comments \
  -H "Content-Type: application/json" \
  -d '{"author":"Alice","content":"Great post!"}'
```

Success (`201 Created`, with `Location` header pointing back to the parent post):

```json
{
  "id": "be03b1f0-94c9-4eed-a3f2-c2531f70b134",
  "author": "Alice",
  "content": "Great post!",
  "createdAt": "2026-06-23T14:30:12.519Z"
}
```

## Architecture decisions

| Decision | Why |
|---|---|
| Clean Architecture (4 projects) | Predictable dependency direction, easy to test, mirrors enterprise patterns. |
| SQLite | Zero configuration for the reviewer to run. EF Core migrations handle schema. |
| Repository interface in **Domain** | Pure DDD — the aggregate owns its persistence contract. |
| Query interface in **Application** | DTO shape is an application-level concern; keeps Domain clean. |
| Single `GetByIdAsync` (no `WithComments` variant) | Aggregates are atomic — loading them is a single operation, not a choice. |
| Notification pattern + `ThrowIfInvalidAsync` | Validation happens once at the use-case boundary, errors flow as exceptions caught by middleware. Result pattern reserved for non-validation business outcomes. |
| Mappers (`*BuilderMap`) | Use cases never `new` an entity; mapping is a single concern. |
| Primary constructors | Less boilerplate, modern C# idiom. |
| `EntityConfiguration<T>` base | Single source of truth for entity-level EF Core conventions (PK, value generation, ignored domain members). Future entities inherit for free. |
| `Entity.Id` not pre-populated; `ValueGeneratedOnAdd()` | Works correctly with EF Core's change tracker for aggregate-navigation insertion. Without this, the empty-key heuristic gets bypassed and EF Core generates UPDATE for new entities. |
| Simple GUID `traceId` (not W3C trace context) | Easy to copy from a screenshot, grep in logs. Distributed tracing not needed for a single-service API. |
| Drop redundant `instance` and `notifications` fields from ProblemDetails | 5-field RFC 7807 response: `type`, `title`, `status`, `detail`, `traceId`. No noise. |

## AI workflow — the `.claude/` workspace

This project was developed using **Claude Code**, and the repo ships with its own AI workspace under `.claude/`. The goal: anyone (a reviewer, a future contributor, or the next AI session) gets the same context, the same conventions, and the same playbooks for extending the codebase.

### What lives there

| File | Purpose |
|---|---|
| `.claude/CLAUDE.md` | The single source of project context: architecture, layer responsibilities, coding conventions (primary constructors, brace policy, BCL guards), the notification pattern + Result pattern split, test conventions (GWT, file path mirroring, naming), and a "what NOT to do" list. |
| `.claude/commands/add-endpoint.md` | Step-by-step playbook for adding a new endpoint end-to-end: domain entity, validator, mapper, use case, controller, unit tests, Postman update. |
| `.claude/commands/add-use-case.md` | Playbook for adding a use case that does not expose a new HTTP endpoint (background job, internal orchestration). |
| `.claude/commands/add-test.md` | Playbook for adding tests: file path mirroring, GWT body structure, naming convention, what to cover for use cases, validators and mappers. |
| `.claude/agents/pr-reviewer.md` | Subagent definition for reviewing a PR or branch diff against the documented conventions. Spawns in its own context window, reads the diff + affected files, returns a `PASS / NEEDS-FIX` report with `file:line` citations and grouped highlights / suggested next steps. Read-only — never modifies files. |

### How it's used

When working in this codebase via Claude Code, a prompt like *"add an endpoint for X"* triggers Claude to read the matching command file before writing any code. The result is consistent output: thin controllers, mapper + `ThrowIfInvalidAsync` validation, repository + UoW, paired unit tests, and matching Postman scenarios — every time, without architectural drift.

`CLAUDE.md` also lists explicit anti-patterns (don't `new` an entity in a use case, don't add a `WithX` variant of `GetByIdAsync`, don't pre-set `Entity.Id`) so common mistakes are caught before they're written.

For reviews, prompting *"review this PR"* (or *"check this branch"*) spawns the **`pr-reviewer` subagent**. It reads the diff, the affected files and `CLAUDE.md` in its own isolated context window, then returns a structured `PASS / NEEDS-FIX` report with `file:line` citations of any violations. Keeps the main session's context clean even when the review touches a dozen files.

### Reviewing a PR

Once a feature branch is ready, **start a fresh Claude Code session** in the repo and prompt:

> review this PR

A new session is intentional: the reviewer reads the diff with no leftover assumptions from how the feature was implemented, the same way a human reviewer would. From there Claude:

1. Runs `git diff main...HEAD` and lists the commits in scope.
2. Spawns the **`pr-reviewer` subagent**, which reads `CLAUDE.md` and every changed file in its own isolated context window and audits them against the documented conventions.
3. Writes the findings to **`.claude/reports/pr-review.html`** — a single-file, zero-dependency report with the verdict (`PASS` / `NEEDS-FIX`), severity-scored issue cards (problem, suggested fix, why it matters, code sample, ready-to-paste PR comment), the good practices observed, and suggested follow-ups.
4. Opens the report in your default browser.

The action checklist persists in `localStorage`, so you can close the tab, fix items between sessions, and come back to your tracked progress. Keyboard shortcuts: `E` expand all, `C` collapse all, `P` print or save to PDF.

<!-- screenshot: hero with verdict pill, action board, and progress bar -->
![PR review report — action board](./docs/images/pr-review-action-board.png)

<!-- screenshot: medium priority issue card with code sample and copyable PR comment -->
![PR review report — medium issue](./docs/images/pr-review-medium-issue.png)

### Possible extensions

These would slot naturally into the same `.claude/` workspace:

- A **Postman collection sync agent** that, given a new endpoint added to a controller, automatically updates `postman/BlogPlatform-Endpoints.postman_collection.json` with the new request and extends `postman/BlogPlatform-Integration.postman_collection.json` with sequenced happy-path + validation + not-found scenarios. Keeps the integration suite synchronized without hand-editing JSON.
- A **migration agent** that diffs the entity model against the latest EF Core snapshot and proposes the next `dotnet ef migrations add` command with a sensible name.
- A **README freshness agent** that walks controllers, mappers and configs and verifies the README's endpoint reference, sample JSON and architecture tables still match the code.

## Git workflow

- `main`: protected branch (GitHub ruleset enforces no direct pushes, no force pushes, no deletions). Only updated via PR from `develop`.
- `develop`: integration branch. Feature branches merge here first.
- `feature/*`: per logical chunk of work (`feature/setup-clean-architecture`, `feature/posts-endpoints`, `feature/comments-endpoint`, `feature/unit-tests`, `feature/docker-and-docs`).
- All commits follow [Conventional Commits](https://www.conventionalcommits.org/).
- CI runs on every push and PR: restore, build, test, upload test results.

## Next steps

Given more time, the following would be added:

- **Authentication & Authorization** (JWT + refresh tokens; per-action policy attributes)
- **Pagination** (cursor-based for `/api/posts`)
- **Integration tests** in xUnit using `WebApplicationFactory` and an in-memory SQLite database
- **Caching** (Redis or `IMemoryCache` for hot reads like `/api/posts`)
- **Rate limiting** on write endpoints
- **Full observability** (OpenTelemetry exporter with traces/metrics; structured Serilog enrichers for correlation if downstream services are added)
- **CI/CD pipeline to a real environment** (GitHub Actions builds Docker image to a registry, deploys to a serverless container host like Azure Container Apps)
- **Frontend SPA** (React + TypeScript) consuming this API

## License

MIT. See [LICENSE](./LICENSE).
