---
name: lambda-api-hosting
description: Guia para configurar API .NET 10 com AddAWSLambdaHosting, API Gateway (GATEWAY_STAGE, GATEWAY_PATH_PREFIX) e documentação OpenAPI (Scalar ou Swagger). Use quando criar nova Lambda API .NET, hospedar API no Lambda, configurar AddAWSLambdaHosting, API Gateway, GATEWAY_PATH_PREFIX, GATEWAY_STAGE ou documentação OpenAPI atrás do gateway.
---

# Lambda API Hosting e Gateway

Guia para configurar APIs .NET 10 hospedadas em AWS Lambda com **AddAWSLambdaHosting** e tratamento de **API Gateway** (stage, prefixo de path) e **OpenAPI** (Scalar ou Swagger UI).

---

## Quando Usar Esta Skill

- Criar nova Lambda com API .NET (endpoints)
- Configurar **AddAWSLambdaHosting** em projeto ASP.NET Core
- API atrás de **API Gateway HTTP API (v2)** com prefixo de path e/ou stage nomeado
- **GATEWAY_PATH_PREFIX**, **GATEWAY_STAGE** — variáveis e middleware de path
- Documentação **OpenAPI** (Scalar ou Swagger) quando API está atrás do gateway

**Não cobre:** Clean Architecture, camadas, UseCases/Controllers (ver `core-clean-architecture` e `core-dotnet`).

---

## Fluxo Inicial — Perguntar ao Usuário

Antes de aplicar tratamentos, perguntar:

1. **Usará documentação OpenAPI?** (Scalar e/ou Swagger UI)
2. **A API será exposta atrás de API Gateway** (HTTP API) com prefixo de path e/ou stage nomeado?

Só então configurar middleware de gateway, variáveis de ambiente e OpenAPI server (filter) conforme as respostas.

---

## Variáveis de Ambiente (Gateway)

| Variável | Obrigatória | Uso |
|----------|-------------|-----|
| `GATEWAY_PATH_PREFIX` | Não | Prefixo que o gateway adiciona antes da Lambda (ex.: `/auth`). Quando definida, o middleware define PathBase/Path para roteamento. Case-insensitive. |
| `GATEWAY_STAGE` | Não | Nome do stage do HTTP API quando **não** é `$default` (ex.: `default`, `dev`). O middleware remove esse segmento do path. Se usar `$default`, não definir. |
| `API_PUBLIC_BASE_URL` | Não | URL base pública (ex.: `https://xxx.../dev/auth`). Opcional: usado como fallback para o server do OpenAPI quando o doc é gerado sem contexto de request; na prática o filter a partir do request já resolve o "Try it". |

**Exemplo de path:** Request `rawPath = "/default/auth/health"` com `GATEWAY_STAGE=default` e `GATEWAY_PATH_PREFIX=/auth` → após middleware: PathBase=`/auth`, Path=`/health`.

---

## Bootstrap com AddAWSLambdaHosting

No `Program.cs`, no builder:

```csharp
using Amazon.Lambda.AspNetCoreServer.Hosting;

// ...
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);
```

- **LambdaEventSource.HttpApi** — evento no formato API Gateway HTTP API (v2).
- Rotas da aplicação devem ser definidas **sem** prefixo nem stage (ex.: `/health`, `/login`); o gateway adiciona externamente.

---

## Pipeline e Middleware de Gateway

