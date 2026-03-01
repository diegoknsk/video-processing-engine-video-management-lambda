using System.ComponentModel.DataAnnotations;

namespace VideoProcessing.VideoManagement.Infra.CrossCutting.Configuration;

public record DynamoDbOptions
{
    [Required] public string TableName { get; init; } = string.Empty;
    [Required] public string Region { get; init; } = string.Empty;
}
