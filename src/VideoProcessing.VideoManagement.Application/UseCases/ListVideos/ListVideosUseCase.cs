using VideoProcessing.VideoManagement.Application.Models.Mappers;
using VideoProcessing.VideoManagement.Application.Models.ResponseModels;
using VideoProcessing.VideoManagement.Application.Ports;

namespace VideoProcessing.VideoManagement.Application.UseCases.ListVideos;

public class ListVideosUseCase(IVideoRepository repository) : IListVideosUseCase
{
    private const int MaxLimit = 100;
    private const int DefaultLimit = 50;

    public async Task<VideoListResponseModel> ExecuteAsync(string userId, int limit, string? nextToken, CancellationToken ct = default)
    {
        var effectiveLimit = limit <= 0 ? DefaultLimit : Math.Min(limit, MaxLimit);

        var (items, paginationToken) = await repository.GetByUserIdAsync(userId, effectiveLimit, nextToken, ct);

        var videos = items.Select(VideoResponseModelMapper.ToResponseModel).ToList();

        return new VideoListResponseModel
        {
            Videos = videos,
            NextToken = paginationToken
        };
    }
}
