using System.Security.Claims;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using VideoProcessing.VideoManagement.Api.Controllers;
using VideoProcessing.VideoManagement.Application.Models.InputModels;
using VideoProcessing.VideoManagement.Application.Models.ResponseModels;
using VideoProcessing.VideoManagement.Application.UseCases.GetVideoById;
using VideoProcessing.VideoManagement.Application.UseCases.ListVideos;
using VideoProcessing.VideoManagement.Application.UseCases.UpdateVideo;
using VideoProcessing.VideoManagement.Application.UseCases.UploadVideo;
using Xunit;

namespace VideoProcessing.VideoManagement.UnitTests.Api.Controllers;

public class VideosControllerAuthTests
{
    private readonly Mock<IUploadVideoUseCase> _uploadUseCaseMock;
    private readonly Mock<IListVideosUseCase> _listUseCaseMock;
    private readonly Mock<IGetVideoByIdUseCase> _getByIdUseCaseMock;
    private readonly Mock<IUpdateVideoUseCase> _updateVideoUseCaseMock;
    private readonly Mock<IValidator<UpdateVideoInputModel>> _updateVideoValidatorMock;
    private readonly VideosController _controller;

    public VideosControllerAuthTests()
    {
        _uploadUseCaseMock = new Mock<IUploadVideoUseCase>();
        _listUseCaseMock = new Mock<IListVideosUseCase>();
        _getByIdUseCaseMock = new Mock<IGetVideoByIdUseCase>();
        _updateVideoUseCaseMock = new Mock<IUpdateVideoUseCase>();
        _updateVideoValidatorMock = new Mock<IValidator<UpdateVideoInputModel>>();
        _controller = new VideosController(
            _uploadUseCaseMock.Object,
            _listUseCaseMock.Object,
            _getByIdUseCaseMock.Object,
            _updateVideoUseCaseMock.Object,
            _updateVideoValidatorMock.Object);
    }

    private static void SetUser(VideosController controller, ClaimsPrincipal? user)
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user ?? new ClaimsPrincipal() }
        };
    }

    [Fact]
    public async Task UploadVideo_WhenSubClaimMissing_ReturnsUnauthorized()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity());
        SetUser(_controller, user);

        var input = new UploadVideoInputModel
        {
            OriginalFileName = "test.mp4",
            ContentType = "video/mp4",
            SizeKb = 1
        };

        // Act
        var result = await _controller.UploadVideo(input, CancellationToken.None);

        // Assert
        result.Should().BeOfType<UnauthorizedResult>();
        _uploadUseCaseMock.Verify(
            x => x.ExecuteAsync(It.IsAny<UploadVideoInputModel>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UploadVideo_WhenSubClaimIsNotGuid_ReturnsUnauthorized()
    {
        // Arrange
        var identity = new ClaimsIdentity();
        identity.AddClaim(new Claim("sub", "not-a-guid"));
        var user = new ClaimsPrincipal(identity);
        SetUser(_controller, user);

        var input = new UploadVideoInputModel
        {
            OriginalFileName = "test.mp4",
            ContentType = "video/mp4",
            SizeKb = 1
        };

        // Act
        var result = await _controller.UploadVideo(input, CancellationToken.None);

        // Assert
        result.Should().BeOfType<UnauthorizedResult>();
        _uploadUseCaseMock.Verify(
            x => x.ExecuteAsync(It.IsAny<UploadVideoInputModel>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UploadVideo_WhenSubClaimIsValidGuid_CallsUseCaseWithCorrectUserId()
    {
        // Arrange
        var expectedUserId = Guid.NewGuid();
        var identity = new ClaimsIdentity();
        identity.AddClaim(new Claim("sub", expectedUserId.ToString()));
        var user = new ClaimsPrincipal(identity);
        SetUser(_controller, user);

        var input = new UploadVideoInputModel
        {
            OriginalFileName = "test.mp4",
            ContentType = "video/mp4",
            SizeKb = 1
        };

        var expectedResponse = new UploadVideoResponseModel
        {
            VideoId = Guid.NewGuid(),
            UploadUrl = "https://s3.example.com/presigned",
            ExpiresAt = DateTime.UtcNow.AddMinutes(15)
        };

        _uploadUseCaseMock
            .Setup(x => x.ExecuteAsync(It.IsAny<UploadVideoInputModel>(), expectedUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.UploadVideo(input, CancellationToken.None);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.Value.Should().Be(expectedResponse);
        _uploadUseCaseMock.Verify(
            x => x.ExecuteAsync(input, expectedUserId, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
