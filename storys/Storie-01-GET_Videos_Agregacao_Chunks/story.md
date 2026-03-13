# Storie-01: GET /videos — Visão Agregada por Chunks

## Status
- **Estado:** ⏸️ Aguardando desenvolvimento
- **Data de Conclusão:** [DD/MM/AAAA]

---

## Descrição
Como consumidor da API de gerenciamento de vídeos (frontend ou Postman), quero que os endpoints GET /videos e GET /videos/{id} retornem o progresso real do processamento calculado a partir dos chunks, para que eu veja uma visão precisa e amigável do estado de cada vídeo no modelo fan-out.

---

## Objetivo
Evoluir os endpoints GET de listagem e detalhe de vídeo para que passem a retornar uma visão **agregada e calculada dinamicamente** a partir dos registros de chunk no DynamoDB, substituindo a dependência do `progressPercent` gravado no item principal por um cálculo real. O response deve ficar claro para exibição em frontend e apresentação do hackathon, sem expor internals desnecessários. A compatibilidade com vídeos antigos (sem chunks) deve ser mantida.

---

## Contexto Técnico (situação atual)

### O que existe hoje
- `GetVideoByIdUseCase` já consulta chunks, mas de forma incompleta:
  - Usa `CountProcessedAsync` que conta apenas chunks com `status = "completed"`.
  - Usa `parallelChunks` do item de vídeo como denominador (pode divergir do total real de chunks gravados).
  - Não exclui o item `finalize` da contagem de `totalChunks`.
  - Não detalha pendentes, em processamento ou com falha.
  - Não expõe `chunksSummary`, `currentStage`, nem lista de chunks.

- `ListVideosUseCase` **não consulta chunks**:
  - Retorna `progressPercent` diretamente do item principal gravado no DynamoDB.
  - Não reflete o progresso real de vídeos com chunks em andamento.

- `VideoChunkRepository` só expõe:
  - `UpsertAsync`
  - `CountProcessedAsync` (count de completed)

- `VideoResponseModel` possui `ParallelChunks` e `ProcessingSummary` (mapa de `ChunkInfo`) mas **não possui** `chunksSummary` (breakdown por status), `currentStage`, `totalChunks`, `completedChunks`, `processingChunks`, `failedChunks`, `pendingChunks`.

### Estrutura DynamoDB dos chunks
- Tabela separada: `ChunksTableName` (via `DynamoDbOptions`)
- PK: `VIDEO#{videoId}` | SK: `CHUNK#{chunkId}`
- Campos: `chunkId`, `videoId`, `status`, `startSec`, `endSec`, `intervalSec`, `createdAt`, `manifestPrefix`, `framesPrefix`, `processedAt`
- Item especial `finalize` (SK = `CHUNK#finalize`) — **não deve ser contado em `totalChunks`**

---

## Regras de Negócio (progresso e stage)

### Cálculo do `progressPercent`
| Condição | Valor |
|---|---|
| Sem chunks (vídeo antigo / SingleLambda) | `video.ProgressPercent` salvo |
| Com chunks, processando | `floor((completedChunks / totalChunks) * 100)` capped em 94 |
| Com chunks, todos completed mas finalize ainda não completed | 95–99 (fixo em 97) |
| Item `finalize` com `status = completed` | 100 |
| `video.Status == Completed` | 100 (fallback de segurança) |
| `video.Status == Failed` | mantém o percentual calculado até a falha |

### `totalChunks`
- Total de registros de chunk do vídeo, **excluindo** o item com `chunkId = "finalize"`.

### Mapeamento de `currentStage` (amigável)
| Condição | Stage |
|---|---|
| `video.Status == UploadPending` | `"Upload pendente"` |
| `video.Status == ProcessingImages` (com chunks) | `"Processando chunks"` |
| `video.Status == GeneratingZip` | `"Gerando ZIP"` |
| `video.Status == Completed` | `"Concluído"` |
| `video.Status == Failed` | `"Falhou"` |
| `video.Status == Cancelled` | `"Cancelado"` |
| `video.Status == ProcessingImages` (sem chunks) | `"Processando"` |

