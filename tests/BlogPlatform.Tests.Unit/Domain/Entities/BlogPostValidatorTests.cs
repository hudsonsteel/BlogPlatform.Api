using BlogPlatform.Domain.Entities;
using FluentAssertions;

namespace BlogPlatform.Tests.Unit.Domain.Entities;

public sealed class BlogPostValidatorTests
{
    private readonly BlogPostValidator _validator = new();

    [Fact]
    public void Validate_WhenTitleAndContentAreValid_ReturnsValid()
    {
        // Given
        var post = new BlogPost("A valid title", "Some meaningful content.");

        // When
        var result = _validator.Validate(post);

        // Then
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WhenTitleIsEmptyOrWhitespace_FailsOnTitleRequired(string title)
    {
        // Given
        var post = new BlogPost(title, "Some content");

        // When
        var result = _validator.Validate(post);

        // Then
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title" && e.ErrorMessage == "Title is required.");
    }

    [Fact]
    public void Validate_WhenTitleExceeds200Characters_FailsOnTitleLength()
    {
        // Given
        var post = new BlogPost(new string('a', 201), "Some content");

        // When
        var result = _validator.Validate(post);

        // Then
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Title" && e.ErrorMessage.Contains("cannot exceed 200"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WhenContentIsEmptyOrWhitespace_FailsOnContentRequired(string content)
    {
        // Given
        var post = new BlogPost("A valid title", content);

        // When
        var result = _validator.Validate(post);

        // Then
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Content" && e.ErrorMessage == "Content is required.");
    }

    [Fact]
    public void Validate_WhenContentExceeds10000Characters_FailsOnContentLength()
    {
        // Given
        var post = new BlogPost("A valid title", new string('a', 10_001));

        // When
        var result = _validator.Validate(post);

        // Then
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Content" && e.ErrorMessage.Contains("cannot exceed 10000"));
    }
}
