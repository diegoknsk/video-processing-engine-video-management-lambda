# Storie-05: GET /videos e GET /videos/{id} — Consulta e Listagem

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** —

## Descrição
Como usuário autenticado, quero listar meus vídeos cadastrados (GET /videos com paginação) e consultar detalhes de um vídeo específico (GET /videos/{id}), para acompanhar status de processamento, progresso, URLs de download e erros.

## Objetivo
Implementar GET /videos que retorna lista paginada de vídeos do usuário (extraindo userId do JWT), com suporte a limit e nextToken (cursor-based pagination), e GET /videos/{id} que retorna detalhes de um vídeo específico validando ownership (userId do JWT deve corresponder ao userId do vídeo), retornar 404 se não encontrado ou não pertence ao usuário, mapear Video entity para VideoResponseModel, e documentar no OpenAPI.

## Escopo Técnico
- **Tecnologias:** .NET 10, DynamoDB Query (paginação), mapping (Video → VideoResponseModel)
- **Arquivos criados/modificados:**
  - `src/VideoProcessing.VideoManagement.Application/UseCases/ListVideos/ListVideosUseCase.cs`
  - `src/VideoProcessing.VideoManagement.Application/UseCases/GetVideoById/GetVideoByIdUseCase.cs`
  - `src/VideoProcessing.VideoManagement.Api/Endpoints/VideosEndpoints.cs` (adicionar MapGet)
- **Componentes:** ListVideosUseCase, GetVideoByIdUseCase, mappers
- **Pacotes/Dependências:** Nenhum novo (usa IVideoRepository já existente)

## Dependências e Riscos (para estimativa)
- **Dependências:** Story 01, 02 (IVideoRepository com GetByUserIdAsync e GetByIdAsync), Story 03 (ResponseModels), Story 04 (estrutura de endpoints)
- **Riscos:** Paginação via cursor (nextToken) pode ser complexa de testar; documentar limite máximo de items por página (50 padrão)

## Subtasks
- [Subtask 01: Implementar ListVideosUseCase (paginação cursor-based)](./subtask/Subtask-01-ListVideosUseCase_Paginacao.md)
- [Subtask 02: Implementar GetVideoByIdUseCase (ownership validation)](./subtask/Subtask-02-GetVideoByIdUseCase_Ownership.md)
- [Subtask 03: Criar endpoints GET /videos e GET /videos/{id}](./subtask/Subtask-03-Endpoints_GET_Videos_Query_Params.md)
- [Subtask 04: Testes unitários (use cases e endpoints)](./subtask/Subtask-04-Testes_Unitarios_GET_Videos.md)

## Critérios de Aceite da História
- [ ] Endpoint GET /videos implementado; aceita query params `limit` (int, padrão 50, max 100) e `nextToken` (string, opcional)
- [ ] UserId extraído do JWT (claim "sub"); se ausente retorna 401
- [ ] ListVideosUseCase chama IVideoRepository.GetByUserIdAsync com userId, limit e nextToken
- [ ] Response 200 OK com VideoListResponseModel: `videos` (array de VideoResponseModel), `nextToken` (string ou null se última página)
- [ ] Endpoint GET /videos/{id} implementado; {id} é videoId (string)
- [ ] GetVideoByIdUseCase chama IVideoRepository.GetByIdAsync(userId, videoId)
- [ ] Ownership validation: se vídeo não encontrado ou userId do JWT != userId do vídeo, retorna 404 Not Found (não expor existência de vídeos de outros usuários)
- [ ] Response 200 OK com VideoResponseModel mapeando todos os campos de Video
- [ ] Ambos endpoints protegidos com .RequireAuthorization()
- [ ] Documentados no OpenAPI com query params, path params, responses (200, 401, 404, 500)
- [ ] Testes unitários cobrindo: listagem com paginação (2 cenários: com nextToken, sem nextToken), lista vazia, get by id sucesso, get by id não encontrado, get by id ownership mismatch (404)
- [ ] Cobertura >= 80% para use cases
- [ ] Scalar UI "Try it" funciona: GET /videos retorna lista; GET /videos/{id} retorna video ou 404

## Rastreamento (dev tracking)
- **Início:** —
- **Fim:** —
- **Tempo total de desenvolvimento:** —
