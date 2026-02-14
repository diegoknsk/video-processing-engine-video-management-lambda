# Processo de subida – Deploy Lambda (GitHub Actions)

Este documento registra o **processo de subida** e **todas as variáveis e secrets** que precisam ser configurados para o deploy da Auth API no AWS Lambda via GitHub Actions.

---

## 1. Checklist de configuração

### 1.1 GitHub Secrets (obrigatórios para deploy)

Configurar em: **Settings** → **Secrets and variables** → **Actions** → **Secrets** → **New repository secret**.

| Secret | Descrição | Usado quando |
|--------|-----------|--------------|
| **AWS_ACCESS_KEY_ID** | Access Key ID do IAM User/Role (ou credenciais temporárias STS) | Sempre (deploy) |
| **AWS_SECRET_ACCESS_KEY** | Secret Access Key correspondente | Sempre (deploy) |
| **AWS_SESSION_TOKEN** | Token de sessão (obrigatório quando usar credenciais temporárias STS) | Autenticação com token / AssumeRole |

**Região:** use apenas a **Variable** `AWS_REGION` (ou o input manual no workflow). Ordem: input manual → variable `AWS_REGION` → padrão `us-east-1`.

### 1.2 GitHub Variables (opcionais)

Configurar em: **Settings** → **Secrets and variables** → **Actions** → **Variables** → **New repository variable**.

| Variable | Descrição | Padrão no workflow |
|----------|-----------|---------------------|
| **AWS_REGION** | Região AWS do Lambda | `us-east-1` |
| **LAMBDA_FUNCTION_NAME** | Nome da função Lambda | `video-processing-engine-dev-auth` |
| **COGNITO_USER_POOL_ID** | ID do Cognito User Pool (injetado no Lambda como `Cognito__UserPoolId`) | — |
| **COGNITO_CLIENT_ID** | App Client ID do Cognito (injetado no Lambda como `Cognito__ClientId`) | — |
| **GATEWAY_PATH_PREFIX** | Prefixo de path do API Gateway (ex.: `/auth`). Injetado no Lambda; quando a API está atrás de um gateway com prefixo, a aplicação remove esse prefixo do path. Deixe vazio se não usar prefixo. Ver [gateway-path-prefix.md](gateway-path-prefix.md). | — (vazio = path inalterado) |

- O workflow **atualiza as variáveis de ambiente do Lambda** em todo deploy (Cognito, se as Variables acima estiverem preenchidas, e **GATEWAY_PATH_PREFIX**). As variáveis são mescladas com as já existentes na função.
- Valores de referência (ex.: ambiente de desenvolvimento) estão em `src/VideoProcessing.Auth.Api/appsettings.Development.json` (seção `Cognito`). Exemplo:

```json
"Cognito": {
  "Region": "us-east-1",
  "UserPoolId": "us-east-1_LYDOopM5u",
  "ClientId": "5sc99v1fok2qjrpbr06mcr2a1e"
}
```

- **Region** do Cognito no Lambda usa a mesma região do deploy (`AWS_REGION` do workflow).

### 1.3 Inputs do workflow (execução manual)

Ao rodar manualmente: **Actions** → **Deploy Lambda Auth API** → **Run workflow**.

| Input | Obrigatório | Descrição |
|-------|-------------|-----------|
| **lambda_function_name** | Não | Override do nome do Lambda |
| **aws_region** | Não | Override da região AWS |
| **gateway_path_prefix** | Não | Override do prefixo de path do API Gateway (ex.: `/auth`). Usa a Variable `GATEWAY_PATH_PREFIX` se vazio. |

---

## 2. Resumo do que setar para “subir”

| Onde | O que setar |
|------|-------------|
| **GitHub Secrets** | `AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY`; se usar token/STS: `AWS_SESSION_TOKEN` |
| **GitHub Variables** | Opcional: `AWS_REGION`, `LAMBDA_FUNCTION_NAME`; para injetar Cognito no Lambda: `COGNITO_USER_POOL_ID`, `COGNITO_CLIENT_ID`; para prefixo do API Gateway: `GATEWAY_PATH_PREFIX` (ex.: `/auth`) |
| **Lambda (AWS)** | Se não usar Variables do Cognito no workflow: configurar manualmente no Lambda `Cognito__Region`, `Cognito__UserPoolId`, `Cognito__ClientId` |

