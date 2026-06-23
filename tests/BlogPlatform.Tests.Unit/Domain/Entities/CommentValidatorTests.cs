using BlogPlatform.Domain.Entities;
using FluentAssertions;

namespace BlogPlatform.Tests.Unit.Domain.Entities;

public sealed class CommentValidatorTests
{
    private readonly CommentValidator _validator = new();
    private readonly Guid _postId = Guid.NewGuid();

    [Fact]
    public void Validate_WhenAllFieldsAreValid_ReturnsValid()
    {
        // Given
        var comment = new Comment(_postId, "Alice", "Great post!");

        // When
        var result = _validator.Validate(comment);

        // Then
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenBlogPostIdIsEmpty_FailsOnAssociation()
    {
        // Given
        var comment = new Comment(Guid.Empty, "Alice", "Great post!");

        // When
        var result = _validator.Validate(comment);

        // Then
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "BlogPostId" && e.ErrorMessage == "Comment must be associated with a post.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WhenAuthorIsEmptyOrWhitespace_FailsOnAuthorRequired(string author)
    {
        // Given
        var comment = new Comment(_postId, author, "Great post!");

        // When
        var result = _validator.Validate(comment);

        // Then
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Author" && e.ErrorMessage == "Author is required.");
    }

    [Fact]
    public void Validate_WhenAuthorExceeds100Characters_FailsOnAuthorLength()
    {
        // Given
        var comment = new Comment(_postId, new string('a', 101), "Great post!");

        // When
        var result = _validator.Validate(comment);

        // Then
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Author" && e.ErrorMessage.Contains("cannot exceed 100"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WhenContentIsEmptyOrWhitespace_FailsOnContentRequired(string content)
    {
        // Given
        var comment = new Comment(_postId, "Alice", content);

        // When
        var result = _validator.Validate(comment);

        // Then
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Content" && e.ErrorMessage == "Content is required.");
    }

    [Fact]
    public void Validate_WhenContentExceeds2000Characters_FailsOnContentLength()
    {
        // Given
        var comment = new Comment(_postId, "Alice", new string('a', 2_001));

        // When
        var result = _validator.Validate(comment);

        // Then
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Content" && e.ErrorMessage.Contains("cannot exceed 2000"));
    }
}
