# Subtask-04: VideoChunkRepository — Repositório DynamoDB (Infra.Data)

## Descrição
Implementar `VideoChunkRepository` em `Infra.Data` seguindo o padrão do `VideoRepository` existente. O repositório deve ler o nome da tabela de `DynamoDbOptions.ChunksTableName` e implementar `UpsertAsync` e `CountProcessedAsync`.

## Design da Tabela
- **pk** = `VIDEO#{videoId}` (string)
- **sk** = `CHUNK#{chunkId}` (string)
- Atributos: `chunkId`, `videoId`, `status`, `startSec`, `endSec`, `intervalSec`, `manifestPrefix`, `framesPrefix`, `processedAt`, `createdAt`

## Passos de Implementação
1. Criar `VideoChunkRepository.cs` em `Infra.Data/Repositories/` implementando `IVideoChunkRepository`.
   - Receber `IAmazonDynamoDB` e `IOptions<DynamoDbOptions>` via construtor primário.
   - Ler `TableName` de `options.Value.ChunksTableName`.
2. Implementar `UpsertAsync`: montar `Dictionary<string, AttributeValue>` com todos os campos de `VideoChunk` e executar `PutItemAsync` (sem condição — semantica de replace idempotente).
3. Implementar `CountProcessedAsync`: executar `QueryAsync` com `KeyConditionExpression = "pk = :pk"`, `FilterExpression = "#st = :completed"`, `Select = Select.COUNT` e retornar `response.Count`. O valor a filtrar como "concluído" deve ser um status string definido no domínio ou via constante (ex.: `"completed"`).
4. Registrar `IVideoChunkRepository → VideoChunkRepository` como `Scoped` em `ServiceCollectionExtensions`.

## Formas de Teste
1. Teste unitário com mock de `IAmazonDynamoDB`: verificar que `UpsertAsync` chama `PutItemAsync` com `pk = "VIDEO#vid-123"` e `sk = "CHUNK#chunk-01"`.
2. Teste unitário com mock: verificar que `CountProcessedAsync` chama `QueryAsync` com `Select = Select.COUNT` e filtro pelo status correto.
3. Compilação: `dotnet build` sem erros em `Infra.Data` após adicionar o repositório.
4. Verificar no registro de DI que `IVideoChunkRepository` resolve para `VideoChunkRepository` sem exceção.

## Critérios de Aceite
- [ ] `VideoChunkRepository` implementa `IVideoChunkRepository` com `UpsertAsync` e `CountProcessedAsync`.
- [ ] Nome da tabela lido de `DynamoDbOptions.ChunksTableName` — sem string hardcoded.
- [ ] `UpsertAsync` usa `PutItemAsync` com pk/sk no formato `VIDEO#{videoId}` / `CHUNK#{chunkId}`.
- [ ] `CountProcessedAsync` usa `QueryAsync` com `Select.COUNT` e filtro de status.
- [ ] `IVideoChunkRepository` registrado no container de DI.
- [ ] Testes unitários do repositório passando.
