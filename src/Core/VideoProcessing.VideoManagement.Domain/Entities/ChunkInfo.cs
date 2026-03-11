namespace VideoProcessing.VideoManagement.Domain.Entities;

/// <summary>
/// Informações de um chunk de processamento de vídeo.
/// </summary>
public record ChunkInfo(
    string ChunkId,
    double StartSec,
    double EndSec,
    double IntervalSec,
    string ManifestPrefix,
    string FramesPrefix);
