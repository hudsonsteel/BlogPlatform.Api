using BlogPlatform.Domain.Entities;
using BlogPlatform.Domain.Repositories;
using BlogPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BlogPlatform.Infrastructure.Repositories;

internal sealed class BlogPostRepository(BlogPlatformDbContext context) : IBlogPostRepository
{
    public Task AddAsync(BlogPost post, CancellationToken cancellationToken) =>
        context.BlogPosts.AddAsync(post, cancellationToken).AsTask();

    public Task<BlogPost?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.BlogPosts
            .Include(p => p.Comments)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
}
