namespace VideoProcessing.VideoManagement.Application.Ports;

public interface IS3PresignedUrlService
{
    string GeneratePutPresignedUrl(string bucketName, string key, TimeSpan expiry, string contentType);
}
