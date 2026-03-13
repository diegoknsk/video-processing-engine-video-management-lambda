# Subtask-02: Criar ChunkProgressCalculator com lógica de progresso e stage

## Descrição
Criar o serviço de aplicação `ChunkProgressCalculator` (com interface `IChunkProgressCalculator`) que encapsula toda a lógica de cálculo de `progressPercent`, `currentStage` e validação de compatibilidade com vídeos antigos. Esse serviço isola regras de negócio dos use cases e facilita testes unitários.

---

## Contexto
A lógica de cálculo de progresso hoje está acoplada diretamente no `GetVideoByIdUseCase`, incompleta e sem cobertura de todos os cenários. Ao extrair para um serviço dedicado, conseguimos:
- Reutilizar o mesmo cálculo em `GetVideoById` e `ListVideos`
- Testar as regras de progresso de forma isolada
- Evitar duplicação quando o cálculo precisar evoluir futuramente

---

## Passos de Implementação

1. **Criar a interface** — `src/Core/.../Application/Services/IChunkProgressCalculator.cs`:
   ```csharp
   public interface IChunkProgressCalculator
   {
       ChunkProgressResult Calculate(VideoStatus videoStatus, ChunkStatusSummary? summary);
   }
   ```

2. **Criar o tipo de retorno** — `ChunkProgressResult` (record):
   ```csharp
   public record ChunkProgressResult(
       int ProgressPercent,
       string CurrentStage,
       bool HasChunks
   );
   ```

3. **Implementar `ChunkProgressCalculator`** — `src/Core/.../Application/Services/ChunkProgressCalculator.cs`:

   **Regra `HasChunks`:**
   - `summary != null && summary.Total > 0`

   **Regra `ProgressPercent`:**
   ```
   if video.Status == Completed                          → 100
   else if summary == null || summary.Total == 0         → não calcula (retorna -1 para sinalizar "usar valor salvo")
   else
     base = floor((summary.Completed / summary.Total) * 100)
     if summary.FinalizeStatus == "completed"            → 100
     else if base >= 95                                  → 97 (todos chunks done, finalize em andamento)
     else                                                → Min(94, base)
   if video.Status == Failed                             → retorna o base calculado sem cap
   ```

   **Regra `CurrentStage`:**
   ```
   VideoStatus.UploadPending    → "Upload pendente"
   VideoStatus.ProcessingImages + HasChunks → "Processando chunks"
   VideoStatus.ProcessingImages + !HasChunks → "Processando"
   VideoStatus.GeneratingZip    → "Gerando ZIP"
   VideoStatus.Completed        → "Concluído"
   VideoStatus.Failed           → "Falhou"
   VideoStatus.Cancelled        → "Cancelado"
   default                      → status.ToString()
   ```

4. **Registrar no DI** — em `ApplicationServiceCollectionExtensions.cs` (ou no bootstrap do projeto):
   ```csharp
   services.AddSingleton<IChunkProgressCalculator, ChunkProgressCalculator>();
   ```

5. **Garantir que o serviço é stateless** — sem campos mutáveis; método `Calculate` é puro (dado o mesmo input, mesmo output).

---

## Formas de Teste

1. **Testes unitários por tabela de decisão** — cobrir todos os combinações relevantes de `VideoStatus` × `ChunkStatusSummary`:
   - Status `Completed` sempre retorna `ProgressPercent = 100`
   - `summary == null` retorna `HasChunks = false` e sinaliza "usar valor salvo"
   - `FinalizeStatus = "completed"` retorna `ProgressPercent = 100` independente do base
   - `base >= 95` retorna 97 (finalize em andamento)
   - `base = 66` (2 de 3 completed) retorna 66
   - `base = 0` (nenhum completed) retorna 0
   - `VideoStatus.Failed` retorna stage `"Falhou"` e progresso calculado sem forçar 100

2. **Testes de `CurrentStage`** — verificar cada mapeamento de status para texto amigável

3. **Teste de compatibilidade** — `HasChunks = false` sinaliza que o use case deve usar `video.ProgressPercent` (não sobrescrever)

---

## Critérios de Aceite

- [ ] Interface `IChunkProgressCalculator` com método `Calculate(VideoStatus, ChunkStatusSummary?)` criada no layer Application
- [ ] Implementação `ChunkProgressCalculator` registrada como `Singleton` no DI
- [ ] `progressPercent = 100` quando `FinalizeStatus = "completed"` ou `VideoStatus = Completed`
- [ ] `progressPercent = 97` quando todos chunks normais estão completed mas finalize ainda não
- [ ] `currentStage` mapeia corretamente todos os 7 casos de `VideoStatus`
- [ ] `HasChunks = false` quando `summary == null` ou `summary.Total == 0`
- [ ] Testes unitários com cobertura ≥ 90% do `ChunkProgressCalculator`
