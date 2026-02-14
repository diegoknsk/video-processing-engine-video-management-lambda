# Lambda Handler com AddAWSLambdaHosting — Investigação e Referência

Este documento registra os erros encontrados ao testar a Lambda após o deploy e serve como referência para configurar corretamente o **Handler** quando a API usa **`AddAWSLambdaHosting`** (modelo minimal API / Hosting), sem classe `LambdaEntryPoint`.

---

## 1. Contexto do projeto

- A API usa **`Amazon.Lambda.AspNetCoreServer.Hosting`** e **`AddAWSLambdaHosting(LambdaEventSource.HttpApi)`** no `Program.cs`.
- Não existe classe `LambdaEntryPoint` no projeto; a aplicação é tratada como **minimal API / WebApplication** rodando no Lambda via Hosting.
- O deploy é feito via **GitHub Actions**: `dotnet publish` → zip → `aws lambda update-function-code`. O Handler da função é definido na AWS (Console ou IaC), não pelo workflow.

---

## 2. Erros observados

### 2.1 Primeiro erro: assembly "Lambda" não encontrado

```
Amazon.Lambda.RuntimeSupport.ExceptionHandling.LambdaValidationException: 
Could not find the specified handler assembly with the file name 
'Lambda, Culture=neutral, PublicKeyToken=null'. 
The assembly should be located in the root of your uploaded .zip file.
```

**Causa:** O Handler configurado na função Lambda (Console ou IaC) referenciava um assembly chamado **"Lambda"** (ex.: `Lambda::Lambda.Function::FunctionHandler`). No pacote (.zip) gerado pelo publish, o assembly da aplicação é **`VideoProcessing.Auth.Api.dll`**, não `Lambda.dll`. O runtime procura o assembly pelo nome indicado no Handler e não encontra.

**Conclusão:** O Handler na AWS deve referenciar um assembly que **exista no .zip** (no nosso caso, `VideoProcessing.Auth.Api`).

---

### 2.2 Segundo erro: tipo LambdaEntryPoint não encontrado

Após alterar o Handler para:

```
VideoProcessing.Auth.Api::VideoProcessing.Auth.Api.LambdaEntryPoint::FunctionHandlerAsync
```

ocorreu:

```
Amazon.Lambda.RuntimeSupport.ExceptionHandling.LambdaValidationException: 
Unable to load type 'VideoProcessing.Auth.Api.LambdaEntryPoint' 
from assembly 'VideoProcessing.Auth.Api'.
```

**Causa:** Esse formato de Handler (`Assembly::Namespace.Classe::Método`) exige uma **classe de entrada** no projeto (ex.: `LambdaEntryPoint` herdando de `APIGatewayHttpApiV2ProxyFunction`). O projeto atual **não** usa esse modelo; usa apenas `AddAWSLambdaHosting`, sem `LambdaEntryPoint`. Por isso o tipo não existe no assembly.

**Conclusão:** Com **AddAWSLambdaHosting** não devemos usar Handler no formato `Assembly::LambdaEntryPoint::FunctionHandlerAsync` a menos que se adicione explicitamente uma classe `LambdaEntryPoint` (o que não é o desejado aqui).

### 2.3 Terceiro erro: Sandbox.Timedout (Task timed out after 3.00 seconds)

```
"errorType": "Sandbox.Timedout",
"errorMessage": "RequestId: ... Error: Task timed out after 3.00 seconds"
```

**Causa:** O **timeout** da função Lambda está com o valor padrão da AWS (**3 segundos**). Uma Lambda .NET com ASP.NET Core Hosting precisa de tempo para **cold start** (inicialização do runtime, carregamento do assembly, startup da aplicação), que costuma levar **5–15+ segundos** na primeira invocação. Com timeout de 3 segundos, a função é interrompida antes de responder.

**Solução:** Aumentar o **Timeout** da função para **pelo menos 30 segundos** (recomendado 30–60 s para APIs). Configurar na IaC (Terraform, CloudFormation, etc.) ou no Console AWS (Configuration → General configuration → Edit → Timeout). Opcionalmente, aumentar **Memory** (ex.: 512 MB) melhora CPU e pode reduzir cold start.

