namespace VideoProcessing.VideoManagement.Infra.Data.Repositories;

/// <summary>
/// DTO para DynamoDB single-table. pk = USER#{userId}, sk = VIDEO#{videoId}.
/// GSI_ClientRequestId: gsi1pk = USER#{userId}, gsi1sk = CLIENT_REQUEST#{clientRequestId}.
/// </summary>
public class VideoEntity
{
    public string Pk { get; set; } = string.Empty; // USER#{userId}
    public string Sk { get; set; } = string.Empty;  // VIDEO#{videoId}

    public string VideoId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public double? DurationSec { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ProcessingMode { get; set; } = string.Empty;
    public int ProgressPercent { get; set; }

    public string? S3BucketVideo { get; set; }
    public string? S3KeyVideo { get; set; }
    public string? S3BucketZip { get; set; }
    public string? S3KeyZip { get; set; }
    public string? S3BucketFrames { get; set; }
    public string? FramesPrefix { get; set; }

    public string? StepExecutionArn { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
    public string? ClientRequestId { get; set; }

    public int? ChunkCount { get; set; }
    public double? ChunkDurationSec { get; set; }
    public string? UploadIssuedAt { get; set; }  // ISO 8601
    public string? UploadUrlExpiresAt { get; set; }
    public int? FramesProcessed { get; set; }
    public string? FinalizedAt { get; set; }  // ISO 8601

    public string CreatedAt { get; set; } = string.Empty;  // ISO 8601
    public string? UpdatedAt { get; set; }
    public int? Version { get; set; }

    // GSI_ClientRequestId: query por userId + clientRequestId
    public string? Gsi1Pk { get; set; }  // USER#{userId}
    public string? Gsi1Sk { get; set; }  // CLIENT_REQUEST#{clientRequestId}
}
