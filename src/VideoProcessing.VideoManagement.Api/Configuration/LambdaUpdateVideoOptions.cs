namespace VideoProcessing.VideoManagement.Api.Configuration;

/// <summary>
/// Configuração para invocação da Lambda Update Video (proxy PATCH).
/// </summary>
public class LambdaUpdateVideoOptions
{
    /// <summary>Nome da função Lambda a ser invocada.</summary>
    public string FunctionName { get; set; } = string.Empty;
}
