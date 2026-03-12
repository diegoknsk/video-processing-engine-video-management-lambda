using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VideoProcessing.VideoManagement.Application.Configuration;
using VideoProcessing.VideoManagement.Application.Models.Mappers;
using VideoProcessing.VideoManagement.Application.Models.ResponseModels;
using VideoProcessing.VideoManagement.Application.Ports;
using VideoProcessing.VideoManagement.Domain.Enums;

namespace VideoProcessing.VideoManagement.Application.UseCases.GetVideoById;

public class GetVideoByIdUseCase(
    IVideoRepository repository,
    IVideoChunkRepository chunkRepository,
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

        var progressPercent = video.Status == VideoStatus.Completed
            ? 100
            : ComputeProgressPercent(video.ParallelChunks ?? 0, await chunkRepository.CountProcessedAsync(videoId, ct));

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

        return response with
        {
            ProgressPercent = progressPercent,
            ZipUrl = zipUrl,
            ZipFileName = video.ZipFileName ?? response.ZipFileName
        };
    }

    private static int ComputeProgressPercent(int parallelChunks, int chunksProcessed)
    {
        var divisor = parallelChunks > 0 ? parallelChunks : 1;
        var percent = (int)Math.Floor((chunksProcessed * 100.0) / divisor);
        return Math.Min(100, percent);
    }
}
