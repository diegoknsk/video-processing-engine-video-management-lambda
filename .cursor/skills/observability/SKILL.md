---
name: observability
description: Guia para observabilidade — logging estruturado, métricas, tracing distribuído, health checks e instrumentação. Use quando a tarefa envolver logging, métricas, OpenTelemetry, tracing, health checks ou monitoramento.
---

# Observability — Logging, Métricas e Health Checks

## Quando Usar

- **Logging estruturado**, **health checks**, diagnóstico
- **Métricas**, **tracing distribuído** (OpenTelemetry)
- Instrumentação, monitoramento
- Palavras-chave: "logging", "log", "health check", "métricas", "tracing", "observabilidade", "OpenTelemetry"

## Princípios Essenciais

### ✅ Fazer

- Usar **ILogger<T>** injetado via DI (built-in .NET)
- **Logging estruturado** com placeholders: `logger.LogInformation("User {UserId} created", userId)`
- Logar **início/fim** de operações críticas (APIs externas, DB, processamento pesado)
- **Health checks** para dependências (DB, APIs externas, cache)
- Níveis apropriados: Information (fluxo normal), Warning (recuperável), Error (falhas)
- **LoggerMessage Source Generators** para hot paths (alta performance)

### ❌ Não Fazer

- **Nunca** string interpolation em logs: `$"User {userId}"` ❌ (usar placeholders)
- **Nunca** logar dados sensíveis (senhas, tokens, CPF, cartão)
- **Nunca** logar em excesso (ruído dificulta diagnóstico)
- **Nunca** capturar exceções sem logar: `catch (Exception) { }`
- **Nunca** esquecer health checks para dependências críticas

**Regra de ouro:** Logging estruturado + health checks = 80% da observabilidade.

## Checklist Rápido

1. Injetar `ILogger<T>` via DI no construtor primário
2. Usar placeholders estruturados: `logger.LogInformation("Message {Property}", value)`
3. Logar início e fim de operações críticas (ex.: chamadas externas, queries pesadas)
4. Configurar níveis no `appsettings.json`: Information (prod), Debug (dev)
5. Adicionar health checks: `AddHealthChecks().AddDbContextCheck<AppDbContext>()`
6. Endpoint: `app.MapHealthChecks("/health")`
7. Considerar Serilog para logging avançado (sinks, enrichers)

## Exemplo Mínimo

**Cenário:** Logging estruturado em UseCase + Health Checks

### Logging em UseCase

```csharp
public class CreateUserUseCase(
    IUserRepository repository,
    ILogger<CreateUserUseCase> logger) : ICreateUserUseCase
{
    public async Task<CreateUserResponseModel> ExecuteAsync(CreateUserInput input, CancellationToken ct = default)
    {
        logger.LogInformation("Creating user with email {Email}", input.Email);

        try
        {
            if (await repository.ExistsAsync(input.Email, ct))
            {
                logger.LogWarning("Email {Email} already exists", input.Email);
                throw new InvalidOperationException("Email já está em uso.");
            }

            var user = await repository.CreateAsync(input, ct);
            logger.LogInformation("User {UserId} created successfully", user.Id);
            
            return CreateUserPresenter.Present(user);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create user with email {Email}", input.Email);
            throw;
        }
    }
}
```

### Health Checks

```csharp
// Program.cs
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("database")
    .AddUrlGroup(new Uri("https://external-api.com/health"), "external-api");

var app = builder.Build();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = new
        {
            Status = report.Status.ToString(),
            Checks = report.Entries.Select(e => new
            {
                Name = e.Key,
                Status = e.Value.Status.ToString(),
                Duration = e.Value.Duration.TotalMilliseconds,
                Error = e.Value.Exception?.Message
            }),
            TotalDuration = report.TotalDuration.TotalMilliseconds
        };
        await context.Response.WriteAsJsonAsync(result);
    }
});
```

### Configuração (appsettings.json)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  }
}
```

**Pontos-chave:**
- **Placeholders estruturados** (`{Email}`) permitem busca e análise
- **Health checks** verificam dependências (DB, APIs) automaticamente
- **Níveis apropriados:** Information (fluxo), Warning (recuperável), Error (falha)

## LoggerMessage Source Generators (Performance)

Para hot paths (alto volume), use source generators (evita boxing/alocações):

```csharp
public static partial class Log
{
    [LoggerMessage(EventId = 1001, Level = LogLevel.Information, Message = "Creating user with email {Email}")]
    public static partial void CreatingUser(ILogger logger, string email);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Information, Message = "User {UserId} created")]
    public static partial void UserCreated(ILogger logger, Guid userId);

    [LoggerMessage(EventId = 1003, Level = LogLevel.Error, Message = "Failed to create user {Email}")]
    public static partial void UserCreationFailed(ILogger logger, string email, Exception exception);
}

// Uso
Log.CreatingUser(logger, input.Email);
var user = await repository.CreateAsync(input, ct);
Log.UserCreated(logger, user.Id);
```

## Serilog (Logging Avançado — Opcional)

Para sinks avançados (arquivos, cloud, Elasticsearch):

```bash
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Console
```

```csharp
using Serilog;

// Program.cs
builder.Host.UseSerilog((context, config) =>
{
    config
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .WriteTo.Console()
        .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day);
});
```

## OpenTelemetry (Tracing/Métricas — Opcional)

Para sistemas distribuídos (tracing completo):

```bash
dotnet add package OpenTelemetry.Extensions.Hosting
dotnet add package OpenTelemetry.Instrumentation.AspNetCore
dotnet add package OpenTelemetry.Instrumentation.Http
```

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter());
```

## Níveis de Log

| Nível | Quando Usar |
|-------|-------------|
| **Trace** | Debug extremamente detalhado (raramente usado) |
| **Debug** | Informações de debug (apenas dev) |
| **Information** | Fluxo normal da aplicação (operações importantes) |
| **Warning** | Situações anormais mas recuperáveis (retry, fallback) |
| **Error** | Erros que impedem operação (exceções) |
| **Critical** | Falhas catastróficas (app crash, corrupção de dados) |

## Dados Sensíveis (Nunca Logar)

- ❌ Senhas, tokens, API keys
- ❌ Dados pessoais (CPF, RG, cartão de crédito)
- ❌ Informações financeiras completas
- ✅ Identificadores não-sensíveis (UserId, OrderId, Email parcial)

```csharp
// ❌ NÃO fazer
logger.LogInformation("User {Email} logged in with password {Password}", email, password);

// ✅ Fazer
logger.LogInformation("User {Email} logged in successfully", email);
```

## Referências

- [Logging in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging)
- [Health Checks](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)
- [Serilog Documentation](https://serilog.net/)
- [OpenTelemetry .NET](https://opentelemetry.io/docs/languages/net/)
