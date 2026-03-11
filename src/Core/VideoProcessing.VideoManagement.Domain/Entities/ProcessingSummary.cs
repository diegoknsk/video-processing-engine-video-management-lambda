namespace VideoProcessing.VideoManagement.Domain.Entities;

/// <summary>
/// Resumo do processamento com chunks indexados por ChunkId. Suporta merge incremental idempotente.
/// </summary>
public class ProcessingSummary
{
    public IReadOnlyDictionary<string, ChunkInfo> Chunks { get; }

    public ProcessingSummary(IReadOnlyDictionary<string, ChunkInfo> chunks)
    {
        Chunks = chunks ?? new Dictionary<string, ChunkInfo>();
    }

    /// <summary>
    /// Faz merge incremental e idempotente: incoming null não altera; chunk existente não é sobrescrito.
    /// </summary>
    public static ProcessingSummary? Merge(ProcessingSummary? existing, ProcessingSummary? incoming)
    {
        if (incoming is null)
            return existing;
        if (existing is null)
            return incoming;

        var merged = new Dictionary<string, ChunkInfo>(existing.Chunks);
        foreach (var kv in incoming.Chunks)
        {
            if (!merged.ContainsKey(kv.Key))
                merged[kv.Key] = kv.Value;
        }
        return new ProcessingSummary(merged);
    }
}
