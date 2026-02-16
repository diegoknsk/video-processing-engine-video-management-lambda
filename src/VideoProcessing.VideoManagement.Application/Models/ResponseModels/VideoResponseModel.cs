using System.ComponentModel;
using VideoProcessing.VideoManagement.Domain.Enums;

namespace VideoProcessing.VideoManagement.Application.Models.ResponseModels;

/// <summary>
/// Representação de um vídeo na resposta da API (GET /videos/{id} ou item da listagem).
/// </summary>
public record VideoResponseModel
{
    /// <summary>Identificador único do vídeo.</summary>
    [Description("Identificador único do vídeo")]
    public Guid VideoId { get; init; }

    /// <summary>Identificador do usuário (Cognito sub).</summary>
    [Description("Identificador do usuário")]
    public Guid UserId { get; init; }

    /// <summary>Nome original do arquivo.</summary>
    [Description("Nome original do arquivo")]
    public string OriginalFileName { get; init; } = string.Empty;

    /// <summary>Tipo MIME do conteúdo.</summary>
    [Description("Tipo MIME do conteúdo")]
    public string ContentType { get; init; } = string.Empty;

    /// <summary>Tamanho do arquivo em bytes.</summary>
    [Description("Tamanho em bytes")]
    public long SizeBytes { get; init; }

    /// <summary>Duração do vídeo em segundos.</summary>
    [Description("Duração em segundos")]
    public double? DurationSec { get; init; }

    /// <summary>Status do processamento.</summary>
    [Description("Status do processamento")]
    public VideoStatus Status { get; init; }

    /// <summary>Modo de processamento (SingleLambda ou FanOut).</summary>
    [Description("Modo de processamento")]
    public ProcessingMode ProcessingMode { get; init; }

    /// <summary>Percentual de progresso (0-100).</summary>
    [Description("Percentual de progresso")]
    public int ProgressPercent { get; init; }

    /// <summary>Bucket S3 do vídeo.</summary>
    [Description("Bucket S3 do vídeo")]
    public string? S3BucketVideo { get; init; }

    /// <summary>Chave S3 do vídeo.</summary>
    [Description("Chave S3 do vídeo")]
    public string? S3KeyVideo { get; init; }

    /// <summary>Bucket S3 do ZIP.</summary>
    [Description("Bucket S3 do ZIP")]
    public string? S3BucketZip { get; init; }

    /// <summary>Chave S3 do ZIP.</summary>
    [Description("Chave S3 do ZIP")]
    public string? S3KeyZip { get; init; }

    /// <summary>Bucket S3 dos frames.</summary>
    [Description("Bucket S3 dos frames")]
    public string? S3BucketFrames { get; init; }

    /// <summary>Prefixo dos frames no S3.</summary>
    [Description("Prefixo dos frames")]
    public string? FramesPrefix { get; init; }

    /// <summary>ARN da execução Step Functions.</summary>
    [Description("ARN da execução Step Functions")]
    public string? StepExecutionArn { get; init; }

    /// <summary>Mensagem de erro (se falhou).</summary>
    [Description("Mensagem de erro")]
    public string? ErrorMessage { get; init; }

    /// <summary>Código de erro.</summary>
    [Description("Código de erro")]
    public string? ErrorCode { get; init; }

    /// <summary>Client request id para idempotência.</summary>
    [Description("Client request id")]
    public string? ClientRequestId { get; init; }

    /// <summary>Quantidade de chunks (FanOut).</summary>
    [Description("Quantidade de chunks")]
    public int? ChunkCount { get; init; }

    /// <summary>Duração por chunk em segundos.</summary>
    [Description("Duração por chunk em segundos")]
    public double? ChunkDurationSec { get; init; }

    /// <summary>Data/hora de emissão do upload.</summary>
    [Description("Data/hora de emissão do upload")]
    public DateTime? UploadIssuedAt { get; init; }

    /// <summary>Data/hora de expiração da URL de upload.</summary>
    [Description("Expiração da URL de upload")]
    public DateTime? UploadUrlExpiresAt { get; init; }

    /// <summary>Quantidade de frames processados.</summary>
    [Description("Frames processados")]
    public int? FramesProcessed { get; init; }

    /// <summary>Data/hora de finalização.</summary>
    [Description("Data/hora de finalização")]
    public DateTime? FinalizedAt { get; init; }

    /// <summary>Data de criação.</summary>
    [Description("Data de criação")]
    public DateTime CreatedAt { get; init; }

    /// <summary>Data da última atualização.</summary>
    [Description("Data da última atualização")]
    public DateTime? UpdatedAt { get; init; }

    /// <summary>Versão do registro (otimistic concurrency).</summary>
    [Description("Versão do registro")]
    public int? Version { get; init; }
}
