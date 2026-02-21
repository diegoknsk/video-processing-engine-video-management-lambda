# Subtask 03: Middleware de exceção global e ApiErrorResponse

## Descrição
Garantir que todas as exceções não tratadas sejam capturadas e convertidas em resposta JSON no formato `ApiErrorResponse` (success: false, error.code, error.message, timestamp). Pode ser implementado um novo middleware na API ou a adaptação do `GlobalExceptionHandlerMiddleware` existente em Infra para escrever esse contrato.

## Passos de Implementação
1. Definir local do middleware: `Api/Middleware/GlobalExceptionMiddleware.cs` (novo) ou adaptar `Infra.CrossCutting.Middleware.GlobalExceptionHandlerMiddleware` para usar `ApiErrorResponse.Create(code, message)` e serializar com camelCase.
2. No `InvokeAsync`: try/await _next(context); em catch, chamar método que define (statusCode, code, message) e escreve resposta.
3. Implementar mapeamento de exceção para (statusCode, code, message), por exemplo:
   - `UnauthorizedAccessException` → 401, "Unauthorized", "Acesso não autorizado."
   - `ArgumentException` → 400, "BadRequest", "Requisição inválida."
   - `KeyNotFoundException` ou equivalente "not found" → 404, "NotFound", "Recurso não encontrado."
   - Demais → 500, "InternalServerError", "Erro interno do servidor."
4. Estender `MapException` com exceções do domínio ou de SDK (ex.: Cognito, DynamoDB) conforme necessário.
5. Escrever resposta com `ContentType = application/json`, status code e corpo serializado com `JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })`.
6. Registrar o middleware cedo no pipeline (antes de UseRouting/UseAuthorization), mantendo ou substituindo o middleware atual conforme decisão de local.

## Formas de Teste
1. Forçar uma exceção não tratada (ex.: throw new UnauthorizedAccessException()) em um endpoint e verificar resposta 401 com body `{ "success": false, "error": { "code": "Unauthorized", "message": "..." }, "timestamp": "..." }`.
2. Testar ArgumentException → 400, NotFound → 404, exceção genérica → 500.
3. Teste unitário do mapeamento: dado um tipo de exceção, verificar tripla (statusCode, code, message) retornada.

## Critérios de Aceite da Subtask
- [x] Exceções não tratadas resultam em resposta JSON no formato ApiErrorResponse (success, error.code, error.message, timestamp).
- [x] Mapeamento cobre pelo menos 401, 400, 404 e 500; extensível para exceções de domínio/SDK.
- [x] Resposta usa Content-Type application/json e PropertyNamingPolicy.CamelCase.
- [x] Middleware registrado no pipeline antes de UseRouting (ou na mesma posição do handler atual).
- [x] Testes unitários para o mapeamento de exceções (mínimo 4 cenários).
