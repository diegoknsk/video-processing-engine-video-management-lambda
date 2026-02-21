using System.Collections.Generic;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Options;
using VideoProcessing.VideoManagement.Application.Ports;
using VideoProcessing.VideoManagement.Domain.Entities;
using VideoProcessing.VideoManagement.Domain.Exceptions;
using VideoProcessing.VideoManagement.Infra.CrossCutting.Configuration;
using VideoProcessing.VideoManagement.Infra.Data.Mappers;

namespace VideoProcessing.VideoManagement.Infra.Data.Repositories;

public class VideoRepository(IAmazonDynamoDB dynamoDb, IOptions<DynamoDbOptions> options) : IVideoRepository
{
    private readonly string _tableName = options.Value.TableName;

    public async Task<Video> CreateAsync(Video video, string? clientRequestId, CancellationToken ct = default)
    {
        var entity = VideoMapper.ToEntity(video, clientRequestId);
        var item = EntityToAttributeMap(entity);

        var request = new PutItemRequest
        {
            TableName = _tableName,
            Item = item,
            ConditionExpression = "attribute_not_exists(pk)"
        };

        try
        {
            await dynamoDb.PutItemAsync(request, ct);
        }
        catch (ConditionalCheckFailedException ex)
        {
            throw new InvalidOperationException("Video already exists for this pk/sk.", ex);
        }

        return video;
    }

