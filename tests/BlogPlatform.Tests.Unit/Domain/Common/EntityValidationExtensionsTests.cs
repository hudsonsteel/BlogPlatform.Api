using BlogPlatform.Domain.Common;
using BlogPlatform.Domain.Entities;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace BlogPlatform.Tests.Unit.Domain.Common;

public sealed class EntityValidationExtensionsTests
{
    [Fact]
    public async Task ThrowIfInvalidAsync_WhenValidatorPasses_DoesNotThrowAndLeavesEntityClean()
    {
        // Given
        var entity = new BlogPost("title", "content");
        var validator = new Mock<IValidator<BlogPost>>();
        validator
            .Setup(v => v.ValidateAsync(entity, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // When
        var act = () => entity.ThrowIfInvalidAsync(validator.Object, CancellationToken.None);

        // Then
        await act.Should().NotThrowAsync();
        entity.Notifications.Should().BeEmpty();
    }

    [Fact]
    public async Task ThrowIfInvalidAsync_WhenValidatorFails_PopulatesNotificationsAndThrows()
    {
        // Given
        var entity = new BlogPost("title", "content");
        var failures = new[]
        {
            new ValidationFailure("Title", "Title is required."),
            new ValidationFailure("Content", "Content is required."),
        };
        var validator = new Mock<IValidator<BlogPost>>();
        validator
            .Setup(v => v.ValidateAsync(entity, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures));

        // When
        var act = () => entity.ThrowIfInvalidAsync(validator.Object, CancellationToken.None);

        // Then
        (await act.Should().ThrowAsync<DomainValidationException>())
            .Which.Notifications.Should().BeEquivalentTo(["Title is required.", "Content is required."]);
        entity.Notifications.Should().BeEquivalentTo(["Title is required.", "Content is required."]);
    }
}
