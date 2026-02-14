# Subtask 06: Testes unitários do Repository (mock IAmazonDynamoDB)

## Descrição
Criar suite completa de testes unitários para VideoRepository usando xUnit, Moq (mock de IAmazonDynamoDB) e FluentAssertions, cobrir todos os métodos (CreateAsync, GetByIdAsync, GetByUserIdAsync, UpdateAsync, GetByClientRequestIdAsync), validar mapeamento correto, tratamento de erros, paginação, condições do DynamoDB, e garantir cobertura >= 80%.

## Passos de Implementação
1. **Criar VideoRepositoryTests.cs** em `tests/UnitTests/Infra/Data/Repositories/`
2. **Setup comum**: criar mock de IAmazonDynamoDB, mock de IOptions<DynamoDbOptions> (retornar TableName = "test-table"), instanciar VideoRepository em cada teste
3. **Testar CreateAsync**:
   - Test 1: sucesso — mockar PutItemAsync retornando sucesso, validar que Video é retornado
   - Test 2: ConditionalCheckFailedException — mockar exceção, validar que é propagada (ou convertida)
4. **Testar GetByIdAsync**:
   - Test 1: video encontrado — mockar GetItemAsync retornando Item válido, validar que Video é retornado com campos corretos
   - Test 2: video não encontrado — mockar GetItemAsync retornando Item null, validar que retorna null
5. **Testar GetByUserIdAsync**:
   - Test 1: lista com 2 videos — mockar QueryAsync retornando 2 Items, validar que retorna 2 Videos
   - Test 2: paginação — mockar QueryAsync retornando LastEvaluatedKey, validar que NextToken não é nulo
   - Test 3: lista vazia — mockar QueryAsync retornando Items vazio, validar que retorna lista vazia e NextToken null
6. **Testar UpdateAsync**:
   - Test 1: sucesso — mockar UpdateItemAsync retornando Attributes, validar que Video atualizado é retornado
   - Test 2: ConditionalCheckFailedException — mockar exceção, validar que VideoUpdateConflictException é lançada
7. **Testar GetByClientRequestIdAsync** (idempotência):
   - Test 1: video encontrado — mockar QueryAsync (GSI ou scan) retornando Item, validar que Video é retornado
   - Test 2: não encontrado — mockar retornando Items vazio, validar que retorna null
8. **Validar cobertura**: executar `dotnet test /p:CollectCoverage=true`, garantir >= 80% para VideoRepository

## Formas de Teste
1. **Testes passam**: executar `dotnet test --filter VideoRepositoryTests`; validar que todos passam
2. **Coverage**: executar com coverlet, validar >= 80% para VideoRepository
3. **Mock verification**: usar Moq .Verify() para garantir que métodos do IAmazonDynamoDB são chamados com parâmetros corretos

## Critérios de Aceite da Subtask
- [ ] VideoRepositoryTests.cs criado com mínimo 10 testes (covering CreateAsync, GetByIdAsync, GetByUserIdAsync, UpdateAsync, GetByClientRequestIdAsync)
- [ ] Todos os testes usam Moq para mockar IAmazonDynamoDB e IOptions<DynamoDbOptions>
- [ ] Testes validam mapeamento correto (VideoMapper) e tratamento de erros (ConditionalCheckFailedException → VideoUpdateConflictException)
- [ ] Testes de paginação validam que NextToken é gerado corretamente quando LastEvaluatedKey presente
- [ ] Testes usam FluentAssertions para asserções (ex.: `video.Should().NotBeNull()`, `videos.Should().HaveCount(2)`)
- [ ] Cobertura de testes >= 80% para VideoRepository (executar `dotnet test /p:CollectCoverage=true`)
- [ ] Todos os testes passam; `dotnet test` executa sem falhas
