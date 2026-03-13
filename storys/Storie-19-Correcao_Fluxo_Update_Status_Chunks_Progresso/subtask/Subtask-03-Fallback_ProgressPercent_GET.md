# Subtask-03: Corrigir fallback de ProgressPercent no GetVideoByIdUseCase

## Descrição
O `GetVideoByIdUseCase` recalcula o `progressPercent` a partir da tabela de chunks, ignorando completamente o valor armazenado na entidade `Video.ProgressPercent`. Se a tabela de chunks estiver vazia (situação atual), o progresso retornado no GET é sempre 0% — mesmo que o evento tenha enviado `progressPercent=50` e ele esteja persistido na entidade. Esta subtask corrige o cálculo para usar o valor armazenado como _floor_ (piso), garantindo que o progresso nunca regrida na exibição.

---

## Contexto técnico

### Comportamento atual
```csharp
var progressPercent = video.Status == VideoStatus.Completed
    ? 100
    : ComputeProgressPercent(video.ParallelChunks ?? 0,
        await chunkRepository.CountProcessedAsync(videoId, ct));
```
- Para `Completed`: retorna 100 ✓
- Para não-`Completed` com chunks vazios: retorna 0 ✗ (ignora `video.ProgressPercent`)

### Comportamento desejado
```csharp
var chunksProcessed = await chunkRepository.CountProcessedAsync(videoId, ct);
var computedFromChunks = ComputeProgressPercent(video.ParallelChunks ?? 0, chunksProcessed);

var progressPercent = video.Status == VideoStatus.Completed
    ? 100
    : Math.Max(computedFromChunks, video.ProgressPercent);
```
- Para `Completed`: retorna 100 ✓
- Para não-`Completed` com chunks vazios e `video.ProgressPercent=50`: retorna 50 ✓
- Para não-`Completed` com `computedFromChunks=75` e `video.ProgressPercent=50`: retorna 75 ✓
- Nunca retorna valor menor que o armazenado ✓

### Regra de não-regressão
A guard de não-regressão na **persistência** já existe em dois níveis:
- `Video.SetProgress()` lança `InvalidOperationException` se `percent < ProgressPercent`
- `VideoRepository.UpdateAsync` tem `ConditionExpression: progressPercent <= :newProgress`

O `Math.Max` na **exibição** é complementar: garante consistência na camada de leitura mesmo durante janelas de inconsistência transitória.

---

## Passos de implementação

1. **Extrair a chamada ao repositório de chunks** para variável local antes do condicional:
   ```csharp
   var chunksProcessed = await chunkRepository.CountProcessedAsync(videoId, ct);
   var computedFromChunks = ComputeProgressPercent(video.ParallelChunks ?? 0, chunksProcessed);
   ```

2. **Substituir o cálculo de `progressPercent`:**
   ```csharp
   var progressPercent = video.Status == VideoStatus.Completed
       ? 100
       : Math.Max(computedFromChunks, video.ProgressPercent);
   ```

3. **Manter o método privado `ComputeProgressPercent` inalterado** — apenas o local de chamada muda.

4. **Verificar que o `response with { ProgressPercent = progressPercent }** continua sobrescrevendo o valor do mapper** — o mapper já retorna `video.ProgressPercent`; o `with` garante que o valor calculado/corrigido prevalece. Nenhuma alteração no mapper é necessária.

---

## Formas de teste

1. **Teste unitário — GetVideoByIdUseCase (novo):** vídeo com `Status=ProcessingImages`, `ProgressPercent=50` armazenado, `chunkRepository.CountProcessedAsync` retorna 0, `ParallelChunks=4` → resposta deve ter `ProgressPercent=50`.
2. **Teste unitário:** vídeo com `Status=ProcessingImages`, `ProgressPercent=30` armazenado, `CountProcessedAsync` retorna 2, `ParallelChunks=4` → `computedFromChunks = (2*100)/4 = 50` → resposta deve ter `ProgressPercent=50`.
3. **Teste unitário:** vídeo com `Status=Completed` → resposta deve ter `ProgressPercent=100` independente de `video.ProgressPercent` e do estado dos chunks.
4. **Teste unitário:** vídeo com `Status=ProcessingImages`, `ProgressPercent=0`, `CountProcessedAsync` retorna 0, `ParallelChunks=null` → `computedFromChunks=(0*100)/1=0`, `Math.Max(0,0)=0` → resposta deve ter `ProgressPercent=0`.

---

## Critérios de aceite da subtask

- [ ] GET de vídeo `Completed` sempre retorna `progressPercent=100`.
- [ ] GET de vídeo não-`Completed` retorna o maior entre `ComputeProgressPercent(chunks)` e `video.ProgressPercent`.
- [ ] GET de vídeo com tabela de chunks vazia e `ProgressPercent=50` armazenado retorna `progressPercent=50`.
- [ ] `dotnet test` passa sem falhas; testes existentes de `GetVideoByIdUseCase` não regridem.
