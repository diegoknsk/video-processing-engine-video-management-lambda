using FluentAssertions;
using FluentValidation;
using VideoProcessing.VideoManagement.Application.Models.InputModels;
using VideoProcessing.VideoManagement.Application.Validators;
using VideoProcessing.VideoManagement.Domain.Enums;
using VideoProcessing.VideoManagement.LambdaUpdateVideo.Models;

namespace VideoProcessing.VideoManagement.UnitTests.LambdaUpdateVideo;

/// <summary>
/// Validação do evento da Lambda (UpdateVideoLambdaEvent = UpdateVideoInputModel + videoId) com as mesmas regras do PATCH.
/// </summary>
public class UpdateVideoLambdaEventValidationTests
{
    private readonly IValidator<UpdateVideoInputModel> _validator = new UpdateVideoInputModelValidator();

    [Fact]
    public void Validate_WhenUserIdIsEmpty_ShouldBeInvalid()
    {
        var evt = new UpdateVideoLambdaEvent
        {
            VideoId = Guid.NewGuid(),
            UserId = Guid.Empty,
            Status = VideoStatus.Processing
        };
        var result = _validator.Validate(evt);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WhenNoUpdateFieldPresent_ShouldBeInvalid()
    {
        var evt = new UpdateVideoLambdaEvent
        {
            VideoId = Guid.NewGuid(),
            UserId = Guid.NewGuid()
        };
        var result = _validator.Validate(evt);
        result.IsValid.Should().BeFalse();
        result.Errors.Select(e => e.ErrorMessage).Should().ContainSingle(m => m.Contains("Pelo menos um campo"));
    }

    [Fact]
    public void Validate_WhenUserIdAndStatusPresent_ShouldBeValid()
    {
        var evt = new UpdateVideoLambdaEvent
        {
            VideoId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Status = VideoStatus.Processing
        };
        var result = _validator.Validate(evt);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenProgressPercentOutOfRange_ShouldBeInvalid()
    {
        var evt = new UpdateVideoLambdaEvent
        {
            VideoId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            ProgressPercent = 101
        };
        var result = _validator.Validate(evt);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WhenProgressPercentInRange_ShouldBeValid()
    {
        var evt = new UpdateVideoLambdaEvent
        {
            VideoId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            ProgressPercent = 50
        };
        var result = _validator.Validate(evt);
        result.IsValid.Should().BeTrue();
    }
}
