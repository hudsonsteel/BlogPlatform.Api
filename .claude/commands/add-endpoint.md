# Playbook: Add a new endpoint

Follow this exact sequence when adding a new endpoint that exposes a new operation.

## 1. Domain layer

- If a new entity is needed, add it in `src/BlogPlatform.Domain/Entities/` and have it inherit from `Entity` (no Id default, use the field initializer only for `CreatedAt`).
- Add a parameterless `private` constructor (used by EF Core materialization) and a public constructor that assigns properties without throwing — validation is done by the validator, not the constructor.
- In the same file as the entity, add a `<Entity>Validator : AbstractValidator<<Entity>>` with the field rules.
- If the operation modifies the entity, add the behavior as a method on the entity (e.g. `BlogPost.AddComment`).
- If a new aggregate is needed, add a repository interface in `src/BlogPlatform.Domain/Repositories/`. Single `GetByIdAsync` should load the full aggregate; mutations go through the aggregate root.

## 2. Application layer

- **DTO**: add request/response records in `src/BlogPlatform.Application/DTOs/<Aggregate>/`.
- **Mapper**: add or extend a `<Entity>BuilderMap` in `src/BlogPlatform.Application/Mappers/<Aggregate>/`. Static methods `From(request)` and `ToDto(entity)` keep the use case free of `new` calls.
- **Use case**: create `<Verb><Noun>UseCase` in `src/BlogPlatform.Application/UseCases/<Aggregate>/`.
  - Use a **primary constructor** for DI.
  - Single public method `Handle(request, CancellationToken)` returning `Result<TResponse>` or `Result`.
  - For commands that mutate: load the aggregate via repository, build the new entity via the mapper, validate with `await entity.ThrowIfInvalidAsync(validator, ct)`, then perform the workflow.
  - For queries that read projected DTOs: inject `IBlogPostQueries` (or whichever query interface) and call it.
- **Register** the use case in `Application/DependencyInjection.cs` with `services.AddScoped<<UseCase>>()`.

## 3. Infrastructure layer

- If the use case introduces a new repository method, implement it in the matching repository under `src/BlogPlatform.Infrastructure/Repositories/`.
- If a new query interface is needed, implement it under `src/BlogPlatform.Infrastructure/Queries/`.
- Register any new service in `Infrastructure/DependencyInjection.cs`.
- If schema changes are needed: add an EF Core migration (`dotnet ef migrations add <Name> --project src/BlogPlatform.Infrastructure --startup-project src/BlogPlatform.Presentation --output-dir Persistence/Migrations`). The per-entity config inherits from `EntityConfiguration<TEntity>` and only declares its own table name and entity-specific properties.

## 4. Presentation layer

- Add the controller action in `src/BlogPlatform.Presentation/Controllers/<Aggregate>Controller.cs`.
- Action body is 4-7 lines: inject the use case via `[FromServices]`, call `Handle`, return `HandleResult(result)` or `HandleResult(result, value => CreatedAtRoute(...))` for 201s.
- Annotate the action with **XML doc comments** (`<summary>`, `<remarks>` for validation rules, `<param>`, `<response code="...">`) and `[ProducesResponseType(...)]` for every status code the action can produce. The XML feeds Swagger.

## 5. Tests

- Add unit tests under `tests/BlogPlatform.Tests.Unit/Application/UseCases/<Aggregate>/`.
- Cover at least: happy path, validation failure (throws `DomainValidationException`, never calls UoW), not-found if applicable.
- Use the **Given-When-Then** structure.
- Mock `IBlogPostRepository` / `IUnitOfWork` / `IValidator<T>` / `IBlogPostQueries` via Moq. Construct entities directly. Use `TestEntityExtensions.WithId(...)` if a specific Id is needed.

## 6. Postman

- Add the new request to the **Endpoints** collection under `/postman/` for ad-hoc testing.
- Extend the **Integration** collection with sequenced steps: happy path, validation failures, not-found.
- Bump the assertion count in the README.

## 7. Verify

```bash
dotnet build
dotnet test
dotnet run --project src/BlogPlatform.Presentation
# Hit the new endpoint via Swagger or run the Integration collection in Postman.
```
