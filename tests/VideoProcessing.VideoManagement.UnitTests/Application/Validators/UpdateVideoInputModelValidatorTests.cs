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
            Status = VideoStatus.Processing
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
            Status = VideoStatus.Processing
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
}
