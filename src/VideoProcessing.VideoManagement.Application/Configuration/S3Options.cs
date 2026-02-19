using System.ComponentModel.DataAnnotations;

namespace VideoProcessing.VideoManagement.Application.Configuration;

public record S3Options
{
    [Required] public string BucketVideo { get; init; } = string.Empty;
    [Required] public string BucketFrames { get; init; } = string.Empty;
    [Required] public string BucketZip { get; init; } = string.Empty;
    [Required] public string Region { get; init; } = string.Empty;
    public int PresignedUrlTtlMinutes { get; init; } = 15;
}