    public async Task<Video?> GetByIdAsync(string userId, string videoId, CancellationToken ct = default)
    {
        var request = new GetItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                ["pk"] = new AttributeValue { S = $"USER#{userId}" },
                ["sk"] = new AttributeValue { S = $"VIDEO#{videoId}" }
            }
        };

        var response = await dynamoDb.GetItemAsync(request, ct);
        if (response.Item.Count == 0) return null;

        return VideoMapper.ToDomain(AttributeMapToEntity(response.Item));
    }

    public async Task<(IReadOnlyList<Video> Items, string? NextToken)> GetByUserIdAsync(string userId, int limit = 50, string? paginationToken = null, CancellationToken ct = default)
    {
        var request = new QueryRequest
        {
            TableName = _tableName,
            KeyConditionExpression = "pk = :pk",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":pk"] = new AttributeValue { S = $"USER#{userId}" }
            },
            Limit = limit,
            ScanIndexForward = false
        };

        if (!string.IsNullOrEmpty(paginationToken))
        {
            try
            {
                request.ExclusiveStartKey = JsonSerializer.Deserialize<Dictionary<string, AttributeValue>>(
                    System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(paginationToken)));
            }
            catch
            {
                // Token inválido, ignora
            }
        }

        var response = await dynamoDb.QueryAsync(request, ct);
        if (response == null) return ([], null);

        var items = (response.Items ?? []).Select(AttributeMapToEntity).Select(VideoMapper.ToDomain).ToList();
        var nextToken = response.LastEvaluatedKey != null && response.LastEvaluatedKey.Count > 0
            ? Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response.LastEvaluatedKey)))
            : null;

        return (items, nextToken);
    }

    public async Task<Video> UpdateAsync(Video video, CancellationToken ct = default)
    {
        var entity = VideoMapper.ToEntity(video);
        var pk = $"USER#{video.UserId}";
        var sk = $"VIDEO#{video.VideoId}";

        // Condições: ownership (pk), progressPercent monotônico, status transitions (não voltar de final para Processing)
        var newStatus = video.Status.ToString();
        var condition = "attribute_exists(pk) AND pk = :pk AND (attribute_not_exists(progressPercent) OR progressPercent <= :newProgress) " +
            "AND (attribute_not_exists(#status) OR NOT (#status IN (:completed, :failed, :cancelled)) OR :newStatus IN (:completed, :failed, :cancelled))";
        var attrNames = new Dictionary<string, string> { ["#status"] = "status" };
        var attrValues = new Dictionary<string, AttributeValue>
        {
            [":pk"] = new AttributeValue { S = pk },
            [":newProgress"] = new AttributeValue { N = video.ProgressPercent.ToString() },
            [":newStatus"] = new AttributeValue { S = newStatus },
            [":completed"] = new AttributeValue { S = "Completed" },
            [":failed"] = new AttributeValue { S = "Failed" },
            [":cancelled"] = new AttributeValue { S = "Cancelled" }
        };

        var updateExpr = "SET #status = :status, progressPercent = :progress, updatedAt = :updated";
        attrValues[":status"] = new AttributeValue { S = video.Status.ToString() };
        attrValues[":progress"] = new AttributeValue { N = video.ProgressPercent.ToString() };
        attrValues[":updated"] = new AttributeValue { S = (video.UpdatedAt ?? DateTime.UtcNow).ToString("O") };

        if (!string.IsNullOrEmpty(video.ErrorMessage))
        {
            updateExpr += ", errorMessage = :errMsg";
            attrValues[":errMsg"] = new AttributeValue { S = video.ErrorMessage };
        }
        if (!string.IsNullOrEmpty(video.ErrorCode))
        {
            updateExpr += ", errorCode = :errCode";
            attrValues[":errCode"] = new AttributeValue { S = video.ErrorCode };
        }
        if (!string.IsNullOrEmpty(video.S3BucketZip))
        {
            updateExpr += ", s3BucketZip = :zipBucket, s3KeyZip = :zipKey";
            attrValues[":zipBucket"] = new AttributeValue { S = video.S3BucketZip };
            attrValues[":zipKey"] = new AttributeValue { S = video.S3KeyZip ?? "" };
        }
        if (!string.IsNullOrEmpty(video.FramesPrefix))
        {
            updateExpr += ", framesPrefix = :framesPrefix";
            attrValues[":framesPrefix"] = new AttributeValue { S = video.FramesPrefix };
        }

        var request = new UpdateItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                ["pk"] = new AttributeValue { S = pk },
                ["sk"] = new AttributeValue { S = sk }
            },
            UpdateExpression = updateExpr,
            ConditionExpression = condition,
            ExpressionAttributeNames = attrNames,
            ExpressionAttributeValues = attrValues,
            ReturnValues = ReturnValue.ALL_NEW
        };

        try
        {
            var response = await dynamoDb.UpdateItemAsync(request, ct);
            return VideoMapper.ToDomain(AttributeMapToEntity(response.Attributes));
        }
        catch (ConditionalCheckFailedException ex)
        {
            throw new VideoUpdateConflictException(
                "Update failed: ownership mismatch, progress regression, or invalid status transition.", ex);
        }
    }

    public async Task<Video?> GetByClientRequestIdAsync(string userId, string clientRequestId, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(clientRequestId)) return null;

        var request = new QueryRequest
        {
            TableName = _tableName,
            IndexName = "GSI1",
            KeyConditionExpression = "gsi1pk = :pk AND gsi1sk = :sk",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":pk"] = new AttributeValue { S = $"USER#{userId}" },
                [":sk"] = new AttributeValue { S = $"CLIENT_REQUEST#{clientRequestId}" }
            },
            Limit = 1
        };

        var response = await dynamoDb.QueryAsync(request, ct);
        var item = response.Items.FirstOrDefault();
        return item == null ? null : VideoMapper.ToDomain(AttributeMapToEntity(item));
    }

    private static Dictionary<string, AttributeValue> EntityToAttributeMap(VideoEntity entity)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            ["pk"] = new AttributeValue { S = entity.Pk },
            ["sk"] = new AttributeValue { S = entity.Sk },
            ["videoId"] = new AttributeValue { S = entity.VideoId },
            ["userId"] = new AttributeValue { S = entity.UserId },
            ["originalFileName"] = new AttributeValue { S = entity.OriginalFileName },
            ["contentType"] = new AttributeValue { S = entity.ContentType },
            ["sizeBytes"] = new AttributeValue { N = entity.SizeBytes.ToString() },
            ["status"] = new AttributeValue { S = entity.Status },
            ["processingMode"] = new AttributeValue { S = entity.ProcessingMode },
            ["progressPercent"] = new AttributeValue { N = entity.ProgressPercent.ToString() },
            ["createdAt"] = new AttributeValue { S = entity.CreatedAt }
        };

        if (entity.DurationSec.HasValue) item["durationSec"] = new AttributeValue { N = entity.DurationSec.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) };
        if (!string.IsNullOrEmpty(entity.S3BucketVideo)) item["s3BucketVideo"] = new AttributeValue { S = entity.S3BucketVideo };
        if (!string.IsNullOrEmpty(entity.S3KeyVideo)) item["s3KeyVideo"] = new AttributeValue { S = entity.S3KeyVideo };
        if (!string.IsNullOrEmpty(entity.S3BucketZip)) item["s3BucketZip"] = new AttributeValue { S = entity.S3BucketZip };
        if (!string.IsNullOrEmpty(entity.S3KeyZip)) item["s3KeyZip"] = new AttributeValue { S = entity.S3KeyZip };
        if (!string.IsNullOrEmpty(entity.S3BucketFrames)) item["s3BucketFrames"] = new AttributeValue { S = entity.S3BucketFrames };
        if (!string.IsNullOrEmpty(entity.FramesPrefix)) item["framesPrefix"] = new AttributeValue { S = entity.FramesPrefix };
        if (!string.IsNullOrEmpty(entity.StepExecutionArn)) item["stepExecutionArn"] = new AttributeValue { S = entity.StepExecutionArn };
        if (!string.IsNullOrEmpty(entity.ErrorMessage)) item["errorMessage"] = new AttributeValue { S = entity.ErrorMessage };
        if (!string.IsNullOrEmpty(entity.ErrorCode)) item["errorCode"] = new AttributeValue { S = entity.ErrorCode };
        if (!string.IsNullOrEmpty(entity.ClientRequestId)) item["clientRequestId"] = new AttributeValue { S = entity.ClientRequestId };
        if (entity.ChunkCount.HasValue) item["chunkCount"] = new AttributeValue { N = entity.ChunkCount.Value.ToString() };
        if (entity.ChunkDurationSec.HasValue) item["chunkDurationSec"] = new AttributeValue { N = entity.ChunkDurationSec.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) };
        if (!string.IsNullOrEmpty(entity.UploadIssuedAt)) item["uploadIssuedAt"] = new AttributeValue { S = entity.UploadIssuedAt };
        if (!string.IsNullOrEmpty(entity.UploadUrlExpiresAt)) item["uploadUrlExpiresAt"] = new AttributeValue { S = entity.UploadUrlExpiresAt };
        if (entity.FramesProcessed.HasValue) item["framesProcessed"] = new AttributeValue { N = entity.FramesProcessed.Value.ToString() };
        if (!string.IsNullOrEmpty(entity.FinalizedAt)) item["finalizedAt"] = new AttributeValue { S = entity.FinalizedAt };
        if (!string.IsNullOrEmpty(entity.UpdatedAt)) item["updatedAt"] = new AttributeValue { S = entity.UpdatedAt };
        if (entity.Version.HasValue) item["version"] = new AttributeValue { N = entity.Version.Value.ToString() };
        if (!string.IsNullOrEmpty(entity.Gsi1Pk)) item["gsi1pk"] = new AttributeValue { S = entity.Gsi1Pk };
        if (!string.IsNullOrEmpty(entity.Gsi1Sk)) item["gsi1sk"] = new AttributeValue { S = entity.Gsi1Sk };

        return item;
    }

    private static VideoEntity AttributeMapToEntity(Dictionary<string, AttributeValue> item)
    {
        string GetS(string key) => item.TryGetValue(key, out var v) ? v.S ?? "" : "";
        long GetN(string key) => item.TryGetValue(key, out var v) && v.N != null && long.TryParse(v.N, out var n) ? n : 0;
        int GetInt(string key) => item.TryGetValue(key, out var v) && v.N != null && int.TryParse(v.N, out var n) ? n : 0;
        double GetD(string key) => item.TryGetValue(key, out var v) && v.N != null && double.TryParse(v.N, System.Globalization.NumberStyles.Any, null, out var d) ? d : 0;

        return new VideoEntity
        {
            Pk = GetS("pk"),
            Sk = GetS("sk"),
            VideoId = GetS("videoId"),
            UserId = GetS("userId"),
            OriginalFileName = GetS("originalFileName"),
            ContentType = GetS("contentType"),
            SizeBytes = GetN("sizeBytes"),
            DurationSec = item.TryGetValue("durationSec", out var dur) ? GetD("durationSec") : null,
            Status = GetS("status"),
            ProcessingMode = string.IsNullOrEmpty(GetS("processingMode")) ? "SingleLambda" : GetS("processingMode"),
            ProgressPercent = GetInt("progressPercent"),
            S3BucketVideo = string.IsNullOrEmpty(GetS("s3BucketVideo")) ? null : GetS("s3BucketVideo"),
            S3KeyVideo = string.IsNullOrEmpty(GetS("s3KeyVideo")) ? null : GetS("s3KeyVideo"),
            S3BucketZip = string.IsNullOrEmpty(GetS("s3BucketZip")) ? null : GetS("s3BucketZip"),
            S3KeyZip = string.IsNullOrEmpty(GetS("s3KeyZip")) ? null : GetS("s3KeyZip"),
            S3BucketFrames = string.IsNullOrEmpty(GetS("s3BucketFrames")) ? null : GetS("s3BucketFrames"),
            FramesPrefix = string.IsNullOrEmpty(GetS("framesPrefix")) ? null : GetS("framesPrefix"),
            StepExecutionArn = string.IsNullOrEmpty(GetS("stepExecutionArn")) ? null : GetS("stepExecutionArn"),
            ErrorMessage = string.IsNullOrEmpty(GetS("errorMessage")) ? null : GetS("errorMessage"),
            ErrorCode = string.IsNullOrEmpty(GetS("errorCode")) ? null : GetS("errorCode"),
            ClientRequestId = string.IsNullOrEmpty(GetS("clientRequestId")) ? null : GetS("clientRequestId"),
            ChunkCount = item.ContainsKey("chunkCount") ? GetInt("chunkCount") : null,
            ChunkDurationSec = item.ContainsKey("chunkDurationSec") ? GetD("chunkDurationSec") : null,
            UploadIssuedAt = string.IsNullOrEmpty(GetS("uploadIssuedAt")) ? null : GetS("uploadIssuedAt"),
            UploadUrlExpiresAt = string.IsNullOrEmpty(GetS("uploadUrlExpiresAt")) ? null : GetS("uploadUrlExpiresAt"),
            FramesProcessed = item.ContainsKey("framesProcessed") ? GetInt("framesProcessed") : null,
            FinalizedAt = string.IsNullOrEmpty(GetS("finalizedAt")) ? null : GetS("finalizedAt"),
            CreatedAt = GetS("createdAt"),
            UpdatedAt = string.IsNullOrEmpty(GetS("updatedAt")) ? null : GetS("updatedAt"),
            Version = item.ContainsKey("version") ? GetInt("version") : null
        };
    }
}
