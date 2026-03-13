using VideoProcessing.VideoManagement.Application.Models.Mappers;
using VideoProcessing.VideoManagement.Application.Models.ResponseModels;
using VideoProcessing.VideoManagement.Application.Ports;
using VideoProcessing.VideoManagement.Application.Services;
using VideoProcessing.VideoManagement.Domain.Enums;

namespace VideoProcessing.VideoManagement.Application.UseCases.ListVideos;

public class ListVideosUseCase(
    IVideoRepository repository,
    IVideoChunkRepository chunkRepository,
    IChunkProgressCalculator chunkProgressCalculator) : IListVideosUseCase
{
    private const int MaxLimit = 100;
    private const int DefaultLimit = 50;

    public async Task<VideoListResponseModel> ExecuteAsync(string userId, int limit, string? nextToken, CancellationToken ct = default)
    {
        var effectiveLimit = limit <= 0 ? DefaultLimit : Math.Min(limit, MaxLimit);

        var (items, paginationToken) = await repository.GetByUserIdAsync(userId, effectiveLimit, nextToken, ct);

        var eligibleForEnrichment = items
            .Where(v => v.ProcessingMode == ProcessingMode.FanOut
                && v.Status != VideoStatus.Completed
                && v.Status != VideoStatus.Cancelled)
            .ToList();

        var summaryTasks = eligibleForEnrichment
            .ToDictionary(v => v.VideoId, v => chunkRepository.GetStatusSummaryAsync(v.VideoId.ToString(), ct));
        await Task.WhenAll(summaryTasks.Values).ConfigureAwait(false);

        var summaries = summaryTasks.ToDictionary(kv => kv.Key, kv => kv.Value.Result);

        var videos = items.Select(v =>
        {
            var model = VideoResponseModelMapper.ToResponseModel(v);
            if (!summaries.TryGetValue(v.VideoId, out var summary))
                return model;

            var progressResult = chunkProgressCalculator.Calculate(v.Status, summary);
            var hasChunks = summary.Total > 0;
            return model with
            {
                ProgressPercent = progressResult.HasChunks ? progressResult.ProgressPercent : v.ProgressPercent,
                CurrentStage = progressResult.CurrentStage,
                TotalChunks = hasChunks ? summary.Total : null,
                CompletedChunks = hasChunks ? summary.Completed : null,
                ProcessingChunks = hasChunks ? summary.Processing : null,
                FailedChunks = hasChunks ? summary.Failed : null,
                PendingChunks = hasChunks ? summary.Pending : null,
                ChunksSummary = hasChunks
                    ? new ChunksSummaryResponseModel(summary.Total, summary.Completed, summary.Processing, summary.Failed, summary.Pending)
                    : null,
                Chunks = null
            };
        }).ToList();

        return new VideoListResponseModel
        {
            Videos = videos,
            NextToken = paginationToken
        };
    }
}
