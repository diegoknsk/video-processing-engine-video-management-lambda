# Subtask 03: Criar endpoint PATCH /videos/{id} (sem autenticação JWT por enquanto)

## Descrição
Implementar endpoint PATCH /videos/{id} que recebe UpdateVideoInputModel no body, valida com FluentValidation, chama UpdateVideoUseCase, trata VideoUpdateConflictException retornando 409 Conflict, retorna 200 OK com VideoResponseModel, **NÃO** usa .RequireAuthorization() (rota interna), e documenta no OpenAPI como rota interna.

## Passos de Implementação
1. MapPatch: `app.MapPatch("/videos/{id}", async (string id, UpdateVideoInputModel input, IUpdateVideoUseCase useCase, IValidator<UpdateVideoInputModel> validator, CancellationToken ct) => { ... })`
2. Validar input; se inválido retornar 400
3. Chamar `var response = await useCase.ExecuteAsync(id, input, ct);`
4. Capturar VideoUpdateConflictException: retornar 409 Conflict com ErrorResponse (type = "Conflict", title = "Update conflict", detail = exception message)
5. Retornar 200 OK com VideoResponseModel
6. **NÃO** adicionar .RequireAuthorization()
7. Documentar no OpenAPI: description "Internal route for orchestrator/processor/finalizer"

## Formas de Teste
1. Valid update test: PATCH com input válido, validar 200
2. Validation error: input vazio (todos campos null), validar 400
3. Conflict: mock useCase lança VideoUpdateConflictException, validar 409

## Critérios de Aceite da Subtask
- [ ] PATCH /videos/{id} implementado
- [ ] Validação com FluentValidation
- [ ] VideoUpdateConflictException tratada; retorna 409 Conflict
- [ ] Response 200 OK com VideoResponseModel
- [ ] NÃO usa .RequireAuthorization()
- [ ] Documentado no OpenAPI como rota interna
