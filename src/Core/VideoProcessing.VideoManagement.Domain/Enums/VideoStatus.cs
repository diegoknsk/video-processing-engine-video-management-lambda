namespace VideoProcessing.VideoManagement.Domain.Enums;

/// <summary>
/// Status do vídeo no pipeline de processamento.
/// </summary>
public enum VideoStatus
{
    UploadPending = 0,
    ProcessingImages = 1,
    GeneratingZip = 2,
    Completed = 3,
    Failed = 4,
    Cancelled = 5
}
