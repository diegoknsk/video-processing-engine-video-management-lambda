# Processo de subida – Deploy Lambda Video Management (GitHub Actions)

Este documento registra o **processo de subida** e **variáveis e secrets** necessários para o deploy da **Video Management API** no AWS Lambda via GitHub Actions.

---

## 1. Checklist de configuração

### 1.1 GitHub Secrets (obrigatórios para deploy)

Configurar em: **Settings** → **Secrets and variables** → **Actions** → **Secrets** → **New repository secret**.

| Secret | Descrição | Usado quando |
|--------|-----------|--------------|
| **AWS_ACCESS_KEY_ID** | Access Key ID do IAM User/Role (ou credenciais temporárias STS) | Sempre (deploy) |
| **AWS_SECRET_ACCESS_KEY** | Secret Access Key correspondente | Sempre (deploy) |
| **AWS_SESSION_TOKEN** | Token de sessão (obrigatório quando usar credenciais temporárias STS) | Autenticação com token / AssumeRole |

**Região:** use a **Variable** `AWS_REGION` (ou o input manual no workflow). Ordem: input manual → variable `AWS_REGION` → padrão `us-east-1`.

### 1.2 GitHub Variables (opcionais)

Configurar em: **Settings** → **Secrets and variables** → **Actions** → **Variables** → **New repository variable**.

O workflow envia estas variáveis para o Lambda no step **Update Lambda configuration**. Só preencha as que forem usar.

| Variable | Descrição | Padrão no workflow |
|----------|-----------|---------------------|
| **AWS_REGION** | Região AWS do Lambda | `us-east-1` |
| **LAMBDA_FUNCTION_NAME** | Nome da função Lambda | `video-processing-engine-dev-video-management` |
| **GATEWAY_PATH_PREFIX** | Prefixo de path do API Gateway (ex.: `/videos`). A aplicação remove esse prefixo do path. Deixe vazio se não usar prefixo. Ver [gateway-path-prefix.md](gateway-path-prefix.md). | — (vazio = path inalterado) |
| **GATEWAY_STAGE** | Nome do stage do API Gateway quando a URL inclui o stage (ex.: `/dev/videos/health`). Defina com o valor do segmento na URL (ex.: `dev`). Necessário para o middleware encontrar a rota `/health`. Se usar stage `$default` sem segmento na URL, deixe vazio. | — (vazio = path inalterado) |
| **DYNAMODB_TABLE_NAME** | Nome da tabela DynamoDB (injetado como `DynamoDB__TableName`) | — |
| **S3_BUCKET_VIDEO** | Bucket S3 para vídeos (injetado como `S3__BucketVideo`) | — |
| **S3_BUCKET_FRAMES** | Bucket S3 para frames (injetado como `S3__BucketFrames`) | — |
| **S3_BUCKET_ZIP** | Bucket S3 para zip (injetado como `S3__BucketZip`) | — |
| **COGNITO_USER_POOL_ID** | ID do Cognito User Pool (injetado como `Cognito__UserPoolId`) | — |
| **COGNITO_CLIENT_ID** | App Client ID do Cognito (injetado como `Cognito__ClientId`) | — |

- **DynamoDB / S3:** configure quando for usar persistência e storage (tabela e buckets já criados na AWS).
- **Cognito:** configure quando a API precisar validar tokens do User Pool.
- O workflow sempre envia: `AWS__Region`, `DynamoDB__Region`, `S3__Region`, `Cognito__Region` (usando a região do deploy), `GATEWAY_PATH_PREFIX`, `GATEWAY_STAGE` e `ASPNETCORE_ENVIRONMENT=Production`. As Variables acima preenchem os valores específicos (tabela, buckets, Cognito, gateway).

### 1.3 Execução manual

Ao rodar manualmente: **Actions** → **Deploy Lambda Video Management** → **Run workflow** → escolher a **branch**. Nome do Lambda, região e prefixo do gateway vêm das **Variables** do repositório (não há inputs para preencher).

---

## 2. Resumo do que setar para “subir”

