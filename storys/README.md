# Stories - Etapa 2: Lambda Video Management (Controle de Estado)

Este conjunto de stories técnicas implementa a **Etapa 2 do Video Processing Engine**: o Lambda de gerenciamento de vídeos que controla o estado (DynamoDB), gera URLs pré-assinadas para upload no S3, expõe API REST (GET/POST/PATCH) e prepara a base para as etapas subsequentes (S3 event, SNS/SQS, Step Functions, fan-out/fan-in).

---

## 📋 Índice das Stories

### [Storie-01: Bootstrap do Projeto Lambda Video Management](./Storie-01-Bootstrap_Projeto_Lambda_Video_Management/story.md)
**Objetivo:** Criar estrutura completa do projeto .NET 10 com Clean Architecture, configurar packages essenciais (AWS Lambda Hosting, DynamoDB, S3, Serilog), health check, middleware de erros, DI e testes unitários.

**Subtasks:** 6 (Estrutura projetos, Program.cs + DI, Health check + middleware, Logging Serilog, Modelos de configuração AWS, Estrutura de testes)

**Dependências:** Nenhuma (primeira story)

---

### [Storie-01.2: Health Controller para Teste de Deploy (Gateway)](./Storie-01.2-Health_Controller_Deploy_Test/story.md)
**Objetivo:** Garantir endpoint GET /health pronto para deploy e configurar GatewayPathBaseMiddleware (GATEWAY_PATH_PREFIX, GATEWAY_STAGE) para que a API funcione atrás do API Gateway; documentar URL do smoke test.

**Subtasks:** 3 (Endpoint /health, GatewayPathBaseMiddleware + env vars, Documentar URL smoke test)

**Dependências:** Story 01

---

### [Storie-02: Modelo de Dados DynamoDB e Repository Pattern](./Storie-02-Modelo_Dados_DynamoDB_Repository/story.md)
**Objetivo:** Definir entidade `Video` (Domain) com todos os campos obrigatórios e recomendados para fan-out/fan-in, criar `IVideoRepository` port, implementar `VideoRepository` com operações CRUD idempotentes e condicionais (ownership, monotonia, status transitions).

**Subtasks:** 6 (Entidade Video + enums, IVideoRepository port, VideoEntity DTO + Mapper, CRUD básico, Update condicional, Testes unitários)

**Dependências:** Story 01

---

### [Storie-03: Contratos OpenAPI + Scalar UI (Documentação Completa)](./Storie-03-Contratos_OpenAPI_Scalar_UI/story.md)
**Objetivo:** Configurar OpenAPI com documentação completa (InputModels, ResponseModels, autenticação Cognito, erros), integrar Scalar UI, garantir que API é autodocumentada e testável via "Try it" desde o primeiro deploy.

**Subtasks:** 5 (Swashbuckle + OpenAPI básico, InputModels + ResponseModels, Segurança Cognito, Scalar UI + servers/PathBase, Exemplos + erros padronizados)

**Dependências:** Story 01, Story 01.2, Story 02

---

### [Storie-04: POST /videos — Registro de Vídeo e Pre-signed URL](./Storie-04-POST_Videos_Registro_Presigned_URL/story.md)
**Objetivo:** Implementar POST /videos que valida input, extrai userId do JWT, implementa idempotência com clientRequestId, gera videoId e s3KeyVideo imutável, persiste no DynamoDB, gera pre-signed URL de PUT no S3, e retorna UploadVideoResponseModel.

**Subtasks:** 5 (Validator UploadVideoInputModel, S3PresignedUrlService, UploadVideoUseCase + idempotência, Endpoint POST /videos, Testes unitários)

**Dependências:** Story 01, 02, 03

---

### [Storie-05: GET /videos e GET /videos/{id} — Consulta e Listagem](./Storie-05-GET_Videos_Consulta_Listagem/story.md)
**Objetivo:** Implementar GET /videos (lista paginada com cursor-based pagination) e GET /videos/{id} (detalhes com ownership validation), extrair userId do JWT, mapear Video para VideoResponseModel.

**Subtasks:** 4 (ListVideosUseCase + paginação, GetVideoByIdUseCase + ownership, Endpoints GET, Testes unitários)

**Dependências:** Story 01, 02, 03, 04

---

