# Playbook: Add a new use case

Use this when adding a use case that does not expose a new HTTP endpoint (e.g. background job, internal orchestration).

## Steps

1. Create `<Verb><Noun>UseCase` in `src/BlogPlatform.Application/UseCases/<Aggregate>/`.
2. Define request/response records in `src/BlogPlatform.Application/DTOs/<Aggregate>/` (or reuse existing).
3. Add a `FluentValidation` validator for the request.
4. The use case signature is:
   ```csharp
   public async Task<Result<TResponse>> Handle(
       TRequest request,
       CancellationToken cancellationToken)
   ```
5. Inside `Handle`:
   - Validate first. Return `Result.Failure(...)` on invalid input.
   - Perform the workflow using injected dependencies.
   - Commit the unit of work explicitly if writes are involved.
   - Return `Result.Success(response)`.
6. Register the use case in `Application/DependencyInjection.cs` as a scoped service.
7. Add unit tests covering: success, validation failure, domain failure.
