using VideoProcessing.VideoManagement.Domain.Enums;

namespace VideoProcessing.VideoManagement.Domain.Entities;

public class Video
{
    public Guid VideoId { get; private set; }
    public Guid UserId { get; private set; }
    public string OriginalFileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long SizeBytes { get; private set; }
    public double? DurationSec { get; private set; }
    public VideoStatus Status { get; private set; }
    public ProcessingMode ProcessingMode { get; private set; }
    public int ProgressPercent { get; private set; }
    
    // S3 Details
    public string? S3BucketVideo { get; private set; }
    public string? S3KeyVideo { get; private set; }
    public string? S3BucketZip { get; private set; }
    public string? S3KeyZip { get; private set; }
    public string? S3BucketFrames { get; private set; }
    public string? FramesPrefix { get; private set; }
    
    // Execution Details
    public string? StepExecutionArn { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? ErrorCode { get; private set; }
    public string? ClientRequestId { get; private set; }
    
    // Fan-out/Fan-in
    public int? ChunkCount { get; private set; }
    public double? ChunkDurationSec { get; private set; }
    public DateTime? UploadIssuedAt { get; private set; }
    public DateTime? UploadUrlExpiresAt { get; private set; }
    public int? FramesProcessed { get; private set; }
    public DateTime? FinalizedAt { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public int? Version { get; private set; }

    private Video() { }

    internal Video(VideoRehydrationData d)
    {
        VideoId = d.VideoId;
        UserId = d.UserId;
        OriginalFileName = d.OriginalFileName;
        ContentType = d.ContentType;
        SizeBytes = d.SizeBytes;
        DurationSec = d.DurationSec;
        Status = d.Status;
        ProcessingMode = d.ProcessingMode;
        ProgressPercent = d.ProgressPercent;
        S3BucketVideo = d.S3BucketVideo;
        S3KeyVideo = d.S3KeyVideo;
        S3BucketZip = d.S3BucketZip;
        S3KeyZip = d.S3KeyZip;
        S3BucketFrames = d.S3BucketFrames;
        FramesPrefix = d.FramesPrefix;
        StepExecutionArn = d.StepExecutionArn;
        ErrorMessage = d.ErrorMessage;
        ErrorCode = d.ErrorCode;
        ClientRequestId = d.ClientRequestId;
        ChunkCount = d.ChunkCount;
        ChunkDurationSec = d.ChunkDurationSec;
        UploadIssuedAt = d.UploadIssuedAt;
        UploadUrlExpiresAt = d.UploadUrlExpiresAt;
        FramesProcessed = d.FramesProcessed;
        FinalizedAt = d.FinalizedAt;
        CreatedAt = d.CreatedAt;
        UpdatedAt = d.UpdatedAt;
        Version = d.Version;
    }

    internal static Video FromPersistence(VideoRehydrationData d) => new(d);

    public Video(Guid userId, string originalFileName, string contentType, long sizeBytes, string? clientRequestId = null)
    {
        if (string.IsNullOrWhiteSpace(originalFileName))
            throw new ArgumentException("OriginalFileName cannot be empty.", nameof(originalFileName));
        if (string.IsNullOrWhiteSpace(contentType))
            throw new ArgumentException("ContentType cannot be empty.", nameof(contentType));
        if (sizeBytes < 0)
            throw new ArgumentOutOfRangeException(nameof(sizeBytes), "SizeBytes cannot be negative.");

        VideoId = Guid.NewGuid();
        UserId = userId;
        OriginalFileName = originalFileName;
        ContentType = contentType;
        SizeBytes = sizeBytes;
        Status = VideoStatus.Pending;
        ProcessingMode = ProcessingMode.SingleLambda;
        CreatedAt = DateTime.UtcNow;
        ClientRequestId = clientRequestId;
        ProgressPercent = 0;
    }

    public void UpdateStatus(VideoStatus status)
    {
        if (IsFinalState(Status) && !IsFinalState(status))
        {
             // Allow retries or manual interventions? For now, let's strictly forbid reverting from final unless it's a specific flow.
             // Actually, "Completed" -> "Processing" is definitely weird.
             // "Failed" -> "Pending" (Retry) might be valid.
             if (Status == VideoStatus.Completed)
                 throw new InvalidOperationException($"Cannot transition from {Status} to {status}.");
        }
            
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }
    
    private bool IsFinalState(VideoStatus status) => 
        status == VideoStatus.Completed || status == VideoStatus.Failed || status == VideoStatus.Cancelled;

    public void SetProgress(int percent)
    {
        if (percent < 0 || percent > 100)
            throw new ArgumentOutOfRangeException(nameof(percent), "ProgressPercent must be between 0 and 100.");
        if (percent < ProgressPercent)
            throw new InvalidOperationException("Progress cannot regress.");

        ProgressPercent = percent;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetProcessingMode(ProcessingMode mode, int? chunkCount = null)
    {
        ProcessingMode = mode;
        ChunkCount = chunkCount;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void SetS3Source(string bucket, string key)
    {
        S3BucketVideo = bucket;
        S3KeyVideo = key;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetS3Output(string zipBucket, string zipKey, string framesBucket, string framesPrefix)
    {
        S3BucketZip = zipBucket;
        S3KeyZip = zipKey;
        S3BucketFrames = framesBucket;
        FramesPrefix = framesPrefix;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetExecutionArn(string executionArn)
    {
        StepExecutionArn = executionArn;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(string errorMessage, string? errorCode = null)
    {
        Status = VideoStatus.Failed;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsCompleted()
    {
        Status = VideoStatus.Completed;
        ProgressPercent = 100;
        UpdatedAt = DateTime.UtcNow;
    }
}
