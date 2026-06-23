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
| `BlogPlatform.Domain` | Entities, value objects, domain rules. Zero framework dependencies. | nothing |
| `BlogPlatform.Application` | Use cases, DTOs, validators, interfaces (repositories, UoW). | Domain |
| `BlogPlatform.Infrastructure` | EF Core DbContext, repository implementations, migrations, DI registration. | Application |
| `BlogPlatform.Presentation` | ASP.NET Core controllers, middleware, Program.cs, Swagger. | Infrastructure + Application |
| `BlogPlatform.Tests.Unit` | xUnit tests for use cases and domain logic. | Application + Domain |

## Coding conventions

- **C# 14 / .NET 10**
- **Nullable reference types**: enabled everywhere.
- **`TreatWarningsAsErrors`**: enabled (build fails on warnings).
- **File-scoped namespaces**.
- **Async by default** for I/O paths, always passing `CancellationToken`.
- **Result pattern** for use case return values (no throw for expected failures like validation or not found).
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
