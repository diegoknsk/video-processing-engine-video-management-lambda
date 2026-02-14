---
name: external-api-refit
description: Guia para integração com APIs externas usando Refit — type-safe, resilience patterns, HTTP/3 e tratamento de erros. Use quando a tarefa envolver APIs externas, HttpClient, Refit, consumir APIs ou integração com serviços externos.
---

# External API Integration — Refit

## Quando Usar

- Integração com **APIs externas**
- **Refit**, **HttpClient**, consumir APIs REST
- **Resilience patterns** (retry, circuit breaker, timeout)
- Palavras-chave: "API externa", "Refit", "consumir API", "integração", "webhook"

## Princípios Essenciais

### ✅ Fazer

- Usar **Refit** para APIs externas (type-safe, menos boilerplate)
- Adicionar **resilience** (retry, circuit breaker, timeout) via `AddStandardResilienceHandler()`
- Passar **CancellationToken** para todas as chamadas assíncronas
- Mapear **ApiException** para exceções de domínio (não expor na camada Application)
- Logar chamadas externas (início, sucesso, erro com status code)
- Usar **records** para request/response (imutáveis)

### ❌ Não Fazer

- **Nunca** usar HttpClient manualmente (preferir Refit)
- **Nunca** hardcoded URLs (usar IOptions + appsettings)
- **Nunca** expor `ApiException` para UseCases (mapear para exceções de domínio)
- **Nunca** esquecer timeout configurado
- **Nunca** omitir logging de chamadas externas

**Regra de ouro:** Refit + Resilience + Logging + Mapeamento de erros.

## Checklist Rápido

1. Criar interface Refit com atributos HTTP: `[Get("/endpoint")]`, `[Post]`, `[Header]`
2. Criar contracts (request/response) como records com `JsonPropertyName`
3. Registrar com `AddRefitClient<T>().AddStandardResilienceHandler()`
4. Configurar BaseUrl e Timeout via IOptions (appsettings)
5. Criar service wrapper (implementa Port) que chama interface Refit e mapeia erros
6. Registrar wrapper no DI: `AddScoped<IPort, ServiceWrapper>()`
7. Testar com mock da interface Refit

## Exemplo Mínimo

**Cenário:** Consumir API externa de autenticação (obter token)

### Interface Refit

```csharp
using Refit;

public interface IExternalAuthApi
{
    [Post("/token")]
    Task<TokenResponse> GetTokenAsync(
        [Header("X-Api-Key")] string apiKey,
        [Header("X-Api-Secret")] string apiSecret,
        CancellationToken ct = default);

    [Get("/users/{id}")]
    Task<UserResponse> GetUserAsync(
        string id,
        [Header("Authorization")] string bearerToken,
        CancellationToken ct = default);
}
```

### Contracts

```csharp
public record TokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("expires_in")] int ExpiresIn
);

public record UserResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name
);
```

### Service Wrapper (Implementa Port)

```csharp
using Application.Ports;
using Refit;

public class ExternalAuthService(IExternalAuthApi api, ILogger<ExternalAuthService> logger) 
    : IExternalAuthService
{
    public async Task<string> GetTokenAsync(string apiKey, string apiSecret, CancellationToken ct = default)
    {
        try
        {
            logger.LogInformation("Requesting token from external auth API");
            var response = await api.GetTokenAsync(apiKey, apiSecret, ct);
            logger.LogInformation("Token obtained, expires in {ExpiresIn}s", response.ExpiresIn);
            return response.AccessToken;
        }
        catch (ApiException ex)
        {
            logger.LogError(ex, "Failed to get token: {StatusCode}", ex.StatusCode);
            
            // Mapear para exceção de domínio
            throw ex.StatusCode switch
            {
                HttpStatusCode.Unauthorized => new UnauthorizedAccessException("Invalid API credentials"),
                HttpStatusCode.TooManyRequests => new InvalidOperationException("Rate limit exceeded"),
                _ => new InvalidOperationException($"External API error: {ex.StatusCode}")
            };
        }
    }
}
```

