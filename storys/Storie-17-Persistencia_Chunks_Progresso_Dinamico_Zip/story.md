# Storie-17: Persistência Individual de Chunks, Progresso Dinâmico e Dados do ZIP

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** [DD/MM/AAAA]

## Descrição
Como sistema de processamento de vídeo, quero persistir o estado individual de cada chunk em uma tabela dedicada, calcular o progresso dinamicamente com base nos chunks concluídos e expor os dados do arquivo ZIP final com URL pré-assinada, para que o progresso seja preciso e o usuário tenha acesso direto ao resultado do processamento.

## Objetivo
Evoluir o fluxo do Lambda UpdateStatus e o endpoint GET de vídeo para suportar:
- Persistência individual de chunks em tabela DynamoDB dedicada (`video-processing-engine-dev-video-chunks`), cujo nome é configurável via variável de ambiente;
- Cálculo dinâmico de `progressPercent` com base na contagem de chunks processados versus `parallelChunks` (renomeado de `maxParallelChunks`);
- Recebimento e persistência de dados do arquivo ZIP final (bucket, key, nome do arquivo) pelo Lambda UpdateStatus;
- Exposição dos dados do ZIP e URL pré-assinada AWS no endpoint GET.

## Contexto
- O Lambda **UpdateStatus** (`LambdaUpdateVideo`) é o único responsável por atualizar o estado geral do vídeo. A cada mensagem recebida, também deverá persistir ou atualizar o registro do chunk individual na nova tabela.
- O Lambda **API** (`VideoManagement.Api`) expõe os endpoints GET. O cálculo de progresso passa a ser dinâmico: conta chunks concluídos na tabela de chunks dividido por `parallelChunks`.
- A tabela de chunks já existe na infraestrutura com o nome padrão `video-processing-engine-dev-video-chunks`. Esse nome deve ser lido de variável de ambiente, sem hardcode no código.
- O campo `maxParallelChunks` será renomeado para `parallelChunks` em toda a base de código (domínio, contratos, mapeamentos e pipelines). Se ausente, o valor padrão é `1`.
- A Step Function pode precisar de ajuste mínimo no payload para incluir os novos dados do ZIP (bucket, key, nome); nenhum outro comportamento da orquestração será alterado.

## Proposta Técnica

### Configuração
- Adicionar `ChunksTableName` em `DynamoDbOptions` com default `video-processing-engine-dev-video-chunks`.
- Injetar via variável de ambiente `DynamoDB__ChunksTableName` nos dois pipelines de deploy.

### Domínio
- Nova entidade/record `VideoChunk` (Domain): `ChunkId`, `VideoId`, `Status`, `StartSec`, `EndSec`, `IntervalSec`, `ManifestPrefix`, `FramesPrefix`, `ProcessedAt`, `CreatedAt`.
- Campo `ZipBucket`, `ZipKey`, `ZipFileName` adicionados na entidade `Video`.
- `maxParallelChunks` renomeado para `parallelChunks` em `Video` e `VideoUpdateValues`.

### Application (Ports e UseCases)
- Nova port `IVideoChunkRepository` com operações `UpsertAsync` e `CountByVideoIdAsync` (ou `GetProcessedCountAsync`).
- `UpdateVideoInputModel`: add `ZipBucket?`, `ZipKey?`, `ZipFileName?`; renomear `MaxParallelChunks` → `ParallelChunks`.
- `VideoResponseModel`: add `ZipUrl?` (pré-assinada), `ZipFileName?`; renomear `MaxParallelChunks` → `ParallelChunks`.
- `UpdateVideoUseCase`: após atualizar o vídeo principal, chamar `IVideoChunkRepository.UpsertAsync` para o chunk do payload.
- `GetVideoByIdUseCase`: buscar contagem de chunks processados e calcular `progressPercent = (chunksProcessados / parallelChunks) * 100`; se `parallelChunks` não estiver definido (null/0), usar `1`; gerar URL pré-assinada S3 para o ZIP se `ZipKey` estiver presente.

