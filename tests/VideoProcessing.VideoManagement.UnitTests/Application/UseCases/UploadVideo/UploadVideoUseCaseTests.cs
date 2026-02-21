using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using VideoProcessing.VideoManagement.Application.Configuration;
using VideoProcessing.VideoManagement.Application.Models.InputModels;
using VideoProcessing.VideoManagement.Application.Ports;
using VideoProcessing.VideoManagement.Application.UseCases.UploadVideo;
using VideoProcessing.VideoManagement.Application.Validators;
using VideoProcessing.VideoManagement.Domain.Entities;

namespace VideoProcessing.VideoManagement.UnitTests.Application.UseCases.UploadVideo;

public class UploadVideoUseCaseTests
{
    private readonly Mock<IVideoRepository> _repositoryMock;
    private readonly Mock<IS3PresignedUrlService> _s3ServiceMock;
    private readonly S3Options _s3Options;
    private readonly IValidator<UploadVideoInputModel> _validator;
    private readonly Mock<ILogger<UploadVideoUseCase>> _loggerMock;
    private readonly UploadVideoUseCase _useCase;

    public UploadVideoUseCaseTests()
    {
        _repositoryMock = new Mock<IVideoRepository>();
        _s3ServiceMock = new Mock<IS3PresignedUrlService>();
        _s3Options = new S3Options { BucketVideo = "video-bucket", BucketFrames = "frames-bucket", BucketZip = "zip-bucket", Region = "us-east-1", PresignedUrlTtlMinutes = 15 };
        _validator = new UploadVideoInputModelValidator();
        _loggerMock = new Mock<ILogger<UploadVideoUseCase>>();

        _useCase = new UploadVideoUseCase(
            _repositoryMock.Object,
            _s3ServiceMock.Object,
            Options.Create(_s3Options),
            _validator,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_ValidInput_ShouldCreateVideoAndReturnPresignedUrl()
    {
        // Arrange
        var input = new UploadVideoInputModel
        {
            OriginalFileName = "test.mp4",
            ContentType = "video/mp4",
            SizeKb = 1,
            ClientRequestId = Guid.NewGuid().ToString()
        };
        var userId = Guid.NewGuid();
        var expectedUrl = "https://s3.amazonaws.com/video-bucket/presigned-url";

        _repositoryMock.Setup(r => r.GetByClientRequestIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Video?)null);

        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<Video>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Video v, string? cid, CancellationToken ct) => v);

        _s3ServiceMock.Setup(s => s.GeneratePutPresignedUrl(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<string>()))
            .Returns(expectedUrl);

        // Act
        var result = await _useCase.ExecuteAsync(input, userId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedUrl, result.UploadUrl);
        Assert.NotEqual(Guid.Empty, result.VideoId);

        _repositoryMock.Verify(r => r.CreateAsync(It.Is<Video>(v =>
            v.UserId == userId &&
            v.OriginalFileName == input.OriginalFileName &&
            v.S3BucketVideo == _s3Options.BucketVideo),
            input.ClientRequestId,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_Idempotency_ShouldReturnExistingVideo()
    {
        // Arrange
        var clientRequestId = Guid.NewGuid().ToString();
        var input = new UploadVideoInputModel
        {
            OriginalFileName = "test.mp4",
            ContentType = "video/mp4",
            SizeKb = 1,
            ClientRequestId = clientRequestId
        };
        var userId = Guid.NewGuid();
        var existingVideo = new Video(userId, "test.mp4", "video/mp4", 1024, clientRequestId);
        // We need to set S3 details because GeneratePresignedUrl checks them
        existingVideo.SetS3Source(_s3Options.BucketVideo, $"videos/{userId}/{existingVideo.VideoId}/original");

        var expectedUrl = "https://s3.amazonaws.com/video-bucket/presigned-url-new";

        _repositoryMock.Setup(r => r.GetByClientRequestIdAsync(userId.ToString(), clientRequestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingVideo);

        _s3ServiceMock.Setup(s => s.GeneratePutPresignedUrl(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<string>()))
            .Returns(expectedUrl);

        // Act
        var result = await _useCase.ExecuteAsync(input, userId, CancellationToken.None);

        // Assert
        Assert.Equal(existingVideo.VideoId, result.VideoId);
        Assert.Equal(expectedUrl, result.UploadUrl);

        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Video>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidInput_ShouldThrowValidationException()
    {
        // Arrange
        var input = new UploadVideoInputModel
        {
            OriginalFileName = "", // Invalid
            ContentType = "invalid/type", // Invalid
            SizeKb = -1 // Invalid
        };
        var userId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _useCase.ExecuteAsync(input, userId, CancellationToken.None));
    }

    [Fact]
    public async Task ExecuteAsync_RepositoryThrows_ShouldPropagateException()
    {
        // Arrange
        var input = new UploadVideoInputModel
        {
            OriginalFileName = "test.mp4",
            ContentType = "video/mp4",
            SizeKb = 1
        };
        var userId = Guid.NewGuid();

        _repositoryMock.Setup(r => r.GetByClientRequestIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Video?)null);

        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<Video>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB error"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => _useCase.ExecuteAsync(input, userId, CancellationToken.None));
        Assert.Equal("DB error", ex.Message);
    }
}
