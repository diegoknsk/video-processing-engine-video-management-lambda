namespace VideoProcessing.VideoManagement.Domain.Enums;

public enum VideoStatus
{
    Pending,
    Uploading, //TODO: REMOVER DEPOIS, NAO TENHO COMO PEGAR O STATUS DE UPLOADING, POIS O VIDEO SÓ EXISTE APÓS O UPLOAD, ENTÃO NUNCA VAI ESTAR NESSE STATUS
    Processing,
    Completed,
    Failed,
    Cancelled
}
