using FluentAssertions;
using VideoProcessing.VideoManagement.Domain.Entities;
using Xunit;

namespace VideoProcessing.VideoManagement.UnitTests.Domain;

public class ProcessingSummaryTests
{
    private static ChunkInfo Chunk(string id, double start = 0, double end = 10, double interval = 1) =>
        new(id, start, end, interval, $"manifest-{id}", $"frames-{id}");

    [Fact]
    public void Merge_NullIncoming_ReturnsExistingUnchanged()
    {
        var existing = new ProcessingSummary(new Dictionary<string, ChunkInfo> { ["c1"] = Chunk("c1") });
        var result = ProcessingSummary.Merge(existing, null);
        result.Should().BeSameAs(existing);
        result!.Chunks.Should().HaveCount(1).And.ContainKey("c1");
    }

    [Fact]
    public void Merge_NullExisting_ReturnsIncoming()
    {
        var incoming = new ProcessingSummary(new Dictionary<string, ChunkInfo> { ["c1"] = Chunk("c1"), ["c2"] = Chunk("c2") });
        var result = ProcessingSummary.Merge(null, incoming);
        result.Should().NotBeNull();
        result!.Chunks.Should().HaveCount(2).And.ContainKey("c1").And.ContainKey("c2");
    }

    [Fact]
    public void Merge_BothNull_ReturnsNull()
    {
        var result = ProcessingSummary.Merge(null, null);
        result.Should().BeNull();
    }

    [Fact]
    public void Merge_NewChunk_AddsChunk()
    {
        var existing = new ProcessingSummary(new Dictionary<string, ChunkInfo> { ["c1"] = Chunk("c1") });
        var incoming = new ProcessingSummary(new Dictionary<string, ChunkInfo> { ["c2"] = Chunk("c2") });
        var result = ProcessingSummary.Merge(existing, incoming);
        result!.Chunks.Should().HaveCount(2).And.ContainKey("c1").And.ContainKey("c2");
        result.Chunks["c1"].ChunkId.Should().Be("c1");
        result.Chunks["c2"].ChunkId.Should().Be("c2");
    }

    [Fact]
    public void Merge_DuplicateChunkId_PreservesExisting()
    {
        var existing = new ProcessingSummary(new Dictionary<string, ChunkInfo> { ["c1"] = Chunk("c1", 0, 10, 1) });
        var incoming = new ProcessingSummary(new Dictionary<string, ChunkInfo> { ["c1"] = Chunk("c1", 5, 15, 2) });
        var result = ProcessingSummary.Merge(existing, incoming);
        result!.Chunks.Should().HaveCount(1);
        result.Chunks["c1"].StartSec.Should().Be(0);
        result.Chunks["c1"].EndSec.Should().Be(10);
        result.Chunks["c1"].IntervalSec.Should().Be(1);
    }

    [Fact]
    public void Merge_MixedNewAndDuplicate_AddsOnlyNew()
    {
        var existing = new ProcessingSummary(new Dictionary<string, ChunkInfo>
        {
            ["c1"] = Chunk("c1"),
            ["c2"] = Chunk("c2"),
            ["c3"] = Chunk("c3")
        });
        var incoming = new ProcessingSummary(new Dictionary<string, ChunkInfo>
        {
            ["c2"] = Chunk("c2", 99, 99, 99),
            ["c4"] = Chunk("c4"),
            ["c5"] = Chunk("c5")
        });
        var result = ProcessingSummary.Merge(existing, incoming);
        result!.Chunks.Should().HaveCount(5);
        result.Chunks["c2"].IntervalSec.Should().Be(1);
        result.Chunks["c4"].ChunkId.Should().Be("c4");
        result.Chunks["c5"].ChunkId.Should().Be("c5");
    }
}
