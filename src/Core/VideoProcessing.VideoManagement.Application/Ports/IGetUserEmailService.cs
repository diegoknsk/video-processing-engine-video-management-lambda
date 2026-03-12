namespace VideoProcessing.VideoManagement.Application.Ports;

/// <summary>
/// Port para obter o email do usuário a partir do identificador (ex.: sub do Cognito).
/// Requer permissão IAM cognito-idp:AdminGetUser quando a implementação usa Admin GetUser.
/// </summary>
public interface IGetUserEmailService
{
    /// <summary>
    /// Obtém o email do usuário pelo identificador (ex.: sub).
    /// Retorna null quando o usuário não for encontrado ou não tiver email.
    /// </summary>
    Task<string?> GetEmailByUserIdAsync(string userId, CancellationToken cancellationToken = default);
}
