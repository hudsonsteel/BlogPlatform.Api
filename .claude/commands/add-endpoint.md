# Playbook: Add a new endpoint

Follow this exact sequence when adding a new endpoint that exposes a new operation.

## 1. Domain layer (if a new entity or invariant is needed)

- Add or modify the entity in `src/BlogPlatform.Domain/Entities/`.
- Keep invariants inside the entity constructor or behavior methods.
- No framework attributes on the entity (EF mapping lives in Infrastructure).

## 2. Application layer

- **DTO**: add request/response records in `src/BlogPlatform.Application/DTOs/<Aggregate>/`.
- **Validator**: add a `FluentValidation` validator for the request record.
- **Use case**: create `<Verb><Noun>UseCase` in `src/BlogPlatform.Application/UseCases/<Aggregate>/`.
  - Takes its dependencies via constructor.
  - Single public method `Handle(request, CancellationToken)` returning `Result<TResponse>` or `Result`.
  - Validates input first (call validator), then performs the workflow.
- **Interface** (if not already defined): add repository or service interface in `src/BlogPlatform.Application/Common/Interfaces/`.

## 3. Infrastructure layer

- If the use case introduces a new repository method, implement it in the matching repository under `src/BlogPlatform.Infrastructure/Repositories/`.
- Register any new service in `DependencyInjection.cs`.
- If schema changes are needed: add an EF Core migration (`dotnet ef migrations add <Name> --project src/BlogPlatform.Infrastructure --startup-project src/BlogPlatform.Presentation`).

## 4. Presentation layer

- Add the controller action in `src/BlogPlatform.Presentation/Controllers/<Aggregate>Controller.cs`.
- The action calls the use case and translates the `Result<T>` to an `IActionResult` via the `HandleResult` helper on `ApiControllerBase`.
- Annotate with `[ProducesResponseType(...)]` for each possible HTTP status.

## 5. Tests

- Add unit tests under `tests/BlogPlatform.Tests.Unit/UseCases/<Aggregate>/`.
- Cover at least: happy path, validation failure, not-found (if applicable).
- Use the Given-When-Then structure.

## 6. Verify

```bash
dotnet build
dotnet test
dotnet run --project src/BlogPlatform.Presentation
# Open Swagger, exercise the new endpoint manually.
```
