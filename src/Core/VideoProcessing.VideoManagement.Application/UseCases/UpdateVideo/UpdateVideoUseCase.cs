using VideoProcessing.VideoManagement.Application.Models.InputModels;
using VideoProcessing.VideoManagement.Application.Models.Mappers;
using VideoProcessing.VideoManagement.Application.Models.ResponseModels;
using VideoProcessing.VideoManagement.Application.Ports;
using VideoProcessing.VideoManagement.Domain.Entities;

namespace VideoProcessing.VideoManagement.Application.UseCases.UpdateVideo;

public class UpdateVideoUseCase(IVideoRepository repository) : IUpdateVideoUseCase
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
            StepExecutionArn: input.StepExecutionArn);

        var merged = Video.FromMerge(video, patch);
        var updated = await repository.UpdateAsync(merged, ct);
        return VideoResponseModelMapper.ToResponseModel(updated);
    }
}
