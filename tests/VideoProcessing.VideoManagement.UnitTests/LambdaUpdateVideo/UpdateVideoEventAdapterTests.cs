using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VideoProcessing.VideoManagement.Domain.Enums;
using VideoProcessing.VideoManagement.LambdaUpdateVideo;
using VideoProcessing.VideoManagement.LambdaUpdateVideo.Models;

namespace VideoProcessing.VideoManagement.UnitTests.LambdaUpdateVideo;

/// <summary>
/// Testes do adapter de entrada: detecção SQS vs JSON direto e extração de UpdateVideoLambdaEvent.
/// </summary>
public class UpdateVideoEventAdapterTests
{
    private static readonly ILogger<UpdateVideoEventAdapter> Logger = new Mock<ILogger<UpdateVideoEventAdapter>>().Object;
    private readonly UpdateVideoEventAdapter _sut = new(Logger);

    [Fact]
    public void FromRawEvent_SqsSingleRecordValidBody_ReturnsOneEvent()
    {
        const string bodyPayload = """{"videoId":"3fa85f64-5717-4562-b3fc-2c963f66afa6","userId":"7c9e6679-7425-40de-944b-e07fc1f90ae7","status":2,"progressPercent":50}""";
        string bodyEscaped = bodyPayload.Replace("\"", "\\\"");
        string sqsJson = $"{{\"Records\":[{{\"messageId\":\"msg-1\",\"body\":\"{bodyEscaped}\"}}]}}";
        using var doc = JsonDocument.Parse(sqsJson);
        var result = _sut.FromRawEvent(doc);
        result.Should().HaveCount(1);
        result[0].VideoId.Should().Be(Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"));
        result[0].UserId.Should().Be(Guid.Parse("7c9e6679-7425-40de-944b-e07fc1f90ae7"));
        result[0].Status.Should().Be(VideoStatus.Processing);
        result[0].ProgressPercent.Should().Be(50);
    }

    [Fact]
    public void FromRawEvent_DirectJsonValid_ReturnsOneEvent()
    {
        const string directJson = """
            {"videoId":"3fa85f64-5717-4562-b3fc-2c963f66afa6","userId":"7c9e6679-7425-40de-944b-e07fc1f90ae7","status":3,"progressPercent":100}
            """;
        using var doc = JsonDocument.Parse(directJson);
        var result = _sut.FromRawEvent(doc);
        result.Should().HaveCount(1);
        result[0].VideoId.Should().Be(Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"));
        result[0].UserId.Should().Be(Guid.Parse("7c9e6679-7425-40de-944b-e07fc1f90ae7"));
        result[0].Status.Should().Be(VideoStatus.Completed);
        result[0].ProgressPercent.Should().Be(100);
    }

    [Fact]
    public void FromRawEvent_SqsTwoRecordsValidBodies_ReturnsTwoEvents()
    {
        const string body1 = """{"videoId":"11111111-1111-1111-1111-111111111111","userId":"aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa","status":0}""";
        const string body2 = """{"videoId":"22222222-2222-2222-2222-222222222222","userId":"bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb","status":1}""";
        string b1 = body1.Replace("\"", "\\\"");
        string b2 = body2.Replace("\"", "\\\"");
        string sqsJson = $"{{\"Records\":[{{\"messageId\":\"m1\",\"body\":\"{b1}\"}},{{\"messageId\":\"m2\",\"body\":\"{b2}\"}}]}}";
        using var doc = JsonDocument.Parse(sqsJson);
        var result = _sut.FromRawEvent(doc);
        result.Should().HaveCount(2);
        result[0].VideoId.Should().Be(Guid.Parse("11111111-1111-1111-1111-111111111111"));
        result[0].Status.Should().Be(VideoStatus.Pending);
        result[1].VideoId.Should().Be(Guid.Parse("22222222-2222-2222-2222-222222222222"));
        result[1].Status.Should().Be(VideoStatus.Uploading);
    }

    [Fact]
    public void FromRawEvent_SqsRecordsEmpty_ReturnsEmptyList()
    {
        const string sqsJson = """{"Records":[]}""";
        using var doc = JsonDocument.Parse(sqsJson);
        var result = _sut.FromRawEvent(doc);
        result.Should().BeEmpty();
    }

    [Fact]
    public void FromRawEvent_SqsRecordBodyInvalidJson_ReturnsEmptyList()
    {
        // body é string válida no envelope; conteúdo não é JSON válido para o DTO
        const string sqsJson = """{"Records":[{"messageId":"m1","body":"not-valid-json"}]}""";
        using var doc = JsonDocument.Parse(sqsJson);
        var result = _sut.FromRawEvent(doc);
        result.Should().BeEmpty();
    }

    [Fact]
    public void FromRawEvent_SqsRecordBodyEmpty_ReturnsEmptyList()
    {
        const string sqsJson = """{"Records":[{"messageId":"m1","body":""}]}""";
        using var doc = JsonDocument.Parse(sqsJson);
        var result = _sut.FromRawEvent(doc);
        result.Should().BeEmpty();
    }

    [Fact]
    public void FromRawEvent_DirectJsonMalformed_ReturnsEmptyList()
    {
        // videoId não é Guid válido → desserialização lança e adapter retorna vazio
        const string directJson = "{\"videoId\":\"not-a-guid\",\"userId\":\"7c9e6679-7425-40de-944b-e07fc1f90ae7\"}";
        using var doc = JsonDocument.Parse(directJson);
        var result = _sut.FromRawEvent(doc);
        result.Should().BeEmpty();
    }

    [Fact]
    public void FromRawEvent_NullDocument_ReturnsEmptyList()
    {
        var result = _sut.FromRawEvent(null!);
        result.Should().BeEmpty();
    }

    [Fact]
    public void FromRawEvent_ObjectWithRecordsButNoBody_TreatedAsDirect_ReturnsSingleWithDefaultGuids()
    {
        // Objeto com "Records" mas sem "body" no primeiro item → não é SQS, tratado como JSON direto
        // Desserialização produz um DTO com propriedades em default (VideoId/UserId = Guid.Empty); validação no handler rejeita depois
        const string json = """{"Records":[{"messageId":"m1"}]}""";
        using var doc = JsonDocument.Parse(json);
        var result = _sut.FromRawEvent(doc);
        result.Should().HaveCount(1);
        result[0].VideoId.Should().Be(Guid.Empty);
        result[0].UserId.Should().Be(Guid.Empty);
    }

    [Fact]
    public void FromRawEvent_DirectJsonWithAllFields_MapsCorrectly()
    {
        const string directJson = """
            {
              "videoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
              "userId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
              "status": 3,
              "progressPercent": 100,
              "framesPrefix": "videos/frames/",
              "s3BucketFrames": "my-bucket",
              "stepExecutionArn": "arn:aws:states:us-east-1:123:execution:sm:exec-1"
            }
            """;
        using var doc = JsonDocument.Parse(directJson);
        var result = _sut.FromRawEvent(doc);
        result.Should().HaveCount(1);
        result[0].FramesPrefix.Should().Be("videos/frames/");
        result[0].S3BucketFrames.Should().Be("my-bucket");
        result[0].StepExecutionArn.Should().Be("arn:aws:states:us-east-1:123:execution:sm:exec-1");
    }
}
