# BlogPlatform.Api — Project Instructions for Claude Code

## What this is

A RESTful API for a simple blogging platform. Built as a backend coding challenge.

The codebase is intentionally small but production-leaning: Clean Architecture, Result pattern, FluentValidation, EF Core + SQLite, xUnit tests with Given-When-Then structure, Swagger, Docker, and a GitHub Actions CI pipeline.

## Architecture

Clean Architecture with strict dependency direction:

```
Presentation  →  Application  →  Domain
       ↓                ↑
Infrastructure  ────────┘
```

| Layer | Responsibility | Depends on |
|---|---|---|
| `BlogPlatform.Domain` | Entities, value objects, validators, repository interfaces (`IBlogPostRepository`, `IUnitOfWork`). Only depends on FluentValidation. | nothing (except FluentValidation) |
| `BlogPlatform.Application` | Use cases, DTOs, query interfaces (`IBlogPostQueries` for projections), mappers. | Domain |
| `BlogPlatform.Infrastructure` | EF Core DbContext, repository + query implementations, migrations, DI registration. | Application + Domain |
| `BlogPlatform.Presentation` | ASP.NET Core controllers, middleware, Program.cs, Swagger. | Infrastructure + Application |
| `BlogPlatform.Tests.Unit` | xUnit tests for use cases and domain logic. | Application + Domain |

### Repository vs Queries (CQRS-lite)

- **Write side (Domain)**: `IBlogPostRepository` works with aggregates only. Operations like `AddAsync(BlogPost)`, `GetByIdAsync(Guid)`. No DTOs in the signatures. The interface lives in Domain because the aggregate owns its persistence contract (textbook DDD).
- **Read side (Application)**: `IBlogPostQueries` returns projected DTOs (`PostListItemDto`) directly from the database. It lives in Application because the shape of the read model is an Application concern (what the use case wants to expose), not a Domain concept.
- Infrastructure implements both interfaces with separate classes (`BlogPostRepository`, `BlogPostQueries`) so each follows Single Responsibility.

## Coding conventions

- **C# 14 / .NET 10**
- **Nullable reference types**: enabled everywhere.
- **`TreatWarningsAsErrors`**: enabled (build fails on warnings).
- **File-scoped namespaces**.
- **Primary constructors** for dependency injection (use cases, repositories, handlers). No `private readonly` fields plus boilerplate constructor when a primary constructor is enough.
- **Brace-less single-statement guard clauses**. `if (cond) throw ...;` over a four-line `if (cond) { throw ...; }`. Braces are still required (and enforced by `.editorconfig`) for any multi-line body.
- **Modern BCL guard helpers** for argument validation: `ArgumentException.ThrowIfNullOrWhiteSpace`, `ArgumentNullException.ThrowIfNull`, `ArgumentOutOfRangeException.ThrowIf*`. Prefer these over hand-written `if`/`throw`.
- **Async by default** for I/O paths, always passing `CancellationToken`.
- **Result pattern** for use-case business outcomes (NotFound, Conflict). **Notification pattern + `ThrowIfInvalid`** for entity-level validation failures (caught by the global middleware and returned as 400 ProblemDetails).
- **Validators live next to their entity** in the Domain layer. `BlogPostValidator : AbstractValidator<BlogPost>` sits in the same file as `BlogPost.cs`.
- **One-line validation in use cases**: `await entity.ThrowIfInvalidAsync(validator, ct)`. The extension method (in `Domain/Common/EntityValidationExtensions.cs`) runs the validator, copies the errors into the entity's notifications and throws if invalid. Use cases never validate manually.
- **Mappers (`*BuilderMap`)** in the Application layer translate DTOs to entities and back. Example: `BlogPostBuilderMap.From(request)` returns a `BlogPost`; `BlogPostBuilderMap.ToDetailDto(post)` returns a `PostDetailDto`. Use cases never `new` an entity directly; they call the mapper.
- **Thin controllers**: every controller action is 4-7 lines. Inject the use case, call `Handle`, return `HandleResult(result)`. No `if`, no `try`, no validation calls, no business rules. All logic lives in the Application layer.
- **Guard clauses / Early return**: validate first, return on first failure. No nested `if/else`.
- **No anemic services**: use cases own a single workflow; entities own invariants.
- **No magic strings for routes**: define them as `const string` on the controller when reused.
- **DRY**: extract shared logic to base classes (`ApiControllerBase`, `Entity`) or shared MSBuild props. Never copy code between use cases — extract a helper or interface.
- **Global exception middleware** handles unexpected exceptions (5xx). Use cases never `throw` for expected outcomes — they return `Result.Failure(...)`.

## Test conventions

- Framework: **xUnit + FluentAssertions + Moq**.
- Structuring pattern: **Given-When-Then (GWT)**, written as comments inside the test body.
- Test method naming: `MethodUnderTest_StateUnderTest_ExpectedBehavior`.
- One assertion concept per test (multiple `Should()` calls for the same concept are fine).

Example:

```csharp
[Fact]
public async Task Handle_WhenTitleIsEmpty_ReturnsValidationFailure()
{
    // Given
    var useCase = new CreatePostUseCase(_repository.Object, _unitOfWork.Object);
    var request = new CreatePostRequest(Title: "", Content: "valid content");

    // When
    var result = await useCase.Handle(request, CancellationToken.None);

    // Then
    result.IsSuccess.Should().BeFalse();
    result.Error.Should().Contain("Title");
}
```

## How to add common things

| Task | Read playbook |
|---|---|
| Add a new endpoint (full stack: controller + use case + repository + test) | `.claude/commands/add-endpoint.md` |
| Add a new use case only (no new endpoint) | `.claude/commands/add-use-case.md` |
| Add a unit test for an existing use case | `.claude/commands/add-test.md` |

## Build & run commands

```bash
# Restore + build
dotnet restore
dotnet build

# Run tests
dotnet test

# Run the API locally
dotnet run --project src/BlogPlatform.Presentation
# Swagger: http://localhost:5xxx/swagger

# Apply EF Core migrations (runs automatically at startup in Development)
dotnet ef database update --project src/BlogPlatform.Infrastructure --startup-project src/BlogPlatform.Presentation
```

## Git workflow

- `main`: protected, production-clean. Only updated via PR from `develop`.
- `develop`: integration branch. Feature branches PR here.
- `feature/*`: per logical chunk of work.
- **Conventional Commits** for all commit messages (`feat:`, `fix:`, `test:`, `docs:`, `chore:`, `build:`, `refactor:`).

## What NOT to do

- Don't add abstractions until two concrete callers exist.
- Don't catch exceptions to convert to Result inside use cases — let middleware handle truly unexpected ones.
- Don't add authentication for this challenge (out of scope, listed under "Next Steps" in README).
- Don't introduce a second persistence provider for now — SQLite is enough.
- Don't write tests against the controller layer (covered by integration tests, also listed as next step). Unit tests focus on use cases and domain.
