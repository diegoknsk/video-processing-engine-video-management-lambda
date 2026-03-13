using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using VideoProcessing.VideoManagement.Application.Models;
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

    [Fact]
    public async Task GetStatusSummaryAsync_WhenNoChunks_ReturnsZeroSummaryAndNullFinalize()
    {
        _dynamoMock.Setup(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QueryResponse { Items = [], LastEvaluatedKey = null });

        var result = await _repository.GetStatusSummaryAsync("vid-123");

        result.Total.Should().Be(0);
        result.Completed.Should().Be(0);
        result.Processing.Should().Be(0);
        result.Failed.Should().Be(0);
        result.Pending.Should().Be(0);
        result.FinalizeStatus.Should().BeNull();
    }

    [Fact]
    public async Task GetStatusSummaryAsync_WhenMixedChunksAndFinalize_AggregatesCorrectlyAndExcludesFinalizeFromTotal()
    {
        var items = new List<Dictionary<string, AttributeValue>>
        {
            new() { ["chunkId"] = new AttributeValue { S = "chunk-0" }, ["status"] = new AttributeValue { S = "completed" } },
            new() { ["chunkId"] = new AttributeValue { S = "chunk-1" }, ["status"] = new AttributeValue { S = "completed" } },
            new() { ["chunkId"] = new AttributeValue { S = "chunk-2" }, ["status"] = new AttributeValue { S = "processing" } },
            new() { ["chunkId"] = new AttributeValue { S = "chunk-3" }, ["status"] = new AttributeValue { S = "failed" } },
            new() { ["chunkId"] = new AttributeValue { S = "chunk-4" }, ["status"] = new AttributeValue { S = "pending" } },
            new() { ["chunkId"] = new AttributeValue { S = "finalize" }, ["status"] = new AttributeValue { S = "completed" } }
        };
        _dynamoMock.Setup(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QueryResponse { Items = items, LastEvaluatedKey = null });

        var result = await _repository.GetStatusSummaryAsync("vid-123");

        result.Total.Should().Be(5);
        result.Completed.Should().Be(2);
        result.Processing.Should().Be(1);
        result.Failed.Should().Be(1);
        result.Pending.Should().Be(1);
        result.FinalizeStatus.Should().Be("completed");
    }

    [Fact]
    public async Task GetStatusSummaryAsync_WhenOnlyFinalize_ReturnsTotalZeroAndFinalizeStatus()
    {
        var items = new List<Dictionary<string, AttributeValue>>
        {
            new() { ["chunkId"] = new AttributeValue { S = "finalize" }, ["status"] = new AttributeValue { S = "processing" } }
        };
        _dynamoMock.Setup(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QueryResponse { Items = items, LastEvaluatedKey = null });

        var result = await _repository.GetStatusSummaryAsync("vid-123");

        result.Total.Should().Be(0);
        result.FinalizeStatus.Should().Be("processing");
    }

    [Fact]
    public async Task GetStatusSummaryAsync_SendsQueryWithProjectionChunkIdAndStatus()
    {
        _dynamoMock.Setup(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QueryResponse { Items = [], LastEvaluatedKey = null });

        await _repository.GetStatusSummaryAsync("vid-456");

        _dynamoMock.Verify(x => x.QueryAsync(
            It.Is<QueryRequest>(r =>
                r.TableName == "video-chunks-table" &&
                r.KeyConditionExpression == "pk = :pk" &&
                r.ProjectionExpression != null &&
                r.ProjectionExpression.Contains("chunkId") &&
                r.ProjectionExpression.Contains("#st")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpsertAsync_WhenOptionalFieldsMissing_ShouldNotIncludeManifestFramesProcessedAt()
    {
        PutItemRequest? captured = null;
        _dynamoMock.Setup(x => x.PutItemAsync(It.IsAny<PutItemRequest>(), It.IsAny<CancellationToken>()))
            .Callback<PutItemRequest, CancellationToken>((req, _) => captured = req)
            .ReturnsAsync(new PutItemResponse());

        var chunk = new VideoChunk(
            ChunkId: "chunk-02",
            VideoId: "vid-456",
            Status: "pending",
            StartSec: 0,
            EndSec: 5,
            IntervalSec: 0.5,
            ManifestPrefix: null,
            FramesPrefix: null,
            ProcessedAt: null,
            CreatedAt: DateTime.UtcNow);

        await _repository.UpsertAsync(chunk);

        captured.Should().NotBeNull();
        captured!.Item.Should().NotContainKey("manifestPrefix");
        captured.Item.Should().NotContainKey("framesPrefix");
        captured.Item.Should().NotContainKey("processedAt");
        captured.Item["pk"].S.Should().Be("VIDEO#vid-456");
        captured.Item["sk"].S.Should().Be("CHUNK#chunk-02");
    }

    [Fact]
    public async Task CountProcessedAsync_WhenResponseCountZero_ShouldReturnZero()
    {
        _dynamoMock.Setup(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QueryResponse { Count = 0, Items = [] });

        var count = await _repository.CountProcessedAsync("vid-123");

        count.Should().Be(0);
    }

    [Fact]
    public async Task GetStatusSummaryAsync_WhenPaginated_ShouldAggregateAllPages()
    {
        var page1 = new List<Dictionary<string, AttributeValue>>
        {
            new() { ["chunkId"] = new AttributeValue { S = "chunk-0" }, ["status"] = new AttributeValue { S = "completed" } }
        };
        var page2 = new List<Dictionary<string, AttributeValue>>
        {
            new() { ["chunkId"] = new AttributeValue { S = "chunk-1" }, ["status"] = new AttributeValue { S = "pending" } }
        };
        var lastKey = new Dictionary<string, AttributeValue> { ["pk"] = new AttributeValue { S = "VIDEO#vid" }, ["sk"] = new AttributeValue { S = "CHUNK#chunk-0" } };
        _dynamoMock.SetupSequence(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QueryResponse { Items = page1, LastEvaluatedKey = lastKey })
            .ReturnsAsync(new QueryResponse { Items = page2, LastEvaluatedKey = null });

        var result = await _repository.GetStatusSummaryAsync("vid-123");

        result.Total.Should().Be(2);
        result.Completed.Should().Be(1);
        result.Pending.Should().Be(1);
        _dynamoMock.Verify(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task GetStatusSummaryAsync_WhenChunkIdEmpty_ShouldSkipItem()
    {
        var items = new List<Dictionary<string, AttributeValue>>
        {
            new() { ["chunkId"] = new AttributeValue { S = "" }, ["status"] = new AttributeValue { S = "completed" } },
            new() { ["chunkId"] = new AttributeValue { S = "chunk-0" }, ["status"] = new AttributeValue { S = "completed" } }
        };
        _dynamoMock.Setup(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QueryResponse { Items = items, LastEvaluatedKey = null });

        var result = await _repository.GetStatusSummaryAsync("vid-123");

        result.Total.Should().Be(1);
        result.Completed.Should().Be(1);
    }

    [Fact]
    public async Task GetStatusSummaryAsync_WhenStatusNullOrUnknown_ShouldCountAsPending()
    {
        var items = new List<Dictionary<string, AttributeValue>>
        {
            new() { ["chunkId"] = new AttributeValue { S = "c1" } },
            new() { ["chunkId"] = new AttributeValue { S = "c2" }, ["status"] = new AttributeValue { S = "unknown" } }
        };
        _dynamoMock.Setup(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QueryResponse { Items = items, LastEvaluatedKey = null });

        var result = await _repository.GetStatusSummaryAsync("vid-123");

        result.Total.Should().Be(2);
        result.Pending.Should().Be(2);
    }

    [Fact]
    public async Task GetChunksAsync_WhenSinglePage_ReturnsMappedChunks()
    {
        var createdAt = DateTime.UtcNow.ToString("O");
        var items = new List<Dictionary<string, AttributeValue>>
        {
            new()
            {
                ["chunkId"] = new AttributeValue { S = "chunk-0" },
                ["status"] = new AttributeValue { S = "completed" },
                ["startSec"] = new AttributeValue { N = "0" },
                ["endSec"] = new AttributeValue { N = "10" },
                ["intervalSec"] = new AttributeValue { N = "1" },
                ["manifestPrefix"] = new AttributeValue { S = "m/" },
                ["framesPrefix"] = new AttributeValue { S = "f/" },
                ["processedAt"] = new AttributeValue { S = createdAt },
                ["createdAt"] = new AttributeValue { S = createdAt }
            }
        };
        _dynamoMock.Setup(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QueryResponse { Items = items, LastEvaluatedKey = null });

        var result = await _repository.GetChunksAsync("vid-123");

        result.Should().HaveCount(1);
        result[0].ChunkId.Should().Be("chunk-0");
        result[0].VideoId.Should().Be("vid-123");
        result[0].Status.Should().Be("completed");
        result[0].StartSec.Should().Be(0);
        result[0].EndSec.Should().Be(10);
        result[0].IntervalSec.Should().Be(1);
        result[0].ManifestPrefix.Should().Be("m/");
        result[0].FramesPrefix.Should().Be("f/");
    }

    [Fact]
    public async Task GetChunksAsync_WhenFinalizePresent_ShouldExcludeFromList()
    {
        var createdAt = DateTime.UtcNow.ToString("O");
        var items = new List<Dictionary<string, AttributeValue>>
        {
            new()
            {
                ["chunkId"] = new AttributeValue { S = "finalize" },
                ["status"] = new AttributeValue { S = "completed" },
                ["startSec"] = new AttributeValue { N = "0" },
                ["endSec"] = new AttributeValue { N = "0" },
                ["intervalSec"] = new AttributeValue { N = "0" },
                ["createdAt"] = new AttributeValue { S = createdAt }
            },
            new()
            {
                ["chunkId"] = new AttributeValue { S = "chunk-0" },
                ["status"] = new AttributeValue { S = "pending" },
                ["startSec"] = new AttributeValue { N = "0" },
                ["endSec"] = new AttributeValue { N = "10" },
                ["intervalSec"] = new AttributeValue { N = "1" },
                ["createdAt"] = new AttributeValue { S = createdAt }
            }
        };
        _dynamoMock.Setup(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QueryResponse { Items = items, LastEvaluatedKey = null });

        var result = await _repository.GetChunksAsync("vid-123");

        result.Should().HaveCount(1);
        result[0].ChunkId.Should().Be("chunk-0");
    }

    [Fact]
    public async Task GetChunksAsync_WhenEmptyChunkId_ShouldSkip()
    {
        var createdAt = DateTime.UtcNow.ToString("O");
        var items = new List<Dictionary<string, AttributeValue>>
        {
            new()
            {
                ["chunkId"] = new AttributeValue { S = "" },
                ["status"] = new AttributeValue { S = "pending" },
                ["startSec"] = new AttributeValue { N = "0" },
                ["endSec"] = new AttributeValue { N = "0" },
                ["intervalSec"] = new AttributeValue { N = "0" },
                ["createdAt"] = new AttributeValue { S = createdAt }
            }
        };
        _dynamoMock.Setup(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QueryResponse { Items = items, LastEvaluatedKey = null });

        var result = await _repository.GetChunksAsync("vid-123");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetChunksAsync_WhenPaginated_ShouldReturnAllChunks()
    {
        var createdAt = DateTime.UtcNow.ToString("O");
        var page1 = new List<Dictionary<string, AttributeValue>>
        {
            new()
            {
                ["chunkId"] = new AttributeValue { S = "chunk-0" },
                ["status"] = new AttributeValue { S = "completed" },
                ["startSec"] = new AttributeValue { N = "0" },
                ["endSec"] = new AttributeValue { N = "5" },
                ["intervalSec"] = new AttributeValue { N = "1" },
                ["createdAt"] = new AttributeValue { S = createdAt }
            }
        };
        var page2 = new List<Dictionary<string, AttributeValue>>
        {
            new()
            {
                ["chunkId"] = new AttributeValue { S = "chunk-1" },
                ["status"] = new AttributeValue { S = "pending" },
                ["startSec"] = new AttributeValue { N = "5" },
                ["endSec"] = new AttributeValue { N = "10" },
                ["intervalSec"] = new AttributeValue { N = "1" },
                ["createdAt"] = new AttributeValue { S = createdAt }
            }
        };
        var lastKey = new Dictionary<string, AttributeValue> { ["pk"] = new AttributeValue { S = "VIDEO#vid" }, ["sk"] = new AttributeValue { S = "CHUNK#chunk-0" } };
        _dynamoMock.SetupSequence(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QueryResponse { Items = page1, LastEvaluatedKey = lastKey })
            .ReturnsAsync(new QueryResponse { Items = page2, LastEvaluatedKey = null });

        var result = await _repository.GetChunksAsync("vid-123");

        result.Should().HaveCount(2);
        result[0].ChunkId.Should().Be("chunk-0");
        result[1].ChunkId.Should().Be("chunk-1");
    }

    [Fact]
    public async Task GetChunksAsync_WhenOptionalFieldsMissing_ShouldUseDefaults()
    {
        var createdAt = DateTime.UtcNow.ToString("O");
        var items = new List<Dictionary<string, AttributeValue>>
        {
            new()
            {
                ["chunkId"] = new AttributeValue { S = "chunk-0" },
                ["status"] = new AttributeValue { S = "processing" },
                ["createdAt"] = new AttributeValue { S = createdAt }
            }
        };
        _dynamoMock.Setup(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QueryResponse { Items = items, LastEvaluatedKey = null });

        var result = await _repository.GetChunksAsync("vid-123");

        result.Should().HaveCount(1);
        result[0].StartSec.Should().Be(0);
        result[0].EndSec.Should().Be(0);
        result[0].IntervalSec.Should().Be(0);
        result[0].ManifestPrefix.Should().BeNull();
        result[0].FramesPrefix.Should().BeNull();
        result[0].ProcessedAt.Should().BeNull();
    }
}
