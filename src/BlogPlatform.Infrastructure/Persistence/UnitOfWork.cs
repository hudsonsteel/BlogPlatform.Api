using BlogPlatform.Domain.Repositories;

namespace BlogPlatform.Infrastructure.Persistence;

internal sealed class UnitOfWork(BlogPlatformDbContext context) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
        context.SaveChangesAsync(cancellationToken);
}
