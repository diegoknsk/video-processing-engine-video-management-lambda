using FluentAssertions;
using VideoProcessing.VideoManagement.Application.Models.InputModels;
using VideoProcessing.VideoManagement.Application.Validators;
using VideoProcessing.VideoManagement.Domain.Enums;

namespace VideoProcessing.VideoManagement.UnitTests.Application.Validators;

public class UpdateVideoInputModelValidatorTests
{
    private readonly UpdateVideoInputModelValidator _validator = new();

    [Fact]
    public void Validate_UserIdEmpty_ShouldFail()
    {
        var input = new UpdateVideoInputModel
        {
            UserId = Guid.Empty,
            Status = VideoStatus.ProcessingImages
        };

        var result = _validator.Validate(input);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "UserId é obrigatório.");
    }

    [Fact]
    public void Validate_AllUpdateFieldsNull_ShouldFail()
    {
        var input = new UpdateVideoInputModel
        {
            UserId = Guid.NewGuid()
        };

        var result = _validator.Validate(input);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Pelo menos um campo de atualização deve ser informado.");
    }

    [Fact]
    public void Validate_OnlyStatusPresent_ShouldPass()
    {
        var input = new UpdateVideoInputModel
        {
            UserId = Guid.NewGuid(),
            Status = VideoStatus.ProcessingImages
        };

        var result = _validator.Validate(input);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ProgressPercent101_ShouldFail()
    {
        var input = new UpdateVideoInputModel
        {
            UserId = Guid.NewGuid(),
            ProgressPercent = 101
        };

        var result = _validator.Validate(input);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ProgressPercent");
    }

    [Fact]
    public void Validate_ProgressPercent50_ShouldPass()
    {
        var input = new UpdateVideoInputModel
        {
            UserId = Guid.NewGuid(),
            ProgressPercent = 50
        };

        var result = _validator.Validate(input);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ValidUserIdAndErrorMessage_ShouldPass()
    {
        var input = new UpdateVideoInputModel
        {
            UserId = Guid.NewGuid(),
            ErrorMessage = "Something failed"
        };

        var result = _validator.Validate(input);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ProgressPercentZero_ShouldPass()
    {
        var input = new UpdateVideoInputModel
        {
            UserId = Guid.NewGuid(),
            ProgressPercent = 0
        };

        var result = _validator.Validate(input);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ProgressPercent100_ShouldPass()
    {
        var input = new UpdateVideoInputModel
        {
            UserId = Guid.NewGuid(),
            ProgressPercent = 100
        };

        var result = _validator.Validate(input);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_MaxParallelChunksZero_ShouldFail()
    {
        var input = new UpdateVideoInputModel
        {
            UserId = Guid.NewGuid(),
            MaxParallelChunks = 0
        };

        var result = _validator.Validate(input);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "MaxParallelChunks");
    }

    [Fact]
    public void Validate_MaxParallelChunks101_ShouldFail()
    {
        var input = new UpdateVideoInputModel
        {
            UserId = Guid.NewGuid(),
            MaxParallelChunks = 101
        };

        var result = _validator.Validate(input);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_MaxParallelChunks10_ShouldPass()
    {
        var input = new UpdateVideoInputModel
        {
            UserId = Guid.NewGuid(),
            MaxParallelChunks = 10
        };

        var result = _validator.Validate(input);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ProcessingSummaryChunkWithEmptyChunkId_ShouldFail()
    {
        var input = new UpdateVideoInputModel
        {
            UserId = Guid.NewGuid(),
            ProcessingSummary = new ProcessingSummaryInputModel
            {
                Chunks = new Dictionary<string, ChunkInfoInputModel>
                {
                    ["k1"] = new ChunkInfoInputModel { ChunkId = "", StartSec = 0, EndSec = 10, IntervalSec = 1 }
                }
            }
        };

        var result = _validator.Validate(input);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage != null && e.ErrorMessage.Contains("ChunkId"));
    }

    [Fact]
    public void Validate_ProcessingSummaryChunkWithEndSecLessThanOrEqualStartSec_ShouldFail()
    {
        var input = new UpdateVideoInputModel
        {
            UserId = Guid.NewGuid(),
            ProcessingSummary = new ProcessingSummaryInputModel
            {
                Chunks = new Dictionary<string, ChunkInfoInputModel>
                {
                    ["c1"] = new ChunkInfoInputModel { ChunkId = "c1", StartSec = 10, EndSec = 10, IntervalSec = 1 }
                }
            }
        };

        var result = _validator.Validate(input);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage != null && e.ErrorMessage.Contains("EndSec"));
    }

    [Fact]
    public void Validate_ProcessingSummaryChunkValid_ShouldPass()
    {
        var input = new UpdateVideoInputModel
        {
            UserId = Guid.NewGuid(),
            ProcessingSummary = new ProcessingSummaryInputModel
            {
                Chunks = new Dictionary<string, ChunkInfoInputModel>
                {
                    ["c1"] = new ChunkInfoInputModel { ChunkId = "c1", StartSec = 0, EndSec = 10, IntervalSec = 1 }
                }
            }
        };

        var result = _validator.Validate(input);

        result.IsValid.Should().BeTrue();
    }
}
