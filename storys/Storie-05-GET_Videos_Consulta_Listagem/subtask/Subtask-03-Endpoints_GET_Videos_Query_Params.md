# Subtask 03: Implementar actions GET /videos e GET /videos/{id} no VideosController

## Descrição
Implementar os stubs `ListVideos` e `GetVideo` já declarados em `VideosController`, extraindo `userId` do claim "sub" seguindo o padrão estabelecido em 04.2, renomeando query param `pageSize` → `limit`, chamando os use cases correspondentes, retornando responses corretas (200, 401, 404) e documentando no OpenAPI.

> **Nota:** `VideosController` já possui `[Authorize]` em nível de classe (Story 04.2). Não é necessário nenhum atributo adicional de autenticação nos actions GET.

## Passos de Implementação
1. Renomear parâmetro `pageSize` → `limit` no action `ListVideos`; manter `nextToken`
2. Implementar `ListVideos`:
   ```csharp
   var sub = User.FindFirst("sub")?.Value;
   if (string.IsNullOrEmpty(sub) || !Guid.TryParse(sub, out var userId))
       return Unauthorized();
   var response = await listVideosUseCase.ExecuteAsync(userId.ToString(), limit ?? 50, nextToken, cancellationToken);
   return Ok(response);
   ```
3. Implementar `GetVideo`:
   ```csharp
   var sub = User.FindFirst("sub")?.Value;
   if (string.IsNullOrEmpty(sub) || !Guid.TryParse(sub, out var userId))
       return Unauthorized();
   var response = await getVideoByIdUseCase.ExecuteAsync(userId.ToString(), id.ToString(), cancellationToken);
   if (response is null) return NotFound();
   return Ok(response);
   ```
4. Injetar `IListVideosUseCase` e `IGetVideoByIdUseCase` no construtor primário do controller
5. Atualizar `[ProducesResponseType]` existentes se necessário (400 não se aplica ao GET, já correto)
6. Documentar query params `limit` e `nextToken` com `[FromQuery]`

## Formas de Teste
1. `GET /videos` com token válido → 200 com `VideoListResponseModel`
2. `GET /videos/{id}` com token válido e vídeo existente → 200 com `VideoResponseModel`
3. `GET /videos/{id}` com vídeo não encontrado → 404
4. `GET /videos` sem token → 401 (tratado pela camada de `[Authorize]` da classe)

## Critérios de Aceite da Subtask
- [ ] `ListVideos` action implementado com `[FromQuery] int? limit` e `[FromQuery] string? nextToken`
- [ ] `GetVideo` action implementado com `Guid id` via `[HttpGet("{id:guid}")]`
- [ ] `UserId` extraído com `User.FindFirst("sub")` + `Guid.TryParse`; ausência/inválido retorna 401
- [ ] Não usa `.RequireAuthorization()` nem `[Authorize]` por action — herda da classe
- [ ] `ListVideos` retorna 200 com `VideoListResponseModel`
- [ ] `GetVideo` retorna 200 com `VideoResponseModel` ou 404 se não encontrado
- [ ] `IListVideosUseCase` e `IGetVideoByIdUseCase` injetados no construtor primário
- [ ] Documentado no OpenAPI (`[ProducesResponseType]` para 200, 401, 404, 500)
