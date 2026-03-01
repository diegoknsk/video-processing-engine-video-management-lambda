using FluentValidation;
using VideoProcessing.VideoManagement.Application.Models.InputModels;

namespace VideoProcessing.VideoManagement.Application.Validators;

public class UpdateVideoInputModelValidator : AbstractValidator<UpdateVideoInputModel>
{
    public UpdateVideoInputModelValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId é obrigatório.");

        RuleFor(x => x)
            .Must(x => x.Status != null || x.ProgressPercent != null || x.ErrorMessage != null
                || x.ErrorCode != null || x.FramesPrefix != null || x.S3KeyZip != null
                || x.S3BucketFrames != null || x.S3BucketZip != null || x.StepExecutionArn != null)
            .WithMessage("Pelo menos um campo de atualização deve ser informado.");

        RuleFor(x => x.ProgressPercent)
            .InclusiveBetween(0, 100).When(x => x.ProgressPercent.HasValue);

        RuleFor(x => x.Status)
            .IsInEnum().When(x => x.Status.HasValue);
    }
}
