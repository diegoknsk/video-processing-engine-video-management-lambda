using VideoProcessing.VideoManagement.Application.Models.InputModels;
using VideoProcessing.VideoManagement.Application.Validators;

namespace VideoProcessing.VideoManagement.UnitTests.Application.Validators;

public class UploadVideoInputModelValidatorTests
{
    private readonly UploadVideoInputModelValidator _validator = new();

    [Fact]
    public void Validate_ValidInput_ShouldPass()
    {
        var input = new UploadVideoInputModel
        {
            OriginalFileName = "video.mp4",
            ContentType = "video/mp4",
            SizeKb = 1,
            DurationSec = 30.0,
            ClientRequestId = Guid.NewGuid().ToString()
        };

        var result = _validator.Validate(input);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyOriginalFileName_ShouldFail()
    {
        var input = new UploadVideoInputModel
        {
            OriginalFileName = "",
            ContentType = "video/mp4",
            SizeKb = 1
        };

        var result = _validator.Validate(input);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "OriginalFileName is required.");
    }

    [Fact]
    public void Validate_OriginalFileNameExceedsMaxLength_ShouldFail()
    {
        var input = new UploadVideoInputModel
        {
            OriginalFileName = new string('a', 256),
            ContentType = "video/mp4",
            SizeKb = 1
        };

        var result = _validator.Validate(input);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "OriginalFileName");
    }

    [Fact]
    public void Validate_InvalidContentType_ShouldFail()
    {
        var input = new UploadVideoInputModel
        {
            OriginalFileName = "video.png",
            ContentType = "image/png",
            SizeKb = 1
        };

        var result = _validator.Validate(input);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "ContentType");
    }

    [Fact]
    public void Validate_SizeKbZero_ShouldFail()
    {
        var input = new UploadVideoInputModel
        {
            OriginalFileName = "video.mp4",
            ContentType = "video/mp4",
            SizeKb = 0
        };

        var result = _validator.Validate(input);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "SizeKb must be greater than 0.");
    }

    [Fact]
    public void Validate_SizeKbExceedsLimit_ShouldFail()
    {
        var input = new UploadVideoInputModel
        {
            OriginalFileName = "video.mp4",
            ContentType = "video/mp4",
            SizeKb = 6L * 1024 * 1024
        };

        var result = _validator.Validate(input);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "SizeKb");
    }

    [Fact]
    public void Validate_ClientRequestIdNotUuid_ShouldFail()
    {
        var input = new UploadVideoInputModel
        {
            OriginalFileName = "video.mp4",
            ContentType = "video/mp4",
            SizeKb = 1,
            ClientRequestId = "not-a-guid"
        };

        var result = _validator.Validate(input);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "ClientRequestId must be a valid UUID (GUID format).");
    }

    [Fact]
    public void Validate_ClientRequestIdNull_ShouldPass()
    {
        var input = new UploadVideoInputModel
        {
            OriginalFileName = "video.mp4",
            ContentType = "video/mp4",
            SizeKb = 1,
            ClientRequestId = null
        };

        var result = _validator.Validate(input);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_DurationSecNegative_ShouldFail()
    {
        var input = new UploadVideoInputModel
        {
            OriginalFileName = "video.mp4",
            ContentType = "video/mp4",
            SizeKb = 1,
            DurationSec = -1
        };

        var result = _validator.Validate(input);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "DurationSec must be greater than 0 if provided.");
    }
}
