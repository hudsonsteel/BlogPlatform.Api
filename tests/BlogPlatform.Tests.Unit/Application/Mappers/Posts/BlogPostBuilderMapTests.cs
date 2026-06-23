using BlogPlatform.Application.DTOs.Posts;
using BlogPlatform.Application.Mappers.Posts;
using BlogPlatform.Domain.Entities;
using BlogPlatform.Tests.Unit.TestSupport;
using FluentAssertions;

namespace BlogPlatform.Tests.Unit.Application.Mappers.Posts;

public sealed class BlogPostBuilderMapTests
{
    [Fact]
    public void From_TrimsTitleAndContent()
    {
        // Given
        var request = new CreatePostRequest("  hello  ", "  world  ");

        // When
        var post = BlogPostBuilderMap.From(request);

        // Then
        post.Title.Should().Be("hello");
        post.Content.Should().Be("world");
        post.Comments.Should().BeEmpty();
    }

    [Fact]
    public void ToDetailDto_ReturnsAllFieldsAndOrdersCommentsByCreatedAtAscending()
    {
        // Given
        var postId = Guid.NewGuid();
        var post = new BlogPost("title", "content").WithId(postId);

        var newer = new Comment(postId, "Bob", "newer")
            .WithCreatedAt(new DateTime(2026, 06, 23, 12, 0, 0, DateTimeKind.Utc));
        var older = new Comment(postId, "Alice", "older")
            .WithCreatedAt(new DateTime(2026, 06, 23, 11, 0, 0, DateTimeKind.Utc));

        post.AddComment(newer);
        post.AddComment(older);

        // When
        var dto = BlogPostBuilderMap.ToDetailDto(post);

        // Then
        dto.Id.Should().Be(postId);
        dto.Title.Should().Be("title");
        dto.Content.Should().Be("content");
        dto.Comments.Should().HaveCount(2);
        dto.Comments[0].Author.Should().Be("Alice");
        dto.Comments[1].Author.Should().Be("Bob");
    }

    [Fact]
    public void ToListItemDto_ProjectsIdTitleAndCommentCount()
    {
        // Given
        var post = new BlogPost("title", "content");
        post.AddComment(new Comment(Guid.NewGuid(), "Alice", "hi"));
        post.AddComment(new Comment(Guid.NewGuid(), "Bob", "hi"));

        // When
        var dto = BlogPostBuilderMap.ToListItemDto(post);

        // Then
        dto.Title.Should().Be("title");
        dto.CommentCount.Should().Be(2);
    }
}
