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
  - `tests/BlogPlatform.Tests.Unit/UseCases/Posts/CreatePostUseCaseTests.cs`
- Test class naming: `<ClassUnderTest>Tests`.
- Use `[Fact]` for single scenarios and `[Theory]` with `[InlineData]` for parameterized.
- Mock only interfaces from `Application/Common/Interfaces/`. Never mock entities.

## What to cover for a use case

1. **Happy path** — valid input produces success.
2. **Validation failure** — invalid input returns `Result.Failure` without touching repositories.
3. **Not-found / domain failure** — when applicable.
4. **Persistence interaction** — confirm `UnitOfWork.SaveChangesAsync` was called on success and not called on failure.