O **Handler** da função Lambda deve ser configurado via IaC (Terraform/CloudFormation) na criação da função.

---

## 3. Autenticação com token (STS / credenciais temporárias)

Quando a autenticação for com **credenciais temporárias** (ex.: AssumeRole, STS), além de Access Key e Secret Key é necessário o **session token**:

1. Em **GitHub** → **Settings** → **Secrets and variables** → **Actions** → **Secrets**, crie/edite:
   - **AWS_ACCESS_KEY_ID**
   - **AWS_SECRET_ACCESS_KEY**
   - **AWS_SESSION_TOKEN** (obrigatório nesse cenário)

2. O workflow já está preparado: o step **Configure AWS credentials** recebe o token pelo **input** `aws-session-token` (valor de `secrets.AWS_SESSION_TOKEN`). O token deve ser passado pelo input da action, não só por variável de ambiente, para que as credenciais temporárias sejam usadas corretamente nos steps seguintes.

3. Região: **Variable** `AWS_REGION` ou input manual no Run workflow; padrão `us-east-1`.

---

## 4. Configuração Cognito (api / appsettings)

A API lê Cognito de variáveis de ambiente no Lambda com os nomes:

- `Cognito__Region`
- `Cognito__UserPoolId`
- `Cognito__ClientId`

Duas formas de preencher:

1. **Pelo workflow (recomendado):** setar no repositório as GitHub Variables **COGNITO_USER_POOL_ID** e **COGNITO_CLIENT_ID**. O workflow aplica essas variáveis na função Lambda (mesclando com as já existentes). A região usada é a do deploy (`AWS_REGION`).
2. **Manual na AWS:** na função Lambda, em **Configuration** → **Environment variables**, definir `Cognito__Region`, `Cognito__UserPoolId` e `Cognito__ClientId`.

Os valores podem ser os mesmos do `appsettings.Development.json` (para dev) ou outros para produção.

---

## 5. Processo de subida (passo a passo)

1. **Repositório**
   - Configurar todos os **Secrets** necessários (incluindo `AWS_SESSION_TOKEN` se for auth com token).
   - (Opcional) Configurar **Variables**: `AWS_REGION`, `LAMBDA_FUNCTION_NAME`, `COGNITO_USER_POOL_ID`, `COGNITO_CLIENT_ID`.

2. **AWS**
   - Lambda já criada (nome igual a `LAMBDA_FUNCTION_NAME`).
   - IAM User/Role das credenciais com permissões: `lambda:UpdateFunctionCode`, `lambda:GetFunction`, `lambda:GetFunctionConfiguration` (esta última necessária para o step que atualiza variáveis de ambiente do Lambda).

3. **Deploy automático**
   - **Push** ou **merge** na branch `main` → o workflow roda e faz deploy (e, se as Variables de Cognito estiverem setadas, atualiza as env vars do Lambda).

4. **Deploy manual**
   - **Actions** → **Deploy Lambda Auth API** → **Run workflow** → escolher branch e, se quiser, preencher **aws_region** e **lambda_function_name**.

5. **Verificação**
   - Ver o run em **Actions** e o step **Verify deployment**.
   - No Lambda: **Configuration** → **Environment variables** (e **Monitoring** / CloudWatch em caso de erro).

---

## 6. Referência rápida (tabela única)

| Tipo | Nome | Obrigatório | Observação |
|------|------|-------------|------------|
| Secret | AWS_ACCESS_KEY_ID | Sim | Deploy |
| Secret | AWS_SECRET_ACCESS_KEY | Sim | Deploy |
| Secret | AWS_SESSION_TOKEN | Sim (com token/STS) | Deploy com credenciais temporárias |
| Variable | AWS_REGION | Não | Padrão: `us-east-1` |
| Variable | LAMBDA_FUNCTION_NAME | Não | Padrão: `video-processing-engine-dev-auth` |
| Variable | COGNITO_USER_POOL_ID | Não* | *Para injetar no Lambda; senão configurar no Lambda |
| Variable | COGNITO_CLIENT_ID | Não* | *Para injetar no Lambda; senão configurar no Lambda |

Documentação detalhada do workflow e troubleshooting: [deploy-github-actions.md](./deploy-github-actions.md).
