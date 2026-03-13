# Subtask-02: Criar chunk de finalização no UpdateVideoUseCase

## Descrição
O `UpdateVideoUseCase.ExecuteAsync` só insere registros na tabela de chunks quando `merged.ProcessingSummary?.Chunks` é não nulo e não vazio. O evento de finalização (`status=Completed`) enviado pelo processing engine não contém `processingSummary.chunks`, então a tabela permanece vazia. Esta subtask adiciona um fallback: quando o status resultante for `Completed` (ou `GeneratingZip`) e nenhum chunk tiver sido persistido via `processingSummary`, criar um registro sintético de finalização na tabela de chunks usando `ChunkId = "finalize"`.

---

## Contexto técnico
- O bloco de upsert existente é `fire-and-warn` (falha não aborta o update principal) — o fallback deve seguir o mesmo padrão.
- O chunk sintético usa `ChunkId = "finalize"` para distinguir de chunks reais numerados. A tabela usa SK = `CHUNK#{chunkId}`, portanto não há colisão.
- O `UpdateVideoInputModel` não expõe diretamente o `FinalizeInfo`; o use case recebe apenas o `UpdateVideoInputModel` padrão. A passagem do `FinalizeInfo` (mapeado na Subtask-01) deve ser feita pela borda Lambda (`UpdateVideoHandler`) como campos já existentes no `UpdateVideoInputModel` — `FramesPrefix` e `S3BucketFrames` — que já estão presentes no evento de finalização. Assim, **não é necessário alterar a interface do use case**.

---

## Passos de implementação

1. **Identificar o ponto de inserção do fallback** no `UpdateVideoUseCase.ExecuteAsync`: logo após o bloco existente de upsert via `processingSummary.chunks`:

   ```csharp
   // bloco existente — inalterado
   if (merged.ProcessingSummary?.Chunks is { } chunks && chunks.Count > 0)
   {
       // ... upsert dos chunks do processingSummary
   }

   // NOVO: fallback para evento de finalização sem chunks
   else if (merged.Status == VideoStatus.Completed || merged.Status == VideoStatus.GeneratingZip)
   {
       try
       {
           var finalizationChunk = new VideoChunk(
               ChunkId: "finalize",
               VideoId: videoId.ToString(),
               Status: merged.Status == VideoStatus.Completed ? "completed" : "processing",
               StartSec: 0,
               EndSec: 0,
               IntervalSec: 0,
               ManifestPrefix: null,
               FramesPrefix: string.IsNullOrEmpty(merged.FramesPrefix) ? null : merged.FramesPrefix,
               ProcessedAt: merged.Status == VideoStatus.Completed ? DateTime.UtcNow : null,
               CreatedAt: DateTime.UtcNow);
           await chunkRepository.UpsertAsync(finalizationChunk, ct);
           logger.LogInformation(
               "Chunk de finalização inserido para VideoId: {VideoId}, Status: {Status}",
               videoId, merged.Status);
       }
       catch (Exception ex)
       {
           logger.LogWarning(ex, "Falha ao persistir chunk de finalização do vídeo {VideoId}; atualização principal mantida.", videoId);
       }
   }
   ```

2. **Verificar que `merged.FramesPrefix`** é preenchido a partir de `input.FramesPrefix` (que vem no evento como `framesPrefix`) — isso já ocorre via `patch.FramesPrefix ?? existing.FramesPrefix` em `Video.FromMerge`. Nenhuma alteração adicional necessária.

3. **Garantir idempotência:** o `IVideoChunkRepository.UpsertAsync` usa `PutItem` com SK = `CHUNK#finalize`, portanto invocações repetidas do mesmo evento sobrescrevem o mesmo item — comportamento correto.

---

## Formas de teste

1. **Teste unitário — UpdateVideoUseCaseTests:** cenário onde `input.ProcessingSummary` é `null` e `input.Status = VideoStatus.Completed`; verificar que `chunkRepository.UpsertAsync` é chamado exatamente uma vez com `ChunkId = "finalize"` e `Status = "completed"`.
2. **Teste unitário — UpdateVideoUseCaseTests:** cenário onde `processingSummary.chunks` tem entradas; verificar que o fallback NÃO é executado (o bloco existente já cobre; `UpsertAsync` não é chamado com `ChunkId = "finalize"`).
3. **Teste unitário — UpdateVideoUseCaseTests:** cenário onde `chunkRepository.UpsertAsync` lança exceção no fallback; verificar que `UpdateAsync` do repositório principal não é afetado e o use case retorna o `VideoResponseModel` normalmente.

---

## Critérios de aceite da subtask

- [ ] Quando `status=Completed` e `processingSummary` é `null`, exatamente um registro com `ChunkId="finalize"` e `Status="completed"` é persistido na tabela de chunks.
- [ ] Quando `processingSummary.chunks` está preenchido, o fallback não é executado e somente os chunks reais são upsertados.
- [ ] Falha no upsert do chunk de finalização não propaga exceção para o chamador do use case.
- [ ] `dotnet test` passa sem falhas; testes existentes de `UpdateVideoUseCase` não regridem.
