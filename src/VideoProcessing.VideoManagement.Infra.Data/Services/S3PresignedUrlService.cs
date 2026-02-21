using Amazon.S3;
using Amazon.S3.Model;
using VideoProcessing.VideoManagement.Application.Ports;

namespace VideoProcessing.VideoManagement.Infra.Data.Services;

public class S3PresignedUrlService : IS3PresignedUrlService
{
    private readonly IAmazonS3 _s3Client;

    public S3PresignedUrlService(IAmazonS3 s3Client)
    {
        _s3Client = s3Client;
    }

    public string GeneratePutPresignedUrl(string bucketName, string key, TimeSpan expiry, string contentType)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = bucketName,
            Key = key,
            Verb = HttpVerb.PUT,
            Expires = DateTime.UtcNow.Add(expiry),
            ContentType = contentType
        };

        return _s3Client.GetPreSignedURL(request);
    }
}
