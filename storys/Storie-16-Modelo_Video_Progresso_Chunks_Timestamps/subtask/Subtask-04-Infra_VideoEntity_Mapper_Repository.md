# Subtask 04: Infra — VideoEntity, VideoMapper e VideoRepository (DynamoDB)

## Descrição
Adaptar a camada de persistência (Infra.Data) para suportar os novos campos do modelo de vídeo. Atualizar `VideoEntity` com as novas propriedades, ajustar `VideoMapper` para converter entre domínio e entidade (incluindo serialização de `ProcessingSummary` como Map DynamoDB), e atualizar `VideoRepository` para persistir e ler os novos campos, com atenção especial ao merge incremental de `processingSummary` no DynamoDB usando path expressions.

## Passos de Implementação
1. Adicionar na `VideoEntity` as propriedades correspondentes aos novos campos:
   - `string? ProcessingStartedAt`
   - `string? ImagesProcessingCompletedAt`
   - `string? ProcessingCompletedAt`
   - `string? LastFailedAt`
   - `string? LastCancelledAt`
   - `int? MaxParallelChunks`
   - `Dictionary<string, object>? ProcessingSummary` (ou tipo adequado para representar o Map DynamoDB)
2. Atualizar `VideoMapper.ToEntity(Video)` para mapear os novos campos de domínio → entidade, serializando timestamps como ISO 8601 (`"O"` format) e `ProcessingSummary` como dicionário serializável.
3. Atualizar `VideoMapper.ToDomain(VideoEntity)` para mapear entidade → domínio, desserializando timestamps (string → DateTime?) e reconstruindo `ProcessingSummary` a partir do Map DynamoDB.
4. Atualizar `EntityToAttributeMap` no `VideoRepository` para incluir os novos atributos no item DynamoDB:
   - Timestamps como `AttributeValue { S = ... }` (ISO 8601)
   - `maxParallelChunks` como `AttributeValue { N = ... }`
   - `processingSummary` como `AttributeValue { M = ... }` (Map com chunks como sub-Maps)
5. Atualizar `AttributeMapToEntity` no `VideoRepository` para ler os novos atributos do item DynamoDB, tratando campos ausentes como `null` (compatibilidade com itens existentes).
6. Atualizar `UpdateAsync` no `VideoRepository`:
   - Incluir novos campos na `UpdateExpression` (SET)
   - Para `processingSummary.chunks`: usar path expression DynamoDB para merge incremental (ex.: `SET processingSummary.chunks.#chunkId = :chunkValue` para cada novo chunk), ou alternativamente persistir o summary já mergeado no domínio via SET do campo completo (decisão a tomar durante implementação conforme complexidade)
   - Garantir que campos null no domínio não são escritos no DynamoDB (preservar comportamento atual de omitir atributos opcionais)

## Formas de Teste
1. Teste unitário: `VideoMapper.ToEntity` com Video contendo todos os novos campos preenchidos; verificar que a entidade possui os valores corretos (timestamps como string ISO, processingSummary como dicionário).
2. Teste unitário: `VideoMapper.ToDomain` com VideoEntity contendo novos campos; verificar que o Video de domínio possui os valores tipados corretos.
3. Teste unitário: `VideoMapper.ToDomain` com VideoEntity sem os novos campos (simulando item antigo); verificar que os campos ficam `null` sem erro.
4. Verificar que `dotnet build` compila sem erros e `dotnet test` passa (testes existentes não quebraram).

## Critérios de Aceite da Subtask
- [ ] `VideoEntity` possui todas as novas propriedades correspondentes ao modelo de domínio.
- [ ] `VideoMapper.ToEntity` serializa timestamps como ISO 8601 e `ProcessingSummary` como estrutura serializável.
- [ ] `VideoMapper.ToDomain` desserializa os novos campos corretamente; campos ausentes resultam em `null` sem exceção.
- [ ] `EntityToAttributeMap` inclui os novos atributos no item DynamoDB com os tipos corretos (S, N, M).
- [ ] `AttributeMapToEntity` lê os novos atributos do DynamoDB; itens existentes sem os campos continuam funcionando.
- [ ] `UpdateAsync` persiste os novos campos; `processingSummary` é persistido como Map DynamoDB.
- [ ] `dotnet build` e `dotnet test` passam sem regressão.
