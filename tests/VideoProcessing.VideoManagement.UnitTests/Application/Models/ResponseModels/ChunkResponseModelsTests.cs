using FluentAssertions;
using VideoProcessing.VideoManagement.Application.Models.ResponseModels;
using Xunit;

namespace VideoProcessing.VideoManagement.UnitTests.Application.Models.ResponseModels;

public class ChunkItemResponseModelTests
{
    [Fact]
    public void Constructor_ShouldSetAllProperties()
    {
        var model = new ChunkItemResponseModel("chunk-1", 0.0, 10.5, "completed");

        model.ChunkId.Should().Be("chunk-1");
        model.StartSec.Should().Be(0.0);
        model.EndSec.Should().Be(10.5);
        model.Status.Should().Be("completed");
    }
}

public class ChunksSummaryResponseModelTests
{
    [Fact]
    public void Constructor_ShouldSetAllProperties()
    {
        var model = new ChunksSummaryResponseModel(10, 5, 2, 1, 2);

        model.Total.Should().Be(10);
        model.Completed.Should().Be(5);
        model.Processing.Should().Be(2);
        model.Failed.Should().Be(1);
        model.Pending.Should().Be(2);
    }
}
