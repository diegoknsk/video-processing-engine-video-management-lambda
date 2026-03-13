using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using VideoProcessing.VideoManagement.Infra.CrossCutting.Configuration;
using VideoProcessing.VideoManagement.Infra.Data.Services;
using Xunit;

namespace VideoProcessing.VideoManagement.UnitTests.Infra.Data.Services;

public class CognitoUserEmailServiceTests
{
    private readonly Mock<IAmazonCognitoIdentityProvider> _cognitoMock;
    private readonly CognitoOptions _options;
    private readonly Mock<ILogger<CognitoUserEmailService>> _loggerMock;
    private readonly CognitoUserEmailService _sut;

    public CognitoUserEmailServiceTests()
    {
        _cognitoMock = new Mock<IAmazonCognitoIdentityProvider>();
        _options = new CognitoOptions { UserPoolId = "us-east-1_AbCdEf", ClientId = "client", Region = "us-east-1" };
        _loggerMock = new Mock<ILogger<CognitoUserEmailService>>();
        _sut = new CognitoUserEmailService(_cognitoMock.Object, Options.Create(_options), _loggerMock.Object);
    }

    [Fact]
    public async Task GetEmailByUserIdAsync_WhenUserHasEmail_ShouldReturnEmail()
    {
        var userId = Guid.NewGuid().ToString();
        var email = "usuario@exemplo.com";
        _cognitoMock
            .Setup(c => c.ListUsersAsync(It.IsAny<ListUsersRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ListUsersResponse
            {
                Users =
                [
                    new UserType
                    {
                        Attributes =
                        [
                            new AttributeType { Name = "email", Value = email },
                            new AttributeType { Name = "sub", Value = userId }
                        ]
                    }
                ]
            });

        var result = await _sut.GetEmailByUserIdAsync(userId, CancellationToken.None);

        result.Should().Be(email);
    }

    [Fact]
    public async Task GetEmailByUserIdAsync_WhenUserHasNoEmailAttribute_ShouldReturnNull()
    {
        var userId = Guid.NewGuid().ToString();
        _cognitoMock
            .Setup(c => c.ListUsersAsync(It.IsAny<ListUsersRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ListUsersResponse
            {
                Users =
                [
                    new UserType { Attributes = [new AttributeType { Name = "sub", Value = userId }] }
                ]
            });

        var result = await _sut.GetEmailByUserIdAsync(userId, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetEmailByUserIdAsync_WhenListUsersReturnsEmpty_ShouldReturnNull()
    {
        var userId = Guid.NewGuid().ToString();
        _cognitoMock
            .Setup(c => c.ListUsersAsync(It.IsAny<ListUsersRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ListUsersResponse { Users = [] });

        var result = await _sut.GetEmailByUserIdAsync(userId, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetEmailByUserIdAsync_WhenUserIdIsNullOrWhiteSpace_ShouldReturnNull()
    {
        var resultEmpty = await _sut.GetEmailByUserIdAsync("", CancellationToken.None);
        var resultNull = await _sut.GetEmailByUserIdAsync("   ", CancellationToken.None);

        resultEmpty.Should().BeNull();
        resultNull.Should().BeNull();
        _cognitoMock.Verify(c => c.ListUsersAsync(It.IsAny<ListUsersRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetEmailByUserIdAsync_WhenCognitoThrows_ShouldReturnNull()
    {
        var userId = Guid.NewGuid().ToString();
        _cognitoMock
            .Setup(c => c.ListUsersAsync(It.IsAny<ListUsersRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotAuthorizedException("Not authorized"));

        var result = await _sut.GetEmailByUserIdAsync(userId, CancellationToken.None);

        result.Should().BeNull();
    }
}
