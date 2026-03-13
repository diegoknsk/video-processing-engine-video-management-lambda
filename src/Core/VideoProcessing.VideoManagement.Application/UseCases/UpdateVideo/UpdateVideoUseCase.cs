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
            const string chunkStatus = VideoChunkConstants.StatusCompleted;
            var processedAt = DateTime.UtcNow;
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
        else if (input.Chunk is { } singleChunk)
        {
            try
            {
                const string chunkStatus = VideoChunkConstants.StatusCompleted;
                var processedAt = DateTime.UtcNow;
                var videoChunk = new VideoChunk(
                    ChunkId: singleChunk.ChunkId,
                    VideoId: videoId.ToString(),
                    Status: chunkStatus,
                    StartSec: singleChunk.StartSec,
                    EndSec: singleChunk.EndSec,
                    IntervalSec: singleChunk.IntervalSec,
                    ManifestPrefix: string.IsNullOrEmpty(singleChunk.ManifestPrefix) ? null : singleChunk.ManifestPrefix,
                    FramesPrefix: string.IsNullOrEmpty(singleChunk.FramesPrefix) ? null : singleChunk.FramesPrefix,
                    ProcessedAt: processedAt,
                    CreatedAt: DateTime.UtcNow);
                await chunkRepository.UpsertAsync(videoChunk, ct);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Falha ao persistir chunk singular {ChunkId} do vídeo {VideoId}; atualização principal mantida.", singleChunk.ChunkId, videoId);
            }
        }
        else if (merged.Status == Domain.Enums.VideoStatus.Completed || merged.Status == Domain.Enums.VideoStatus.GeneratingZip)
        {
            try
            {
                var finalizationChunk = new VideoChunk(
                    ChunkId: "finalize",
                    VideoId: videoId.ToString(),
                    Status: merged.Status == Domain.Enums.VideoStatus.Completed ? "completed" : "processing",
                    StartSec: 0,
                    EndSec: 0,
                    IntervalSec: 0,
                    ManifestPrefix: null,
                    FramesPrefix: string.IsNullOrEmpty(merged.FramesPrefix) ? null : merged.FramesPrefix,
                    ProcessedAt: merged.Status == Domain.Enums.VideoStatus.Completed ? DateTime.UtcNow : null,
                    CreatedAt: DateTime.UtcNow);
                await chunkRepository.UpsertAsync(finalizationChunk, ct);
                logger.LogInformation(
                    "Chunk de finalização inserido para VideoId: {VideoId}, Status: {Status}",
                    videoId, merged.Status);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Falha ao persistir chunk de finalização do vídeo {VideoId}; atualização principal mantida.", videoId);
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
