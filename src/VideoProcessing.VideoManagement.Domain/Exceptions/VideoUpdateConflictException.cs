namespace VideoProcessing.VideoManagement.Domain.Exceptions;

/// <summary>
/// Lançada quando o UpdateAsync falha por violação de condições (ownership, monotonia de progress, transição de status inválida).
/// </summary>
public class VideoUpdateConflictException(string message, Exception? innerException = null)
    : Exception(message, innerException);
