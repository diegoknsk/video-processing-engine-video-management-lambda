# Subtask-03: Evoluir VideoResponseModel com campos de agregação de chunks

## Descrição
Adicionar ao `VideoResponseModel` os novos campos de agregação de chunks (`chunksSummary`, `currentStage`, `totalChunks`, `completedChunks`, `processingChunks`, `failedChunks`, `pendingChunks`) e criar os novos DTOs auxiliares `ChunksSummaryResponseModel` e `ChunkItemResponseModel`. Nenhum campo existente deve ser removido.

---

## Contexto
O `VideoResponseModel` é um `record` com `init` properties. A evolução consiste em **adicionar** campos opcionais (`int?`, `string?`, tipos nulável), garantindo que respostas de vídeos antigos simplesmente omitam esses campos (serialização JSON com `JsonIgnore(Condition = WhenWritingNull)`).

O campo `chunks` (lista detalhada dos chunks) deve aparecer **apenas no `GET /videos/{id}`** — não no `GET /videos`. Para isso, o mapper será chamado com um parâmetro adicional ou o use case montará o modelo de forma diferente.

---

## Passos de Implementação

1. **Criar `ChunksSummaryResponseModel`** — `src/Core/.../Application/Models/ResponseModels/ChunksSummaryResponseModel.cs`:
   ```csharp
   public record ChunksSummaryResponseModel(
       int Total,
       int Completed,
       int Processing,
       int Failed,
       int Pending
   );
   ```

2. **Criar `ChunkItemResponseModel`** — `src/Core/.../Application/Models/ResponseModels/ChunkItemResponseModel.cs`:
   ```csharp
   public record ChunkItemResponseModel(
       string ChunkId,
       double StartSec,
       double EndSec,
       string Status
   );
   ```

3. **Adicionar campos ao `VideoResponseModel`** — apenas adição, sem remoção dos campos existentes:
   ```csharp
   // Agregação de chunks (fan-out)
   public string? CurrentStage { get; init; }
   public int? TotalChunks { get; init; }
   public int? CompletedChunks { get; init; }
   public int? ProcessingChunks { get; init; }
   public int? FailedChunks { get; init; }
   public int? PendingChunks { get; init; }
   public ChunksSummaryResponseModel? ChunksSummary { get; init; }
   public IReadOnlyList<ChunkItemResponseModel>? Chunks { get; init; }
   ```
   - Usar `[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]` em todos os novos campos para não poluir o response de vídeos antigos

4. **Ajustar `VideoResponseModelMapper`** — o método `ToResponseModel(Video video)` existente pode continuar igual (novos campos ficarão `null`). O preenchimento dos novos campos será feito nos use cases via `record with { ... }`.
   - Alternativa: adicionar overload `ToResponseModel(Video video, ChunkStatusSummary? summary, IReadOnlyList<VideoChunk>? chunks = null)` que encapsula o `with { ... }` — avalia junto com a Subtask-04.

5. **Verificar serialização JSON** — garantir que a ordem dos campos no JSON seja clara para o consumidor (campos novos de chunk agrupados logicamente após `progressPercent` e `processingMode`).

---

## Formas de Teste

1. **Testes de serialização** — criar instâncias de `VideoResponseModel` com e sem os novos campos e verificar que o JSON serializado contém apenas os campos preenchidos (nulos omitidos via `WhenWritingNull`)

2. **Testes do mapper** — verificar que `VideoResponseModelMapper.ToResponseModel(video)` continua mapeando os campos existentes sem regressão

3. **Validação manual via Postman** — chamar `GET /videos/{id}` e confirmar presença dos novos campos; chamar `GET /videos` e confirmar ausência da lista `chunks` mas presença de `chunksSummary` quando aplicável

---

## Critérios de Aceite

- [ ] `ChunksSummaryResponseModel` e `ChunkItemResponseModel` criados como `record` no layer Application
- [ ] `VideoResponseModel` possui os 8 novos campos (`CurrentStage`, `TotalChunks`, `CompletedChunks`, `ProcessingChunks`, `FailedChunks`, `PendingChunks`, `ChunksSummary`, `Chunks`)
- [ ] Todos os novos campos são anuláveis (`?`) e decorados com `[JsonIgnore(Condition = WhenWritingNull)]`
- [ ] Nenhum campo existente do `VideoResponseModel` foi removido ou renomeado
- [ ] Testes do mapper existente continuam passando sem alteração
