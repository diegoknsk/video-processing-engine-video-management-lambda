using VideoProcessing.VideoManagement.Application.Models.InputModels;
using VideoProcessing.VideoManagement.Application.Models.ResponseModels;

namespace VideoProcessing.VideoManagement.Application.UseCases.UpdateVideo;

/// <summary>
/// Caso de uso para atualização parcial de vídeo (PATCH). Rota interna; UserId vem do body.
/// </summary>
public interface IUpdateVideoUseCase
{
    /// <summary>
    /// Executa a atualização. Retorna null se o vídeo não for encontrado (controller deve retornar 404).
    /// Propaga VideoUpdateConflictException para o controller retornar 409.
    /// </summary>
    Task<VideoResponseModel?> ExecuteAsync(Guid videoId, UpdateVideoInputModel input, CancellationToken ct = default);
}
