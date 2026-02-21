---
name: api-response-standardization
description: Padroniza respostas de sucesso e erro da API em formato único (success, data/error, timestamp). Use quando padronizar respostas HTTP, criar envelope de API, formatar erros globais, ApiResponse, ApiErrorResponse, filtros de resposta ou middleware de exceção.
---

# API Response Standardization

## Quando Usar

- Padronizar **respostas de sucesso e erro** da API em um formato único
- Implementar **envelope** de resposta (`success`, `data`/`error`, `timestamp`)
- Criar **filtro global** para encapsular 200/201 automaticamente
- Tratar **exceções** globalmente e retornar erros em formato consistente
- Palavras-chave: "padronizar resposta", "envelope API", "formato de erro", "ApiResponse", "GlobalException"

## Contrato JSON

### Sucesso (200/201)

```json
{
    "success": true,
    "data": { ... },
    "timestamp": "2026-02-21T20:14:35.487525Z"
}
```

- `success`: sempre `true`
- `data`: payload retornado pelo endpoint (objeto ou array)
- `timestamp`: ISO 8601 UTC

### Erro (4xx/5xx)

```json
{
    "success": false,
    "error": {
        "code": "Unauthorized",
        "message": "Acesso não autorizado."
    },
    "timestamp": "2026-02-21T20:14:26.9071028Z"
}
```

- `success`: sempre `false`
- `error.code`: identificador do erro (ex.: Unauthorized, InvalidCredentials, BadRequest)
- `error.message`: mensagem descritiva para o cliente
- `timestamp`: ISO 8601 UTC

## Implementação

### 1. Modelos (Api/Models)

**ApiResponse.cs** — resposta de sucesso:

```csharp
namespace YourApi.Models;

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
```

**ApiErrorResponse.cs** — resposta de erro:

```csharp
namespace YourApi.Models;

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

public class ErrorDetail
{
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
```

### 2. Filtro de sucesso (Api/Filters)

**ApiResponseFilter.cs** — encapsula automaticamente 200/201 em `ApiResponse<T>`:

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using YourApi.Models;

public class ApiResponseFilter : IActionFilter
{
    private readonly ILogger<ApiResponseFilter> _logger;

    public ApiResponseFilter(ILogger<ApiResponseFilter> logger) => _logger = logger;

    public void OnActionExecuting(ActionExecutingContext context) { }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Result is OkObjectResult okResult)
        {
            context.Result = new ObjectResult(ApiResponse<object>.CreateSuccess(okResult.Value!))
                { StatusCode = StatusCodes.Status200OK };
            return;
        }
        if (context.Result is ObjectResult objectResult &&
            (objectResult.StatusCode == StatusCodes.Status200OK || objectResult.StatusCode == StatusCodes.Status201Created))
        {
            context.Result = new ObjectResult(ApiResponse<object>.CreateSuccess(objectResult.Value!))
                { StatusCode = objectResult.StatusCode };
        }
    }
}
```

Controllers continuam retornando `Ok(model)` ou `CreatedAtAction(..., model)`; o filtro envolve em `{ "success": true, "data": model, "timestamp": "..." }`.

### 3. Middleware de exceção (Api/Middleware)

**GlobalExceptionMiddleware.cs** — captura exceções, mapeia para (statusCode, code, message) e retorna `ApiErrorResponse`:

```csharp
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using YourApi.Models;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {ExceptionType}", ex.GetType().Name);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, code, message) = MapException(exception);
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var errorResponse = ApiErrorResponse.Create(code, message);
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, options));
    }

    private static (int statusCode, string code, string message) MapException(Exception exception)
    {
        return exception switch
        {
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized", "Acesso não autorizado."),
            ArgumentException => (StatusCodes.Status400BadRequest, "BadRequest", "Requisição inválida."),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "NotFound", "Recurso não encontrado."),
            _ => (StatusCodes.Status500InternalServerError, "InternalServerError", "Erro interno do servidor.")
        };
    }
}
```

**Importante:** estender `MapException` com os tipos de exceção do domínio ou de SDKs (ex.: Cognito, EF Core). Sempre retornar tripla `(statusCode, code, message)` e usar `ApiErrorResponse.Create(code, message)`.

### 4. Registro (Program.cs)

```csharp
// Filtro global de resposta (sucesso)
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiResponseFilter>();
});

// JSON: camelCase e timestamp consistente
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Middleware de exceção (deve estar cedo no pipeline)
var app = builder.Build();
app.UseMiddleware<GlobalExceptionMiddleware>();
// ... UseRouting, UseAuthorization, MapControllers
```

## Princípios

- **Sucesso:** controllers retornam apenas o payload; o filtro adiciona `success`, `data` e `timestamp`.
- **Erro:** exceções são tratadas no middleware; nunca retornar corpo de erro manualmente nos controllers para erros genéricos (401, 404, 500).
- **Códigos de erro:** usar códigos estáveis (ex.: `Unauthorized`, `InvalidCredentials`) para que clientes possam tratar por `error.code`; `message` pode ser localizada ou mais amigável.
- **Timestamp:** sempre UTC (ISO 8601) para auditoria e debugging.

## Exclusões opcionais

- Endpoints como `/health` que precisam de corpo específico: no filtro, não encapsular quando `context.ActionDescriptor.RouteValues["action"] == "Health"` (ou excluir por atributo/controller).
- Respostas que já são `ProblemDetails` ou outro contrato: o filtro só atua em `OkObjectResult` e `ObjectResult` com 200/201; demais resultados não são alterados.

## Checklist de adoção em novo projeto

1. Criar `ApiResponse<T>`, `ApiErrorResponse` e `ErrorDetail` em `Api/Models`.
2. Criar `ApiResponseFilter` em `Api/Filters` e registrar em `AddControllers(options => options.Filters.Add<ApiResponseFilter>())`.
3. Criar `GlobalExceptionMiddleware` em `Api/Middleware`, implementar `MapException` com exceções do projeto e registrar com `app.UseMiddleware<GlobalExceptionMiddleware>()`.
4. Garantir que JSON use `PropertyNamingPolicy.CamelCase` para que o contrato saia como `success`, `data`, `error`, `timestamp`, `code`, `message`.
