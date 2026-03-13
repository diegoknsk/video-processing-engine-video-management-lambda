# Subtask-04: Refatorar GetVideoByIdUseCase para usar novo calculador

## Descrição
Refatorar o `GetVideoByIdUseCase` para substituir a lógica de progresso incompleta atual pelo novo `IChunkProgressCalculator` e pelo `GetStatusSummaryAsync` do repositório. Adicionar a consulta da lista resumida de chunks (sem o item `finalize`) para preencher o campo `chunks` do response do `GET /videos/{id}`.

---

## Contexto
O use case atual:
- Chama `chunkRepository.CountProcessedAsync(videoId)` — conta apenas completed
- Usa `parallelChunks` do item de vídeo como denominador — pode divergir do total real
- Não expõe `chunksSummary`, `currentStage`, nem lista de chunks
- Não exclui o item `finalize` de nenhuma conta

Após a refatoração:
- Chama `chunkRepository.GetStatusSummaryAsync(videoId)` — retorna breakdown completo
- Usa `IChunkProgressCalculator.Calculate(video.Status, summary)` para o progresso
- Monta `chunksSummary` a partir do `ChunkStatusSummary`
- Busca lista de `VideoChunk` para montar `chunks` resumidos
- Mantém fallback para vídeos sem chunks (compatibilidade retroativa)

---

## Passos de Implementação

1. **Injetar `IChunkProgressCalculator`** no construtor primário do use case (além das dependências já existentes).

2. **Substituir `CountProcessedAsync` por `GetStatusSummaryAsync`**:
   ```csharp
   var summary = await chunkRepository.GetStatusSummaryAsync(videoId, ct);
   ```

3. **Calcular progresso com o novo serviço**:
   ```csharp
   var progressResult = calculator.Calculate(video.Status, summary);
   var progressPercent = progressResult.HasChunks
       ? progressResult.ProgressPercent
       : video.ProgressPercent; // compatibilidade com vídeos antigos
   ```

4. **Buscar lista resumida de chunks** — adicionar método `GetAllAsync(videoId, ct)` ao repositório (ou reutilizar query existente) e filtrar o item `finalize`:
   - Ou aproveitar os dados da query de `GetStatusSummaryAsync` se já retornar a lista (avaliar trade-off de uma query vs duas)
   - A lista deve conter apenas `chunkId`, `startSec`, `endSec`, `status`

5. **Montar o response com os novos campos via `with { ... }`**:
   ```csharp
   return response with
   {
       ProgressPercent = progressPercent,
       ZipUrl = zipUrl,
       ZipFileName = video.ZipFileName ?? response.ZipFileName,
       CurrentStage = progressResult.CurrentStage,
       TotalChunks = summary?.Total,
       CompletedChunks = summary?.Completed,
       ProcessingChunks = summary?.Processing,
       FailedChunks = summary?.Failed,
       PendingChunks = summary?.Pending,
       ChunksSummary = summary != null ? new ChunksSummaryResponseModel(...) : null,
       Chunks = chunks?.Select(c => new ChunkItemResponseModel(...)).ToList()
   };
   ```

6. **Manter compatibilidade** — se `summary == null` ou `summary.Total == 0`, os novos campos ficam `null` e o `progressPercent` usa o valor salvo no item principal.

---

## Formas de Teste

1. **Testes unitários com mock de `IChunkProgressCalculator`** — verificar que o use case chama `calculator.Calculate(video.Status, summary)` e usa o resultado corretamente; cobrir caso `HasChunks = false` (usa `video.ProgressPercent`)

2. **Testes de montagem do response** — verificar que `chunksSummary`, `currentStage`, e `chunks` são preenchidos corretamente quando `summary.Total > 0`

3. **Teste de regressão — vídeo sem chunks** — mock de `GetStatusSummaryAsync` retornando `Total = 0`; verificar que `TotalChunks = null`, `chunksSummary = null`, `chunks = null` e `progressPercent = video.ProgressPercent`

4. **Teste de `progressPercent = 100`** — quando `video.Status = Completed`, independente do resultado do calculator, deve retornar 100

5. **Teste de `zipUrl`** — verificar que a geração de `zipUrl` via `IS3PresignedUrlService` continua funcionando após a refatoração

---

## Critérios de Aceite

- [ ] `GetVideoByIdUseCase` injeta e usa `IChunkProgressCalculator`
- [ ] `CountProcessedAsync` não é mais chamado neste use case (substituído por `GetStatusSummaryAsync`)
- [ ] Response inclui `currentStage`, `totalChunks`, `completedChunks`, `processingChunks`, `failedChunks`, `pendingChunks`, `chunksSummary` e `chunks`
- [ ] Vídeos antigos (sem chunks) retornam os novos campos como `null` e `progressPercent` inalterado
- [ ] Todos os testes unitários existentes continuam passando; novos testes adicionados para os cenários de chunks