---

## Response Esperado

### GET /videos/{id} — campos novos/ajustados

```json
{
  "videoId": "...",
  "userId": "...",
  "originalFileName": "...",
  "status": "ProcessingImages",
  "statusDescription": "Processando chunks",
  "progressPercent": 66,
  "processingMode": "FanOut",
  "currentStage": "Processando chunks",
  "totalChunks": 3,
  "completedChunks": 2,
  "processingChunks": 1,
  "failedChunks": 0,
  "pendingChunks": 0,
  "chunksSummary": {
    "total": 3,
    "completed": 2,
    "processing": 1,
    "failed": 0,
    "pending": 0
  },
  "chunks": [
    { "chunkId": "chunk-0", "startSec": 0, "endSec": 15, "status": "completed" },
    { "chunkId": "chunk-1", "startSec": 15, "endSec": 30, "status": "completed" },
    { "chunkId": "chunk-2", "startSec": 30, "endSec": 45, "status": "processing" }
  ],
  "zipUrl": null,
  "processingStartedAt": "...",
  "imagesProcessingCompletedAt": null,
  "processingCompletedAt": null,
  "updatedAt": "..."
}
```

### GET /videos — campos de progresso por vídeo (sem lista de chunks)
- Todos os campos acima exceto `chunks` (lista detalhada — apenas no `GET /{id}`)
- `chunksSummary` presente quando `processingMode = FanOut`

---

## Escopo Técnico

- **Tecnologias:** .NET 10, C# 13, AWS DynamoDB SDK, Clean Architecture
- **Arquivos afetados:**
  - `src/Core/.../Domain/Entities/VideoChunk.cs` — possível adição de constante `FinalizeChunkId`
  - `src/Core/.../Application/Ports/IVideoChunkRepository.cs` — novo método `GetStatusSummaryAsync`
  - `src/Core/.../Application/UseCases/GetVideoById/GetVideoByIdUseCase.cs` — refatorar lógica de progresso
  - `src/Core/.../Application/UseCases/ListVideos/ListVideosUseCase.cs` — enriquecer com chunks
  - `src/Core/.../Application/Models/ResponseModels/VideoResponseModel.cs` — novos campos
  - `src/Core/.../Application/Models/ResponseModels/ChunksSummaryResponseModel.cs` — novo record
  - `src/Core/.../Application/Models/ResponseModels/ChunkItemResponseModel.cs` — novo record (lista resumida)
  - `src/Core/.../Application/Services/ChunkProgressCalculator.cs` — novo serviço de cálculo
  - `src/Core/.../Application/Services/IChunkProgressCalculator.cs` — interface do serviço
  - `src/Core/.../Application/Mappers/VideoResponseModelMapper.cs` — ajustar mapeamento
  - `src/Infra/.../Repositories/VideoChunkRepository.cs` — implementar `GetStatusSummaryAsync`
  - `tests/.../UseCases/GetVideoById/GetVideoByIdUseCaseTests.cs` — atualizar e ampliar
  - `tests/.../UseCases/ListVideos/ListVideosUseCaseTests.cs` — atualizar e ampliar
  - `tests/.../Services/ChunkProgressCalculatorTests.cs` — novos testes unitários
  - `tests/.../Repositories/VideoChunkRepositoryTests.cs` — testar novo método

- **Componentes criados/modificados:**
  - `ChunkProgressCalculator` — serviço de aplicação para calcular progresso, stage e summary
  - `IChunkProgressCalculator` — interface para DI e testabilidade
  - `ChunksSummaryResponseModel` — DTO do summary de chunks
  - `ChunkItemResponseModel` — DTO resumido de cada chunk (para lista no GetById)
  - `GetStatusSummaryAsync` — novo método no repositório de chunks

