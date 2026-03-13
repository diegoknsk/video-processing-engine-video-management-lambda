# Subtask-01: Expandir IVideoChunkRepository e VideoChunkRepository com GetStatusSummaryAsync

## Descrição
Adicionar ao repositório de chunks um novo método `GetStatusSummaryAsync` que retorne a contagem de chunks agrupados por status (completed, processing, failed, pending), excluindo o item especial `finalize`. Esse método é a base para o cálculo de progresso real nos use cases.

---

## Contexto
Hoje o repositório só possui `CountProcessedAsync`, que conta apenas chunks com `status = "completed"` e não diferencia os demais estados. Para calcular o progresso real e o breakdown de status, precisamos de uma query que retorne as contagens por status em uma única operação (ou em operações paralelas mínimas).

**Convenção do item `finalize`:**
- SK = `CHUNK#finalize` → `chunkId = "finalize"`
- Este item representa a etapa de geração do ZIP e **não deve ser contado em `totalChunks`**
- Deve ser retornado separadamente para influenciar o stage final e o `progressPercent = 100`

---

## Passos de Implementação

1. **Definir constante no domínio** — em `VideoChunk.cs` (ou em uma classe `VideoChunkConstants`), adicionar:
   ```csharp
   public const string FinalizeChunkId = "finalize";
   ```

2. **Criar o tipo de retorno `ChunkStatusSummary`** — record no domínio ou na Application:
   ```csharp
   public record ChunkStatusSummary(
       int Total,
       int Completed,
       int Processing,
       int Failed,
       int Pending,
       string? FinalizeStatus  // status do item finalize, null se não existir
   );
   ```

3. **Adicionar método à interface** — em `IVideoChunkRepository.cs`:
   ```csharp
   Task<ChunkStatusSummary> GetStatusSummaryAsync(string videoId, CancellationToken ct = default);
   ```

4. **Implementar no repositório** — em `VideoChunkRepository.cs`:
   - Executar `QueryRequest` com `pk = VIDEO#{videoId}` sem filtro de status (retorna todos os chunks do vídeo)
   - Usar `Select = "ALL_ATTRIBUTES"` ou `Select = "SPECIFIC_ATTRIBUTES"` com `ProjectionExpression = "chunkId, #st"` (apenas `chunkId` e `status`)
   - Iterar os itens retornados:
     - Se `chunkId == "finalize"`: capturar `FinalizeStatus`, não contar em `Total`
     - Para os demais: incrementar o contador correspondente ao status e `Total`
   - Retornar `ChunkStatusSummary` calculado

5. **Remover ou marcar como obsoleto `CountProcessedAsync`** — verificar se ainda é usado após a refatoração dos use cases (Subtask-04 e 05). Se não houver mais consumidores, remover para evitar código morto.

---

## Considerações de Performance
- A query traz apenas `chunkId` e `status` via `ProjectionExpression`, minimizando o payload
- Para vídeos com muitos chunks, DynamoDB pode paginar — usar loop de paginação com `LastEvaluatedKey`
- Não usar `FilterExpression` nesta query para não comprometer o count (um FilterExpression não impede cobrança de RCU pela leitura completa, mas mais importante: perderíamos o item `finalize` se filtrássemos por status)

---

## Formas de Teste

1. **Teste unitário do repositório** — mockar `IAmazonDynamoDB`, verificar que:
   - O `QueryRequest` enviado tem o `KeyConditionExpression` correto (`pk = :pk`)
   - O `ProjectionExpression` inclui `chunkId` e `status`
   - O resultado agrega corretamente contagens por status
   - O item `finalize` é excluído de `Total` mas tem seu status capturado em `FinalizeStatus`

2. **Teste com cenários de borda** — vídeo sem chunks: retorna `ChunkStatusSummary(0, 0, 0, 0, 0, null)`; vídeo com apenas o item `finalize`: retorna `Total = 0` com `FinalizeStatus` preenchido

3. **Verificação manual via Postman** — chamar `GET /videos/{id}` de um vídeo em processamento fan-out e confirmar que `chunksSummary` reflete o estado real da tabela de chunks no DynamoDB

---

## Critérios de Aceite

- [ ] `IVideoChunkRepository` possui o método `GetStatusSummaryAsync(string videoId, CancellationToken ct)`
- [ ] `VideoChunkRepository` implementa o método com paginação correta (loop por `LastEvaluatedKey`)
- [ ] O item `finalize` (`chunkId = "finalize"`) **não** é contado em `Total` e seu status é retornado em `FinalizeStatus`
- [ ] Constante `FinalizeChunkId = "finalize"` definida no domínio e usada no repositório
- [ ] Testes unitários do repositório passando para os cenários: sem chunks, com chunks variados, com item finalize presente
