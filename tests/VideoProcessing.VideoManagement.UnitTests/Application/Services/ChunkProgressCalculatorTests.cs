using FluentAssertions;
using VideoProcessing.VideoManagement.Application.Models;
using VideoProcessing.VideoManagement.Application.Services;
using VideoProcessing.VideoManagement.Domain.Enums;
using Xunit;

namespace VideoProcessing.VideoManagement.UnitTests.Application.Services;

public class ChunkProgressCalculatorTests
{
    private readonly ChunkProgressCalculator _sut = new();

    [Fact]
    public void Calculate_WhenStatusCompleted_AlwaysReturnsProgress100()
    {
        var result = _sut.Calculate(VideoStatus.Completed, null);
        result.ProgressPercent.Should().Be(100);
        result.CurrentStage.Should().Be("Concluído");
        result.HasChunks.Should().BeFalse();

        var summary = new ChunkStatusSummary(3, 1, 2, 0, 0, null);
        var result2 = _sut.Calculate(VideoStatus.Completed, summary);
        result2.ProgressPercent.Should().Be(100);
        result2.CurrentStage.Should().Be("Concluído");
    }

    [Fact]
    public void Calculate_WhenSummaryNull_ReturnsHasChunksFalseAndProgressMinus1()
    {
        var result = _sut.Calculate(VideoStatus.ProcessingImages, null);
        result.HasChunks.Should().BeFalse();
        result.ProgressPercent.Should().Be(-1);
        result.CurrentStage.Should().Be("Processando");
    }

    [Fact]
    public void Calculate_WhenSummaryTotalZero_ReturnsHasChunksFalseAndProgressMinus1()
    {
        var summary = new ChunkStatusSummary(0, 0, 0, 0, 0, null);
        var result = _sut.Calculate(VideoStatus.ProcessingImages, summary);
        result.HasChunks.Should().BeFalse();
        result.ProgressPercent.Should().Be(-1);
    }

    [Fact]
    public void Calculate_When1Of3Completed_Returns33PercentAndProcessandoChunks()
    {
        var summary = new ChunkStatusSummary(3, 1, 1, 0, 1, null);
        var result = _sut.Calculate(VideoStatus.ProcessingImages, summary);
        result.HasChunks.Should().BeTrue();
        result.ProgressPercent.Should().Be(33);
        result.CurrentStage.Should().Be("Processando chunks");
    }

    [Fact]
    public void Calculate_When2Of3Completed_Returns66Percent()
    {
        var summary = new ChunkStatusSummary(3, 2, 1, 0, 0, null);
        var result = _sut.Calculate(VideoStatus.ProcessingImages, summary);
        result.HasChunks.Should().BeTrue();
        result.ProgressPercent.Should().Be(66);
        result.CurrentStage.Should().Be("Processando chunks");
    }

    [Fact]
    public void Calculate_WhenAllChunksCompletedAndFinalizeNull_Returns97AndGerandoZip()
    {
        var summary = new ChunkStatusSummary(3, 3, 0, 0, 0, null);
        var result = _sut.Calculate(VideoStatus.GeneratingZip, summary);
        result.HasChunks.Should().BeTrue();
        result.ProgressPercent.Should().Be(97);
        result.CurrentStage.Should().Be("Gerando ZIP");
    }

    [Fact]
    public void Calculate_WhenFinalizeCompleted_Returns100()
    {
        var summary = new ChunkStatusSummary(3, 3, 0, 0, 0, "completed");
        var result = _sut.Calculate(VideoStatus.ProcessingImages, summary);
        result.HasChunks.Should().BeTrue();
        result.ProgressPercent.Should().Be(100);
    }

    [Fact]
    public void Calculate_WhenStatusFailed_ReturnsCalculatedProgressWithoutCap()
    {
        var summary = new ChunkStatusSummary(3, 1, 0, 2, 0, null);
        var result = _sut.Calculate(VideoStatus.Failed, summary);
        result.HasChunks.Should().BeTrue();
        result.ProgressPercent.Should().Be(33);
        result.CurrentStage.Should().Be("Falhou");
    }

    [Fact]
    public void Calculate_WhenUploadPending_ReturnsUploadPendente()
    {
        var result = _sut.Calculate(VideoStatus.UploadPending, null);
        result.CurrentStage.Should().Be("Upload pendente");
    }

    [Fact]
    public void Calculate_WhenCancelled_ReturnsCancelado()
    {
        var result = _sut.Calculate(VideoStatus.Cancelled, null);
        result.CurrentStage.Should().Be("Cancelado");
    }

    [Fact]
    public void Calculate_WhenBasePercentAbove94_CapsAt94()
    {
        var summary = new ChunkStatusSummary(10, 10, 0, 0, 0, null);
        var result = _sut.Calculate(VideoStatus.GeneratingZip, summary);
        result.ProgressPercent.Should().Be(97);
    }

    [Fact]
    public void Calculate_WhenZeroCompleted_Returns0()
    {
        var summary = new ChunkStatusSummary(3, 0, 2, 0, 1, null);
        var result = _sut.Calculate(VideoStatus.ProcessingImages, summary);
        result.ProgressPercent.Should().Be(0);
    }

    [Fact]
    public void Calculate_WhenStatusFailedAndBasePercentHigh_ReturnsCappedAt100()
    {
        var summary = new ChunkStatusSummary(1, 1, 0, 0, 0, null);
        var result = _sut.Calculate(VideoStatus.Failed, summary);
        result.ProgressPercent.Should().Be(100);
        result.CurrentStage.Should().Be("Falhou");
    }

    [Fact]
    public void Calculate_WhenBasePercentBetween0And94_ReturnsThatPercent()
    {
        var summary = new ChunkStatusSummary(10, 5, 0, 0, 5, null);
        var result = _sut.Calculate(VideoStatus.GeneratingZip, summary);
        result.ProgressPercent.Should().Be(50);
        result.CurrentStage.Should().Be("Gerando ZIP");
    }

    [Fact]
    public void Calculate_WhenUnknownVideoStatus_ReturnsStageAsToString()
    {
        const int unknownStatusValue = 99;
        var unknownStatus = (VideoStatus)unknownStatusValue;
        var result = _sut.Calculate(unknownStatus, null);
        result.CurrentStage.Should().Be(unknownStatusValue.ToString());
    }
}
