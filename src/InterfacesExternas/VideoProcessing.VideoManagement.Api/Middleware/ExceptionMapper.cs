using FluentValidation;

namespace VideoProcessing.VideoManagement.Api.Middleware;

/// <summary>
/// Mapeia tipos de exceção para (statusCode, code, message). Extensível para exceções de domínio/SDK.
/// </summary>
public static class ExceptionMapper
{
    public static (int statusCode, string code, string message) Map(Exception exception)
    {
        return exception switch
        {
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized", "Acesso não autorizado."),
            ArgumentException => (StatusCodes.Status400BadRequest, "BadRequest", "Requisição inválida."),
            ValidationException => (StatusCodes.Status400BadRequest, "BadRequest", exception.Message),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "NotFound", "Recurso não encontrado."),
            _ => (StatusCodes.Status500InternalServerError, "InternalServerError", "Erro interno do servidor.")
        };
    }
}
