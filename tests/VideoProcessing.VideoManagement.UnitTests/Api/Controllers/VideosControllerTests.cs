using System.Security.Claims;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using VideoProcessing.VideoManagement.Api.Controllers;
using VideoProcessing.VideoManagement.Api.Models;
using VideoProcessing.VideoManagement.Application.Models;
using VideoProcessing.VideoManagement.Application.Models.InputModels;
using VideoProcessing.VideoManagement.Application.Models.ResponseModels;
using VideoProcessing.VideoManagement.Application.UseCases.GetVideoById;
using VideoProcessing.VideoManagement.Application.UseCases.ListVideos;
using VideoProcessing.VideoManagement.Application.UseCases.UpdateVideo;
using VideoProcessing.VideoManagement.Application.UseCases.UploadVideo;
using VideoProcessing.VideoManagement.Domain.Enums;
using VideoProcessing.VideoManagement.Domain.Exceptions;
using Xunit;

namespace VideoProcessing.VideoManagement.UnitTests.Api.Controllers;

public class VideosControllerTests
{
    private readonly Mock<IUploadVideoUseCase> _uploadUseCaseMock = new();
    private readonly Mock<IListVideosUseCase> _listUseCaseMock = new();
    private readonly Mock<IGetVideoByIdUseCase> _getByIdUseCaseMock = new();
    private readonly Mock<IUpdateVideoUseCase> _updateVideoUseCaseMock = new();
    private readonly Mock<IValidator<UpdateVideoInputModel>> _updateVideoValidatorMock = new();
    private readonly VideosController _controller;

    public VideosControllerTests()
    {
        _controller = new VideosController(
            _uploadUseCaseMock.Object,
            _listUseCaseMock.Object,
            _getByIdUseCaseMock.Object,
            _updateVideoUseCaseMock.Object,
            _updateVideoValidatorMock.Object);
    }

    private void SetUser(string? sub = null)
    {
        var identity = new ClaimsIdentity();
        if (sub != null)
            identity.AddClaim(new Claim("sub", sub));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
    }

    // ─────────────────────────── UploadVideo ───────────────────────────

    [Fact]
    public async Task UploadVideo_WhenValidInput_ReturnsCreated()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetUser(userId.ToString());

        var input = new UploadVideoInputModel { OriginalFileName = "video.mp4", ContentType = "video/mp4", SizeKb = 100 };
        var responseModel = new UploadVideoResponseModel
        {
            VideoId = Guid.NewGuid(),
            UploadUrl = "https://s3.example.com/presigned",
            ExpiresAt = DateTime.UtcNow.AddMinutes(15)
        };

        _uploadUseCaseMock.Setup(x => x.ExecuteAsync(input, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseModel);

        // Act
        var result = await _controller.UploadVideo(input, CancellationToken.None);

        // Assert
        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.Value.Should().Be(responseModel);
        created.ActionName.Should().Be(nameof(VideosController.GetVideo));
    }

    [Fact]
    public async Task UploadVideo_WhenUseCaseThrowsValidationException_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetUser(userId.ToString());

        var input = new UploadVideoInputModel { OriginalFileName = "video.mp4", ContentType = "video/mp4", SizeKb = 100 };
        var failures = new[] { new ValidationFailure("OriginalFileName", "Invalid file name") };

