using VideoProcessing.VideoManagement.Application.Models.ResponseModels;

namespace VideoProcessing.VideoManagement.Application.UseCases.GetVideoById;

/// <summary>
/// Use case para consulta de vídeo por identificador (com validação de ownership).
/// </summary>
public interface IGetVideoByIdUseCase
{
    /// <summary>
    /// Obtém vídeo por userId e videoId. Retorna null se não encontrado ou não pertence ao usuário.
    /// </summary>
    /// <param name="userId">Identificador do usuário (Cognito sub).</param>
    /// <param name="videoId">Identificador do vídeo.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<VideoResponseModel?> ExecuteAsync(string userId, string videoId, CancellationToken ct = default);
}
