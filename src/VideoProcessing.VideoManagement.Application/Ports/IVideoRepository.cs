using VideoProcessing.VideoManagement.Domain.Entities;

namespace VideoProcessing.VideoManagement.Application.Ports;

/// <summary>
/// Port de persistência para a entidade Video (DynamoDB single-table).
/// </summary>
public interface IVideoRepository
{
    /// <summary>Cria um vídeo e associa clientRequestId para deduplicação.</summary>
    Task<Video> CreateAsync(Video video, string? clientRequestId, CancellationToken ct = default);

    /// <summary>Retorna vídeo por userId e videoId, ou null se não encontrado.</summary>
    Task<Video?> GetByIdAsync(string userId, string videoId, CancellationToken ct = default);

    /// <summary>Lista vídeos do usuário com paginação. Retorna Items e NextToken para próxima página.</summary>
    Task<(IReadOnlyList<Video> Items, string? NextToken)> GetByUserIdAsync(string userId, int limit = 50, string? paginationToken = null, CancellationToken ct = default);

    /// <summary>Atualiza vídeo com condições (ownership, monotonia, transições de status). Lança VideoUpdateConflictException se condição falhar.</summary>
    Task<Video> UpdateAsync(Video video, CancellationToken ct = default);

    /// <summary>Retorna vídeo já criado com esse clientRequestId (idempotência), ou null.</summary>
    Task<Video?> GetByClientRequestIdAsync(string userId, string clientRequestId, CancellationToken ct = default);
}
