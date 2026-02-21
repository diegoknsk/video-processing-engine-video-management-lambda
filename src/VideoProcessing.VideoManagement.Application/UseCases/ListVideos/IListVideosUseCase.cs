using VideoProcessing.VideoManagement.Application.Models.ResponseModels;

namespace VideoProcessing.VideoManagement.Application.UseCases.ListVideos;

/// <summary>
/// Use case para listagem paginada de vídeos do usuário.
/// </summary>
public interface IListVideosUseCase
{
    /// <summary>
    /// Lista vídeos do usuário com paginação cursor-based.
    /// </summary>
    /// <param name="userId">Identificador do usuário (Cognito sub).</param>
    /// <param name="limit">Quantidade máxima de itens (padrão 50, máx 100).</param>
    /// <param name="nextToken">Token para próxima página (opcional).</param>
    /// <param name="ct">Cancellation token.</param>
    Task<VideoListResponseModel> ExecuteAsync(string userId, int limit, string? nextToken, CancellationToken ct = default);
}