---

## 3. O que precisa ser definido (para investigação)

Com **AddAWSLambdaHosting** e deploy via **.zip** no runtime gerenciado .NET (ex.: dotnet10), o Handler correto ainda precisa ser confirmado na prática. Possíveis cenários:

1. **Handler = apenas o nome do assembly**  
   Alguma documentação e exemplos (ex.: Clear Measure, .NET 6 minimal API) indicam que, nesse modelo, o Handler pode ser só o nome do assembly, ex.:  
   `VideoProcessing.Auth.Api`  
   (sem `::Classe::Método`). O runtime trataria a aplicação como “entry point” do assembly. **A confirmar** para o runtime .NET 10 e para o conteúdo exato do zip (arquivos de bootstrap, etc.).

2. **Handler no assembly do pacote Hosting**  
   O entry point real pode estar no pacote **Amazon.Lambda.AspNetCoreServer.Hosting** (outro assembly no .zip). Nesse caso, o Handler poderia ser algo como:  
   `Amazon.Lambda.AspNetCoreServer.Hosting::...::FunctionHandlerAsync`  
   (namespace/classe exatos a conferir na documentação ou no código-fonte do repositório aws-lambda-dotnet).

3. **Ferramentas oficiais (SAM / CDK / dotnet lambda deploy)**  
   Ao usar `dotnet lambda deploy-function` ou modelos SAM/CDK para ASP.NET Core minimal API, o Handler costuma ser definido automaticamente. Quem faz apenas **update-function-code** (como no workflow atual) precisa replicar o mesmo valor de Handler que essas ferramentas usariam, ou documentá-lo no IaC.

**Recomendação:** Testar na função Lambda (por exemplo na Console), na ordem:

- Handler = `VideoProcessing.Auth.Api` (só o nome do assembly).
- Se falhar, consultar a documentação e o repositório **aws/aws-lambda-dotnet** (em especial **Amazon.Lambda.AspNetCoreServer.Hosting**) para o formato exato de Handler quando se usa apenas Hosting, sem `LambdaEntryPoint`.

**Confirmado:** Handler = `VideoProcessing.Auth.Api` funciona para este projeto (sem `LambdaEntryPoint`).

---

## 4. Teste da Lambda pelo Console (evento HTTP API v2)

Com **AddAWSLambdaHosting(LambdaEventSource.HttpApi)** o handler espera o formato de evento do **API Gateway HTTP API (v2)**. O template **Hello World** do console envia um JSON simples e causa `NullReferenceException` em `MarshallRequest`. Para testar direto pelo console da Lambda (por exemplo para chegar na rota `/health`), use um evento no formato correto.

### 4.1 Passos no Console AWS

1. Abra a função Lambda no **Console da AWS** → aba **Test**.
2. **Create new event** ou **Edit saved event**.
3. **Template:** deixe em branco ou use um template de API Gateway se existir — **não use "Hello World"**.
4. Em **Event JSON**, cole o JSON da seção abaixo (ex.: `GET /health`).
5. **Save** (opcional) e **Test**.

### 4.2 Evento de exemplo — GET /health

Use este payload para simular uma requisição **GET /health**:

```json
{
  "version": "2.0",
  "routeKey": "GET /health",
  "rawPath": "/health",
  "rawQueryString": "",
  "requestContext": {
    "accountId": "123456789012",
    "apiId": "test-api",
    "domainName": "test.execute-api.us-east-1.amazonaws.com",
    "http": {
      "method": "GET",
      "path": "/health",
      "protocol": "HTTP/1.1",
      "sourceIp": "127.0.0.1",
      "userAgent": "Test"
    },
    "requestId": "test-request-id",
    "routeKey": "GET /health",
    "stage": "$default"
  },
  "headers": {},
  "body": null,
  "isBase64Encoded": false
}
```

Para outras rotas, altere `routeKey`, `rawPath` e `requestContext.http.path` (ex.: `POST /auth/login`), e use `body` com o JSON do request quando for POST.

