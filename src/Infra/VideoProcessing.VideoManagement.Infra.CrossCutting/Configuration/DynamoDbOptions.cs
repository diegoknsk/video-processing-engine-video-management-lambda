using System.ComponentModel.DataAnnotations;

namespace VideoProcessing.VideoManagement.Infra.CrossCutting.Configuration;

public record DynamoDbOptions
{
    [Required] public string TableName { get; init; } = string.Empty;
    [Required] public string Region { get; init; } = string.Empty;
    /// <summary>Nome da tabela DynamoDB de chunks. Variável: DynamoDB__ChunksTableName.</summary>
    public string ChunksTableName { get; init; } = "video-processing-engine-dev-video-chunks";
}
