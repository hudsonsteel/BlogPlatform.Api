# Playbook: Add a new use case

Use this when adding a use case that does not expose a new HTTP endpoint (e.g. background job, internal orchestration).

## Steps

1. Add the request and (if needed) response records in `src/BlogPlatform.Application/DTOs/<Aggregate>/`.
2. Add or extend the mapper at `src/BlogPlatform.Application/Mappers/<Aggregate>/<Entity>BuilderMap.cs` (`From(request)`, `ToDto(entity)`).
3. If the use case validates entity invariants, ensure the corresponding `<Entity>Validator` is in the Domain layer next to the entity.
4. Create `<Verb><Noun>UseCase` in `src/BlogPlatform.Application/UseCases/<Aggregate>/` using a primary constructor for DI.
5. The use case signature is:
   ```csharp
   public async Task<Result<TResponse>> Handle(
       TRequest request,
       CancellationToken cancellationToken)
   ```
6. Inside `Handle`:
   - For commands: load the aggregate via repository, map the request to a child/new entity, call `await entity.ThrowIfInvalidAsync(validator, ct)`, mutate via the aggregate root method, save via `IUnitOfWork`.
   - For queries that return projected DTOs: call `IBlogPostQueries` (or whichever query interface).
   - Use `Result.Failure(Error.NotFound(...))` for expected "not found" outcomes. Validation failures throw `DomainValidationException`, which the middleware translates to 400.
7. Register the use case in `Application/DependencyInjection.cs` as a scoped service.
8. Add unit tests covering: success, validation failure (throws), not-found if applicable.
