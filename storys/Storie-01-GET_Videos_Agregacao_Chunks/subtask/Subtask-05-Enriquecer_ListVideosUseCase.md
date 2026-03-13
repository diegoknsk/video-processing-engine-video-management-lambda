# Subtask-05: Enriquecer ListVideosUseCase com agregação de chunks (com controle de N+1)

## Descrição
Evoluir o `ListVideosUseCase` para enriquecer os vídeos da listagem com `progressPercent` calculado e `chunksSummary` quando `processingMode = FanOut`. Usar paralelismo via `Task.WhenAll` para minimizar latência e restringir o enriquecimento a vídeos que realmente precisam (evitar N+1 desnecessário).

---

## Contexto
Hoje o `ListVideosUseCase` não consulta a tabela de chunks e retorna o `progressPercent` gravado diretamente no item de vídeo, que pode estar desatualizado para vídeos com processamento fan-out em andamento. Para a listagem, o usuário precisa ver o progresso aproximado real — mas **sem a lista detalhada de `chunks`** (que fica apenas no `GET /videos/{id}`).

### Estratégia para evitar N+1
- **Somente enriquecer vídeos elegíveis:** `processingMode == FanOut` **E** `status ∉ { Completed, Cancelled }`
  - Vídeos já concluídos ou cancelados não precisam recalcular (são estáveis)
  - Vídeos SingleLambda não têm chunks
- **Paralelismo:** usar `Task.WhenAll` para disparar todas as queries de chunks simultaneamente
- **Limite prático:** com paginação de 50 itens (padrão), o pior caso é 50 queries paralelas ao DynamoDB — aceitável para o contexto do hackathon

---

## Passos de Implementação

1. **Injetar dependências** — adicionar ao construtor primário do use case:
   - `IVideoChunkRepository chunkRepository`
   - `IChunkProgressCalculator calculator`

2. **Após obter a lista de vídeos, identificar os elegíveis**:
   ```csharp
   var eligibleForEnrichment = items
       .Where(v => v.ProcessingMode == ProcessingMode.FanOut
                && v.Status != VideoStatus.Completed
                && v.Status != VideoStatus.Cancelled)
       .ToList();
   ```

3. **Disparar consultas de chunks em paralelo**:
   ```csharp
   var summaryTasks = eligibleForEnrichment
       .ToDictionary(
           v => v.VideoId,
           v => chunkRepository.GetStatusSummaryAsync(v.VideoId, ct)
       );
   await Task.WhenAll(summaryTasks.Values);
   var summaries = summaryTasks.ToDictionary(
       kv => kv.Key,
       kv => kv.Value.Result
   );
   ```

4. **Mapear os vídeos aplicando enriquecimento condicional**:
   ```csharp
   var videos = items.Select(v =>
   {
       var model = VideoResponseModelMapper.ToResponseModel(v);
       if (!summaries.TryGetValue(v.VideoId, out var summary))
           return model;

       var progressResult = calculator.Calculate(v.Status, summary);
       return model with
       {
           ProgressPercent = progressResult.HasChunks
               ? progressResult.ProgressPercent
               : v.ProgressPercent,
           CurrentStage = progressResult.CurrentStage,
           TotalChunks = summary.Total > 0 ? summary.Total : null,
           CompletedChunks = summary.Total > 0 ? summary.Completed : null,
           ProcessingChunks = summary.Total > 0 ? summary.Processing : null,
           FailedChunks = summary.Total > 0 ? summary.Failed : null,
           PendingChunks = summary.Total > 0 ? summary.Pending : null,
           ChunksSummary = summary.Total > 0
               ? new ChunksSummaryResponseModel(...)
               : null,
           Chunks = null  // lista detalhada não exposta no GET /videos
       };
   }).ToList();
   ```

5. **Garantir que `Chunks` nunca é preenchido na listagem** — campo `Chunks` permanece `null` para todos os itens da listagem (lista de chunks é exclusiva do `GET /videos/{id}`).

---

## Formas de Teste

1. **Teste de elegibilidade** — mock com 5 vídeos: 2 FanOut em andamento, 1 FanOut Completed, 1 SingleLambda, 1 FanOut Cancelled; verificar que apenas os 2 FanOut em andamento tiveram `GetStatusSummaryAsync` chamado

2. **Teste de paralelismo** — verificar que `GetStatusSummaryAsync` foi chamado com `await Task.WhenAll` (todos os mocks recebem a chamada antes de qualquer `await` individual)

3. **Teste de compatibilidade** — vídeos não elegíveis retornam `ChunksSummary = null`, `Chunks = null`, `ProgressPercent = video.ProgressPercent` original

4. **Teste de `progressPercent` calculado** — mock de `GetStatusSummaryAsync` retornando `summary` com 2 completed de 3; verificar que o vídeo na listagem tem `ProgressPercent = 66`

5. **Teste de paginação** — verificar que `NextToken` ainda é passado corretamente e o `VideoListResponseModel` permanece íntegro após a evolução

---

## Critérios de Aceite

- [ ] `ListVideosUseCase` injeta `IVideoChunkRepository` e `IChunkProgressCalculator`
- [ ] Enriquecimento com chunks é restrito a vídeos `FanOut` com status diferente de `Completed` e `Cancelled`
- [ ] As queries de chunks são executadas com `Task.WhenAll` (paralelismo)
- [ ] `Chunks` (lista detalhada) permanece `null` em todos os itens do `GET /videos`
- [ ] Vídeos não elegíveis retornam `progressPercent` e campos novos inalterados (compatibilidade retroativa)
- [ ] Testes unitários cobrindo elegibilidade, paralelismo e mapeamento correto do response
