using VideoProcessing.VideoManagement.Application.Models.InputModels;
using VideoProcessing.VideoManagement.Application.Models.ResponseModels;

namespace VideoProcessing.VideoManagement.Application.UseCases.UploadVideo;

public interface IUploadVideoUseCase
{
    Task<UploadVideoResponseModel> ExecuteAsync(UploadVideoInputModel input, Guid userId, CancellationToken cancellationToken);
}
