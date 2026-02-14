# Subtask 04: Testes unitários (use cases e endpoints)

## Descrição
Criar testes unitários para ListVideosUseCase, GetVideoByIdUseCase e endpoints GET /videos e GET /videos/{id}, cobrindo paginação, ownership, erros, garantindo cobertura >= 80%.

## Passos de Implementação
1. ListVideosUseCaseTests: mock repository, testar com nextToken, sem nextToken, lista vazia, limit > 100
2. GetVideoByIdUseCaseTests: mock repository, testar found, not found
3. Endpoints tests (opcional): mock use cases, validar status codes

## Formas de Teste
1. All tests pass: `dotnet test --filter ListVideos* | GetVideoById*`
2. Coverage: >= 80%

## Critérios de Aceite da Subtask
- [ ] ListVideosUseCaseTests: mínimo 3 testes (com nextToken, sem nextToken, limit > 100)
- [ ] GetVideoByIdUseCaseTests: mínimo 2 testes (found, not found)
- [ ] Todos os testes passam
- [ ] Cobertura >= 80%
