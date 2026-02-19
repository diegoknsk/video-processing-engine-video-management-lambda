using FluentValidation;
using VideoProcessing.VideoManagement.Application.Models.InputModels;

namespace VideoProcessing.VideoManagement.Application.Validators;

public class UploadVideoInputModelValidator : AbstractValidator<UploadVideoInputModel>
{
    private static readonly string[] AllowedContentTypes = 
    [
        "video/mp4", 
        "video/quicktime", 
        "video/x-msvideo", 
        "video/webm",
        "video/3gpp"
    ];

    public UploadVideoInputModelValidator()
    {
        RuleFor(x => x.OriginalFileName)
            .NotEmpty().WithMessage("OriginalFileName is required.")
            .MaximumLength(255).WithMessage("OriginalFileName must be less than 255 characters.");

        RuleFor(x => x.ContentType)
            .NotEmpty().WithMessage("ContentType is required.")
            .Must(ct => AllowedContentTypes.Contains(ct))
            .WithMessage($"ContentType not allowed. Allowed types: {string.Join(", ", AllowedContentTypes)}");

        RuleFor(x => x.SizeKb)
            .GreaterThan(0).WithMessage("SizeKb must be greater than 0.")
            // 5GB limit = 5 * 1024 * 1024 KB
            .LessThanOrEqualTo(5L * 1024 * 1024).WithMessage("SizeKb must be less than 5GB (5242880 KB).");

        RuleFor(x => x.DurationSec)
            .GreaterThan(0).When(x => x.DurationSec.HasValue)
            .WithMessage("DurationSec must be greater than 0 if provided.");
            
        // ClientRequestId is optional but if provided must be a valid UUID
        RuleFor(x => x.ClientRequestId)
            .MaximumLength(100)
            .Must(id => Guid.TryParse(id, out _))
            .WithMessage("ClientRequestId must be a valid UUID (GUID format).")
            .When(x => !string.IsNullOrEmpty(x.ClientRequestId));
    }
}
