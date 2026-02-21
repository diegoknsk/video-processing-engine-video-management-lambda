# Storie-05: GET /videos e GET /videos/{id} — Consulta e Listagem

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** —

## Descrição
Como usuário autenticado, quero listar meus vídeos cadastrados (GET /videos com paginação) e consultar detalhes de um vídeo específico (GET /videos/{id}), para acompanhar status de processamento, progresso, URLs de download e erros.

## Objetivo
Implementar os actions `ListVideos` e `GetVideo` já declarados como stubs em `VideosController`, extraindo userId do JWT (claim "sub"), suportando paginação cursor-based via `limit` e `nextToken` na listagem, validando ownership implícito no `GetByIdAsync` (retorna 404 se não encontrado ou não pertence ao usuário), mapeando `Video` entity para `VideoResponseModel`, e documentando no OpenAPI.

## Escopo Técnico
- **Tecnologias:** .NET 10, DynamoDB Query (paginação), mapeamento Video → VideoResponseModel
- **Arquivos criados/modificados:**
  - `src/VideoProcessing.VideoManagement.Api/Controllers/VideosController.cs` (implementar stubs ListVideos e GetVideo — já declarados)
  - `src/VideoProcessing.VideoManagement.Application/UseCases/ListVideos/ListVideosUseCase.cs` (novo)
  - `src/VideoProcessing.VideoManagement.Application/UseCases/GetVideoById/GetVideoByIdUseCase.cs` (novo)
  - `src/VideoProcessing.VideoManagement.Api/DependencyInjection/ServiceCollectionExtensions.cs` (registrar novos use cases)
  - `tests/VideoProcessing.VideoManagement.UnitTests/Application/UseCases/ListVideos/ListVideosUseCaseTests.cs` (novo)
  - `tests/VideoProcessing.VideoManagement.UnitTests/Application/UseCases/GetVideoById/GetVideoByIdUseCaseTests.cs` (novo)
- **Componentes:** `VideosController`, `ListVideosUseCase`, `GetVideoByIdUseCase`, mapper `Video → VideoResponseModel`
- **Pacotes/Dependências:** Nenhum novo (usa `IVideoRepository` já existente)

## Dependências e Riscos (para estimativa)
- **Dependências:** Story 01, 02 (`IVideoRepository` com `GetByUserIdAsync` e `GetByIdAsync`), Story 03 (ResponseModels), Story 04 + 04.1 + 04.2 (estrutura MVC Controllers com `[Authorize]` na classe). A **Storie-05.1** (padronização de respostas) é executada em seguida; os retornos 200/404 destes endpoints passarão a ser encapsulados pelo filtro da 05.1 — controllers continuam retornando `Ok(model)` ou `NotFound()`.
- **Riscos:** Paginação via cursor (nextToken) pode ser complexa de testar; documentar limite máximo de itens por página (100 máximo, 50 padrão)

## Subtasks
- [Subtask 01: Implementar ListVideosUseCase (paginação cursor-based)](./subtask/Subtask-01-ListVideosUseCase_Paginacao.md)
- [Subtask 02: Implementar GetVideoByIdUseCase (ownership validation)](./subtask/Subtask-02-GetVideoByIdUseCase_Ownership.md)
- [Subtask 03: Implementar actions GET /videos e GET /videos/{id} no VideosController](./subtask/Subtask-03-Endpoints_GET_Videos_Query_Params.md)
- [Subtask 04: Testes unitários (use cases)](./subtask/Subtask-04-Testes_Unitarios_GET_Videos.md)

## Critérios de Aceite da História
- [ ] Action `ListVideos` implementado no `VideosController`; query param `limit` (int, padrão 50, máx 100) e `nextToken` (string, opcional) — stub existente usa `pageSize`, renomear para `limit`
- [ ] Action `GetVideo` implementado no `VideosController`; recebe `Guid id` via `[HttpGet("{id:guid}")]`; UseCase converte para `string` ao chamar `IVideoRepository.GetByIdAsync`
- [ ] `UserId` extraído do claim "sub" em ambos os actions seguindo o padrão de 04.2: `User.FindFirst("sub")?.Value` + `Guid.TryParse`; claim ausente ou inválido retorna 401
- [ ] Ambos os actions protegidos por herança do `[Authorize]` na classe `VideosController` — **sem** necessidade de atributo adicional por action
- [ ] `ListVideosUseCase` chama `IVideoRepository.GetByUserIdAsync` com `userId`, `limit` e `nextToken`
- [ ] Response 200 OK com `VideoListResponseModel`: `videos` (array de `VideoResponseModel`), `nextToken` (string ou null se última página)
- [ ] `GetVideoByIdUseCase` chama `IVideoRepository.GetByIdAsync(userId, videoId)`; ownership implícito: se null, retornar null → 404
- [ ] Response 200 OK com `VideoResponseModel` mapeando todos os campos de `Video`
- [ ] `CreatedAtAction(nameof(GetVideo), ...)` em `UploadVideo` (Story 04) resolve corretamente após implementação do action `GetVideo`
- [ ] Documentados no OpenAPI com query params, path params, responses (200, 401, 404, 500). Após Storie-05.1, 200 e 404 seguirão o envelope padronizado (success/data ou success/error, timestamp).
- [ ] Testes unitários cobrindo: listagem com nextToken, sem nextToken, lista vazia, limit > 100, get by id encontrado, get by id não encontrado (404)
- [ ] Cobertura >= 80% para use cases
- [ ] `dotnet test` passa sem erros após a implementação

## Rastreamento (dev tracking)
- **Início:** 19/02/2026, às 00:12 (Brasília)
- **Fim:** —
- **Tempo total de desenvolvimento:** —
