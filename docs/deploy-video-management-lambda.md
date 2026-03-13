# Deploy via GitHub Actions - Video Management Lambda

Este documento descreve o processo de deploy automatizado da aplicação **Video Management Lambda** para AWS via GitHub Actions.

## 📋 Índice

- [Visão Geral](#visão-geral)
- [Pré-requisitos AWS](#pré-requisitos-aws)
- [Configuração GitHub](#configuração-github)
- [Variáveis de Ambiente](#variáveis-de-ambiente)
- [Execução](#execução)
- [Troubleshooting](#troubleshooting)

## 🎯 Visão Geral

O workflow `.github/workflows/deploy-lambda-video-management.yml` automatiza o build, teste e deploy da aplicação .NET 10.

### Fluxo do Processo
1. **Build & Test**: Compila a aplicação e executa testes unitários. Se algum teste falhar, o deploy é interrompido.
2. **Publish**: Gera os binários para `linux-x64`.
3. **Package**: Cria um arquivo ZIP para o Lambda.
4. **Deploy**: Atualiza o código da função Lambda na AWS.
5. **Configure**: Atualiza o Handler e as Variáveis de Ambiente.

## ☁️ Pré-requisitos AWS

1. **Lambda Function**: Deve estar criada (ex: `video-processing-engine-dev-video-management`).
2. **IAM — usuário do GitHub Actions**: precisa de:
   - `lambda:UpdateFunctionCode`
   - `lambda:UpdateFunctionConfiguration`
   - `lambda:GetFunctionConfiguration`
   - `lambda:GetFunction`
3. **IAM — role de execução da Lambda**: a role (ex.: `video-processing-engine-dev-lambda-exec-role`) deve ter permissão na tabela DynamoDB de **chunks** (`video-processing-engine-dev-video-chunks`), pois o GET individual de vídeo usa `dynamodb:Query` nessa tabela. Ver [Permissão DynamoDB — Tabela de Chunks](iam-lambda-exec-role-chunks.md).

## 🔐 Configuração GitHub

### 1. GitHub Secrets (Obrigatórios)
Configure em `Settings > Secrets and variables > Actions`:

| Secret Name | Descrição |
|-------------|-----------|
| `AWS_ACCESS_KEY_ID` | Access Key do usuário IAM |
| `AWS_SECRET_ACCESS_KEY` | Secret Key correspondente |
| `AWS_SESSION_TOKEN` | Token de sessão (apenas se usar credenciais temporárias) |

### 2. GitHub Variables (Recomendados)
Configure em `Settings > Secrets and variables > Actions > Variables`:

| Variable Name | Descrição | Exemplo |
|---------------|-----------|---------|
| `AWS_REGION` | Região AWS | `us-east-1` |
| `LAMBDA_FUNCTION_NAME` | Nome da função Lambda | `video-processing-engine-dev-video-management` |
| `DYNAMODB_TABLE_NAME` | Tabela do DynamoDB | `videos-dev` |
| `S3_BUCKET_VIDEO` | Bucket para vídeos originais | `video-process-input-dev` |
| `S3_BUCKET_FRAMES` | Bucket para frames extraídos | `video-process-frames-dev` |
| `S3_BUCKET_ZIP` | Bucket para arquivos ZIP | `video-process-output-dev` |
| `COGNITO_USER_POOL_ID`| ID do User Pool | `us-east-1_XXXXX` |
| `COGNITO_CLIENT_ID` | Client ID do Cognito | `YYYYY` |
| `GATEWAY_PATH_PREFIX`| Prefixo do Gateway | `/videos` |

## 🚀 Execução

### Automática
- Push ou Pull Request para a branch `main`.

### Manual
1. Vá na aba **Actions** do GitHub.
2. Selecione **Deploy Lambda Video Management**.
3. Clique em **Run workflow**.
4. (Opcional) Informe os overrides de região ou nome da função.

## 🩺 Troubleshooting

- **Erro de Handler**: O handler deve ser `VideoProcessing.VideoManagement.Api`. O workflow configura isso automaticamente.
- **Timeout**: Se a Lambda der timeout no cold start, aumente o timeout na AWS (mínimo recomendado: 30s).
- **Access Denied**: Verifique se o `AWS_SESSION_TOKEN` expirou ou se as políticas do IAM estão corretas.
- **404 no Health Check**: Verifique se o `GATEWAY_PATH_PREFIX` condiz com a rota configurada no API Gateway.
