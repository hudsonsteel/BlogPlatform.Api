using BlogPlatform.Application.Common.Queries;
using BlogPlatform.Application.DTOs.Posts;
using BlogPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BlogPlatform.Infrastructure.Queries;

internal sealed class BlogPostQueries(BlogPlatformDbContext context) : IBlogPostQueries
{
    public async Task<IReadOnlyList<PostListItemDto>> ListAsync(CancellationToken cancellationToken)
    {
        var items = await context.BlogPosts
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PostListItemDto(
                p.Id,
                p.Title,
                p.CreatedAt,
                p.Comments.Count))
            .ToListAsync(cancellationToken);

        return items;
    }
}
