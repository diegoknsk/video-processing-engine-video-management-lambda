using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using VideoProcessing.VideoManagement.Api.Controllers.Internal;
using VideoProcessing.VideoManagement.Application.Models.ResponseModels;
using VideoProcessing.VideoManagement.Application.UseCases.GetVideoById;
using Xunit;

namespace VideoProcessing.VideoManagement.UnitTests.Api.Controllers;

public class VideosInternalControllerTests
{
    private readonly Mock<IGetVideoByIdUseCase> _getByIdUseCaseMock = new();
    private readonly VideosInternalController _controller;

    public VideosInternalControllerTests()
    {
        _controller = new VideosInternalController(_getByIdUseCaseMock.Object);
    }

    [Fact]
    public async Task GetVideo_WhenFound_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var videoId = Guid.NewGuid();
        var videoResponse = new VideoResponseModel { VideoId = videoId };

        _getByIdUseCaseMock.Setup(x => x.ExecuteAsync(userId.ToString(), videoId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(videoResponse);

        // Act
        var result = await _controller.GetVideo(userId, videoId, CancellationToken.None);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(videoResponse);
        _getByIdUseCaseMock.Verify(x => x.ExecuteAsync(userId.ToString(), videoId.ToString(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetVideo_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var videoId = Guid.NewGuid();

        _getByIdUseCaseMock.Setup(x => x.ExecuteAsync(userId.ToString(), videoId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((VideoResponseModel?)null);

        // Act
        var result = await _controller.GetVideo(userId, videoId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
}
