namespace VideoProcessing.VideoManagement.Application.Ports;

public interface IS3PresignedUrlService
{
    string GeneratePutPresignedUrl(string bucketName, string key, TimeSpan expiry, string contentType);

    /// <summary>Gera URL pré-assinada para download (GET) do objeto S3.</summary>
    string? GenerateGetPresignedUrl(string bucketName, string key, TimeSpan expiry);
}