### Infra
- `VideoChunkRepository` (Infra.Data): operações `UpsertAsync` (`PutItem` idempotente) e `CountByVideoIdAsync` (`Query` com `Count`) na tabela de chunks.
- Design da tabela: `pk = VIDEO#{videoId}`, `sk = CHUNK#{chunkId}`.
- `VideoMapper`: mapear campos ZIP + renomear `maxParallelChunks` → `parallelChunks`.
- Registrar `IVideoChunkRepository → VideoChunkRepository` no container de DI.

## Impacto em Configuração / Variáveis de Ambiente

| Variável | Descrição | Default (code) | Origem no pipeline |
|---|---|---|---|
| `DynamoDB__ChunksTableName` | Nome da tabela de chunks | `video-processing-engine-dev-video-chunks` | `vars.DYNAMODB_CHUNKS_TABLE_NAME` |

Ambos os workflows (`deploy-lambda-video-management.yml` e `deploy-lambda-update-video.yml`) devem passar essa variável na etapa `update-function-configuration`.

## Arquivos Afetados

**Domain:**
- `src/Core/VideoProcessing.VideoManagement.Domain/Entities/Video.cs`
- `src/Core/VideoProcessing.VideoManagement.Domain/Entities/VideoChunk.cs` _(novo)_

**Application:**
- `src/Core/VideoProcessing.VideoManagement.Application/Ports/IVideoChunkRepository.cs` _(novo)_
- `src/Core/VideoProcessing.VideoManagement.Application/Models/UpdateVideoInputModel.cs`
- `src/Core/VideoProcessing.VideoManagement.Application/Models/VideoResponseModel.cs`
- `src/Core/VideoProcessing.VideoManagement.Application/UseCases/UpdateVideoUseCase.cs`
- `src/Core/VideoProcessing.VideoManagement.Application/UseCases/GetVideoByIdUseCase.cs`

**Infra.Data:**
- `src/Infra/VideoProcessing.VideoManagement.Infra.Data/Options/DynamoDbOptions.cs`
- `src/Infra/VideoProcessing.VideoManagement.Infra.Data/Repositories/VideoChunkRepository.cs` _(novo)_
- `src/Infra/VideoProcessing.VideoManagement.Infra.Data/Repositories/VideoRepository.cs`
- `src/Infra/VideoProcessing.VideoManagement.Infra.Data/Mappers/VideoMapper.cs`

**Infra.CrossCutting:**
- `src/Infra/VideoProcessing.VideoManagement.Infra.CrossCutting/DependencyInjection/ServiceCollectionExtensions.cs`

**Config:**
- `src/InterfacesExternas/VideoProcessing.VideoManagement.Api/appsettings.json`
- `src/InterfacesExternas/VideoProcessing.VideoManagement.Api/appsettings.Development.json`
- `src/InterfacesExternas/VideoProcessing.VideoManagement.LambdaUpdateVideo/appsettings.json` _(se existir)_

**Pipelines:**
- `.github/workflows/deploy-lambda-video-management.yml`
- `.github/workflows/deploy-lambda-update-video.yml`

**Testes:**
- `tests/VideoProcessing.VideoManagement.UnitTests/`

## Escopo Técnico
- Tecnologias: .NET 10, C# 13, AWS DynamoDB SDK, AWSSDK.S3 (URL pré-assinada), Clean Architecture
- Pacotes/Dependências:
  - AWSSDK.DynamoDBv2 (versão já em uso)
  - AWSSDK.S3 (versão já em uso — verificar se já disponível no projeto Api)

## Dependências e Riscos (para estimativa)
- **Dependências:** Story 16 (campos `maxParallelChunks`, `ProcessingSummary`, timestamps) deve estar concluída antes do desenvolvimento.
- **Pré-condições:** Tabela DynamoDB `video-processing-engine-dev-video-chunks` provisionada na infraestrutura com design `pk=VIDEO#{videoId}`, `sk=CHUNK#{chunkId}`.
- **Riscos:**
  - Renomear `maxParallelChunks` → `parallelChunks` requer migração silenciosa dos dados existentes no DynamoDB (campo antigo pode coexistir temporariamente; mapper deve ler ambos durante transição).
  - URL pré-assinada exige que a role Lambda tenha permissão `s3:GetObject` no bucket ZIP.
  - Contagem de chunks assume que cada chunk reporta status individualmente; se o pipeline não enviar todos os chunks, o progresso ficará abaixo de 100% mesmo quando concluído — mitigar com regra: se status do vídeo for `Completed`, retornar `progressPercent = 100`.

