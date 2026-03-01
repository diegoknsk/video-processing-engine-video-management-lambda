using System.ComponentModel;

namespace VideoProcessing.VideoManagement.Application.Models.ResponseModels;

/// <summary>
/// Resposta paginada da listagem de vídeos (GET /videos).
/// </summary>
public record VideoListResponseModel
{
    /// <summary>Lista de vídeos da página atual.</summary>
    [Description("Lista de vídeos")]
    public required IReadOnlyList<VideoResponseModel> Videos { get; init; }

    /// <summary>Token para a próxima página (null se não houver mais).</summary>
    [Description("Token para a próxima página")]
    public string? NextToken { get; init; }
}