### [Storie-06: PATCH /videos/{id} — Update Interno Idempotente](./Storie-06-PATCH_Videos_Update_Interno_Idempotente/story.md)
**Objetivo:** Implementar PATCH /videos/{id} (rota interna para orchestrator/processor/finalizer) que atualiza campos opcionais (status, progressPercent, errorMessage, framesPrefix, s3KeyZip, etc.) de forma idempotente, com validações condicionais (ownership, monotonia, status transitions), retorna 409 Conflict quando condição falha.

**Subtasks:** 4 (Validator UpdateVideoInputModel, UpdateVideoUseCase idempotente + condicional, Endpoint PATCH /videos/{id}, Testes unitários)

**Dependências:** Story 01, 02, 03

---

### [Storie-07: Deploy AWS Lambda + Handler + IaC](./Storie-07-Deploy_AWS_Handler_IaC_Smoke_Test/story.md)
**Objetivo:** Criar workflow GitHub Actions de deploy (build → test → publish → zip → deploy), configurar Handler correto (`VideoProcessing.VideoManagement.Api`), variáveis de ambiente (DynamoDB, S3, Cognito, GATEWAY_PATH_PREFIX, GATEWAY_STAGE), documentar processo e troubleshooting. Smoke test na Storie-08.

**Subtasks:** 3 (Workflow GitHub Actions, Configurar Handler + env vars, Documentação + troubleshooting)

**Dependências:** Story 01, 01.2, 02–06 (aplicação completa + health/gateway)

---

### [Storie-08: Smoke Test pós-Deploy (GET /health no Gateway)](./Storie-08-Smoke_Test_Pos_Deploy/story.md)
**Objetivo:** Executar deploy, montar URL do smoke test com GATEWAY_PATH_PREFIX e GATEWAY_STAGE, validar GET /health (200 + JSON), incluir step smoke test no workflow, documentar smoke test manual e troubleshooting.

**Subtasks:** 3 (Executar deploy + smoke test, Smoke test no workflow, Documentar smoke test manual)

**Dependências:** Story 01.2, Story 07

---

### [Storie-10: Extrair Update de Vídeo para Lambda Pura (LambdaUpdateVideo)](./Storie-10-Extrair_Update_Video_Lambda_Pura/story.md)
**Objetivo:** Criar novo projeto Lambda pura (VideoProcessing.VideoManagement.LambdaUpdateVideo), handler padrão .NET sem AddAWSLambdaHosting, com mesmo contrato do PATCH atual (update parcial no DynamoDB); documentar event shape e exemplos JSON (mínimo e completo); adaptar VideoManagement (remover ou encaminhar PATCH para a nova Lambda).

**Subtasks:** 5 (Projeto + contrato event, Documentação contrato + exemplos JSON, Handler + DynamoDB, Validação + testes unitários, Adaptar VideoManagement PATCH)

**Dependências:** Story 02, Story 06

---

### [Storie-11: Deploy GitHub Actions do Lambda Update Video (ZIP)](./Storie-11-Deploy_GitHub_Actions_Lambda_Update_Video/story.md)
**Objetivo:** Pipeline de deploy via GitHub Actions para a Lambda Update Video: build, test, publish, ZIP, update-function-code e update-function-configuration (Handler); variável LAMBDA_FUNCTION_UPDATE_STATUS_NAME e credenciais AWS Academy; documentação e teste manual pós-deploy.

**Subtasks:** 4 (Workflow build/test/publish/ZIP, Deploy AWS + Handler, Variáveis e secrets AWS Academy, Documentação e teste manual)

**Dependências:** Story 10

---

## 🎯 Resumo Executivo

### Total de Stories: **10** (incl. 01.2, 08, 10 e 11)
### Total de Subtasks: **49**

### Escopo Funcional Coberto:
✅ Bootstrap completo do projeto (.NET 10, Clean Architecture, AWS Lambda Hosting)  
✅ Persistência DynamoDB (single-table, operações condicionais, idempotência)  
✅ Documentação OpenAPI + Scalar UI desde o início  
✅ POST /videos (registro + pre-signed URL para upload direto no S3)  
✅ GET /videos (listagem paginada) e GET /videos/{id} (consulta com ownership)  
✅ PATCH /videos/{id} (update interno idempotente para múltiplos writers)  
✅ Endpoint /health e gateway (GATEWAY_PATH_PREFIX, GATEWAY_STAGE) para smoke test (Story 01.2)  
✅ Deploy automatizado via GitHub Actions (Story 07)  
✅ Smoke test pós-deploy GET /health no gateway (Story 08)

