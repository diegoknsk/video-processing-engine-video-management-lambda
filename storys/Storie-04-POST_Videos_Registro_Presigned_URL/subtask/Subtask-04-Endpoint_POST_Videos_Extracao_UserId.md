# Subtask 04: Criar endpoint POST /videos (extração de userId do JWT)

## Descrição
Implementar endpoint POST /videos que recebe UploadVideoInputModel no body, valida via FluentValidation, extrai userId do JWT (claim "sub"), chama UploadVideoUseCase, retorna 201 Created com UploadVideoResponseModel, trata erros (400 validação, 401 sem userId, 500 internal), e documenta no OpenAPI.

## Passos de Implementação
1. Criar `VideosEndpoints.cs` com extension method `MapVideosEndpoints(this IEndpointRouteBuilder app)`
2. Implementar POST /videos: `app.MapPost("/videos", async (UploadVideoInputModel input, HttpContext httpContext, IUploadVideoUseCase useCase, IValidator<UploadVideoInputModel> validator, CancellationToken ct) => { ... })`
3. Validar input: `var validationResult = await validator.ValidateAsync(input, ct); if (!validationResult.IsValid) return Results.ValidationProblem(validationResult.ToDictionary());`
4. Extrair userId: `var userId = httpContext.User.FindFirst("sub")?.Value; if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();`
5. Chamar use case: `var response = await useCase.ExecuteAsync(input, userId, ct);`
6. Retornar 201: `return Results.Created($"/videos/{response.VideoId}", response);`
7. Adicionar .RequireAuthorization() ao endpoint
8. Registrar no Program.cs: `app.MapVideosEndpoints();`

## Formas de Teste
1. Valid request test (integration ou mock): POST com input válido e token JWT válido, validar 201 com response correto
2. Unauthorized test: POST sem token, validar 401
3. Validation error test: POST com originalFileName vazio, validar 400 com erros de validação

## Critérios de Aceite da Subtask
- [ ] Endpoint POST /videos implementado em VideosEndpoints.cs
- [ ] Validação de input com FluentValidation; erros retornam 400 com ValidationProblem
- [ ] UserId extraído do JWT claim "sub"; ausência retorna 401 Unauthorized
- [ ] UploadVideoUseCase chamado com input e userId
- [ ] Response 201 Created com Location header `/videos/{videoId}` e body UploadVideoResponseModel
- [ ] Endpoint protegido com .RequireAuthorization()
- [ ] Documentado no OpenAPI com ProducesResponseType (201, 400, 401, 500)
- [ ] MapVideosEndpoints registrado no Program.cs
