using System.ComponentModel.DataAnnotations;

namespace VideoProcessing.VideoManagement.Infra.CrossCutting.Configuration;

public record DynamoDbOptions(
    [Required] string TableName,
    [Required] string Region
);
