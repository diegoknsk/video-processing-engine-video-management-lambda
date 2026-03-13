using System.Globalization;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Options;
using VideoProcessing.VideoManagement.Application.Models;
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

    public async Task<ChunkStatusSummary> GetStatusSummaryAsync(string videoId, CancellationToken ct = default)
    {
        int total = 0, completed = 0, processing = 0, failed = 0, pending = 0;
        string? finalizeStatus = null;

        var request = new QueryRequest
        {
            TableName = _tableName,
            KeyConditionExpression = "pk = :pk",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":pk"] = new AttributeValue { S = $"VIDEO#{videoId}" }
            },
            ProjectionExpression = "chunkId, #st",
            ExpressionAttributeNames = new Dictionary<string, string> { ["#st"] = "status" }
        };

        Dictionary<string, AttributeValue>? lastKey = null;
        do
        {
            if (lastKey is not null)
                request.ExclusiveStartKey = lastKey;

            var response = await dynamoDb.QueryAsync(request, ct);
            foreach (var item in response.Items)
            {
                var chunkId = item.TryGetValue("chunkId", out var c) ? c.S : null;
                var status = item.TryGetValue("status", out var s) ? s.S : null;

                if (string.IsNullOrEmpty(chunkId))
                    continue;

                if (chunkId == VideoChunkConstants.FinalizeChunkId)
                {
                    finalizeStatus = status;
                    continue;
                }

                total++;
                switch (status?.ToLowerInvariant())
                {
                    case "completed": completed++; break;
                    case "processing": processing++; break;
                    case "failed": failed++; break;
                    case "pending":
                    default: pending++; break;
                }
            }

            lastKey = response.LastEvaluatedKey?.Count > 0 ? response.LastEvaluatedKey : null;
        } while (lastKey is not null);

        return new ChunkStatusSummary(total, completed, processing, failed, pending, finalizeStatus);
    }

    public async Task<IReadOnlyList<VideoChunk>> GetChunksAsync(string videoId, CancellationToken ct = default)
    {
        var list = new List<VideoChunk>();
        var request = new QueryRequest
        {
            TableName = _tableName,
            KeyConditionExpression = "pk = :pk",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":pk"] = new AttributeValue { S = $"VIDEO#{videoId}" }
            },
            ProjectionExpression = "chunkId, #st, startSec, endSec, intervalSec, manifestPrefix, framesPrefix, processedAt, createdAt",
            ExpressionAttributeNames = new Dictionary<string, string> { ["#st"] = "status" }
        };

        Dictionary<string, AttributeValue>? lastKey = null;
        do
        {
            if (lastKey is not null)
                request.ExclusiveStartKey = lastKey;

            var response = await dynamoDb.QueryAsync(request, ct);
            foreach (var item in response.Items)
            {
                var chunkId = item.TryGetValue("chunkId", out var c) ? c.S : null;
                if (string.IsNullOrEmpty(chunkId) || chunkId == VideoChunkConstants.FinalizeChunkId)
                    continue;

                var status = item.TryGetValue("status", out var s) ? s.S : "pending";
                var startSec = item.TryGetValue("startSec", out var ss) && double.TryParse(ss.N, NumberStyles.Float, CultureInfo.InvariantCulture, out var start) ? start : 0;
                var endSec = item.TryGetValue("endSec", out var es) && double.TryParse(es.N, NumberStyles.Float, CultureInfo.InvariantCulture, out var end) ? end : 0;
                var intervalSec = item.TryGetValue("intervalSec", out var isec) && double.TryParse(isec.N, NumberStyles.Float, CultureInfo.InvariantCulture, out var iv) ? iv : 0;
                var manifestPrefix = item.TryGetValue("manifestPrefix", out var mp) ? mp.S : null;
                var framesPrefix = item.TryGetValue("framesPrefix", out var fp) ? fp.S : null;
                DateTime? processedAt = null;
                if (item.TryGetValue("processedAt", out var pa) && !string.IsNullOrEmpty(pa.S) && DateTime.TryParse(pa.S, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var pat))
                    processedAt = pat;
                var createdAt = item.TryGetValue("createdAt", out var ca) && DateTime.TryParse(ca.S, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var created)
                    ? created
                    : DateTime.UtcNow;

                list.Add(new VideoChunk(chunkId, videoId, status, startSec, endSec, intervalSec, manifestPrefix, framesPrefix, processedAt, createdAt));
            }

            lastKey = response.LastEvaluatedKey?.Count > 0 ? response.LastEvaluatedKey : null;
        } while (lastKey is not null);

        return list;
    }
}
