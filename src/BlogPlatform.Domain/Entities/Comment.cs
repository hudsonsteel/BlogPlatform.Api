using BlogPlatform.Domain.Common;
using FluentValidation;

namespace BlogPlatform.Domain.Entities;

public sealed class Comment : Entity
{
    private Comment()
    {
    }

    public Comment(Guid blogPostId, string author, string content)
    {
        BlogPostId = blogPostId;
        Author = (author ?? string.Empty).Trim();
        Content = (content ?? string.Empty).Trim();
    }

    public Guid BlogPostId { get; private set; }

    public string Author { get; private set; } = string.Empty;

    public string Content { get; private set; } = string.Empty;
}

public sealed class CommentValidator : AbstractValidator<Comment>
{
    public const int MaxAuthorLength = 100;
    public const int MaxContentLength = 2_000;

    public CommentValidator()
    {
        RuleFor(c => c.BlogPostId)
            .NotEqual(Guid.Empty)
            .WithMessage("Comment must be associated with a post.");

        RuleFor(c => c.Author)
            .NotEmpty().WithMessage("Author is required.")
            .MaximumLength(MaxAuthorLength)
            .WithMessage($"Author cannot exceed {MaxAuthorLength} characters.");

        RuleFor(c => c.Content)
            .NotEmpty().WithMessage("Content is required.")
            .MaximumLength(MaxContentLength)
            .WithMessage($"Content cannot exceed {MaxContentLength} characters.");
    }
}
