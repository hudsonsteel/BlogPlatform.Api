using BlogPlatform.Domain.Entities;

namespace BlogPlatform.Domain.Repositories;

public interface IBlogPostRepository
{
    Task AddAsync(BlogPost post, CancellationToken cancellationToken);

    Task<BlogPost?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
