using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VideoProcessing.VideoManagement.Application.Configuration;
using VideoProcessing.VideoManagement.Application.Models.InputModels;
using VideoProcessing.VideoManagement.Application.Models.ResponseModels;
using VideoProcessing.VideoManagement.Application.Ports;
using VideoProcessing.VideoManagement.Domain.Entities;

namespace VideoProcessing.VideoManagement.Application.UseCases.UploadVideo;

public class UploadVideoUseCase(
    IVideoRepository repository,
    IS3PresignedUrlService s3PresignedUrlService,
    IOptions<S3Options> s3Options,
    IValidator<UploadVideoInputModel> validator,
    ILogger<UploadVideoUseCase> logger) : IUploadVideoUseCase
{
    private readonly S3Options _s3Options = s3Options.Value;

    public async Task<UploadVideoResponseModel> ExecuteAsync(UploadVideoInputModel input, Guid userId, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(input, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // Idempotency Check
        if (!string.IsNullOrEmpty(input.ClientRequestId))
        {
            var existingVideo = await repository.GetByClientRequestIdAsync(userId.ToString(), input.ClientRequestId, cancellationToken);
            if (existingVideo != null)
            {
                logger.LogInformation("Idempotency hit: Returning existing video {VideoId} for request {ClientRequestId}", existingVideo.VideoId, input.ClientRequestId);

                // Re-generate presigned URL as previous might have expired
                var existingPresignedUrl = GeneratePresignedUrl(existingVideo);

                return new UploadVideoResponseModel
                {
                    VideoId = existingVideo.VideoId,
                    UploadUrl = existingPresignedUrl,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(_s3Options.PresignedUrlTtlMinutes)
                };
            }
        }

        // Create new Video (converte SizeKb para bytes para armazenamento interno)
        var sizeBytes = input.SizeKb * 1024L;
        var video = new Video(
            userId,
            input.OriginalFileName,
            input.ContentType,
            sizeBytes,
            input.ClientRequestId
        );

        if (input.DurationSec.HasValue)
            video.SetDuration(input.DurationSec.Value);

        // Generate S3 Key: videos/{userId}/{videoId}/original
        var s3Key = $"videos/{userId}/{video.VideoId}/original";
        video.SetS3Source(_s3Options.BucketVideo, s3Key);

        // Save to DB and use the persisted instance
        video = await repository.CreateAsync(video, input.ClientRequestId, cancellationToken);

        // Generate Presigned URL
        var presignedUrl = GeneratePresignedUrl(video);

        logger.LogInformation("Video {VideoId} created for user {UserId}", video.VideoId, userId);

        return new UploadVideoResponseModel
        {
            VideoId = video.VideoId,
            UploadUrl = presignedUrl,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_s3Options.PresignedUrlTtlMinutes)
        };
    }

    private string GeneratePresignedUrl(Video video)
    {
        if (string.IsNullOrEmpty(video.S3BucketVideo) || string.IsNullOrEmpty(video.S3KeyVideo))
        {
            throw new InvalidOperationException("Video S3 details are missing.");
        }

        return s3PresignedUrlService.GeneratePutPresignedUrl(
            video.S3BucketVideo,
            video.S3KeyVideo,
            TimeSpan.FromMinutes(_s3Options.PresignedUrlTtlMinutes),
            video.ContentType
        );
    }
}
