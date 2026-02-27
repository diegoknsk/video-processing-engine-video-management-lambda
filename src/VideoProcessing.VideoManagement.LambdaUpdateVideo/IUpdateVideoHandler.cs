using VideoProcessing.VideoManagement.LambdaUpdateVideo.Models;

namespace VideoProcessing.VideoManagement.LambdaUpdateVideo;

/// <summary>
/// Processa o evento de update de vídeo (UpdateVideoInputModel + videoId) e retorna resposta tipada (200/400/404/409).
/// </summary>
public interface IUpdateVideoHandler
{
    Task<UpdateVideoLambdaResponse> HandleAsync(UpdateVideoLambdaEvent evt, CancellationToken cancellationToken = default);
}
