using System.IO;
using System.Text;
using System.Text.Json;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Options;
using Moq;
using VideoProcessing.VideoManagement.Api.Configuration;
using VideoProcessing.VideoManagement.Api.Services;
using VideoProcessing.VideoManagement.Application.Models.InputModels;
using VideoProcessing.VideoManagement.Application.Models.ResponseModels;
using VideoProcessing.VideoManagement.Domain.Exceptions;
using VideoProcessing.VideoManagement.LambdaUpdateVideo.Models;
using Xunit;

namespace VideoProcessing.VideoManagement.UnitTests.Api.Services;

public class UpdateVideoLambdaProxyUseCaseTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private readonly Mock<IAmazonLambda> _lambdaClientMock = new();
    private readonly UpdateVideoLambdaProxyUseCase _sut;

    public UpdateVideoLambdaProxyUseCaseTests()
    {
        var options = Options.Create(new LambdaUpdateVideoOptions { FunctionName = "update-video-fn" });
        _sut = new UpdateVideoLambdaProxyUseCase(_lambdaClientMock.Object, options);
    }

    private static MemoryStream BuildPayload(UpdateVideoLambdaResponse response)
    {
        var json = JsonSerializer.Serialize(response, JsonOptions);
        return new MemoryStream(Encoding.UTF8.GetBytes(json));
    }

    [Fact]
    public async Task ExecuteAsync_WhenLambdaReturns200_ReturnsVideoResponseModel()
    {
        // Arrange
        var videoId = Guid.NewGuid();
        var expectedVideo = new VideoResponseModel { VideoId = videoId };
        var lambdaResponse = UpdateVideoLambdaResponse.Ok(expectedVideo);

        _lambdaClientMock.Setup(x => x.InvokeAsync(It.IsAny<InvokeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InvokeResponse
            {
                Payload = BuildPayload(lambdaResponse),
                FunctionError = null
            });

        var input = new UpdateVideoInputModel { UserId = Guid.NewGuid() };

        // Act
        var result = await _sut.ExecuteAsync(videoId, input, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.VideoId.Should().Be(videoId);
    }

    [Fact]
    public async Task ExecuteAsync_WhenLambdaReturns404_ReturnsNull()
    {
        // Arrange
        var videoId = Guid.NewGuid();
        var lambdaResponse = UpdateVideoLambdaResponse.NotFound();

        _lambdaClientMock.Setup(x => x.InvokeAsync(It.IsAny<InvokeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InvokeResponse
            {
                Payload = BuildPayload(lambdaResponse),
                FunctionError = null
            });

        var input = new UpdateVideoInputModel { UserId = Guid.NewGuid() };

        // Act
        var result = await _sut.ExecuteAsync(videoId, input, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenLambdaReturns400_ThrowsValidationException()
    {
        // Arrange
        var videoId = Guid.NewGuid();
        var lambdaResponse = UpdateVideoLambdaResponse.ValidationError("Progress cannot regress");

        _lambdaClientMock.Setup(x => x.InvokeAsync(It.IsAny<InvokeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InvokeResponse
            {
                Payload = BuildPayload(lambdaResponse),
                FunctionError = null
            });

        var input = new UpdateVideoInputModel { UserId = Guid.NewGuid() };

        // Act & Assert
        await _sut.Invoking(x => x.ExecuteAsync(videoId, input, CancellationToken.None))
            .Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task ExecuteAsync_WhenLambdaReturns409_ThrowsVideoUpdateConflictException()
    {
        // Arrange
        var videoId = Guid.NewGuid();
        var lambdaResponse = UpdateVideoLambdaResponse.Conflict("Concurrent update conflict");

        _lambdaClientMock.Setup(x => x.InvokeAsync(It.IsAny<InvokeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InvokeResponse
            {
                Payload = BuildPayload(lambdaResponse),
                FunctionError = null
            });

        var input = new UpdateVideoInputModel { UserId = Guid.NewGuid() };

        // Act & Assert
        await _sut.Invoking(x => x.ExecuteAsync(videoId, input, CancellationToken.None))
            .Should().ThrowAsync<VideoUpdateConflictException>()
            .WithMessage("*Concurrent update conflict*");
    }

    [Fact]
    public async Task ExecuteAsync_WhenLambdaReturnsUnexpectedStatusCode_ThrowsInvalidOperationException()
    {
        // Arrange
        var videoId = Guid.NewGuid();
        var lambdaResponse = new UpdateVideoLambdaResponse { StatusCode = 500, ErrorMessage = "Internal error" };

        _lambdaClientMock.Setup(x => x.InvokeAsync(It.IsAny<InvokeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InvokeResponse
            {
                Payload = BuildPayload(lambdaResponse),
                FunctionError = null
            });

        var input = new UpdateVideoInputModel { UserId = Guid.NewGuid() };

        // Act & Assert
        await _sut.Invoking(x => x.ExecuteAsync(videoId, input, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*status inesperado*");
    }

    [Fact]
    public async Task ExecuteAsync_WhenLambdaClientThrows_ThrowsInvalidOperationException()
    {
        // Arrange
        var videoId = Guid.NewGuid();

        _lambdaClientMock.Setup(x => x.InvokeAsync(It.IsAny<InvokeRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Network error"));

        var input = new UpdateVideoInputModel { UserId = Guid.NewGuid() };

        // Act & Assert
        await _sut.Invoking(x => x.ExecuteAsync(videoId, input, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Falha ao invocar Lambda*");
    }

    [Fact]
    public async Task ExecuteAsync_WhenFunctionErrorSet_ThrowsInvalidOperationException()
    {
        // Arrange
        var videoId = Guid.NewGuid();
        var lambdaResponse = new UpdateVideoLambdaResponse { StatusCode = 200 };

        _lambdaClientMock.Setup(x => x.InvokeAsync(It.IsAny<InvokeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InvokeResponse
            {
                Payload = BuildPayload(lambdaResponse),
                FunctionError = "Unhandled"
            });

        var input = new UpdateVideoInputModel { UserId = Guid.NewGuid() };

        // Act & Assert
        await _sut.Invoking(x => x.ExecuteAsync(videoId, input, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Lambda retornou erro*");
    }

    [Fact]
    public async Task ExecuteAsync_WhenLambdaReturns200WithNullVideo_ThrowsInvalidOperationException()
    {
        // Arrange
        var videoId = Guid.NewGuid();
        var lambdaResponse = new UpdateVideoLambdaResponse { StatusCode = 200, Video = null };

        _lambdaClientMock.Setup(x => x.InvokeAsync(It.IsAny<InvokeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InvokeResponse
            {
                Payload = BuildPayload(lambdaResponse),
                FunctionError = null
            });

        var input = new UpdateVideoInputModel { UserId = Guid.NewGuid() };

        // Act & Assert
        await _sut.Invoking(x => x.ExecuteAsync(videoId, input, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*200 sem body de vídeo*");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSendCorrectFunctionNameAndPayload()
    {
        // Arrange
        var videoId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var lambdaResponse = UpdateVideoLambdaResponse.Ok(new VideoResponseModel { VideoId = videoId });

        InvokeRequest? capturedRequest = null;
        _lambdaClientMock.Setup(x => x.InvokeAsync(It.IsAny<InvokeRequest>(), It.IsAny<CancellationToken>()))
            .Callback<InvokeRequest, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new InvokeResponse
            {
                Payload = BuildPayload(lambdaResponse),
                FunctionError = null
            });

        var input = new UpdateVideoInputModel { UserId = userId };

        // Act
        await _sut.ExecuteAsync(videoId, input, CancellationToken.None);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.FunctionName.Should().Be("update-video-fn");
        capturedRequest.InvocationType.Should().Be(InvocationType.RequestResponse);
    }
}
