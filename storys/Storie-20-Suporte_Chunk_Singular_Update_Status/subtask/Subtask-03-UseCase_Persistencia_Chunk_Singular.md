# Subtask-03: Adaptar UseCase para Persistir Chunk Singular via Repositório

## Descrição
Adaptar `UpdateVideoUseCase` para detectar quando `input.Chunk` (singular) está preenchido e persistir o chunk real via `IVideoChunkRepository.UpsertAsync`, com os dados corretos e o status baseado no `merged.Status`. O fallback de chunk `"finalize"` não deve ser disparado quando `input.Chunk` estiver presente.

## Passos de Implementação
1. Após o bloco existente de `processingSummary.chunks`, adicionar tratamento para `input.Chunk`:
   ```csharp
   else if (input.Chunk is { } singleChunk)
   {
       // persistir o chunk singular diretamente
   }
   ```
   Dessa forma, os três caminhos ficam mutuamente exclusivos:
   - `processingSummary.chunks` presente → persiste dicionário de chunks
   - `input.Chunk` (singular) presente → persiste o chunk singular
   - nenhum dos dois → fallback "finalize" (apenas quando `Completed` ou `GeneratingZip`)

2. No bloco do chunk singular, construir o `VideoChunk`:
   - `ChunkId`: `singleChunk.ChunkId`
   - `VideoId`: `videoId.ToString()`
   - `Status`: `"completed"` se `merged.Status == VideoStatus.Completed`, senão `"processing"`
   - `StartSec`, `EndSec`, `IntervalSec`: valores do `singleChunk`
   - `ManifestPrefix`: `singleChunk.ManifestPrefix` (null se vazio)
   - `FramesPrefix`: `singleChunk.FramesPrefix` (null se vazio)
   - `ProcessedAt`: `DateTime.UtcNow` apenas se status for `"completed"`
   - `CreatedAt`: `DateTime.UtcNow`

3. Chamar `chunkRepository.UpsertAsync(videoChunk, ct)` dentro de try/catch, com log de warning em caso de falha (manter o mesmo padrão resiliente do bloco de processingSummary).

4. Revisar a condição do bloco `else if (Completed || GeneratingZip)` de fallback `"finalize"` para garantir que só é executado quando **nem** `processingSummary.chunks` **nem** `input.Chunk` estiverem presentes.

## Formas de Teste
- Payload com `chunk` singular: confirmar que `UpsertAsync` é chamado uma vez com `ChunkId` correto.
- Payload com `chunk` singular + `status: 2` (GeneratingZip): confirmar que o chunk `"finalize"` **não** é inserido.
- Payload sem `chunk` e sem `processingSummary`, com `status: 2`: confirmar que o fallback `"finalize"` continua sendo inserido.
- Payload com `processingSummary.chunks`: confirmar que o comportamento existente é preservado (regressão).

## Critérios de Aceite
- [ ] `UpsertAsync` é chamado com o `ChunkId` real do chunk singular quando `input.Chunk != null`
- [ ] O fallback `"finalize"` não é inserido quando `input.Chunk != null`
- [ ] O fallback `"finalize"` continua sendo inserido quando `input.Chunk == null` e `processingSummary == null` e status é `Completed` ou `GeneratingZip`
- [ ] Os três caminhos são mutuamente exclusivos e cobertos por testes unitários
