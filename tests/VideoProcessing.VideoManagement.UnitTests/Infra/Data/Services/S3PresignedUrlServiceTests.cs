using Amazon.S3;
using Amazon.S3.Model;
using FluentAssertions;
using Moq;
using VideoProcessing.VideoManagement.Infra.Data.Services;
using Xunit;

namespace VideoProcessing.VideoManagement.UnitTests.Infra.Data.Services;

public class S3PresignedUrlServiceTests
{
    private readonly Mock<IAmazonS3> _s3ClientMock = new();
    private readonly S3PresignedUrlService _service;

    public S3PresignedUrlServiceTests()
    {
        _service = new S3PresignedUrlService(_s3ClientMock.Object);
    }

    [Fact]
    public void GeneratePutPresignedUrl_ValidParams_ShouldCallSdkWithCorrectParameters()
    {
        // Arrange
        var bucketName = "my-video-bucket";
        var key = "videos/user-id/video-id/original";
        var expiry = TimeSpan.FromMinutes(15);
        var contentType = "video/mp4";
        var expectedUrl = "https://s3.amazonaws.com/my-video-bucket/videos/user-id/video-id/original?presigned=token";

        _s3ClientMock.Setup(s => s.GetPreSignedURL(It.IsAny<GetPreSignedUrlRequest>()))
            .Returns(expectedUrl);

        // Act
        var result = _service.GeneratePutPresignedUrl(bucketName, key, expiry, contentType);

        // Assert
        Assert.Equal(expectedUrl, result);

        _s3ClientMock.Verify(s => s.GetPreSignedURL(It.Is<GetPreSignedUrlRequest>(r =>
            r.BucketName == bucketName &&
            r.Key == key &&
            r.Verb == HttpVerb.PUT &&
            r.ContentType == contentType
        )), Times.Once);
    }

    [Fact]
    public void GenerateGetPresignedUrl_ValidParams_ShouldCallSdkWithGetVerb()
    {
        // Arrange
        var bucketName = "my-frames-bucket";
        var key = "videos/user-id/video-id/frames/";
        var expiry = TimeSpan.FromHours(1);
        var expectedUrl = "https://s3.amazonaws.com/my-frames-bucket/frames?presigned=token";

        _s3ClientMock.Setup(s => s.GetPreSignedURL(It.IsAny<GetPreSignedUrlRequest>()))
            .Returns(expectedUrl);

        // Act
        var result = _service.GenerateGetPresignedUrl(bucketName, key, expiry);

        // Assert
        result.Should().Be(expectedUrl);
        _s3ClientMock.Verify(s => s.GetPreSignedURL(It.Is<GetPreSignedUrlRequest>(r =>
            r.BucketName == bucketName &&
            r.Key == key &&
            r.Verb == HttpVerb.GET
        )), Times.Once);
    }

    [Fact]
    public void GenerateGetPresignedUrl_WhenBucketEmpty_ShouldReturnNull()
    {
        // Act
        var result = _service.GenerateGetPresignedUrl("", "some-key", TimeSpan.FromMinutes(15));

        // Assert
        result.Should().BeNull();
        _s3ClientMock.Verify(s => s.GetPreSignedURL(It.IsAny<GetPreSignedUrlRequest>()), Times.Never);
    }

    [Fact]
    public void GenerateGetPresignedUrl_WhenBucketWhitespace_ShouldReturnNull()
    {
        // Act
        var result = _service.GenerateGetPresignedUrl("   ", "some-key", TimeSpan.FromMinutes(15));

        // Assert
        result.Should().BeNull();
        _s3ClientMock.Verify(s => s.GetPreSignedURL(It.IsAny<GetPreSignedUrlRequest>()), Times.Never);
    }

    [Fact]
    public void GenerateGetPresignedUrl_WhenKeyEmpty_ShouldReturnNull()
    {
        // Act
        var result = _service.GenerateGetPresignedUrl("my-bucket", "", TimeSpan.FromMinutes(15));

        // Assert
        result.Should().BeNull();
        _s3ClientMock.Verify(s => s.GetPreSignedURL(It.IsAny<GetPreSignedUrlRequest>()), Times.Never);
    }
}
