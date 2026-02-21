# Subtask 02: Filtro ApiResponseFilter (encapsular 200/201)

## Descrição
Implementar um `IActionFilter` que, após a execução da action, intercepte resultados `OkObjectResult` e `ObjectResult` com status 200 ou 201 e os encapsule em `ApiResponse<object>.CreateSuccess(value)`, de modo que os controllers continuem retornando apenas o payload.

## Passos de Implementação
1. Criar `ApiResponseFilter.cs` em `src/VideoProcessing.VideoManagement.Api/Filters/`.
2. Implementar `IActionFilter`: `OnActionExecuting` vazio; em `OnActionExecuted`:
   - Se `context.Result` é `OkObjectResult`, substituir por `ObjectResult(ApiResponse<object>.CreateSuccess(okResult.Value))` com StatusCode 200.
   - Se `context.Result` é `ObjectResult` com StatusCode 200 ou 201, encapsular o `Value` em `ApiResponse<object>.CreateSuccess(value)` mantendo o mesmo StatusCode.
3. Opcional: receber `ILogger<ApiResponseFilter>` via construtor para diagnóstico.
4. (Exclusões como /health ficam na Subtask 04; aqui o filtro pode aplicar a todos 200/201 ou checar action/controller para não encapsular quando for Health.)

## Formas de Teste
1. Chamar um endpoint que retorna `Ok(model)` e verificar que a resposta JSON contém `success: true`, `data` com o payload e `timestamp`.
2. Chamar um endpoint que retorna `CreatedAtAction(..., model)` e verificar envelope com status 201.
3. Verificar que respostas 400/404/500 não são alteradas pelo filtro.

## Critérios de Aceite da Subtask
- [x] `ApiResponseFilter` implementa `IActionFilter` e encapsula `OkObjectResult` e `ObjectResult` (200/201) em `ApiResponse<object>`.
- [x] Controllers não precisam mudar assinatura; retornam `Ok(...)` ou `CreatedAtAction(...)` como hoje.
- [x] Resposta serializada contém `success`, `data` e `timestamp` em camelCase quando aplicável.
- [x] Outros resultados (ex.: NotFound, BadRequest) não são modificados.
