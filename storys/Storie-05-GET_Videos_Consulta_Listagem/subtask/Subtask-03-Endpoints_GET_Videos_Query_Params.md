# Subtask 03: Criar endpoints GET /videos e GET /videos/{id}

## Descrição
Implementar GET /videos com query params limit e nextToken, e GET /videos/{id} com path param videoId, ambos extraindo userId do JWT, chamando use cases, retornando responses corretas (200, 401, 404), e documentando no OpenAPI.

## Passos de Implementação
1. GET /videos: `app.MapGet("/videos", async (HttpContext ctx, IListVideosUseCase useCase, int? limit, string? nextToken, CancellationToken ct) => { ... })` extrair userId, chamar useCase, retornar 200 com VideoListResponseModel
2. GET /videos/{id}: `app.MapGet("/videos/{id}", async (string id, HttpContext ctx, IGetVideoByIdUseCase useCase, CancellationToken ct) => { ... })` extrair userId, chamar useCase, se null retornar 404, senão 200 com VideoResponseModel
3. Ambos .RequireAuthorization()
4. Documentar com ProducesResponseType

## Formas de Teste
1. GET /videos test: mock useCase, validar 200 com lista
2. GET /videos/{id} found: validar 200
3. GET /videos/{id} not found: validar 404

## Critérios de Aceite da Subtask
- [ ] GET /videos implementado com query params limit e nextToken
- [ ] GET /videos/{id} implementado com path param id
- [ ] UserId extraído do JWT; ausência retorna 401
- [ ] GET /videos retorna 200 com VideoListResponseModel
- [ ] GET /videos/{id} retorna 200 com VideoResponseModel ou 404
- [ ] Ambos protegidos e documentados no OpenAPI
