using System.Collections.Generic;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using VideoProcessing.VideoManagement.Domain.Entities;
using VideoProcessing.VideoManagement.Domain.Exceptions;
using VideoProcessing.VideoManagement.Infra.CrossCutting.Configuration;
using VideoProcessing.VideoManagement.Infra.Data.Repositories;
using Xunit;

namespace VideoProcessing.VideoManagement.UnitTests.Infra.Data.Repositories;

public class VideoRepositoryTests
{
    private readonly Mock<IAmazonDynamoDB> _dynamoMock;
    private readonly VideoRepository _repository;

    public VideoRepositoryTests()
    {
        _dynamoMock = new Mock<IAmazonDynamoDB>();
        var options = Options.Create(new DynamoDbOptions("test-table", "us-east-1"));
        _repository = new VideoRepository(_dynamoMock.Object, options);
    }

    [Fact]
    public async Task CreateAsync_ShouldCallPutItem()
    {
        var userId = Guid.NewGuid();
        var video = new Video(userId, "test.mp4", "video/mp4", 1024);

        _dynamoMock.Setup(x => x.PutItemAsync(It.IsAny<PutItemRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PutItemResponse());

        var result = await _repository.CreateAsync(video, null);

        result.Should().Be(video);
        _dynamoMock.Verify(x => x.PutItemAsync(
            It.Is<PutItemRequest>(r => r.TableName == "test-table" && r.Item["pk"].S == $"USER#{userId}" && r.ConditionExpression == "attribute_not_exists(pk)"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithClientRequestId_ShouldStoreInEntity()
    {
        var userId = Guid.NewGuid();
        var video = new Video(userId, "test.mp4", "video/mp4", 1024);

        _dynamoMock.Setup(x => x.PutItemAsync(It.IsAny<PutItemRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PutItemResponse());

        await _repository.CreateAsync(video, "req-123");

        _dynamoMock.Verify(x => x.PutItemAsync(
            It.Is<PutItemRequest>(r => r.Item["clientRequestId"].S == "req-123"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ShouldReturnVideo()
    {
        var userId = Guid.NewGuid();
        var videoId = Guid.NewGuid();
        var item = new Dictionary<string, AttributeValue>
        {
            ["pk"] = new AttributeValue { S = $"USER#{userId}" },
            ["sk"] = new AttributeValue { S = $"VIDEO#{videoId}" },
            ["videoId"] = new AttributeValue { S = videoId.ToString() },
            ["userId"] = new AttributeValue { S = userId.ToString() },
            ["originalFileName"] = new AttributeValue { S = "test.mp4" },
            ["contentType"] = new AttributeValue { S = "video/mp4" },
            ["sizeBytes"] = new AttributeValue { N = "1024" },
            ["status"] = new AttributeValue { S = "Pending" },
            ["processingMode"] = new AttributeValue { S = "SingleLambda" },
            ["progressPercent"] = new AttributeValue { N = "0" },
            ["createdAt"] = new AttributeValue { S = DateTime.UtcNow.ToString("O") }
        };

        _dynamoMock.Setup(x => x.GetItemAsync(It.IsAny<GetItemRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetItemResponse { Item = item });

        var result = await _repository.GetByIdAsync(userId.ToString(), videoId.ToString());

        result.Should().NotBeNull();
        result!.VideoId.Should().Be(videoId);
        result.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotExists_ShouldReturnNull()
    {
        _dynamoMock.Setup(x => x.GetItemAsync(It.IsAny<GetItemRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetItemResponse { Item = new Dictionary<string, AttributeValue>() });

        var result = await _repository.GetByIdAsync(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnItemsAndNextToken()
    {
        var userId = Guid.NewGuid();
        var videoId = Guid.NewGuid();
        var item = new Dictionary<string, AttributeValue>
        {
            ["pk"] = new AttributeValue { S = $"USER#{userId}" },
            ["sk"] = new AttributeValue { S = $"VIDEO#{videoId}" },
            ["videoId"] = new AttributeValue { S = videoId.ToString() },
            ["userId"] = new AttributeValue { S = userId.ToString() },
            ["originalFileName"] = new AttributeValue { S = "test.mp4" },
            ["contentType"] = new AttributeValue { S = "video/mp4" },
            ["sizeBytes"] = new AttributeValue { N = "1024" },
            ["status"] = new AttributeValue { S = "Pending" },
            ["processingMode"] = new AttributeValue { S = "SingleLambda" },
            ["progressPercent"] = new AttributeValue { N = "0" },
            ["createdAt"] = new AttributeValue { S = DateTime.UtcNow.ToString("O") }
        };

        _dynamoMock.Setup(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QueryResponse
            {
                Items = [item],
                LastEvaluatedKey = new Dictionary<string, AttributeValue> { ["pk"] = new AttributeValue { S = "x" } }
            });

        var (items, nextToken) = await _repository.GetByUserIdAsync(userId.ToString(), 10);

        items.Should().HaveCount(1);
        nextToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetByUserIdAsync_WhenEmpty_ShouldReturnEmptyListAndNullToken()
    {
        var userId = Guid.NewGuid();
        _dynamoMock.Setup(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QueryResponse { Items = [] });

        var (items, nextToken) = await _repository.GetByUserIdAsync(userId.ToString(), 10);

        items.Should().BeEmpty();
        nextToken.Should().BeNull();
    }

    [Fact]
    public async Task GetByClientRequestIdAsync_WhenExists_ShouldReturnVideo()
    {
        var userId = Guid.NewGuid();
        var videoId = Guid.NewGuid();
        var item = new Dictionary<string, AttributeValue>
        {
            ["pk"] = new AttributeValue { S = $"USER#{userId}" },
            ["sk"] = new AttributeValue { S = $"VIDEO#{videoId}" },
            ["videoId"] = new AttributeValue { S = videoId.ToString() },
            ["userId"] = new AttributeValue { S = userId.ToString() },
            ["originalFileName"] = new AttributeValue { S = "test.mp4" },
            ["contentType"] = new AttributeValue { S = "video/mp4" },
            ["sizeBytes"] = new AttributeValue { N = "1024" },
            ["status"] = new AttributeValue { S = "Pending" },
            ["processingMode"] = new AttributeValue { S = "SingleLambda" },
            ["progressPercent"] = new AttributeValue { N = "0" },
            ["createdAt"] = new AttributeValue { S = DateTime.UtcNow.ToString("O") }
        };

        _dynamoMock.Setup(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QueryResponse { Items = [item] });

        var result = await _repository.GetByClientRequestIdAsync(userId.ToString(), "req-123");

        result.Should().NotBeNull();
        result!.VideoId.Should().Be(videoId);
    }

    [Fact]
    public async Task GetByClientRequestIdAsync_WhenNotExists_ShouldReturnNull()
    {
        _dynamoMock.Setup(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QueryResponse { Items = [] });

        var result = await _repository.GetByClientRequestIdAsync(Guid.NewGuid().ToString(), "req-123");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByClientRequestIdAsync_WhenClientRequestIdEmpty_ShouldReturnNull()
    {
        var result = await _repository.GetByClientRequestIdAsync(Guid.NewGuid().ToString(), "");

        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_WhenSuccess_ShouldReturnUpdatedVideo()
    {
        var userId = Guid.NewGuid();
        var video = new Video(userId, "test.mp4", "video/mp4", 1024);
        video.SetProgress(50);

        var attributes = new Dictionary<string, AttributeValue>
        {
            ["pk"] = new AttributeValue { S = $"USER#{userId}" },
            ["sk"] = new AttributeValue { S = $"VIDEO#{video.VideoId}" },
            ["videoId"] = new AttributeValue { S = video.VideoId.ToString() },
            ["userId"] = new AttributeValue { S = userId.ToString() },
            ["originalFileName"] = new AttributeValue { S = "test.mp4" },
            ["contentType"] = new AttributeValue { S = "video/mp4" },
            ["sizeBytes"] = new AttributeValue { N = "1024" },
            ["status"] = new AttributeValue { S = "Processing" },
            ["processingMode"] = new AttributeValue { S = "SingleLambda" },
            ["progressPercent"] = new AttributeValue { N = "50" },
            ["createdAt"] = new AttributeValue { S = DateTime.UtcNow.ToString("O") },
            ["updatedAt"] = new AttributeValue { S = DateTime.UtcNow.ToString("O") }
        };

        _dynamoMock.Setup(x => x.UpdateItemAsync(It.IsAny<UpdateItemRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UpdateItemResponse { Attributes = attributes });

        var result = await _repository.UpdateAsync(video);

        result.Should().NotBeNull();
        result.ProgressPercent.Should().Be(50);
    }

    [Fact]
    public async Task UpdateAsync_WhenConditionalCheckFails_ShouldThrowVideoUpdateConflictException()
    {
        var userId = Guid.NewGuid();
        var video = new Video(userId, "test.mp4", "video/mp4", 1024);

        _dynamoMock.Setup(x => x.UpdateItemAsync(It.IsAny<UpdateItemRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConditionalCheckFailedException("Condition failed"));

        var act = () => _repository.UpdateAsync(video);

        await act.Should().ThrowAsync<VideoUpdateConflictException>();
    }
}
