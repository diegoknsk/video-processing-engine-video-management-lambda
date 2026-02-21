# Storie-07: Deploy AWS Lambda + Handler + IaC

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** —

## Descrição
Como DevOps/desenvolvedor, quero realizar o deploy da aplicação Video Management Lambda na AWS via pipeline CI/CD (GitHub Actions), configurar o Handler correto para AddAWSLambdaHosting, ajustar variáveis de ambiente no Lambda (DynamoDB, S3, Cognito, GATEWAY_PATH_PREFIX, GATEWAY_STAGE), e documentar o processo de deploy e troubleshooting.

## Objetivo
Criar workflow de deploy no GitHub Actions (build → test → publish → zip → deploy), configurar Handler = `VideoProcessing.VideoManagement.Api` (nome do assembly), configurar env vars do Lambda (DynamoDB TableName, S3 buckets, Cognito, GATEWAY_PATH_PREFIX/GATEWAY_STAGE), e documentar processo e troubleshooting comum. A validação pós-deploy via smoke test (GET /health) fica na Storie-08.

## Escopo Técnico
- **Tecnologias:** GitHub Actions, AWS Lambda, AWS CLI, dotnet publish, .NET 10
- **Arquivos criados/modificados:**
  - `.github/workflows/deploy-lambda-video-management.yml` (workflow CI/CD)
  - `docs/deploy-video-management-lambda.md` (documentação do processo)
  - `README.md` (atualizar com instruções de deploy e variáveis de ambiente)
- **Componentes:**
  - Workflow GitHub Actions
  - Configuração de Handler
  - Configuração de env vars na Lambda
- **Pacotes/Dependências:** Nenhum novo (usa AWS CLI e dotnet CLI)

## Dependências e Riscos (para estimativa)
- **Dependências:**
  - Stories 01, 01.2, 02–05, **05.1** (padronização de respostas) e 06 concluídas (aplicação funcional localmente; GET /health e gateway configurados; respostas no envelope padrão)
  - Lambda function provisionada no AWS (via IaC; nome esperado: video-processing-engine-dev-video-management)
  - API Gateway HTTP API criado e integrado com a Lambda (via IaC)
  - DynamoDB table, S3 buckets, Cognito User Pool provisionados (via IaC)
  - GitHub repository configurado com secrets/variables
- **Riscos:**
  - Handler incorreto causa erro "assembly not found" ou "type not found"; documentar Handler = nome do assembly (VideoProcessing.VideoManagement.Api)
  - Timeout padrão de 3s causa Sandbox.Timedout no cold start; documentar aumento para 30–60s no IaC
  - GATEWAY_PATH_PREFIX/GATEWAY_STAGE incorretos causam 404; documentar como configurar baseado na setup do API Gateway

## Subtasks
- [Subtask 01: Criar workflow GitHub Actions de deploy (build, test, publish, zip, deploy)](./subtask/Subtask-01-Workflow_GitHub_Actions_Deploy.md)
- [Subtask 02: Configurar Handler e variáveis de ambiente da Lambda (via workflow ou manual)](./subtask/Subtask-02-Configurar_Handler_Env_Vars_Lambda.md)
- [Subtask 03: Documentar processo de deploy, troubleshooting e variáveis de ambiente](./subtask/Subtask-04-Documentar_Deploy_Troubleshooting.md)

## Critérios de Aceite da História
- [ ] Workflow `.github/workflows/deploy-lambda-video-management.yml` criado com jobs: build (restore, build, test), publish (dotnet publish linux-x64), deploy (create zip, aws lambda update-function-code, wait for update, update handler, update env vars)
- [ ] GitHub Secrets configurados: AWS_ACCESS_KEY_ID, AWS_SECRET_ACCESS_KEY, AWS_SESSION_TOKEN (se temporário)
- [ ] GitHub Variables configurados: AWS_REGION (us-east-1), LAMBDA_FUNCTION_NAME (video-processing-engine-dev-video-management), DYNAMODB_TABLE_NAME, S3_BUCKET_VIDEO, S3_BUCKET_FRAMES, S3_BUCKET_ZIP, COGNITO_USER_POOL_ID, COGNITO_CLIENT_ID, GATEWAY_PATH_PREFIX (ex.: /videos), GATEWAY_STAGE (se não for $default)
- [ ] Step "Update Lambda handler" no workflow configura Handler = `VideoProcessing.VideoManagement.Api` usando `aws lambda update-function-configuration --handler`
- [ ] Step "Update Lambda environment variables" mescla env vars: Cognito (UserPoolId, ClientId, Region), DynamoDB (TableName, Region), S3 (BucketVideo, BucketFrames, BucketZip, Region), GATEWAY_PATH_PREFIX, GATEWAY_STAGE, API_PUBLIC_BASE_URL (opcional)
- [ ] Workflow executa `dotnet test`; se falhar, deploy é abortado
- [ ] Workflow cria ZIP com arquivos publicados na raiz (VideoProcessing.VideoManagement.Api.dll, deps, etc.)
- [ ] Deploy via `aws lambda update-function-code --zip-file fileb://deployment-package.zip`
- [ ] Step "Wait for Lambda update" aguarda função ficar em estado Active (timeout 5 min)
- [ ] Documentação criada em `docs/deploy-video-management-lambda.md` com: pré-requisitos, configuração de secrets/variables, execução manual do workflow, troubleshooting (Handler incorreto, Timeout, GATEWAY_PATH_PREFIX, erros de env vars)
- [ ] README.md atualizado com seção "Deploy" linkando para docs e listando variáveis de ambiente obrigatórias
- [ ] Deploy manual executado com sucesso (smoke test é validado na Storie-08)

## Rastreamento (dev tracking)
- **Início:** —
- **Fim:** —
- **Tempo total de desenvolvimento:** —
