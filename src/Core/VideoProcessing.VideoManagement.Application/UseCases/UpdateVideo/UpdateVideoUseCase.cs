using Microsoft.Extensions.Logging;
using VideoProcessing.VideoManagement.Application.Models.InputModels;
using VideoProcessing.VideoManagement.Application.Models.Mappers;
using VideoProcessing.VideoManagement.Application.Models.ResponseModels;
using VideoProcessing.VideoManagement.Application.Ports;
using VideoProcessing.VideoManagement.Domain.Entities;

namespace VideoProcessing.VideoManagement.Application.UseCases.UpdateVideo;

public class UpdateVideoUseCase(
    IVideoRepository repository,
    IVideoChunkRepository chunkRepository,
    ILogger<UpdateVideoUseCase> logger) : IUpdateVideoUseCase
{
    public async Task<VideoResponseModel?> ExecuteAsync(Guid videoId, UpdateVideoInputModel input, CancellationToken ct = default)
    {
        var video = await repository.GetByIdAsync(input.UserId.ToString(), videoId.ToString(), ct);
        if (video is null)
            return null;

        var patch = new VideoUpdateValues(
            Status: input.Status,
            ProgressPercent: input.ProgressPercent,
            ErrorMessage: input.ErrorMessage,
            ErrorCode: input.ErrorCode,
            FramesPrefix: input.FramesPrefix,
            S3KeyZip: input.S3KeyZip,
            S3BucketFrames: input.S3BucketFrames,
            S3BucketZip: input.S3BucketZip,
            ZipBucket: input.ZipBucket,
            ZipKey: input.ZipKey,
            ZipFileName: input.ZipFileName,
            StepExecutionArn: input.StepExecutionArn,
            ParallelChunks: input.ParallelChunks,
            ProcessingStartedAt: input.ProcessingStartedAt,
            ProcessingSummary: input.ProcessingSummary is null ? null : MapProcessingSummaryFromInput(input.ProcessingSummary));

        var merged = Video.FromMerge(video, patch);
        if (patch.Status is { } newStatus && newStatus != video.Status)
        {
            var timestampFieldsUpdated = merged.ApplyTransitionTimestamps(video.Status, newStatus);
            logger.LogInformation(
                "Video status transition — VideoId: {VideoId}, PreviousStatus: {PreviousStatus}, NewStatus: {NewStatus}, TimestampFieldsUpdated: {TimestampFieldsUpdated}",
                videoId,
                video.Status.ToString(),
                newStatus.ToString(),
                timestampFieldsUpdated.Count > 0 ? string.Join(", ", timestampFieldsUpdated) : "(none)");
        }

        var updated = await repository.UpdateAsync(merged, ct);

        if (merged.ProcessingSummary?.Chunks is { } chunks && chunks.Count > 0)
        {
            var chunkStatus = merged.Status == Domain.Enums.VideoStatus.Completed ? "completed" : "processing";
            var processedAt = chunkStatus == "completed" ? DateTime.UtcNow : (DateTime?)null;
            foreach (var (chunkId, chunkInfo) in chunks)
            {
                try
                {
                    var videoChunk = new VideoChunk(
                        ChunkId: chunkId,
                        VideoId: videoId.ToString(),
                        Status: chunkStatus,
                        StartSec: chunkInfo.StartSec,
                        EndSec: chunkInfo.EndSec,
                        IntervalSec: chunkInfo.IntervalSec,
                        ManifestPrefix: string.IsNullOrEmpty(chunkInfo.ManifestPrefix) ? null : chunkInfo.ManifestPrefix,
                        FramesPrefix: string.IsNullOrEmpty(chunkInfo.FramesPrefix) ? null : chunkInfo.FramesPrefix,
                        ProcessedAt: processedAt,
                        CreatedAt: DateTime.UtcNow);
                    await chunkRepository.UpsertAsync(videoChunk, ct);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Falha ao persistir chunk {ChunkId} do vídeo {VideoId}; atualização principal mantida.", chunkId, videoId);
                }
            }
        }

        return VideoResponseModelMapper.ToResponseModel(updated);
    }

    private static ProcessingSummary? MapProcessingSummaryFromInput(ProcessingSummaryInputModel? input)
    {
        if (input?.Chunks is null || input.Chunks.Count == 0)
            return null;
        var chunks = new Dictionary<string, ChunkInfo>();
        foreach (var (chunkId, chunkInput) in input.Chunks)
        {
            if (string.IsNullOrWhiteSpace(chunkId))
                continue;
            chunks[chunkId] = new ChunkInfo(
                chunkInput.ChunkId,
                chunkInput.StartSec,
                chunkInput.EndSec,
                chunkInput.IntervalSec,
                chunkInput.ManifestPrefix ?? string.Empty,
                chunkInput.FramesPrefix ?? string.Empty);
        }
        return new ProcessingSummary(chunks);
    }
}
