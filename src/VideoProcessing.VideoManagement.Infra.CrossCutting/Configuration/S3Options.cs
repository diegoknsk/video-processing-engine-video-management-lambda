using System.ComponentModel.DataAnnotations;

namespace VideoProcessing.VideoManagement.Infra.CrossCutting.Configuration;

public record S3Options(
    [Required] string BucketVideo,
    [Required] string BucketFrames,
    [Required] string BucketZip,
    [Required] string Region,
    int PresignedUrlTtlMinutes = 15
);