- **Pacotes/Dependências:** nenhum pacote novo — apenas reuso do AWSSDK.DynamoDBv2 já existente

---

## Dependências e Riscos

- **Dependências:**
  - A tabela de chunks deve estar populada corretamente pelo pipeline fan-out (Lambda de extração de frames).
  - O item `finalize` deve ter `chunkId = "finalize"` — confirmar convenção com o pipeline.
  - `DynamoDbOptions.ChunksTableName` deve estar configurado no ambiente.

- **Riscos:**
  - **N+1 no `GET /videos`:** consultar chunks para cada vídeo da lista pode gerar múltiplas queries ao DynamoDB. **Mitigação:** usar `Task.WhenAll` para paralelizar e limitar enriquecimento apenas a vídeos com `processingMode = FanOut` e que não estejam em `Completed` ou `Cancelled`.
  - **Divergência do item `finalize`:** se o pipeline gravar o item com chunkId diferente de `"finalize"`, a exclusão não funcionará. Mitigação: constante compartilhada no domínio.
  - **Compatibilidade retroativa:** vídeos antigos sem chunks devem funcionar sem alteração visual. Mitigação: toda lógica nova é condicional ao resultado da consulta de chunks.
  - **Mudança de contrato do `VideoResponseModel`:** adicionar campos é retrocompatível (JSON opcional). Não remover campos existentes.

---

## Subtasks

- [Subtask 01: Expandir IVideoChunkRepository e VideoChunkRepository com GetStatusSummaryAsync](./subtask/Subtask-01-Expandir_VideoChunkRepository.md)
- [Subtask 02: Criar ChunkProgressCalculator com lógica de progresso e stage](./subtask/Subtask-02-ChunkProgressCalculator.md)
- [Subtask 03: Evoluir VideoResponseModel com campos de agregação de chunks](./subtask/Subtask-03-Evoluir_VideoResponseModel.md)
- [Subtask 04: Refatorar GetVideoByIdUseCase para usar novo calculador](./subtask/Subtask-04-Refatorar_GetVideoByIdUseCase.md)
- [Subtask 05: Enriquecer ListVideosUseCase com agregação de chunks (com controle de N+1)](./subtask/Subtask-05-Enriquecer_ListVideosUseCase.md)
- [Subtask 06: Atualizar e ampliar testes unitários](./subtask/Subtask-06-Testes_Unitarios.md)

---

## Critérios de Aceite da História

- [ ] `GET /videos/{id}` retorna `progressPercent` calculado dinamicamente a partir dos chunks quando `processingMode = FanOut`, não apenas o valor salvo no item principal
- [ ] `totalChunks` não conta o item `finalize` do DynamoDB
- [ ] Quando o item `finalize` tem `status = completed`, `progressPercent` é 100 e `status` é `Completed`
- [ ] `chunksSummary` retorna contagem correta de `total`, `completed`, `processing`, `failed` e `pending`
- [ ] `currentStage` exibe texto amigável correspondente ao estado atual do processamento
- [ ] `GET /videos/{id}` inclui lista resumida `chunks` com `chunkId`, `startSec`, `endSec` e `status`
- [ ] `GET /videos` (listagem) retorna `progressPercent` calculado e `chunksSummary` para vídeos FanOut, sem expor lista detalhada de chunks
- [ ] Vídeos antigos (sem chunks / `processingMode = SingleLambda`) continuam funcionando corretamente com valores do item principal
- [ ] Enriquecimento por chunks no `GET /videos` usa `Task.WhenAll` e é restrito a vídeos `FanOut` ainda em processamento
- [ ] Testes unitários passando com cobertura ≥ 80% nos use cases e no `ChunkProgressCalculator`
- [ ] Nenhum campo existente do `VideoResponseModel` foi removido (compatibilidade retroativa garantida)

---

## Rastreamento (dev tracking)
- **Início:** 13/03/2026, às 15:36 (Brasília)
- **Fim:** —
- **Tempo total de desenvolvimento:** —
