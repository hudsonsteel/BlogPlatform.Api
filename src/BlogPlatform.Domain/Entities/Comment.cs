using BlogPlatform.Domain.Common;

namespace BlogPlatform.Domain.Entities;

public sealed class Comment : Entity
{
    private Comment()
    {
    }

    public Comment(Guid blogPostId, string author, string content)
    {
        if (blogPostId == Guid.Empty)
            throw new ArgumentException("BlogPostId is required.", nameof(blogPostId));

        ArgumentException.ThrowIfNullOrWhiteSpace(author);
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        BlogPostId = blogPostId;
        Author = author.Trim();
        Content = content.Trim();
    }

    public Guid BlogPostId { get; private set; }

    public string Author { get; private set; } = string.Empty;

    public string Content { get; private set; } = string.Empty;
}