        _uploadUseCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<UploadVideoInputModel>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException(failures));

        // Act
        var result = await _controller.UploadVideo(input, CancellationToken.None);

        // Assert
        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.Value.Should().BeOfType<ErrorResponse>();
    }

    // ─────────────────────────── ListVideos ───────────────────────────

    [Fact]
    public async Task ListVideos_WhenAuthenticated_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetUser(userId.ToString());

        var listResponse = new VideoListResponseModel
        {
            Videos = [],
            NextToken = null
        };

        _listUseCaseMock.Setup(x => x.ExecuteAsync(userId.ToString(), 50, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(listResponse);

        // Act
        var result = await _controller.ListVideos(null, null, CancellationToken.None);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(listResponse);
    }

    [Fact]
    public async Task ListVideos_WhenAuthenticated_WithCustomLimit_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetUser(userId.ToString());

        var listResponse = new VideoListResponseModel { Videos = [], NextToken = "token-123" };

        _listUseCaseMock.Setup(x => x.ExecuteAsync(userId.ToString(), 10, "token-abc", It.IsAny<CancellationToken>()))
            .ReturnsAsync(listResponse);

        // Act
        var result = await _controller.ListVideos(10, "token-abc", CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ListVideos_WhenSubMissing_ReturnsUnauthorized()
    {
        // Arrange
        SetUser();

        // Act
        var result = await _controller.ListVideos(null, null, CancellationToken.None);

        // Assert
        result.Should().BeOfType<UnauthorizedResult>();
        _listUseCaseMock.Verify(x => x.ExecuteAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ListVideos_WhenSubIsNotGuid_ReturnsUnauthorized()
    {
        // Arrange
        SetUser("not-a-guid");

        // Act
        var result = await _controller.ListVideos(null, null, CancellationToken.None);

        // Assert
        result.Should().BeOfType<UnauthorizedResult>();
    }

    // ─────────────────────────── GetVideo ───────────────────────────

    [Fact]
    public async Task GetVideo_WhenFound_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var videoId = Guid.NewGuid();
        SetUser(userId.ToString());

        var videoResponse = new VideoResponseModel { VideoId = videoId };

        _getByIdUseCaseMock.Setup(x => x.ExecuteAsync(userId.ToString(), videoId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(videoResponse);

        // Act
        var result = await _controller.GetVideo(videoId, CancellationToken.None);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(videoResponse);
    }

    [Fact]
    public async Task GetVideo_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var videoId = Guid.NewGuid();
        SetUser(userId.ToString());

        _getByIdUseCaseMock.Setup(x => x.ExecuteAsync(userId.ToString(), videoId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((VideoResponseModel?)null);

        // Act
        var result = await _controller.GetVideo(videoId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetVideo_WhenSubMissing_ReturnsUnauthorized()
    {
        // Arrange
        SetUser();

        // Act
        var result = await _controller.GetVideo(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeOfType<UnauthorizedResult>();
        _getByIdUseCaseMock.Verify(x => x.ExecuteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetVideo_WhenSubIsNotGuid_ReturnsUnauthorized()
    {
        // Arrange
        SetUser("not-a-guid");

        // Act
        var result = await _controller.GetVideo(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeOfType<UnauthorizedResult>();
    }

    // ─────────────────────────── UpdateVideo ───────────────────────────

    [Fact]
    public async Task UpdateVideo_WhenValid_ReturnsOk()
    {
        // Arrange
        SetUser();
        var videoId = Guid.NewGuid();
        var input = new UpdateVideoInputModel { Status = VideoStatus.ProcessingImages };
        var validationResult = new ValidationResult();
        var videoResponse = new VideoResponseModel { VideoId = videoId };

        _updateVideoValidatorMock.Setup(x => x.ValidateAsync(input, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        _updateVideoUseCaseMock.Setup(x => x.ExecuteAsync(videoId, input, It.IsAny<CancellationToken>()))
            .ReturnsAsync(videoResponse);

        // Act
        var result = await _controller.UpdateVideo(videoId, input, CancellationToken.None);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(videoResponse);
    }

    [Fact]
    public async Task UpdateVideo_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        SetUser();
        var videoId = Guid.NewGuid();
        var input = new UpdateVideoInputModel { Status = VideoStatus.ProcessingImages };
        var validationResult = new ValidationResult();

        _updateVideoValidatorMock.Setup(x => x.ValidateAsync(input, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        _updateVideoUseCaseMock.Setup(x => x.ExecuteAsync(videoId, input, It.IsAny<CancellationToken>()))
            .ReturnsAsync((VideoResponseModel?)null);

        // Act
        var result = await _controller.UpdateVideo(videoId, input, CancellationToken.None);

        // Assert
        var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFound.Value.Should().BeOfType<ApiErrorResponse>();
    }

    [Fact]
    public async Task UpdateVideo_WhenValidatorReturnsInvalid_ReturnsBadRequest()
    {
        // Arrange
        SetUser();
        var videoId = Guid.NewGuid();
        var input = new UpdateVideoInputModel();
        var failures = new List<ValidationFailure> { new("Status", "Status is required") };
        var invalidResult = new ValidationResult(failures);

        _updateVideoValidatorMock.Setup(x => x.ValidateAsync(input, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invalidResult);

        // Act
        var result = await _controller.UpdateVideo(videoId, input, CancellationToken.None);

        // Assert
        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var errorResponse = badRequest.Value.Should().BeOfType<ApiErrorResponse>().Subject;
        errorResponse.Error.Message.Should().Contain("Status is required");

        _updateVideoUseCaseMock.Verify(x => x.ExecuteAsync(It.IsAny<Guid>(), It.IsAny<UpdateVideoInputModel>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateVideo_WhenUseCaseThrowsValidationException_ReturnsBadRequest()
    {
        // Arrange
        SetUser();
        var videoId = Guid.NewGuid();
        var input = new UpdateVideoInputModel { Status = VideoStatus.ProcessingImages };
        var validationResult = new ValidationResult();
        var failures = new[] { new ValidationFailure("ProgressPercent", "Progress cannot regress") };

        _updateVideoValidatorMock.Setup(x => x.ValidateAsync(input, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        _updateVideoUseCaseMock.Setup(x => x.ExecuteAsync(videoId, input, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException(failures));

        // Act
        var result = await _controller.UpdateVideo(videoId, input, CancellationToken.None);

        // Assert
        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.Value.Should().BeOfType<ApiErrorResponse>();
    }

    [Fact]
    public async Task UpdateVideo_WhenUseCaseThrowsValidationExceptionWithNoErrors_ReturnsBadRequestWithMessage()
    {
        // Arrange
        SetUser();
        var videoId = Guid.NewGuid();
        var input = new UpdateVideoInputModel { Status = VideoStatus.ProcessingImages };
        var validationResult = new ValidationResult();

        _updateVideoValidatorMock.Setup(x => x.ValidateAsync(input, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        _updateVideoUseCaseMock.Setup(x => x.ExecuteAsync(videoId, input, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("General validation error"));

        // Act
        var result = await _controller.UpdateVideo(videoId, input, CancellationToken.None);

        // Assert
        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var errorResponse = badRequest.Value.Should().BeOfType<ApiErrorResponse>().Subject;
        errorResponse.Error.Message.Should().Contain("General validation error");
    }

    [Fact]
    public async Task UpdateVideo_WhenConflictException_ReturnsConflict()
    {
        // Arrange
        SetUser();
        var videoId = Guid.NewGuid();
        var input = new UpdateVideoInputModel { Status = VideoStatus.ProcessingImages };
        var validationResult = new ValidationResult();

        _updateVideoValidatorMock.Setup(x => x.ValidateAsync(input, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        _updateVideoUseCaseMock.Setup(x => x.ExecuteAsync(videoId, input, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new VideoUpdateConflictException("Conflict occurred"));

        // Act
        var result = await _controller.UpdateVideo(videoId, input, CancellationToken.None);

        // Assert
        var conflict = result.Should().BeOfType<ConflictObjectResult>().Subject;
        var errorResponse = conflict.Value.Should().BeOfType<ApiErrorResponse>().Subject;
        errorResponse.Error.Message.Should().Contain("Conflict occurred");
    }
}
