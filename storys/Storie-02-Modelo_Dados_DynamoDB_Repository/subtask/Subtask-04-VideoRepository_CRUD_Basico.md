# Subtask 04: Implementar VideoRepository (CRUD básico: Create, GetById, GetByUserId)

## Descrição
Implementar classe `VideoRepository` que herda de `IVideoRepository`, injetar `IAmazonDynamoDB` e `IOptions<DynamoDbOptions>` via construtor primário, implementar métodos CreateAsync (PutItem), GetByIdAsync (GetItem), GetByUserIdAsync (Query com paginação usando LastEvaluatedKey), garantir que todas as operações tratam erros do DynamoDB (ProvisionedThroughputExceededException, ResourceNotFoundException) e usam VideoMapper para conversão.

## Passos de Implementação
1. **Criar VideoRepository class** em `Infra.Data/Repositories/VideoRepository.cs`: `public class VideoRepository(IAmazonDynamoDB dynamoDb, IOptions<DynamoDbOptions> options) : IVideoRepository`
2. **Implementar CreateAsync**:
   - Converter Video para VideoEntity usando VideoMapper.ToEntity
   - Setar ClientRequestId e ClientRequestIdCreatedAt
   - Criar PutItemRequest com Item (atributos do VideoEntity convertidos via Document.FromObject ou AttributeValue)
   - ConditionExpression: `attribute_not_exists(pk)` (evitar sobrescrever; idempotência via GetByClientRequestIdAsync)
   - Executar dynamoDb.PutItemAsync
   - Tratar ConditionalCheckFailedException (item já existe; não deveria ocorrer com idempotência prévia)
   - Retornar Video criado
3. **Implementar GetByIdAsync**:
   - Criar GetItemRequest com Key (pk, sk)
   - Executar dynamoDb.GetItemAsync
   - Se Item null, retornar null
   - Converter Item para VideoEntity e então para Video usando VideoMapper.ToDomain
   - Retornar Video ou null
4. **Implementar GetByUserIdAsync (paginado)**:
   - Criar QueryRequest: KeyConditionExpression = "pk = :pk", ExpressionAttributeValues = { ":pk": USER#{userId} }
   - Limit = limit, ExclusiveStartKey = deserializar paginationToken (base64?)
   - ScanIndexForward = false (mais recentes primeiro)
   - Executar dynamoDb.QueryAsync
   - Converter Items para List<Video> usando VideoMapper
   - Serializar LastEvaluatedKey como NextToken (base64 do JSON)
   - Retornar (Items, NextToken)
5. **Tratar erros AWS**: wrap operações em try-catch, logar erros, re-throw como exceções de domínio ou infraestrutura

## Formas de Teste
1. **Create test (mock)**: mockar IAmazonDynamoDB, validar que PutItemAsync é chamado com Item correto
2. **GetById test (mock)**: mockar GetItemAsync retornando Item, validar que Video é retornado; mockar Item null, validar que retorna null
3. **GetByUserId test (mock)**: mockar QueryAsync retornando 2 items e LastEvaluatedKey, validar que retorna 2 Videos e NextToken não nulo

## Critérios de Aceite da Subtask
- [ ] VideoRepository implementado com construtor primário injetando IAmazonDynamoDB e IOptions<DynamoDbOptions>
- [ ] CreateAsync implementado usando PutItem com ConditionExpression (attribute_not_exists)
- [ ] GetByIdAsync implementado usando GetItem, retorna Video ou null
- [ ] GetByUserIdAsync implementado usando Query com paginação (Limit, ExclusiveStartKey, LastEvaluatedKey → NextToken)
- [ ] Todos os métodos usam VideoMapper para conversão DTO ↔ Domain
- [ ] Erros AWS (ConditionalCheckFailedException, etc.) tratados e logados
- [ ] Testes unitários com mocks de IAmazonDynamoDB validam comportamento de CreateAsync, GetByIdAsync, GetByUserIdAsync (cobertura >= 80%)
