# Subtask 03: Implementar action PATCH /videos/{id} no VideosController

## Descrição
Implementar o stub `UpdateVideo` já declarado em `VideosController`, adicionando `[AllowAnonymous]` para sobrescrever o `[Authorize]` da classe (rota interna sem JWT), validando input com FluentValidation, chamando `UpdateVideoUseCase`, tratando `VideoUpdateConflictException` como 409 Conflict, e documentando no OpenAPI como rota interna.

> **Importante:** `VideosController` tem `[Authorize]` em nível de classe (Story 04.2). O action `UpdateVideo` precisa de `[AllowAnonymous]` explícito para ser acessível sem token — exatamente o mesmo padrão do `HealthController`. Os demais actions (POST, GET) **não** são afetados.

## Passos de Implementação
1. Adicionar `[AllowAnonymous]` ao action `UpdateVideo`
2. Injetar `IUpdateVideoUseCase` e `IValidator<UpdateVideoInputModel>` no construtor primário do controller
3. Implementar o action:
   ```csharp
   [AllowAnonymous]
   [HttpPatch("{id:guid}")]
   // ... ProducesResponseType attributes ...
   public async Task<IActionResult> UpdateVideo(Guid id, [FromBody] UpdateVideoInputModel input, CancellationToken cancellationToken)
   {
       var validation = await validator.ValidateAsync(input, cancellationToken);
       if (!validation.IsValid)
           return BadRequest(new ErrorResponse(...));

       try
       {
           var response = await updateVideoUseCase.ExecuteAsync(id, input, cancellationToken);
           if (response is null)
               return NotFound(new ErrorResponse(...));
           return Ok(response);
       }
       catch (VideoUpdateConflictException ex)
       {
           return Conflict(new ErrorResponse(
               "https://tools.ietf.org/html/rfc7231#section-6.5.8",
               "Update Conflict",
               StatusCodes.Status409Conflict,
               ex.Message,
               HttpContext.TraceIdentifier
           ));
       }
   }
   ```
4. Atualizar `[ProducesResponseType]` existentes: confirmar presença de 200, 400, 404, 409, 500
5. Adicionar `[EndpointDescription("Internal route for orchestrator/processor/finalizer")]` ou equivalente via XML doc

## Formas de Teste
1. `PATCH /videos/{id}` com body válido → 200 com `VideoResponseModel`
2. `PATCH /videos/{id}` sem token → 200 (não retorna 401 — `[AllowAnonymous]`)
3. `PATCH /videos/{id}` com body inválido (sem `UserId`) → 400
4. `PATCH /videos/{id}` com `VideoUpdateConflictException` → 409
5. `PATCH /videos/{id}` vídeo não encontrado → 404

## Critérios de Aceite da Subtask
- [ ] Action `UpdateVideo` possui `[AllowAnonymous]` explícito
- [ ] `PATCH /videos/{id}` acessível sem token JWT (não retorna 401)
- [ ] Validação com `IValidator<UpdateVideoInputModel>`; body inválido retorna 400
- [ ] `VideoUpdateConflictException` capturada; retorna 409 Conflict com `ErrorResponse`
- [ ] Vídeo não encontrado (UseCase retorna null) retorna 404
- [ ] Response 200 OK com `VideoResponseModel`
- [ ] `[ProducesResponseType]` para 200, 400, 404, 409, 500
- [ ] Documentado no OpenAPI como rota interna
- [ ] `IUpdateVideoUseCase` injetado no construtor primário
- [ ] Demais actions do controller (POST, GET) continuam retornando 401 sem token