## Observações Arquiteturais
- **Separação de responsabilidades entre Lambdas:** o Lambda `UpdateStatus` continua sendo o único escritor de estado. O Lambda `Api` é somente leitor — nunca grava chunks diretamente.
- **Idempotência:** `UpsertAsync` no `VideoChunkRepository` deve usar `PutItem` com semântica de replace (não condicional), já que um chunk pode ser re-reportado pela Step Function em caso de retry.
- **Progresso monotônico:** a lógica monotônica de status no `VideoRepository.UpdateAsync` é preservada; o `progressPercent` calculado dinamicamente não precisa de restrição monotônica separada pois é derivado de contagem.
- **URL pré-assinada:** gerada no UseCase (Application), injetando `IAmazonS3` via port ou serviço dedicado; TTL configurável via `S3Options.PresignedUrlTtlMinutes` já existente.
- **Step Function:** ajuste mínimo esperado — apenas incluir `ZipBucket`, `ZipKey` e `ZipFileName` no payload da última task que invoca o Lambda UpdateStatus. Nenhuma mudança de fluxo ou estado.
- **Retrocompatibilidade:** o campo `parallelChunks` no DynamoDB será gravado com o novo nome a partir desta story; registros anteriores com `maxParallelChunks` serão lidos com fallback no mapper.

## Subtasks
- [Subtask 01: Configuração ChunksTableName por variável de ambiente](./subtask/Subtask-01-Configuracao_ChunksTableName_Env.md)
- [Subtask 02: Renomear maxParallelChunks para parallelChunks](./subtask/Subtask-02-Rename_MaxParallelChunks_ParallelChunks.md)
- [Subtask 03: Modelo VideoChunk e Port IVideoChunkRepository](./subtask/Subtask-03-Modelo_VideoChunk_Port.md)
- [Subtask 04: VideoChunkRepository — Repositório DynamoDB](./subtask/Subtask-04-VideoChunkRepository_DynamoDB.md)
- [Subtask 05: Campos ZIP no domínio e persistência no Lambda UpdateStatus](./subtask/Subtask-05-Campos_ZIP_Persistencia_UpdateStatus.md)
- [Subtask 06: GET dinâmico — progressPercent e URL pré-assinada ZIP](./subtask/Subtask-06-GET_Progresso_Dinamico_PresignedUrl.md)
- [Subtask 07: Testes unitários](./subtask/Subtask-07-Testes_Unitarios.md)

## Critérios de Aceite da História
- [ ] `DynamoDbOptions` possui campo `ChunksTableName`; valor lido de `DynamoDB__ChunksTableName` sem hardcode no código; default `video-processing-engine-dev-video-chunks`.
- [ ] Ambos os pipelines de deploy passam `DynamoDB__ChunksTableName` via `vars.DYNAMODB_CHUNKS_TABLE_NAME`.
- [ ] Campo renomeado de `maxParallelChunks` para `parallelChunks` em domínio, contratos, mapper e pipelines; retrocompatibilidade de leitura no mapper para registros antigos.
- [ ] Lambda UpdateStatus persiste ou atualiza o chunk individual na tabela de chunks a cada mensagem recebida.
- [ ] Lambda UpdateStatus persiste `ZipBucket`, `ZipKey` e `ZipFileName` no item principal do vídeo quando presentes no payload.
- [ ] GET `/videos/{id}` calcula `progressPercent` dinamicamente: `(chunksProcessados / parallelChunks) * 100`; se `parallelChunks` ausente, assume `1`; se status `Completed`, retorna `100`.
- [ ] GET `/videos/{id}` expõe `ZipFileName` e `ZipUrl` (URL pré-assinada S3 com TTL configurável) quando `ZipKey` estiver presente no vídeo.
- [ ] Testes unitários passando com cobertura ≥ 80% nos novos use cases e repositórios.
- [ ] `dotnet build` e `dotnet test` passam sem erros nos dois Lambdas.

## Rastreamento (dev tracking)
- **Início:** 12/03/2026, às 15:14 (Brasília)
- **Fim:** —
- **Tempo total de desenvolvimento:** —
