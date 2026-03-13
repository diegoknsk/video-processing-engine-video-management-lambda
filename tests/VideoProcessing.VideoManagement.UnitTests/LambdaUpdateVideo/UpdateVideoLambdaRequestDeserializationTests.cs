using System.Text.Json;
using FluentAssertions;
using VideoProcessing.VideoManagement.Domain.Enums;
using VideoProcessing.VideoManagement.LambdaUpdateVideo.Models;

namespace VideoProcessing.VideoManagement.UnitTests.LambdaUpdateVideo;

/// <summary>
/// Garante que os exemplos JSON documentados (mínimo e completo) deserializam corretamente para UpdateVideoLambdaEvent (UpdateVideoInputModel + videoId).
/// </summary>
public class UpdateVideoLambdaEventDeserializationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Fact]
    public void Deserialize_ExampleMinimal_ShouldMapCorrectly()
    {
        var json = """
            {
              "videoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
              "userId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
              "status": 1,
              "progressPercent": 50
            }
            """;
        var result = JsonSerializer.Deserialize<UpdateVideoLambdaEvent>(json, JsonOptions);
        result.Should().NotBeNull();
        result!.VideoId.Should().Be(Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"));
        result.UserId.Should().Be(Guid.Parse("7c9e6679-7425-40de-944b-e07fc1f90ae7"));
        result.Status.Should().Be(VideoStatus.ProcessingImages);
        result.ProgressPercent.Should().Be(50);
        result.ErrorMessage.Should().BeNull();
        result.FramesPrefix.Should().BeNull();
    }

    [Fact]
    public void Deserialize_ExampleComplete_ShouldMapAllFields()
    {
        var json = """
            {
              "videoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
              "userId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
              "status": 3,
              "progressPercent": 100,
              "errorMessage": null,
              "errorCode": null,
              "framesPrefix": "videos/7c9e6679-7425-40de-944b-e07fc1f90ae7/3fa85f64-5717-4562-b3fc-2c963f66afa6/frames/",
              "s3KeyZip": "videos/7c9e6679-7425-40de-944b-e07fc1f90ae7/3fa85f64-5717-4562-b3fc-2c963f66afa6/out.zip",
              "s3BucketFrames": "my-bucket-frames",
              "s3BucketZip": "my-bucket-zip",
              "stepExecutionArn": "arn:aws:states:us-east-1:123456789012:execution:MyStateMachine:exec-123"
            }
            """;
        var result = JsonSerializer.Deserialize<UpdateVideoLambdaEvent>(json, JsonOptions);
        result.Should().NotBeNull();
        result!.VideoId.Should().Be(Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"));
        result.UserId.Should().Be(Guid.Parse("7c9e6679-7425-40de-944b-e07fc1f90ae7"));
        result.Status.Should().Be(VideoStatus.Completed);
        result.ProgressPercent.Should().Be(100);
        result.FramesPrefix.Should().Be("videos/7c9e6679-7425-40de-944b-e07fc1f90ae7/3fa85f64-5717-4562-b3fc-2c963f66afa6/frames/");
        result.S3KeyZip.Should().Be("videos/7c9e6679-7425-40de-944b-e07fc1f90ae7/3fa85f64-5717-4562-b3fc-2c963f66afa6/out.zip");
        result.S3BucketFrames.Should().Be("my-bucket-frames");
        result.S3BucketZip.Should().Be("my-bucket-zip");
        result.StepExecutionArn.Should().Be("arn:aws:states:us-east-1:123456789012:execution:MyStateMachine:exec-123");
    }

    [Fact]
    public void Deserialize_PayloadWithChunkSingular_ShouldMapChunkCorrectly()
    {
        var json = """
            {
              "videoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
              "userId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
              "status": 2,
              "chunk": {
                "chunkId": "chunk-001",
                "startSec": 23,
                "endSec": 45,
                "intervalSec": 5,
                "framesPrefix": "videos/frames/chunk-001/",
                "manifestPrefix": "videos/manifest/chunk-001/"
              }
            }
            """;
        var result = JsonSerializer.Deserialize<UpdateVideoLambdaEvent>(json, JsonOptions);
        result.Should().NotBeNull();
        result!.Chunk.Should().NotBeNull();
        result.Chunk!.ChunkId.Should().Be("chunk-001");
        result.Chunk.StartSec.Should().Be(23);
        result.Chunk.EndSec.Should().Be(45);
        result.Chunk.IntervalSec.Should().Be(5);
        result.Chunk.FramesPrefix.Should().Be("videos/frames/chunk-001/");
        result.Chunk.ManifestPrefix.Should().Be("videos/manifest/chunk-001/");
    }
}
