using System.ComponentModel.DataAnnotations;

namespace VideoProcessing.VideoManagement.Infra.CrossCutting.Configuration;

public record AwsOptions(
    [Required] string Region
);
