using BlogPlatform.Domain.Common;

namespace BlogPlatform.Tests.Unit.TestSupport;

internal static class TestEntityExtensions
{
    public static T WithId<T>(this T entity, Guid id) where T : Entity
    {
        typeof(Entity)
            .GetProperty(nameof(Entity.Id))!
            .SetValue(entity, id);
        return entity;
    }

    public static T WithCreatedAt<T>(this T entity, DateTime createdAt) where T : Entity
    {
        typeof(Entity)
            .GetProperty(nameof(Entity.CreatedAt))!
            .SetValue(entity, createdAt);
        return entity;
    }
}
