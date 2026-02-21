# Subtask 04: Testes unitários (use cases)

## Descrição
Criar testes unitários para `ListVideosUseCase` e `GetVideoByIdUseCase`, cobrindo paginação cursor-based, ownership implícito, lista vazia, limit > 100 e cenário não encontrado, garantindo cobertura >= 80%.

> **Nota:** Testes de controller (actions GET no `VideosController`) são opcionais nesta subtask, pois a lógica de auth já é coberta pelos testes da Story 04.2 (`VideosControllerAuthTests`). Focar a cobertura nos use cases.

## Passos de Implementação
1. `ListVideosUseCaseTests`:
   - Mock `IVideoRepository`
   - Cenário com `nextToken` retornado: mock `GetByUserIdAsync` retorna `(items, "token123")`, validar que `VideoListResponseModel.NextToken == "token123"`
   - Cenário sem `nextToken`: mock retorna `(items, null)`, validar `NextToken == null`
   - Lista vazia: mock retorna `([], null)`, validar `Videos` vazio
   - `limit > 100`: UseCase deve ajustar para 100 antes de chamar repositório
2. `GetVideoByIdUseCaseTests`:
   - Cenário encontrado: mock `GetByIdAsync` retorna video, validar `VideoResponseModel` retornado com campos corretos
   - Cenário não encontrado: mock `GetByIdAsync` retorna `null`, validar que UseCase retorna `null`
3. Verificar que `dotnet test` passa sem erros

## Formas de Teste
```
dotnet test --filter "ListVideos|GetVideoById"
```

## Critérios de Aceite da Subtask
- [ ] `ListVideosUseCaseTests` com mínimo 4 testes (com nextToken, sem nextToken, lista vazia, limit > 100)
- [ ] `GetVideoByIdUseCaseTests` com mínimo 2 testes (encontrado, não encontrado)
- [ ] `GetByUserIdAsync` chamado com `userId`, `limit` e `nextToken` corretos
- [ ] `GetByIdAsync` chamado com `userId` e `videoId` como strings (Guid.ToString())
- [ ] Todos os testes passam
- [ ] Cobertura >= 80% para os use cases
