using System;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using VideoProcessing.VideoManagement.Api.Controllers;
using Xunit;

namespace VideoProcessing.VideoManagement.UnitTests.Api.Controllers;

public class HealthControllerTests
{
    [Fact]
    public void Get_ShouldReturnOk_WithHealthyStatusAndTimestamp()
    {
        // Arrange
        var controller = new HealthController();

        // Act
        var result = controller.Get();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var value = okResult.Value;
        
        // Use reflection or dynamic to access anonymous type properties
        var statusProperty = value.GetType().GetProperty("status");
        var timestampProperty = value.GetType().GetProperty("timestamp");

        statusProperty.Should().NotBeNull();
        timestampProperty.Should().NotBeNull();

        var status = statusProperty.GetValue(value) as string;
        var timestamp = timestampProperty.GetValue(value);

        status.Should().Be("healthy");
        timestamp.Should().BeAssignableTo<DateTime>();
        ((DateTime)timestamp).Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
