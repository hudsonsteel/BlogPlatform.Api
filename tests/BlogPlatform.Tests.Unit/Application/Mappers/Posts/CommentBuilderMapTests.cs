using BlogPlatform.Application.DTOs.Posts;
using BlogPlatform.Application.Mappers.Posts;
using BlogPlatform.Domain.Entities;
using BlogPlatform.Tests.Unit.TestSupport;
using FluentAssertions;

namespace BlogPlatform.Tests.Unit.Application.Mappers.Posts;

public sealed class CommentBuilderMapTests
{
    [Fact]
    public void From_TrimsAuthorAndContentAndSetsBlogPostId()
    {
        // Given
        var postId = Guid.NewGuid();
        var request = new AddCommentRequest("  Alice  ", "  Great post!  ");

        // When
        var comment = CommentBuilderMap.From(request, postId);

        // Then
        comment.BlogPostId.Should().Be(postId);
        comment.Author.Should().Be("Alice");
        comment.Content.Should().Be("Great post!");
    }

    [Fact]
    public void ToDto_MapsAllFields()
    {
        // Given
        var commentId = Guid.NewGuid();
        var createdAt = new DateTime(2026, 06, 23, 12, 0, 0, DateTimeKind.Utc);
        var comment = new Comment(Guid.NewGuid(), "Alice", "Great post!")
            .WithId(commentId)
            .WithCreatedAt(createdAt);

        // When
        var dto = CommentBuilderMap.ToDto(comment);

        // Then
        dto.Id.Should().Be(commentId);
        dto.Author.Should().Be("Alice");
        dto.Content.Should().Be("Great post!");
        dto.CreatedAt.Should().Be(createdAt);
    }
}
