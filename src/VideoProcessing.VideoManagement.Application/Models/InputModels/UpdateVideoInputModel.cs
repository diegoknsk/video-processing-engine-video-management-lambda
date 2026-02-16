using System.ComponentModel;
using VideoProcessing.VideoManagement.Domain.Enums;

namespace VideoProcessing.VideoManagement.Application.Models.InputModels;

/// <summary>
/// Contrato de entrada para atualização parcial de um vídeo (status, progresso, erros, S3, etc.).
/// Pelo menos uma propriedade deve ser informada.
/// </summary>
public record UpdateVideoInputModel
{
    /// <summary>Status do processamento do vídeo.</summary>
    [Description("Status do processamento")]
    public VideoStatus? Status { get; init; }

    /// <summary>Percentual de progresso (0-100).</summary>
    [Description("Percentual de progresso (0-100)")]
    public int? ProgressPercent { get; init; }

    /// <summary>Mensagem de erro em caso de falha.</summary>
    [Description("Mensagem de erro em caso de falha")]
    public string? ErrorMessage { get; init; }

    /// <summary>Código de erro opcional.</summary>
    [Description("Código de erro")]
    public string? ErrorCode { get; init; }

    /// <summary>Prefixo dos frames no S3.</summary>
    [Description("Prefixo dos frames no bucket S3")]
    public string? FramesPrefix { get; init; }

    /// <summary>Chave S3 do arquivo ZIP de saída.</summary>
    [Description("Chave S3 do arquivo ZIP")]
    public string? S3KeyZip { get; init; }

    /// <summary>Bucket S3 dos frames.</summary>
    [Description("Bucket S3 dos frames")]
    public string? S3BucketFrames { get; init; }

    /// <summary>Bucket S3 do ZIP.</summary>
    [Description("Bucket S3 do ZIP")]
    public string? S3BucketZip { get; init; }

    /// <summary>ARN da execução Step Functions (se aplicável).</summary>
    [Description("ARN da execução Step Functions")]
    public string? StepExecutionArn { get; init; }
}
