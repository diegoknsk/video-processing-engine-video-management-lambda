using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using VideoProcessing.VideoManagement.Infra.CrossCutting.Configuration;
using Xunit;

namespace VideoProcessing.VideoManagement.UnitTests.Infra.CrossCutting.Configuration;

public class DynamoDbOptionsTests
{
    [Fact]
    public void Validation_ShouldPass_WhenAllPropertiesAreSet()
    {
        // Arrange
        var options = new DynamoDbOptions(
            TableName: "videos-table",
            Region: "us-east-1"
        );

        // Act
        var context = new ValidationContext(options);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(options, context, results, true);

        // Assert
        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Fact]
    public void Validation_ShouldFail_WhenTableNameIsEmpty()
    {
        // Arrange
        var options = new DynamoDbOptions(
            TableName: null!, // Simulating null binding
            Region: "us-east-1"
        );

        // Act
        var context = new ValidationContext(options);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(options, context, results, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(DynamoDbOptions.TableName)));
    }
}
