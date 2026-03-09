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
    public double? FrameIntervalSec { get; private set; }
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
    public int? MaxParallelChunks { get; private set; }
    public ProcessingSummary? ProcessingSummary { get; private set; }

    // Pipeline timestamps
    public DateTime? ProcessingStartedAt { get; private set; }
    public DateTime? ImagesProcessingCompletedAt { get; private set; }
    public DateTime? ProcessingCompletedAt { get; private set; }
    public DateTime? LastFailedAt { get; private set; }
    public DateTime? LastCancelledAt { get; private set; }

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
        FrameIntervalSec = d.FrameIntervalSec;
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
        MaxParallelChunks = d.MaxParallelChunks;
        ProcessingSummary = d.ProcessingSummary;
        ProcessingStartedAt = d.ProcessingStartedAt;
        ImagesProcessingCompletedAt = d.ImagesProcessingCompletedAt;
        ProcessingCompletedAt = d.ProcessingCompletedAt;
        LastFailedAt = d.LastFailedAt;
        LastCancelledAt = d.LastCancelledAt;
        CreatedAt = d.CreatedAt;
        UpdatedAt = d.UpdatedAt;
        Version = d.Version;
    }

    internal static Video FromPersistence(VideoRehydrationData d) => new(d);

    /// <summary>
    /// Cria uma nova instância de Video com os campos do patch aplicados (apenas valores não nulos sobrescrevem).
    /// UpdatedAt é definido como UtcNow.
    /// </summary>
    public static Video FromMerge(Video existing, VideoUpdateValues patch)
    {
        var updatedAt = DateTime.UtcNow;
        var data = new VideoRehydrationData(
            VideoId: existing.VideoId,
            UserId: existing.UserId,
            OriginalFileName: existing.OriginalFileName,
            ContentType: existing.ContentType,
            SizeBytes: existing.SizeBytes,
            DurationSec: existing.DurationSec,
            FrameIntervalSec: existing.FrameIntervalSec,
            Status: patch.Status ?? existing.Status,
            ProcessingMode: existing.ProcessingMode,
            ProgressPercent: patch.ProgressPercent ?? existing.ProgressPercent,
            S3BucketVideo: existing.S3BucketVideo,
            S3KeyVideo: existing.S3KeyVideo,
            S3BucketZip: patch.S3BucketZip ?? existing.S3BucketZip,
            S3KeyZip: patch.S3KeyZip ?? existing.S3KeyZip,
            S3BucketFrames: patch.S3BucketFrames ?? existing.S3BucketFrames,
            FramesPrefix: patch.FramesPrefix ?? existing.FramesPrefix,
            StepExecutionArn: patch.StepExecutionArn ?? existing.StepExecutionArn,
            ErrorMessage: patch.ErrorMessage ?? existing.ErrorMessage,
            ErrorCode: patch.ErrorCode ?? existing.ErrorCode,
            ClientRequestId: existing.ClientRequestId,
            ChunkCount: existing.ChunkCount,
            ChunkDurationSec: existing.ChunkDurationSec,
            UploadIssuedAt: existing.UploadIssuedAt,
            UploadUrlExpiresAt: existing.UploadUrlExpiresAt,
            FramesProcessed: existing.FramesProcessed,
            FinalizedAt: existing.FinalizedAt,
            MaxParallelChunks: patch.MaxParallelChunks ?? existing.MaxParallelChunks,
            ProcessingSummary: ProcessingSummary.Merge(existing.ProcessingSummary, patch.ProcessingSummary),
            ProcessingStartedAt: patch.ProcessingStartedAt ?? existing.ProcessingStartedAt,
            ImagesProcessingCompletedAt: existing.ImagesProcessingCompletedAt,
            ProcessingCompletedAt: existing.ProcessingCompletedAt,
            LastFailedAt: existing.LastFailedAt,
            LastCancelledAt: existing.LastCancelledAt,
            CreatedAt: existing.CreatedAt,
            UpdatedAt: updatedAt,
            Version: existing.Version);
        return FromPersistence(data);
    }

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
        Status = VideoStatus.UploadPending;
        ProcessingMode = ProcessingMode.SingleLambda;
        CreatedAt = DateTime.UtcNow;
        ClientRequestId = clientRequestId;
        ProgressPercent = 0;
    }

    public void UpdateStatus(VideoStatus status)
    {
        var previousStatus = Status;
        if (IsFinalState(previousStatus) && !IsFinalState(status))
        {
            if (previousStatus == VideoStatus.Completed)
                throw new InvalidOperationException($"Cannot transition from {previousStatus} to {status}.");
        }

        Status = status;
        UpdatedAt = DateTime.UtcNow;
        ApplyTransitionTimestamps(previousStatus, status);
    }

    /// <summary>
    /// Aplica timestamps do pipeline conforme a transição de status (chamado por UpdateStatus ou pelo UseCase após merge).
    /// Regras: ProcessingStartedAt na primeira entrada em ProcessingImages (não sobrescreve); ImagesProcessingCompletedAt
    /// na primeira entrada em GeneratingZip (não sobrescreve); ProcessingCompletedAt na primeira entrada em Completed
    /// (não sobrescreve); LastFailedAt/LastCancelledAt sempre que entrar em Failed/Cancelled (podem ser sobrescritos).
    /// Retorna os nomes dos campos de timestamp que foram alterados para logging.
    /// </summary>
    public IReadOnlyList<string> ApplyTransitionTimestamps(VideoStatus previousStatus, VideoStatus newStatus)
    {
        var updated = new List<string>();

        // 1. ProcessingStartedAt: primeira vez que entra em ProcessingImages; não sobrescrever
        if (newStatus == VideoStatus.ProcessingImages && !ProcessingStartedAt.HasValue)
        {
            ProcessingStartedAt = DateTime.UtcNow;
            updated.Add(nameof(ProcessingStartedAt));
        }

        // 2. ImagesProcessingCompletedAt: primeira vez que entra em GeneratingZip; não sobrescrever
        if (newStatus == VideoStatus.GeneratingZip && !ImagesProcessingCompletedAt.HasValue)
        {
            ImagesProcessingCompletedAt = DateTime.UtcNow;
            updated.Add(nameof(ImagesProcessingCompletedAt));
        }

        // 3. ProcessingCompletedAt: primeira vez que entra em Completed; não sobrescrever
        if (newStatus == VideoStatus.Completed && !ProcessingCompletedAt.HasValue)
        {
            ProcessingCompletedAt = DateTime.UtcNow;
            updated.Add(nameof(ProcessingCompletedAt));
        }

        // 4. LastFailedAt: sempre que entrar em Failed; pode ser sobrescrito
        if (newStatus == VideoStatus.Failed)
        {
            LastFailedAt = DateTime.UtcNow;
            updated.Add(nameof(LastFailedAt));
        }

        // 5. LastCancelledAt: sempre que entrar em Cancelled; pode ser sobrescrito
        if (newStatus == VideoStatus.Cancelled)
        {
            LastCancelledAt = DateTime.UtcNow;
            updated.Add(nameof(LastCancelledAt));
        }

        return updated;
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

    public void SetMaxParallelChunks(int value)
    {
        if (value < 1 || value > 100)
            throw new ArgumentOutOfRangeException(nameof(value), "MaxParallelChunks must be between 1 and 100.");
        MaxParallelChunks = value;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetDuration(double durationSec)
    {
        if (durationSec <= 0)
            throw new ArgumentOutOfRangeException(nameof(durationSec), "DurationSec must be greater than 0.");
        DurationSec = durationSec;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetFrameIntervalSec(double frameIntervalSec)
    {
        if (frameIntervalSec <= 0)
            throw new ArgumentOutOfRangeException(nameof(frameIntervalSec), "FrameIntervalSec must be greater than 0.");
        FrameIntervalSec = frameIntervalSec;
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
        LastFailedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsCompleted()
    {
        Status = VideoStatus.Completed;
        ProgressPercent = 100;
        if (!ProcessingCompletedAt.HasValue)
            ProcessingCompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
