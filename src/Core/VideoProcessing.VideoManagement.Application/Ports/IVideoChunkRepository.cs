using VideoProcessing.VideoManagement.Application.Models;
using VideoProcessing.VideoManagement.Domain.Entities;

namespace VideoProcessing.VideoManagement.Application.Ports;

/// <summary>
/// Port para persistência de chunks de vídeo na tabela dedicada DynamoDB.
/// </summary>
public interface IVideoChunkRepository
{
    /// <summary>Insere ou atualiza o registro do chunk (PutItem idempotente).</summary>
    Task UpsertAsync(VideoChunk chunk, CancellationToken ct = default);

    /// <summary>Conta chunks com status concluído (ex.: "completed") para o vídeo.</summary>
    Task<int> CountProcessedAsync(string videoId, CancellationToken ct = default);

    /// <summary>Retorna contagem de chunks por status (excluindo o item finalize do total) e status do finalize.</summary>
    Task<ChunkStatusSummary> GetStatusSummaryAsync(string videoId, CancellationToken ct = default);

    /// <summary>Retorna lista de chunks do vídeo (chunkId, startSec, endSec, status), excluindo o item finalize.</summary>
    Task<IReadOnlyList<VideoChunk>> GetChunksAsync(string videoId, CancellationToken ct = default);
}
