# Prefixo de path do API Gateway (GATEWAY_PATH_PREFIX e GATEWAY_STAGE)

Quando a API Auth roda atrás de um **API Gateway** (HTTP API) que expõe as rotas sob um prefixo (ex.: `/auth`, `/dev/auth`), o path que chega na Lambda inclui esse prefixo (ex.: `rawPath = "/auth/health"`). Além disso, quando o stage do HTTP API **não** é `$default` (ex.: stage nomeado `default`), o API Gateway inclui o stage no path (ex.: `rawPath = "/default/health"`). A aplicação define rotas sem prefixo nem stage (ex.: `/health`, `/login`, `/users/create`), o que geraria 404.

O **middleware** `GatewayPathBaseMiddleware` remove o stage (quando configurado) e o prefixo configurado do path e define `PathBase` e `Path` para que o roteamento da aplicação funcione sem alterar as rotas no código.

## Variáveis de ambiente

| Nome | Obrigatória | Descrição |
|------|-------------|-----------|
| `GATEWAY_PATH_PREFIX` | Não | Prefixo de path que o gateway adiciona antes de encaminhar para a Lambda (ex.: `/auth`, `/autenticacao`). |
| `GATEWAY_STAGE` | Não | Nome do stage do API Gateway HTTP API quando **não** é `$default`. Quando o stage é nomeado (ex.: `default`), o path que chega na Lambda vem com o stage no início (ex.: `/default/health`). Defina com o nome do stage (ex.: `default`) para que o middleware remova esse segmento e a rota `/health` seja encontrada. Se usar o stage `$default`, não defina esta variável. |
| `API_PUBLIC_BASE_URL` | Não | **Opcional.** URL pública base da API (ex.: `https://xxx.../dev/auth`). Na prática não é necessária: o server do OpenAPI é preenchido a partir do próprio request (Scheme + Host + PathBase) quando o Scalar solicita o JSON, então o "Try it" já usa a URL correta. Use só se o documento for gerado sem contexto de request (casos edge). |

## Comportamento

1. **GATEWAY_STAGE** (opcional): se definida (ex.: `default`), e o path começar com `/{stage}/` ou for exatamente `/{stage}`, o middleware remove esse primeiro segmento. Ex.: path `/default/health` → Path = `/health`. A comparação é case-insensitive. Use quando o API Gateway HTTP API usar um stage **nomeado** (não `$default`), pois nesse caso o `rawPath` inclui o stage.
2. **GATEWAY_PATH_PREFIX** (opcional):
   - **Não definida ou vazia:** o path do request **não é alterado** por prefixo. Requisições como `GET /health` e `POST /login` funcionam diretamente. Ideal para execução local e testes sem gateway.
   - **Definida (ex.: `/auth`):** se o path da requisição **começar** com esse prefixo, o middleware define:
     - `Request.PathBase` = prefixo presente no request (preserva o casing da requisição)
     - `Request.Path` = restante do path (ex.: `/auth/health` → Path = `/health`)
     - A comparação é **case-insensitive**: `/auth/health`, `/Auth/health` e `/AUTH/health` são tratados da mesma forma.

A ordem de aplicação é: primeiro remoção do stage (GATEWAY_STAGE), depois remoção do prefixo (GATEWAY_PATH_PREFIX).

## Exemplos

| GATEWAY_STAGE | GATEWAY_PATH_PREFIX | Request path       | PathBase (após middleware) | Path (após middleware) |
|---------------|---------------------|--------------------|----------------------------|-------------------------|
| não definida  | não definida       | `/auth/health`     | (inalterado)               | (inalterado)            |
| não definida  | não definida       | `/health`          | (inalterado)               | (inalterado)            |
| não definida  | não definida       | `/default/health`  | (inalterado)               | (inalterado)            |
| `default`     | não definida       | `/default/health`  | (inalterado)               | `/health`               |
| `default`     | `/auth`            | `/default/auth/health` | `/auth`              | `/health`               |
| não definida  | `/auth`            | `/auth/health`     | `/auth`                    | `/health`               |
| não definida  | `/auth`            | `/Auth/login`      | `/Auth`                    | `/login`                |
| não definida  | `/auth`            | `/AUTH/users/create` | `/AUTH`                | `/users/create`         |
| não definida  | `/auth`            | `/other/health`    | (inalterado)               | (inalterado)            |

## Uso no deploy (Lambda + API Gateway)

1. No **API Gateway**, configure a rota que encaminha para a Lambda (ex.: `ANY /auth/{proxy+}` ou `GET /auth/health`, `POST /auth/login`, etc.).
2. Na **Lambda**, defina as variáveis de ambiente conforme o caso:
   - Se o HTTP API usar um **stage nomeado** (ex.: `default`) e não `$default`, defina **`GATEWAY_STAGE`** com o nome do stage (ex.: `default`). Assim, paths como `/default/health` passam a ser roteados como `/health`.
   - Se as rotas do gateway tiverem um prefixo (ex.: `/auth`), defina **`GATEWAY_PATH_PREFIX=/auth`** (ou o valor exato do prefixo que o gateway usa no path enviado à Lambda).
   - Para que o **Scalar UI** (documentação interativa) monte as URLs corretas ao usar "Try it" quando acessado pelo gateway, defina **`API_PUBLIC_BASE_URL`** com a URL base pública completa, incluindo stage e prefixo (ex.: `https://h42x24ov55.execute-api.us-east-1.amazonaws.com/dev/auth`).
3. A aplicação passa a receber o path já “sem” stage e sem prefixo para roteamento: `/health`, `/login`, `/users/create`.

Em **Terraform** (exemplo):

```hcl
resource "aws_lambda_function" "auth_api" {
  # ...
  environment {
    variables = {
      GATEWAY_PATH_PREFIX = "/auth"
      # URL base pública para o Scalar UI "Try it" (stage + prefixo):
      # API_PUBLIC_BASE_URL = "https://xxx.execute-api.us-east-1.amazonaws.com/dev/auth"
      # Se o API Gateway HTTP API usar stage nomeado (não $default):
      # GATEWAY_STAGE = "default"
    }
  }
}
```

Em **CloudFormation** ou **Console da AWS**: adicione as variáveis de ambiente na configuração da função (`GATEWAY_PATH_PREFIX`, `API_PUBLIC_BASE_URL` para o Scalar, e `GATEWAY_STAGE` quando o stage não for `$default`).

## Rotas da aplicação (agnósticas ao gateway)

As rotas expostas pela API são sempre:

- `GET /health` — health check
- `POST /login` — login
- `POST /users/create` — criação de usuário

Localmente, use essas rotas diretamente. Atrás do gateway com prefixo `/auth`, a URL pública será algo como `https://.../auth/health`, `https://.../auth/login`, etc.; internamente o middleware faz o path ser `/health`, `/login`, etc.
