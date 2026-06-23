using BlogPlatform.Domain.Common;
using FluentValidation;

namespace BlogPlatform.Domain.Entities;

public sealed class BlogPost : Entity
{
    private readonly List<Comment> _comments = [];

    private BlogPost()
    {
    }

    public BlogPost(string title, string content)
    {
        Title = (title ?? string.Empty).Trim();
        Content = (content ?? string.Empty).Trim();
    }

    public string Title { get; private set; } = string.Empty;

    public string Content { get; private set; } = string.Empty;

    public IReadOnlyCollection<Comment> Comments => _comments.AsReadOnly();

    public Comment AddComment(string author, string content)
    {
        var comment = new Comment(Id, author, content);
        _comments.Add(comment);
        return comment;
    }
}

public sealed class BlogPostValidator : AbstractValidator<BlogPost>
{
    public const int MaxTitleLength = 200;
    public const int MaxContentLength = 10_000;

    public BlogPostValidator()
    {
        RuleFor(p => p.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(MaxTitleLength)
            .WithMessage($"Title cannot exceed {MaxTitleLength} characters.");

        RuleFor(p => p.Content)
            .NotEmpty().WithMessage("Content is required.")
            .MaximumLength(MaxContentLength)
            .WithMessage($"Content cannot exceed {MaxContentLength} characters.");
    }
}
