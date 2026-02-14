# Storie-02: Modelo de Dados DynamoDB e Repository Pattern

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** —

## Descrição
Como desenvolvedor backend, quero definir o modelo de dados single-table do DynamoDB para vídeos e implementar o Repository Pattern com operações CRUD idempotentes e condicionais, para garantir persistência escalável, consistente e preparada para múltiplos writers (orchestrator, processor, finalizer).

## Objetivo
Definir e implementar a entidade `Video` do Domain com todos os campos obrigatórios (videoId, userId, status, progressPercent, s3 keys, timestamps, etc.) e campos recomendados para fan-out/fan-in (processingMode, chunkCount, etc.), criar `IVideoRepository` port na Application, implementar `VideoRepository` concreto em Infra.Data usando AWSSDK.DynamoDBv2, garantir operações condicionais (ownership, monotonia de progress, transições de status) e idempotência (clientRequestId para deduplicação).

## Escopo Técnico
- **Tecnologias:** .NET 10, AWSSDK.DynamoDBv2 (3.7.x), DynamoDB single-table design
- **Arquivos criados/modificados:**
  - `src/VideoProcessing.VideoManagement.Domain/Entities/Video.cs` (entidade agregado)
  - `src/VideoProcessing.VideoManagement.Domain/Enums/VideoStatus.cs` (enum: Pending, Processing, Completed, Failed, etc.)
  - `src/VideoProcessing.VideoManagement.Domain/Enums/ProcessingMode.cs` (enum: SINGLE_LAMBDA, FANOUT)
  - `src/VideoProcessing.VideoManagement.Application/Ports/IVideoRepository.cs` (interface port)
  - `src/VideoProcessing.VideoManagement.Infra.Data/Repositories/VideoRepository.cs` (implementação)
  - `src/VideoProcessing.VideoManagement.Infra.Data/Repositories/VideoEntity.cs` (DTO para DynamoDB, com atributos [DynamoDBHashKey], [DynamoDBRangeKey])
  - `src/VideoProcessing.VideoManagement.Infra.Data/Mappers/VideoMapper.cs` (mapeamento entre Domain.Video e Infra.VideoEntity)
- **Componentes:** 
  - Video (domain entity)
  - IVideoRepository (port)
  - VideoRepository (adapter)
  - VideoEntity (DTO/data model)
  - VideoMapper (mapper)
- **Pacotes/Dependências:**
  - AWSSDK.DynamoDBv2 (3.7.x) — já adicionado na Story 01
  - AWSSDK.DynamoDBv2.DataModel (3.7.x) — para atributos e DataModel context

## Dependências e Riscos (para estimativa)
- **Dependências:** 
  - Story 01 concluída (estrutura de projetos, DI, AWS clients)
  - Tabela DynamoDB provisionada no AWS (via IaC; nome esperado: video-processing-videos)
- **Riscos:** 
  - Modelo de dados pode evoluir nas Etapas 3–7 (adicionar campos de fan-out/fan-in); decisão: incluir campos recomendados desde o início, mas torná-los opcionais
  - Operações condicionais no DynamoDB (ConditionExpression) exigem tratamento de `ConditionalCheckFailedException`

## Subtasks
- [Subtask 01: Definir entidade Video (Domain) e enums (VideoStatus, ProcessingMode)](./subtask/Subtask-01-Entidade_Video_Domain_Enums.md)
- [Subtask 02: Criar IVideoRepository port (Application) com assinaturas de métodos](./subtask/Subtask-02-IVideoRepository_Port_Application.md)
- [Subtask 03: Implementar VideoEntity (DTO DynamoDB) e VideoMapper](./subtask/Subtask-03-VideoEntity_DTO_Mapper.md)
- [Subtask 04: Implementar VideoRepository (CRUD básico: Create, GetById, GetByUserId)](./subtask/Subtask-04-VideoRepository_CRUD_Basico.md)
- [Subtask 05: Implementar Update condicional (ownership, monotonia, status transitions)](./subtask/Subtask-05-VideoRepository_Update_Condicional.md)
- [Subtask 06: Testes unitários do Repository (mock IAmazonDynamoDB)](./subtask/Subtask-06-Testes_Unitarios_Repository.md)

## Critérios de Aceite da História
- [ ] Entidade `Video` criada no Domain com todos os campos obrigatórios: videoId, userId, originalFileName, contentType, sizeBytes, durationSec, createdAt, updatedAt, status, progressPercent, s3BucketVideo, s3KeyVideo, s3BucketFrames, framesPrefix, s3BucketZip, s3KeyZip, stepExecutionArn, errorMessage, errorCode
- [ ] Campos recomendados para fan-out/fan-in incluídos (opcionais): processingMode, chunkCount, chunkDurationSec, uploadIssuedAt, uploadUrlExpiresAt, framesProcessed, finalizedAt
- [ ] Enums criados: `VideoStatus` (Pending, Uploading, Processing, Completed, Failed, Cancelled), `ProcessingMode` (SINGLE_LAMBDA, FANOUT)
- [ ] Interface `IVideoRepository` criada com métodos: CreateAsync, GetByIdAsync, GetByUserIdAsync (paginado), UpdateAsync (condicional), ExistsByClientRequestIdAsync (idempotência)
- [ ] `VideoEntity` DTO criado com atributos [DynamoDBHashKey("pk")], [DynamoDBRangeKey("sk")]; pk = USER#{userId}, sk = VIDEO#{videoId}
- [ ] `VideoMapper` criado com métodos ToEntity (Domain → DTO) e ToDomain (DTO → Domain)
- [ ] `VideoRepository` implementado usando `IAmazonDynamoDB` (PutItem, GetItem, Query, UpdateItem com ConditionExpression)
- [ ] UpdateAsync implementa condições: ownership (pk do user), progressPercent monotônico (novo valor >= anterior), status transitions válidas (não voltar de Completed para Processing)
- [ ] ExistsByClientRequestIdAsync implementado usando GSI ou scan/query (decisão: criar campo clientRequestId no VideoEntity e query por ele)
- [ ] Testes unitários cobrindo: Create (sucesso), GetById (encontrado/não encontrado), GetByUserId (paginação), Update condicional (sucesso/falha por ownership, monotonia, status invalid)
- [ ] Cobertura de testes >= 80% para VideoRepository e VideoMapper
- [ ] `dotnet build` e `dotnet test` passam sem erros

## Rastreamento (dev tracking)
- **Início:** —
- **Fim:** —
- **Tempo total de desenvolvimento:** —
