# Subtask 01: Modelos ApiResponse, ApiErrorResponse e ErrorDetail

## Descrição
Criar os modelos de contrato para respostas padronizadas da API: `ApiResponse<T>` para sucesso (success, data, timestamp) e `ApiErrorResponse` com `ErrorDetail` (success, error.code, error.message, timestamp), conforme a skill api-response-standardization.

## Passos de Implementação
1. Criar pasta `Models` em `src/VideoProcessing.VideoManagement.Api/` se não existir.
2. Criar `ApiResponse.cs`:
   - Propriedades: `Success` (bool, init, default true), `Data` (T?, init), `Timestamp` (DateTime, init, default DateTime.UtcNow).
   - Método estático `CreateSuccess(T data)` retornando instância com Success=true, Data=data, Timestamp=UtcNow.
3. Criar `ApiErrorResponse.cs` (e classe `ErrorDetail` no mesmo arquivo ou em arquivo separado):
   - `ApiErrorResponse`: `Success` (false), `Error` (ErrorDetail), `Timestamp`.
   - `ErrorDetail`: `Code` (string), `Message` (string).
   - Método estático `ApiErrorResponse.Create(string code, string message)`.
4. Garantir namespace adequado (ex.: `VideoProcessing.VideoManagement.Api.Models`).

## Formas de Teste
1. Instanciar `ApiResponse<string>.CreateSuccess("ok")` e verificar `Success`, `Data`, `Timestamp` preenchidos.
2. Instanciar `ApiErrorResponse.Create("Unauthorized", "Acesso não autorizado.")` e verificar `Success == false`, `Error.Code`, `Error.Message`, `Timestamp`.
3. Serializar com camelCase e validar JSON gerado (success, data/error, timestamp).

## Critérios de Aceite da Subtask
- [x] `ApiResponse<T>` existe com `Success`, `Data`, `Timestamp` e `CreateSuccess(T data)`.
- [x] `ApiErrorResponse` e `ErrorDetail` existem; `ApiErrorResponse.Create(code, message)` preenche `Error.Code` e `Error.Message`.
- [x] Propriedades usam `init` onde apropriado; timestamp em UTC.
- [x] Sem dependências externas além do BCL.
