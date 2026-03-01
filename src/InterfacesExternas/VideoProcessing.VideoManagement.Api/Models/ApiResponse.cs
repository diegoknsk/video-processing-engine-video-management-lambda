namespace VideoProcessing.VideoManagement.Api.Models;

/// <summary>
/// Envelope de resposta de sucesso da API (success, data, timestamp).
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; init; } = true;
    public T? Data { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public static ApiResponse<T> CreateSuccess(T data) => new()
    {
        Success = true,
        Data = data,
        Timestamp = DateTime.UtcNow
    };
}
