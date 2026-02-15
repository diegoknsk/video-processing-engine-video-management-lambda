using System.ComponentModel.DataAnnotations;

namespace VideoProcessing.VideoManagement.Infra.CrossCutting.Configuration;

public record DynamoDbOptions(
    [property: Required] string TableName,
    [property: Required] string Region
);
