namespace VideoProcessing.VideoManagement.Api.Models;

/// <summary>
/// Envelope de resposta de erro da API (success: false, error.code, error.message, timestamp).
/// </summary>
public class ApiErrorResponse
{
    public bool Success { get; init; } = false;
    public ErrorDetail Error { get; init; } = null!;
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public static ApiErrorResponse Create(string code, string message) => new()
    {
        Success = false,
        Error = new ErrorDetail { Code = code, Message = message },
        Timestamp = DateTime.UtcNow
    };
}

/// <summary>
/// Detalhe do erro (code e message) dentro do envelope de erro.
/// </summary>
public class ErrorDetail
{
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