### Rotas Implementadas:
1. **GET /health** — Health check (pública)
2. **POST /videos** — Registrar vídeo + gerar pre-signed URL (autenticada, idempotente)
3. **GET /videos** — Listar vídeos do usuário (autenticada, paginada)
4. **GET /videos/{id}** — Obter detalhes de vídeo (autenticada, ownership validation)
5. **PATCH /videos/{id}** — Atualizar vídeo (interna, idempotente, condicional)

### Modelo de Dados (DynamoDB):
- **Table:** single-table design
- **pk:** USER#{userId}
- **sk:** VIDEO#{videoId}
- **Campos:** videoId, userId, originalFileName, contentType, sizeBytes, durationSec, createdAt, updatedAt, status, progressPercent, s3BucketVideo, s3KeyVideo, s3BucketFrames, framesPrefix, s3BucketZip, s3KeyZip, stepExecutionArn, errorMessage, errorCode, processingMode, chunkCount, chunkDurationSec, uploadIssuedAt, uploadUrlExpiresAt, framesProcessed, finalizedAt, clientRequestId (idempotência)

### Autenticação e Identidade:
- **Cognito Authorizer** no API Gateway (JWT)
- **userId** extraído do claim "sub" do JWT
- **Ownership validation** implícita nas queries DynamoDB (pk = USER#{userId})

### Qualidade e Testes:
- **Testes unitários** em todas as stories (xUnit, Moq, FluentAssertions)
- **Cobertura mínima:** 80% por componente
- **Validação com FluentValidation** em todos os InputModels
- **BDD planejado** (mínimo 1 teste BDD mencionado nos critérios)

### Próximas Etapas (Etapas 3–7):
- **Etapa 3:** S3 event → SNS (vídeo enviado)
- **Etapa 4:** SNS → SQS → Lambda Orchestrator (inicia Step Functions)
- **Etapa 5:** Step Functions → Lambda Processor (processamento single ou fan-out)
- **Etapa 6:** Lambda Finalizer → gera ZIP → notificação
- **Etapa 7:** Observabilidade, monitoring, alertas

---

## 📊 Estatísticas

| Métrica | Valor |
|---------|-------|
| Stories criadas | 8 (incl. 01.2, 08) |
| Subtasks criadas | 40 |
| Arquivos .md gerados | 48 (8 story.md + 40 subtask.md) |
| Média de subtasks por story | ~5 |
| Endpoints REST implementados | 5 |
| Entidades de domínio | 1 (Video) |
| Enums de domínio | 2 (VideoStatus, ProcessingMode) |
| Use cases planejados | 5 (UploadVideo, ListVideos, GetVideoById, UpdateVideo, + health) |
| Repositories | 1 (VideoRepository) |
| Services de infraestrutura | 1 (S3PresignedUrlService) |

---

## 🚀 Ordem de Execução Recomendada

**Sequencial (dependências entre stories):**
1. Story 01 → Story 01.2 → Story 02 → Story 03 → Story 04/05/06 (paralelo) → Story 07 → Story 08

**Nota:** Stories 04, 05 e 06 podem ser desenvolvidas em paralelo após conclusão das Stories 01, 01.2, 02 e 03, desde que diferentes desenvolvedores trabalhem em cada uma (endpoints independentes).

---

## 📝 Convenções e Padrões

Todas as stories seguem:
- **Clean Architecture** (Api → Application → Domain; Infra.* implementa Ports)
- **Convenções .NET** (PascalCase, async/await, System.Text.Json, construtores primários)
- **FluentValidation** para InputModels
- **Serilog** para logging estruturado
- **OpenAPI/Scalar** para documentação
- **xUnit + Moq + FluentAssertions** para testes
- **GitHub Actions** para CI/CD
- **AWS Lambda Hosting** (AddAWSLambdaHosting) para runtime Lambda

---

_Criado em 14/02/2026 às 17:48 (Brasília)_  
_Desenvolvido por Arquiteto/Dev Sênior (.NET + AWS Serverless)_
