using FluentAssertions;
using Moq;
using VideoProcessing.VideoManagement.Application.Ports;
using VideoProcessing.VideoManagement.Application.UseCases.GetVideoById;
using VideoProcessing.VideoManagement.Domain.Entities;

namespace VideoProcessing.VideoManagement.UnitTests.Application.UseCases.GetVideoById;

public class GetVideoByIdUseCaseTests
{
    private readonly Mock<IVideoRepository> _repositoryMock;
    private readonly GetVideoByIdUseCase _sut;

    public GetVideoByIdUseCaseTests()
    {
        _repositoryMock = new Mock<IVideoRepository>();
        _sut = new GetVideoByIdUseCase(_repositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_VideoFound_ShouldReturnVideoResponseModel()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var video = new Video(userId, "test.mp4", "video/mp4", 1024);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(userId.ToString(), video.VideoId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(video);

        // Act
        var result = await _sut.ExecuteAsync(userId.ToString(), video.VideoId.ToString(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.VideoId.Should().Be(video.VideoId);
        result.UserId.Should().Be(video.UserId);
        result.OriginalFileName.Should().Be(video.OriginalFileName);
        result.ContentType.Should().Be(video.ContentType);
        result.SizeBytes.Should().Be(video.SizeBytes);
        result.Status.Should().Be(video.Status);

        _repositoryMock.Verify(r => r.GetByIdAsync(userId.ToString(), video.VideoId.ToString(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_VideoNotFound_ShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var videoId = Guid.NewGuid().ToString();

        _repositoryMock
            .Setup(r => r.GetByIdAsync(userId, videoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Video?)null);

        // Act
        var result = await _sut.ExecuteAsync(userId, videoId, CancellationToken.None);

        // Assert
        result.Should().BeNull();

        _repositoryMock.Verify(r => r.GetByIdAsync(userId, videoId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
