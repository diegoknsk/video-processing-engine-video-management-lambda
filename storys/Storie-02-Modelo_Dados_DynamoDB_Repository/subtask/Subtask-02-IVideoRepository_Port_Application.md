# Subtask 02: Criar IVideoRepository port (Application) com assinaturas de métodos

## Descrição
Definir a interface `IVideoRepository` na camada Application como port (abstração de persistência), declarar métodos para operações CRUD (CreateAsync, GetByIdAsync, GetByUserIdAsync com paginação, UpdateAsync condicional, DeleteAsync se necessário) e operação de idempotência (ExistsByClientRequestIdAsync), garantir que assinaturas usam tipos do Domain (Video entity) e não DTOs, e retornar Task<> para operações assíncronas.

## Passos de Implementação
1. **Criar interface IVideoRepository** em `Application/Ports/IVideoRepository.cs`
2. **Declarar CreateAsync**: `Task<Video> CreateAsync(Video video, string clientRequestId, CancellationToken ct = default);` — persiste vídeo e associa clientRequestId para deduplicação
3. **Declarar GetByIdAsync**: `Task<Video?> GetByIdAsync(string userId, string videoId, CancellationToken ct = default);` — retorna vídeo ou null; ownership implícito (userId no pk)
4. **Declarar GetByUserIdAsync**: `Task<(IReadOnlyList<Video> Items, string? NextToken)> GetByUserIdAsync(string userId, int limit = 50, string? paginationToken = null, CancellationToken ct = default);` — lista vídeos do usuário com paginação
5. **Declarar UpdateAsync condicional**: `Task<Video> UpdateAsync(Video video, CancellationToken ct = default);` — atualiza com condições (ownership, monotonia, status transitions); lança exception se condição falha
6. **Declarar ExistsByClientRequestIdAsync**: `Task<Video?> GetByClientRequestIdAsync(string userId, string clientRequestId, CancellationToken ct = default);` — retorna vídeo já criado com esse clientRequestId (idempotência)
7. **Documentar comportamento com XML comments**: descrever quando métodos lançam exceções (ConditionalCheckFailedException, etc.)

## Formas de Teste
1. **Assinatura test**: compilar interface, validar que todos os métodos retornam Task<>
2. **Usage test (mock)**: criar mock de IVideoRepository usando Moq, validar que pode ser injetado em use case
3. **XML comments**: validar que documentação XML está presente e clara

## Critérios de Aceite da Subtask
- [ ] Interface `IVideoRepository` criada em Application/Ports/
- [ ] Métodos declarados: CreateAsync, GetByIdAsync, GetByUserIdAsync (paginado), UpdateAsync (condicional), GetByClientRequestIdAsync
- [ ] Todos os métodos retornam Task<> (assíncronos)
- [ ] Assinaturas usam tipos do Domain (Video), não DTOs
- [ ] GetByUserIdAsync retorna tupla com Items e NextToken para paginação
- [ ] XML comments documentam comportamento, parâmetros e exceções esperadas
- [ ] Interface compila sem erros
