using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VideoProcessing.VideoManagement.Application.Ports;
using VideoProcessing.VideoManagement.Infra.CrossCutting.Configuration;

namespace VideoProcessing.VideoManagement.Infra.Data.Services;

/// <summary>
/// Obtém o email do usuário via Amazon Cognito (ListUsers com filtro por sub).
/// Requer permissão IAM cognito-idp:ListUsers no User Pool.
/// Em caso de falha ou usuário sem email, retorna null e registra log de aviso.
/// </summary>
public class CognitoUserEmailService(
    IAmazonCognitoIdentityProvider cognito,
    IOptions<CognitoOptions> options,
    ILogger<CognitoUserEmailService> logger) : IGetUserEmailService
{
    public async Task<string?> GetEmailByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return null;

        var userPoolId = options.Value.UserPoolId;
        if (string.IsNullOrWhiteSpace(userPoolId))
        {
            logger.LogWarning("Cognito UserPoolId not configured; skipping user email lookup.");
            return null;
        }

        try
        {
            var request = new ListUsersRequest
            {
                UserPoolId = userPoolId,
                Filter = $"sub = \"{userId}\"",
                Limit = 1
            };

            var response = await cognito.ListUsersAsync(request, cancellationToken);
            var user = response.Users.FirstOrDefault();
            if (user?.Attributes == null)
                return null;

            var emailAttr = user.Attributes.FirstOrDefault(a => string.Equals(a.Name, "email", StringComparison.OrdinalIgnoreCase));
            return emailAttr?.Value;
        }
        catch (NotAuthorizedException ex)
        {
            logger.LogWarning(ex, "Cognito ListUsers not authorized for UserPool {UserPoolId}. Ensure IAM cognito-idp:ListUsers.", userPoolId);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to get user email from Cognito for userId {UserId}. Returning null.", userId);
            return null;
        }
    }
}
