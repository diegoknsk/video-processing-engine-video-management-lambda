using VideoProcessing.VideoManagement.Application.Models.Mappers;
using VideoProcessing.VideoManagement.Application.Models.ResponseModels;
using VideoProcessing.VideoManagement.Application.Ports;

namespace VideoProcessing.VideoManagement.Application.UseCases.GetVideoById;

public class GetVideoByIdUseCase(IVideoRepository repository) : IGetVideoByIdUseCase
{
    public async Task<VideoResponseModel?> ExecuteAsync(string userId, string videoId, CancellationToken ct = default)
    {
        var video = await repository.GetByIdAsync(userId, videoId, ct);
        return video is null ? null : VideoResponseModelMapper.ToResponseModel(video);
    }
}
