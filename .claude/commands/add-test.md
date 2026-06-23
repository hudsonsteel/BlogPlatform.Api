# Playbook: Add a unit test

## Structure

Every test uses **Given-When-Then** as explicit comments:

```csharp
[Fact]
public async Task MethodUnderTest_StateUnderTest_ExpectedBehavior()
{
    // Given
    // arrange dependencies, mocks, input data

    // When
    // invoke the method under test

    // Then
    // assert outcome
}
```

## Conventions

- Framework: **xUnit + FluentAssertions + Moq**.
- File location: mirror the source path under `tests/BlogPlatform.Tests.Unit/`.
  - `src/BlogPlatform.Application/UseCases/Posts/CreatePostUseCase.cs`
  - `tests/BlogPlatform.Tests.Unit/Application/UseCases/Posts/CreatePostUseCaseTests.cs`
- Test class naming: `<ClassUnderTest>Tests`.
- Test method naming: `Method_State_ExpectedBehavior`. The test project locally suppresses CA1707 so the convention is allowed.
- Use `[Fact]` for single scenarios and `[Theory]` + `[InlineData]` for parameterized.
- Mock only interfaces from `Domain/Repositories/`, `Application/Common/Queries/`, and `FluentValidation.IValidator<T>`. **Never mock entities** — construct them directly with `new BlogPost(...)` or `new Comment(...)`.
- Use `TestSupport/TestEntityExtensions.WithId(Guid)` / `.WithCreatedAt(DateTime)` when a test needs to set the protected `Entity` properties for assertion purposes.

## What to cover for a use case

1. **Happy path** — valid input produces success; repository / UoW called as expected.
2. **Validation failure** — invalid input throws `DomainValidationException` without touching the repository or UoW.
3. **Not-found / domain failure** — returns `Result.Failure(Error.NotFound(...))` if applicable.
4. **Persistence interaction** — confirm `IUnitOfWork.SaveChangesAsync` was called on success and **not** called on failure paths.

## What to cover for a validator

1. **Happy path** — valid input returns `IsValid == true`.
2. **Empty / whitespace** — use `[Theory]` + `[InlineData("")]` + `[InlineData("   ")]` for required fields.
3. **Length limits** — pass a string one character over the limit.

## What to cover for a mapper

1. **From request -> entity** — input is trimmed, foreign keys assigned, no throws on valid data.
2. **Entity -> DTO** — all fields mapped; collections ordered correctly (e.g. comments by `CreatedAt`).