| Onde | O que setar |
|------|-------------|
| **GitHub Secrets** | `AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY`; se usar token/STS: `AWS_SESSION_TOKEN` |
| **GitHub Variables** | Opcional: `AWS_REGION`, `LAMBDA_FUNCTION_NAME`, `GATEWAY_PATH_PREFIX` (ex.: `/videos`), `GATEWAY_STAGE` (ex.: `dev` para URL `.../dev/videos/health`). Quando for usar: `DYNAMODB_TABLE_NAME`, `S3_BUCKET_VIDEO`, `S3_BUCKET_FRAMES`, `S3_BUCKET_ZIP`, `COGNITO_USER_POOL_ID`, `COGNITO_CLIENT_ID` |
| **Lambda (AWS)** | Se não usar Variables no workflow: configurar manualmente no Lambda as env vars necessárias (DynamoDB, S3, Cognito, etc.) |

O **Handler** da função Lambda é atualizado pelo workflow: `VideoProcessing.VideoManagement.Api`.

---

## 3. Autenticação com token (STS / credenciais temporárias)

Quando a autenticação for com **credenciais temporárias** (ex.: AssumeRole, STS), além de Access Key e Secret Key é necessário o **session token**:

1. Em **GitHub** → **Settings** → **Secrets and variables** → **Actions** → **Secrets**, crie/edite:
   - **AWS_ACCESS_KEY_ID**
   - **AWS_SECRET_ACCESS_KEY**
   - **AWS_SESSION_TOKEN** (obrigatório nesse cenário)

2. O workflow usa `secrets.AWS_SESSION_TOKEN` no step **Configure AWS credentials**.

3. Região: **Variable** `AWS_REGION` ou input manual no Run workflow; padrão `us-east-1`.

---

## 4. Processo de subida (passo a passo)

1. **Repositório**
   - Configurar **Secrets**: `AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY` (e `AWS_SESSION_TOKEN` se for auth com token).
   - (Opcional) **Variables**: `AWS_REGION`, `LAMBDA_FUNCTION_NAME`, `GATEWAY_PATH_PREFIX`, `GATEWAY_STAGE`; quando for usar: DynamoDB, S3, Cognito (conforme tabela acima).

2. **AWS**
   - Lambda já criada (nome igual a `LAMBDA_FUNCTION_NAME` ou o que for usado no deploy).
   - IAM User/Role das credenciais com permissões: `lambda:UpdateFunctionCode`, `lambda:GetFunction`, `lambda:UpdateFunctionConfiguration`, `lambda:Wait*` (para deploy e atualização de env vars).

3. **Deploy automático**
   - **Push** ou **merge** na branch `main` → o workflow **Deploy Lambda Video Management** roda (build, test, publish, zip, deploy e atualização das env vars do Lambda).

4. **Deploy manual**
   - **Actions** → **Deploy Lambda Video Management** → **Run workflow** → escolher a branch (nome do Lambda, região e gateway vêm das Variables).

5. **Verificação**
   - Ver o run em **Actions** e o step **Verify deployment**.
   - No Lambda: **Configuration** → **Environment variables** (e **Monitoring** / CloudWatch em caso de erro).

---

## 5. Referência rápida (tabela única)

| Tipo | Nome | Obrigatório | Observação |
|------|------|-------------|------------|
| Secret | AWS_ACCESS_KEY_ID | Sim | Deploy |
| Secret | AWS_SECRET_ACCESS_KEY | Sim | Deploy |
| Secret | AWS_SESSION_TOKEN | Sim (com token/STS) | Deploy com credenciais temporárias |
| Variable | AWS_REGION | Não | Padrão: `us-east-1` |
| Variable | LAMBDA_FUNCTION_NAME | Não | Padrão: `video-processing-engine-dev-video-management` |
| Variable | GATEWAY_PATH_PREFIX | Não | Ex.: `/videos`; ver gateway-path-prefix.md |
| Variable | GATEWAY_STAGE | Não | Ex.: `dev` quando a URL do gateway for `.../dev/videos/health` |
| Variable | DYNAMODB_TABLE_NAME | Não* | *Quando for usar DynamoDB |
| Variable | S3_BUCKET_VIDEO | Não* | *Quando for usar S3 (vídeos) |
| Variable | S3_BUCKET_FRAMES | Não* | *Quando for usar S3 (frames) |
| Variable | S3_BUCKET_ZIP | Não* | *Quando for usar S3 (zip) |
| Variable | COGNITO_USER_POOL_ID | Não* | *Quando for usar Cognito |
| Variable | COGNITO_CLIENT_ID | Não* | *Quando for usar Cognito |

Documentação detalhada do workflow e troubleshooting: [deploy-video-management-lambda.md](deploy-video-management-lambda.md).


6. **Testes lambda**
   