### Configuração (Program.cs)

```csharp
using Microsoft.Extensions.Http.Resilience;

// Options
builder.Services.Configure<ExternalAuthApiOptions>(
    builder.Configuration.GetSection("ExternalAuthApi"));

// Refit com Resilience (.NET 8+)
builder.Services.AddRefitClient<IExternalAuthApi>()
    .ConfigureHttpClient((sp, client) =>
    {
        var options = sp.GetRequiredService<IOptions<ExternalAuthApiOptions>>().Value;
        client.BaseAddress = new Uri(options.BaseUrl);
        client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
    })
    .AddStandardResilienceHandler(options =>
    {
        options.Retry.MaxRetryAttempts = 3;
        options.Retry.BackoffType = Polly.DelayBackoffType.Exponential;
        options.CircuitBreaker.FailureRatio = 0.5;
        options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(30);
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
    });

// Registrar wrapper
builder.Services.AddScoped<IExternalAuthService, ExternalAuthService>();
```

### appsettings.json

```json
{
  "ExternalAuthApi": {
    "BaseUrl": "",
    "TimeoutSeconds": 30
  }
}
```

```csharp
public class ExternalAuthApiOptions
{
    public required string BaseUrl { get; init; }
    public int TimeoutSeconds { get; init; } = 30;
}
```

**Pontos-chave:**
- **Interface Refit** define contrato HTTP (atributos `[Post]`, `[Header]`)
- **Resilience handler** adiciona retry, circuit breaker, timeout automaticamente
- **Wrapper service** mapeia `ApiException` para exceções de domínio
- **Estrutura:** `Infra/ExternalApis/<NomeApi>/I<Nome>Api.cs` + `<Nome>Service.cs` + `Contracts/`

## Autenticação Comum

```csharp
// Bearer Token
[Get("/users/{id}")]
[Headers("Authorization: Bearer")]
Task<UserResponse> GetUserAsync(string id, [Authorize] string token, CancellationToken ct = default);

// API Key
[Get("/data")]
Task<DataResponse> GetDataAsync([Header("X-Api-Key")] string apiKey, CancellationToken ct = default);

// OAuth2 com DelegatingHandler (auto-refresh token)
public class OAuth2Handler(ITokenService tokenService) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var token = await tokenService.GetAccessTokenAsync(ct);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await base.SendAsync(request, ct);
    }
}

// Registro
builder.Services.AddTransient<OAuth2Handler>();
builder.Services.AddRefitClient<IProtectedApi>()
    .AddHttpMessageHandler<OAuth2Handler>()
    .AddStandardResilienceHandler();
```

## Estrutura de Pastas

```
<Projeto>.Infra/
  ExternalApis/
    <NomeApi>/
      Contracts/
        TokenRequest.cs
        TokenResponse.cs
      I<Nome>Api.cs              (interface Refit)
      <Nome>Service.cs           (wrapper que implementa Port)
```

## Testes

```csharp
using Moq;
using Refit;

public class ExternalAuthServiceTests
{
    private readonly Mock<IExternalAuthApi> _apiMock = new();
    private readonly Mock<ILogger<ExternalAuthService>> _loggerMock = new();
    private readonly ExternalAuthService _sut;

    public ExternalAuthServiceTests()
    {
        _sut = new ExternalAuthService(_apiMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetTokenAsync_WhenSuccessful_ReturnsAccessToken()
    {
        // Arrange
        var response = new TokenResponse("test-token-123", 3600);
        _apiMock.Setup(x => x.GetTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _sut.GetTokenAsync("key", "secret");

        // Assert
        result.Should().Be("test-token-123");
    }
}
```

## Referências

- [Refit Documentation](https://github.com/reactiveui/refit)
- [Microsoft Resilience HTTP](https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience)
- [Polly Documentation](https://www.pollydocs.org/)
