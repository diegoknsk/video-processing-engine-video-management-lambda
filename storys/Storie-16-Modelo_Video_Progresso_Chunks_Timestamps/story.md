# Storie-16: Evoluir modelo de vídeo — progresso de chunks, timestamps e controle de paralelismo

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** —

## Descrição
Como desenvolvedor do Video Processing Engine, quero evoluir o modelo de vídeo para suportar timestamps do pipeline, controle de paralelismo por vídeo, progresso incremental de chunks e logs de transição de status, para que a Lambda de atualização de status aplique regras de domínio ricas, o orquestrador conheça o limite de paralelismo e seja possível rastrear o progresso granular do processamento via CloudWatch.

## Objetivo
Adicionar ao modelo de vídeo os campos de timestamps do pipeline (`processingStartedAt`, `imagesProcessingCompletedAt`, `processingCompletedAt`, `lastFailedAt`, `lastCancelledAt`), controle de paralelismo (`maxParallelChunks`), e progresso incremental de chunks (`processingSummary`) com merge idempotente. Adaptar a entidade de domínio, DTOs, persistência DynamoDB, Lambda de atualização de status e testes unitários, garantindo:

- Timestamps preenchidos automaticamente nas transições de status
- Merge incremental e idempotente de `processingSummary.chunks`
- Logs estruturados de transição de status visíveis no CloudWatch
- Regras de não regressão de status mantidas

## Escopo Técnico
- **Tecnologias:** .NET 10, C# 13, AWS Lambda, DynamoDB (SDK v3), System.Text.Json, ILogger (CloudWatch)
- **Arquivos afetados:**
  - **Domain:** `Video.cs`, `VideoRehydrationData.cs`, `VideoUpdateValues.cs`, novos: `ProcessingSummary.cs`, `ChunkInfo.cs`
  - **Application:** `UpdateVideoInputModel.cs`, `UpdateVideoInputModelValidator.cs`, `UpdateVideoUseCase.cs`, `VideoResponseModel.cs`, `VideoResponseModelMapper.cs`
  - **Infra.Data:** `VideoEntity.cs`, `VideoMapper.cs`, `VideoRepository.cs`
  - **Lambda:** `UpdateVideoLambdaEvent.cs`, `UpdateVideoHandler.cs`
  - **Docs:** `docs/lambda-update-video-contract.md`
  - **Tests:** `VideoProcessing.VideoManagement.UnitTests`
- **Componentes:** Entidade Video (Domain), ProcessingSummary value object (Domain), VideoMapper (Infra), VideoRepository (Infra), UpdateVideoUseCase (Application), Lambda handler
- **Pacotes/Dependências:** Nenhum pacote novo; uso de System.Text.Json e Microsoft.Extensions.Logging já presentes no projeto.

## Dependências e Riscos (para estimativa)
- **Dependências:** Storie-14 (adapter SQS/JSON), Storie-15 (enum VideoStatus com novos valores).
- **Riscos:**
  - Persistência de `processingSummary` como Map no DynamoDB: requer serialização/deserialização cuidadosa e uso de `SET` com path expression para merge incremental sem sobrescrever o objeto inteiro.
  - Compatibilidade retroativa: vídeos existentes não possuem os novos campos; leitura deve tratar valores ausentes como `null` sem erro.
  - Concorrência: atualizações simultâneas de chunks distintos não devem conflitar; a ConditionExpression existente deve ser revisada para suportar os novos campos.

## Subtasks
- [Subtask 01: Domain — novos campos de timestamps e maxParallelChunks na entidade Video](./subtask/Subtask-01-Domain_Timestamps_MaxParallelChunks.md)
- [Subtask 02: Domain — ProcessingSummary e ChunkInfo com merge incremental idempotente](./subtask/Subtask-02-Domain_ProcessingSummary_ChunkInfo.md)
- [Subtask 03: Domain — timestamps automáticos nas transições de status e logs estruturados](./subtask/Subtask-03-Domain_Timestamps_Automaticos_Logs.md)
- [Subtask 04: Infra — VideoEntity, VideoMapper e VideoRepository (DynamoDB)](./subtask/Subtask-04-Infra_VideoEntity_Mapper_Repository.md)
- [Subtask 05: Application e Lambda — InputModel, Validator, UseCase e evento Lambda](./subtask/Subtask-05-Application_Lambda_InputModel_UseCase.md)
- [Subtask 06: Testes unitários — merge de chunks, idempotência, transições e timestamps](./subtask/Subtask-06-Testes_Unitarios.md)

## Critérios de Aceite da História
- [ ] Entidade `Video` possui os campos `processingStartedAt`, `imagesProcessingCompletedAt`, `processingCompletedAt`, `lastFailedAt`, `lastCancelledAt`, `maxParallelChunks` e `processingSummary`.
- [ ] `ProcessingSummary` suporta merge incremental de chunks: adicionar novo chunk funciona; chunk duplicado é ignorado (idempotente); `processingSummary` null na entrada não modifica o campo existente.
- [ ] Transição `ProcessingImages → GeneratingZip` preenche `imagesProcessingCompletedAt` automaticamente; `GeneratingZip → Completed` preenche `processingCompletedAt`; entrada em `Failed` preenche `lastFailedAt`; entrada em `Cancelled` preenche `lastCancelledAt`.
- [ ] Regra de não regressão de status existente permanece válida (não é possível voltar de `Completed` para status anterior).
- [ ] Logs estruturados com `videoId`, `previousStatus` e `newStatus` são registrados em toda transição de status, visíveis no CloudWatch.
- [ ] `UpdateVideoInputModel` e `UpdateVideoLambdaEvent` suportam os novos campos (`maxParallelChunks`, `processingSummary`, `processingStartedAt`).
- [ ] `VideoResponseModel` expõe os novos campos nas respostas da API e da Lambda.
- [ ] Persistência DynamoDB suporta os novos campos; `processingSummary` persistido como Map com merge incremental (sem sobrescrever o mapa inteiro).
- [ ] Documentação do contrato da Lambda (`docs/lambda-update-video-contract.md`) atualizada com novos campos e exemplos.
- [ ] Testes unitários cobrindo: adição de chunk, chunk duplicado, tentativa de sobrescrever chunk existente, regressão de status, timestamps corretos, merge parcial sem sobrescrever `processingSummary`; `dotnet build` e `dotnet test` passando.

## Rastreamento (dev tracking)
- **Início:** —
- **Fim:** —
- **Tempo total de desenvolvimento:** —
