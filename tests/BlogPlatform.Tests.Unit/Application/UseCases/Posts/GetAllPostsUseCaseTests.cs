using BlogPlatform.Application.Common.Queries;
using BlogPlatform.Application.DTOs.Posts;
using BlogPlatform.Application.UseCases.Posts;
using FluentAssertions;
using Moq;

namespace BlogPlatform.Tests.Unit.Application.UseCases.Posts;

public sealed class GetAllPostsUseCaseTests
{
    private readonly Mock<IBlogPostQueries> _queries = new();

    [Fact]
    public async Task Handle_ReturnsTheListFromQueries()
    {
        // Given
        var posts = new List<PostListItemDto>
        {
            new(Guid.NewGuid(), "First", DateTime.UtcNow, 0),
            new(Guid.NewGuid(), "Second", DateTime.UtcNow, 3),
        };
        _queries
            .Setup(q => q.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(posts);
        var useCase = new GetAllPostsUseCase(_queries.Object);

        // When
        var result = await useCase.Handle(CancellationToken.None);

        // Then
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(posts);
    }

    [Fact]
    public async Task Handle_WhenNoPosts_ReturnsEmptyList()
    {
        // Given
        _queries
            .Setup(q => q.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        var useCase = new GetAllPostsUseCase(_queries.Object);

        // When
        var result = await useCase.Handle(CancellationToken.None);

        // Then
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}
