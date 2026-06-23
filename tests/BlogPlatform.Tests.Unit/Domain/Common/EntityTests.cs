using BlogPlatform.Domain.Common;
using BlogPlatform.Domain.Entities;
using FluentAssertions;

namespace BlogPlatform.Tests.Unit.Domain.Common;

public sealed class EntityTests
{
    [Fact]
    public void IsValid_WhenNoNotifications_ReturnsTrue()
    {
        // Given
        var entity = new BlogPost("title", "content");

        // When
        var isValid = entity.IsValid;

        // Then
        isValid.Should().BeTrue();
        entity.Notifications.Should().BeEmpty();
    }

    [Fact]
    public void AddNotification_AddsMessageAndMakesEntityInvalid()
    {
        // Given
        var entity = new BlogPost("title", "content");

        // When
        entity.AddNotification("something is wrong");

        // Then
        entity.IsValid.Should().BeFalse();
        entity.Notifications.Should().ContainSingle().Which.Should().Be("something is wrong");
    }

    [Fact]
    public void AddNotifications_AppendsAllProvidedMessages()
    {
        // Given
        var entity = new BlogPost("title", "content");

        // When
        entity.AddNotifications(["first", "second", "third"]);

        // Then
        entity.IsValid.Should().BeFalse();
        entity.Notifications.Should().BeEquivalentTo(["first", "second", "third"]);
    }

    [Fact]
    public void ThrowIfInvalid_WhenValid_DoesNotThrow()
    {
        // Given
        var entity = new BlogPost("title", "content");

        // When
        var act = () => entity.ThrowIfInvalid();

        // Then
        act.Should().NotThrow();
    }

    [Fact]
    public void ThrowIfInvalid_WhenInvalid_ThrowsDomainValidationExceptionWithNotifications()
    {
        // Given
        var entity = new BlogPost("title", "content");
        entity.AddNotifications(["title required", "content too long"]);

        // When
        var act = () => entity.ThrowIfInvalid();

        // Then
        act.Should().Throw<DomainValidationException>()
            .Which.Notifications.Should().BeEquivalentTo(["title required", "content too long"]);
    }
}
