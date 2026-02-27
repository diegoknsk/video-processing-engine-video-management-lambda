using FluentAssertions;
using FluentValidation;
using Moq;
using VideoProcessing.VideoManagement.Application.Models.InputModels;
using VideoProcessing.VideoManagement.Application.Models.Mappers;
using VideoProcessing.VideoManagement.Application.Models.ResponseModels;
using VideoProcessing.VideoManagement.Application.UseCases.UpdateVideo;
using VideoProcessing.VideoManagement.Domain.Entities;
using VideoProcessing.VideoManagement.Domain.Enums;
using VideoProcessing.VideoManagement.Domain.Exceptions;
using VideoProcessing.VideoManagement.LambdaUpdateVideo;
using VideoProcessing.VideoManagement.LambdaUpdateVideo.Models;

namespace VideoProcessing.VideoManagement.UnitTests.LambdaUpdateVideo;

public class UpdateVideoHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenValidationFails_ShouldReturn400()
    {
        var validator = new Mock<IValidator<UpdateVideoInputModel>>();
        validator.Setup(v => v.ValidateAsync(It.IsAny<UpdateVideoInputModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(new[] { new FluentValidation.Results.ValidationFailure("UserId", "UserId é obrigatório.") }));
        var useCase = new Mock<IUpdateVideoUseCase>();

        var handler = new UpdateVideoHandler(useCase.Object, validator.Object);
        var evt = new UpdateVideoLambdaEvent { VideoId = Guid.NewGuid(), UserId = Guid.Empty };

        var result = await handler.HandleAsync(evt);

        result.StatusCode.Should().Be(400);
        result.ErrorCode.Should().Be("ValidationFailed");
        result.ErrorMessage.Should().Be("UserId é obrigatório.");
        useCase.Verify(u => u.ExecuteAsync(It.IsAny<Guid>(), It.IsAny<UpdateVideoInputModel>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenValid_ShouldCallUseCaseAndReturn200()
    {
        var userId = Guid.NewGuid();
        var videoId = Guid.NewGuid();
        var evt = new UpdateVideoLambdaEvent
        {
            VideoId = videoId,
            UserId = userId,
            Status = VideoStatus.Processing
        };
        var validator = new Mock<IValidator<UpdateVideoInputModel>>();
        validator.Setup(v => v.ValidateAsync(evt, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        var video = new Video(userId, "test.mp4", "video/mp4", 1024);
        var responseModel = VideoResponseModelMapper.ToResponseModel(video);
        var useCase = new Mock<IUpdateVideoUseCase>();
        useCase.Setup(u => u.ExecuteAsync(videoId, evt, It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseModel);

        var handler = new UpdateVideoHandler(useCase.Object, validator.Object);
        var result = await handler.HandleAsync(evt);

        result.StatusCode.Should().Be(200);
        result.Video.Should().NotBeNull();
        useCase.Verify(u => u.ExecuteAsync(videoId, evt, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenUseCaseReturnsNull_ShouldReturn404()
    {
        var userId = Guid.NewGuid();
        var videoId = Guid.NewGuid();
        var evt = new UpdateVideoLambdaEvent { VideoId = videoId, UserId = userId, Status = VideoStatus.Processing };
        var validator = new Mock<IValidator<UpdateVideoInputModel>>();
        validator.Setup(v => v.ValidateAsync(evt, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        var useCase = new Mock<IUpdateVideoUseCase>();
        useCase.Setup(u => u.ExecuteAsync(videoId, evt, It.IsAny<CancellationToken>()))
            .ReturnsAsync((VideoResponseModel?)null);

        var handler = new UpdateVideoHandler(useCase.Object, validator.Object);
        var result = await handler.HandleAsync(evt);

        result.StatusCode.Should().Be(404);
        result.ErrorCode.Should().Be("NotFound");
        result.Video.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WhenUseCaseThrowsVideoUpdateConflictException_ShouldReturn409()
    {
        var userId = Guid.NewGuid();
        var videoId = Guid.NewGuid();
        var evt = new UpdateVideoLambdaEvent { VideoId = videoId, UserId = userId, ProgressPercent = 50 };
        var validator = new Mock<IValidator<UpdateVideoInputModel>>();
        validator.Setup(v => v.ValidateAsync(evt, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        var useCase = new Mock<IUpdateVideoUseCase>();
        useCase.Setup(u => u.ExecuteAsync(videoId, evt, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new VideoUpdateConflictException("progress regression."));

        var handler = new UpdateVideoHandler(useCase.Object, validator.Object);
        var result = await handler.HandleAsync(evt);

        result.StatusCode.Should().Be(409);
        result.ErrorCode.Should().Be("UpdateConflict");
        result.ErrorMessage.Should().Contain("progress regression");
        result.Video.Should().BeNull();
    }
}
