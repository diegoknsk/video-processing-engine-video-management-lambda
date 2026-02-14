using System.ComponentModel.DataAnnotations;

namespace VideoProcessing.VideoManagement.Infra.CrossCutting.Configuration;

public record CognitoOptions(
    [Required] string UserPoolId,
    [Required] string ClientId,
    [Required] string Region
);
