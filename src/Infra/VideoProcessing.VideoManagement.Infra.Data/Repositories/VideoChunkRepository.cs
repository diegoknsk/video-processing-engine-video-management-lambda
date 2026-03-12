using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Options;
using VideoProcessing.VideoManagement.Application.Ports;
using VideoProcessing.VideoManagement.Domain.Entities;
using VideoProcessing.VideoManagement.Infra.CrossCutting.Configuration;

namespace VideoProcessing.VideoManagement.Infra.Data.Repositories;

/// <summary>
/// Implementação DynamoDB do repositório de chunks. pk = VIDEO#{videoId}, sk = CHUNK#{chunkId}.
/// </summary>
public class VideoChunkRepository(IAmazonDynamoDB dynamoDb, IOptions<DynamoDbOptions> options) : IVideoChunkRepository
{
    private const string StatusCompleted = "completed";
    private readonly string _tableName = options.Value.ChunksTableName;

    public async Task UpsertAsync(VideoChunk chunk, CancellationToken ct = default)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            ["pk"] = new AttributeValue { S = $"VIDEO#{chunk.VideoId}" },
            ["sk"] = new AttributeValue { S = $"CHUNK#{chunk.ChunkId}" },
            ["chunkId"] = new AttributeValue { S = chunk.ChunkId },
            ["videoId"] = new AttributeValue { S = chunk.VideoId },
            ["status"] = new AttributeValue { S = chunk.Status },
            ["startSec"] = new AttributeValue { N = chunk.StartSec.ToString(System.Globalization.CultureInfo.InvariantCulture) },
            ["endSec"] = new AttributeValue { N = chunk.EndSec.ToString(System.Globalization.CultureInfo.InvariantCulture) },
            ["intervalSec"] = new AttributeValue { N = chunk.IntervalSec.ToString(System.Globalization.CultureInfo.InvariantCulture) },
            ["createdAt"] = new AttributeValue { S = chunk.CreatedAt.ToString("O") }
        };
        if (!string.IsNullOrEmpty(chunk.ManifestPrefix))
            item["manifestPrefix"] = new AttributeValue { S = chunk.ManifestPrefix };
        if (!string.IsNullOrEmpty(chunk.FramesPrefix))
            item["framesPrefix"] = new AttributeValue { S = chunk.FramesPrefix };
        if (chunk.ProcessedAt.HasValue)
            item["processedAt"] = new AttributeValue { S = chunk.ProcessedAt.Value.ToString("O") };

        await dynamoDb.PutItemAsync(new PutItemRequest
        {
            TableName = _tableName,
            Item = item
        }, ct);
    }

    public async Task<int> CountProcessedAsync(string videoId, CancellationToken ct = default)
    {
        var request = new QueryRequest
        {
            TableName = _tableName,
            KeyConditionExpression = "pk = :pk",
            FilterExpression = "#st = :completed",
            ExpressionAttributeNames = new Dictionary<string, string> { ["#st"] = "status" },
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":pk"] = new AttributeValue { S = $"VIDEO#{videoId}" },
                [":completed"] = new AttributeValue { S = StatusCompleted }
            },
            Select = Select.COUNT
        };

        var response = await dynamoDb.QueryAsync(request, ct);
        return response?.Count ?? 0;
    }
}