Ordem **obrigatória**: middleware de path **antes** de `UseRouting()`, para que o roteamento use o path já reescrito (ver aspnetcore#49454).

```csharp
app.UseMiddleware<GatewayPathBaseMiddleware>();
app.UseRouting();
// ... outros middlewares (ex.: GlobalExceptionMiddleware, CORS, Authorization)
```

**Comportamento do middleware:**

1. **GATEWAY_STAGE** (se definida): remove o primeiro segmento do path quando coincidir com o stage (case-insensitive). Ex.: `/default/health` → Path = `/health`.
2. **GATEWAY_PATH_PREFIX** (se definida): se o path começar com o prefixo, define `Request.PathBase` = prefixo e `Request.Path` = restante. Ex.: `/auth/health` → PathBase=`/auth`, Path=`/health`. Case-insensitive.
3. Ordem de aplicação: **primeiro stage, depois prefix**. Variáveis não definidas ou vazias: path não é alterado.

Implementar um `GatewayPathBaseMiddleware` que lê `GATEWAY_STAGE` e `GATEWAY_PATH_PREFIX` do ambiente e ajusta `context.Request.Path` e `context.Request.PathBase` conforme acima.

---

## OpenAPI (Scalar ou Swagger) com Gateway

Quando o usuário usar **OpenAPI (Scalar ou Swagger UI)** e **API Gateway**:

1. **DocumentFilter (IDocumentFilter)** que preenche o **Server** do OpenAPI a partir do request: `Scheme + Host + segmento de stage (GATEWAY_STAGE) + PathBase`. O PathBase no request já reflete o prefixo (ex.: `/auth`); o stage foi removido pelo middleware, então o filter deve ler `GATEWAY_STAGE` e prepender ao path do server para URLs como `https://.../dev/auth/login`.
2. **Fallback:** se `API_PUBLIC_BASE_URL` estiver definida, adicionar em `AddSwaggerGen` com `options.AddServer(new OpenApiServer { Url = apiPublicBaseUrl, ... })` para quando o documento for gerado sem contexto de request.
3. Registrar o filter: `options.DocumentFilter<OpenApiServerFromRequestFilter>();` (e injetar `IHttpContextAccessor` no filter). Tanto Swagger UI quanto Scalar consomem o mesmo JSON OpenAPI; o "Try it" usará as URLs corretas.

---

## Handler, Timeout e Evento de Teste

- **Handler (AddAWSLambdaHosting sem LambdaEntryPoint):** na AWS use apenas o **nome do assembly** (ex.: `VideoProcessing.Auth.Api`, `MinhaApi`). Não usar formato `Assembly::Classe::Método` a menos que exista classe de entrada.
- **Timeout:** mínimo **30 segundos** (cold start .NET pode levar 5–15+ s). Padrão de 3 s causa `Sandbox.Timedout`.
- **Memória:** sugerir 512 MB ou mais (melhora cold start).
- **Teste no Console da Lambda:** com `LambdaEventSource.HttpApi`, o evento deve ser **API Gateway HTTP API v2**. Não usar template "Hello World". Exemplo para GET /health:

```json
{
  "version": "2.0",
  "routeKey": "GET /health",
  "rawPath": "/health",
  "rawQueryString": "",
  "requestContext": {
    "http": { "method": "GET", "path": "/health", "protocol": "HTTP/1.1" },
    "routeKey": "GET /health",
    "stage": "$default"
  },
  "headers": {},
  "body": null,
  "isBase64Encoded": false
}
```

---

## Checklist IaC (Terraform / CloudFormation / GitHub Actions)

- **Runtime:** .NET 10 (conforme publish).
- **Handler:** nome do assembly (ex.: `VideoProcessing.Auth.Api`).
- **Timeout:** ≥ 30 s.
- **Memory:** 512 MB ou mais.
- **Variáveis de ambiente:** `GATEWAY_PATH_PREFIX`, `GATEWAY_STAGE` (e opcionalmente `API_PUBLIC_BASE_URL`) quando a API estiver atrás do gateway.
- **Pacote:** .zip com saída de `dotnet publish` (arquivos na raiz do zip).

---

## Documentação do Projeto

Para detalhes e exemplos de código de referência (middleware, filter OpenAPI, exemplos de path), consulte no repositório:

- `docs/gateway-path-prefix.md` — variáveis, comportamento e exemplos de path.
- `docs/lambda-handler-addawslambdahosting.md` — Handler, timeout, evento v2 e resumo IaC.
