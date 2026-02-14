# Configuração de Base Path para API Gateway

## Visão Geral

Quando a Lambda de autenticação é exposta via **AWS API Gateway**, o base path da API pode incluir um stage (ex.: `/prod`, `/dev`, `/staging`). Isso impacta a especificação OpenAPI gerada e, consequentemente, os clientes gerados automaticamente (ex.: Kiota).

## Impacto no OpenAPI

A especificação OpenAPI precisa refletir o base path correto para que:

1. **Clientes gerados** (ex.: Kiota) usem a URL correta nas requisições
2. **Documentação interativa** (Scalar UI) mostre os endpoints corretos
3. **Testes manuais** via UI funcionem corretamente

## Configuração

### Opção 1: Configuração via Variável de Ambiente (Recomendado)

No arquivo `Program.cs`, configure o base path dinamicamente via variável de ambiente:

```csharp
builder.Services.AddSwaggerGen(options =>
{
    // ... outras configurações ...
    
    // Configurar base path via variável de ambiente
    var baseUrl = builder.Configuration["API_BASE_URL"] ?? "http://localhost:5000";
    options.AddServer(new OpenApiServer 
    { 
        Url = baseUrl,
        Description = "API Gateway endpoint"
    });
});
```

**Variáveis de ambiente sugeridas:**
- `API_BASE_URL=https://api.example.com/prod` (produção)
- `API_BASE_URL=https://api.example.com/dev` (desenvolvimento)
- `API_BASE_URL=http://localhost:5000` (local)

### Opção 2: Múltiplos Stages

Para documentar múltiplos stages simultaneamente:

```csharp
builder.Services.AddSwaggerGen(options =>
{
    // ... outras configurações ...
    
    options.AddServer(new OpenApiServer 
    { 
        Url = "https://api.example.com/prod",
        Description = "Production (API Gateway stage: prod)"
    });
    options.AddServer(new OpenApiServer 
    { 
        Url = "https://api.example.com/dev",
        Description = "Development (API Gateway stage: dev)"
    });
    options.AddServer(new OpenApiServer 
    { 
        Url = "http://localhost:5000",
        Description = "Local development"
    });
});
```

### Opção 3: Configuração Condicional por Ambiente

```csharp
builder.Services.AddSwaggerGen(options =>
{
    // ... outras configurações ...
    
    var environment = builder.Environment.EnvironmentName;
    var baseUrl = environment switch
    {
        "Production" => "https://api.example.com/prod",
        "Development" => "https://api.example.com/dev",
        _ => "http://localhost:5000"
    };
    
    options.AddServer(new OpenApiServer 
    { 
        Url = baseUrl,
        Description = $"API Gateway endpoint ({environment})"
    });
});
```

## Verificação

Após configurar, verifique que o JSON OpenAPI contém o campo `servers`:

```json
{
  "openapi": "3.0.1",
  "info": { ... },
  "servers": [
    {
      "url": "https://api.example.com/prod",
      "description": "Production (API Gateway stage: prod)"
    }
  ],
  "paths": { ... }
}
```

## Ajuste Manual em Clientes

Se a configuração de `servers` não for feita antes da geração do cliente:

1. **Kiota**: O cliente gerado pode precisar de ajuste manual do `BaseAddress`:
   ```csharp
   var client = new VideoProcessingAuthClient(
       new HttpClient 
       { 
           BaseAddress = new Uri("https://api.example.com/prod") 
       }
   );
   ```

2. **Outros geradores**: Consulte a documentação específica do gerador para ajustar o base URL.

## Notas Importantes

- **Desenvolvimento Local**: Quando rodando localmente (sem API Gateway), o base path pode ser omitido ou configurado como `http://localhost:5000`
- **Deploy**: Certifique-se de que a variável de ambiente `API_BASE_URL` está configurada corretamente no ambiente de deploy (Lambda, container, etc.)
- **Versionamento**: Se a API tiver versionamento no path (ex.: `/v1`), inclua no `Url` do `OpenApiServer`

## Exemplo Completo

Veja o código comentado no `Program.cs` para referência de implementação completa.
