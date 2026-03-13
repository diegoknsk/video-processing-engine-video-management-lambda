using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VideoProcessing.VideoManagement.Application.Configuration;
using VideoProcessing.VideoManagement.Application.Extensions;
using VideoProcessing.VideoManagement.Application.Models.Mappers;
using VideoProcessing.VideoManagement.Application.Models.ResponseModels;
using VideoProcessing.VideoManagement.Application.Ports;
using VideoProcessing.VideoManagement.Application.Services;
using VideoProcessing.VideoManagement.Domain.Enums;

namespace VideoProcessing.VideoManagement.Application.UseCases.GetVideoById;

public class GetVideoByIdUseCase(
    IVideoRepository repository,
    IVideoChunkRepository chunkRepository,
    IChunkProgressCalculator chunkProgressCalculator,
    IS3PresignedUrlService s3PresignedUrlService,
    IOptions<S3Options> s3Options,
    ILogger<GetVideoByIdUseCase> logger) : IGetVideoByIdUseCase
{
    public async Task<VideoResponseModel?> ExecuteAsync(string userId, string videoId, CancellationToken ct = default)
    {
        var video = await repository.GetByIdAsync(userId, videoId, ct);
        if (video is null)
            return null;

        var response = VideoResponseModelMapper.ToResponseModel(video);

        var summaryTask = chunkRepository.GetStatusSummaryAsync(videoId, ct);
        var chunksTask = chunkRepository.GetChunksAsync(videoId, ct);
        await Task.WhenAll(summaryTask, chunksTask).ConfigureAwait(false);
        var summary = await summaryTask.ConfigureAwait(false);
        var chunks = await chunksTask.ConfigureAwait(false);

        var progressResult = chunkProgressCalculator.Calculate(video.Status, summary);
        var progressPercent = video.Status == VideoStatus.Completed
            ? 100
            : progressResult.HasChunks
                ? progressResult.ProgressPercent
                : video.ProgressPercent;

        string? zipUrl = null;
        if (!string.IsNullOrEmpty(video.ZipKey) && !string.IsNullOrEmpty(video.ZipBucket))
        {
            try
            {
                var ttl = TimeSpan.FromMinutes(s3Options.Value.PresignedUrlTtlMinutes);
                zipUrl = s3PresignedUrlService.GenerateGetPresignedUrl(video.ZipBucket, video.ZipKey, ttl);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Falha ao gerar URL pré-assinada do ZIP para vídeo {VideoId}.", videoId);
            }
        }

        // Guard de consistência eventual: se o status for GeneratingZip mas os chunks ainda não
        // estiverem todos completos (janela de milissegundos do fanout), exibe como ProcessingImages.
        var displayStatus = video.Status is VideoStatus.GeneratingZip
            && (summary is null || summary.Total == 0 || summary.Processing > 0)
                ? VideoStatus.ProcessingImages
                : video.Status;

        ChunksSummaryResponseModel? chunksSummaryModel = null;
        if (summary is not null && summary.Total > 0)
            chunksSummaryModel = new ChunksSummaryResponseModel(
                summary.Total, summary.Completed, summary.Processing, summary.Failed, summary.Pending);

        IReadOnlyList<ChunkItemResponseModel>? chunkItems = null;
        if (chunks is { Count: > 0 })
            chunkItems = chunks.Select(c => new ChunkItemResponseModel(c.ChunkId, c.StartSec, c.EndSec, c.Status)).ToList();

        return response with
        {
            Status = displayStatus,
            StatusDescription = displayStatus.ToFriendlyName(),
            ProgressPercent = progressPercent,
            ZipUrl = zipUrl,
            ZipFileName = video.ZipFileName ?? response.ZipFileName,
            CurrentStage = progressResult.CurrentStage,
            TotalChunks = summary?.Total > 0 ? summary.Total : null,
            CompletedChunks = summary?.Total > 0 ? summary.Completed : null,
            ProcessingChunks = summary?.Total > 0 ? summary.Processing : null,
            FailedChunks = summary?.Total > 0 ? summary.Failed : null,
            PendingChunks = summary?.Total > 0 ? summary.Pending : null,
            ChunksSummary = chunksSummaryModel,
            Chunks = chunkItems
        };
    }
}
