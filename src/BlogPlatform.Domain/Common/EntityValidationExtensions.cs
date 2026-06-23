using FluentValidation;

namespace BlogPlatform.Domain.Common;

public static class EntityValidationExtensions
{
    public static async Task ThrowIfInvalidAsync<TEntity>(
        this TEntity entity,
        IValidator<TEntity> validator,
        CancellationToken cancellationToken = default)
        where TEntity : Entity
    {
        var validation = await validator.ValidateAsync(entity, cancellationToken);

        if (!validation.IsValid)
            entity.AddNotifications(validation.Errors.Select(e => e.ErrorMessage));

        entity.ThrowIfInvalid();
    }
}
