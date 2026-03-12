using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using VideoProcessing.VideoManagement.Domain.Entities;
using VideoProcessing.VideoManagement.Infra.CrossCutting.Configuration;
using VideoProcessing.VideoManagement.Infra.Data.Repositories;
using Xunit;

namespace VideoProcessing.VideoManagement.UnitTests.Infra.Data.Repositories;

public class VideoChunkRepositoryTests
{
    private readonly Mock<IAmazonDynamoDB> _dynamoMock;
    private readonly VideoChunkRepository _repository;

    public VideoChunkRepositoryTests()
    {
        _dynamoMock = new Mock<IAmazonDynamoDB>();
        var options = Options.Create(new DynamoDbOptions
        {
            TableName = "videos",
            Region = "us-east-1",
            ChunksTableName = "video-chunks-table"
        });
        _repository = new VideoChunkRepository(_dynamoMock.Object, options);
    }

    [Fact]
    public async Task UpsertAsync_ShouldCallPutItemWithCorrectPkSk()
    {
        _dynamoMock.Setup(x => x.PutItemAsync(It.IsAny<PutItemRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PutItemResponse());

        var chunk = new VideoChunk(
            ChunkId: "chunk-01",
            VideoId: "vid-123",
            Status: "completed",
            StartSec: 0,
            EndSec: 10,
            IntervalSec: 1,
            ManifestPrefix: "m/",
            FramesPrefix: "f/",
            ProcessedAt: DateTime.UtcNow,
            CreatedAt: DateTime.UtcNow);

        await _repository.UpsertAsync(chunk);

        _dynamoMock.Verify(x => x.PutItemAsync(
            It.Is<PutItemRequest>(r =>
                r.TableName == "video-chunks-table" &&
                r.Item["pk"].S == "VIDEO#vid-123" &&
                r.Item["sk"].S == "CHUNK#chunk-01" &&
                r.Item["chunkId"].S == "chunk-01" &&
                r.Item["status"].S == "completed"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CountProcessedAsync_ShouldReturnCountFromQuery()
    {
        _dynamoMock.Setup(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QueryResponse { Count = 3 });

        var count = await _repository.CountProcessedAsync("vid-123");

        count.Should().Be(3);
        _dynamoMock.Verify(x => x.QueryAsync(
            It.Is<QueryRequest>(r =>
                r.TableName == "video-chunks-table" &&
                r.KeyConditionExpression == "pk = :pk" &&
                r.Select == Select.COUNT),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
