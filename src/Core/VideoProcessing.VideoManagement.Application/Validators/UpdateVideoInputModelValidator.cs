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
                || x.S3BucketFrames != null || x.S3BucketZip != null || x.StepExecutionArn != null
                || x.MaxParallelChunks != null || x.ProcessingStartedAt != null || (x.ProcessingSummary?.Chunks != null && x.ProcessingSummary.Chunks.Count > 0))
            .WithMessage("Pelo menos um campo de atualização deve ser informado.");

        RuleFor(x => x.ProgressPercent)
            .InclusiveBetween(0, 100).When(x => x.ProgressPercent.HasValue);

        RuleFor(x => x.Status)
            .IsInEnum().When(x => x.Status.HasValue);

        RuleFor(x => x.MaxParallelChunks)
            .InclusiveBetween(1, 100).When(x => x.MaxParallelChunks.HasValue);

        RuleForEach(x => x.ProcessingSummary!.Chunks!.Values)
            .ChildRules(chunk =>
            {
                chunk.RuleFor(c => c.ChunkId)
                    .NotEmpty().WithMessage("ChunkId não pode ser vazio.");
                chunk.RuleFor(c => c.StartSec)
                    .GreaterThanOrEqualTo(0).WithMessage("StartSec deve ser >= 0.");
                chunk.RuleFor(c => c.EndSec)
                    .GreaterThan(c => c.StartSec).WithMessage("EndSec deve ser maior que StartSec.");
                chunk.RuleFor(c => c.IntervalSec)
                    .GreaterThan(0).WithMessage("IntervalSec deve ser > 0.");
            })
            .When(x => x.ProcessingSummary?.Chunks != null && x.ProcessingSummary.Chunks.Count > 0);
    }
}