### 4.3 Teste via API Gateway

Se a Lambda estiver atrás de um **API Gateway HTTP API** (v2), as chamadas pelo API Gateway já enviam o evento nesse formato. Basta configurar a rota (ex.: `GET /health`) e a integração Lambda proxy; não é necessário evento de teste manual para esse caso.

---

## 5. Referências úteis

- [Deploy ASP.NET applications - AWS Lambda](https://docs.aws.amazon.com/lambda/latest/dg/csharp-package-asp.html) — minimal API e Web API.
- [Define Lambda function handler in C#](https://docs.aws.amazon.com/lambda/latest/dg/csharp-handler.html) — formato `Assembly::Namespace.Class::Method`.
- [aws/aws-lambda-dotnet – Amazon.Lambda.AspNetCoreServer.Hosting](https://github.com/aws/aws-lambda-dotnet/tree/master/Libraries/src/Amazon.Lambda.AspNetCoreServer.Hosting).
- Exemplo com “só assembly name”: [Hosting a .NET 6 Minimal API in AWS Lambda (Clear Measure)](https://clearmeasure.com/hosting-dot-net-6-minimal-api-aws-lambda) — Handler = nome do assembly (ex.: `LambdaAPI`).
- Documentação do projeto: `docs/deploy-github-actions.md`, `docs/processo-subida-deploy.md`.

---

## 6. Resumo para IaC / Checklist

Ao provisionar a função Lambda (Terraform, CloudFormation, etc.):

- **Runtime:** .NET 10 (C#/F#/PowerShell), conforme usado no publish.
- **Handler:** **`VideoProcessing.Auth.Api`** (apenas o nome do assembly). Confirmado para **AddAWSLambdaHosting** sem `LambdaEntryPoint`.
- **Timeout:** **mínimo 30 segundos** (recomendado 30–60 s). O padrão de 3 s causa `Sandbox.Timedout` no cold start (ver seção 2.3).
- **Memory:** recomendado 512 MB ou mais (melhora CPU e cold start).
- **Código:** O .zip deve ser o resultado de `dotnet publish` do projeto `VideoProcessing.Auth.Api` (linux-x64, mesmo que no workflow atual), com os arquivos na **raiz** do zip (incluindo `VideoProcessing.Auth.Api.dll`, deps, runtimeconfig, etc.).

**Nota sobre deploy via GitHub Actions:** O workflow de deploy (`.github/workflows/deploy-lambda.yml`) já executa um step **Update Lambda handler** que configura o Handler com `VideoProcessing.Auth.Api` em todo deploy. Em deploys via GitHub Actions não é necessário configurar o Handler manualmente no IaC ou no Console — a menos que se queira um override explícito.

Para testar pelo console da Lambda, use evento no formato **API Gateway HTTP API v2** (ver seção 4); não usar template "Hello World".

---

## 7. Prefixo de path e stage no API Gateway (GATEWAY_PATH_PREFIX e GATEWAY_STAGE)

Quando a Lambda está atrás de um API Gateway que expõe rotas com prefixo (ex.: `/auth/*`), o path que chega na Lambda inclui esse prefixo (ex.: `rawPath = "/auth/health"`). Para que a aplicação roteie corretamente sem alterar as rotas no código, use a variável de ambiente **`GATEWAY_PATH_PREFIX`** (ex.: `/auth`). Se não definida ou vazia, o path não é alterado (comportamento atual). A comparação do prefixo é **case-insensitive**.

Quando o API Gateway HTTP API usa um **stage nomeado** (não `$default`), o path que chega na Lambda inclui o stage (ex.: `rawPath = "/default/health"`). Nesse caso, defina **`GATEWAY_STAGE`** com o nome do stage (ex.: `default`) para que o middleware remova esse segmento e a rota `/health` seja encontrada. Se usar o stage `$default`, não defina `GATEWAY_STAGE`.

Detalhes em [gateway-path-prefix.md](gateway-path-prefix.md).
