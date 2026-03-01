using System.ComponentModel.DataAnnotations;

namespace VideoProcessing.VideoManagement.Infra.CrossCutting.Configuration;

public record AwsOptions
{
    [Required] public string Region { get; init; } = string.Empty;
}
