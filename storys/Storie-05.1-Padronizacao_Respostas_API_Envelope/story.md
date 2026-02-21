# Storie-05.1: Padronização de Respostas da API (Envelope success/data/error/timestamp)

## Status
- **Estado:** ✅ Concluída
- **Data de Conclusão:** 21/02/2026

## Descrição
Como consumidor da API, quero receber todas as respostas (sucesso e erro) em um formato único e previsível com `success`, `data` ou `error` e `timestamp`, para integrar de forma consistente e tratar erros por `error.code` sem depender do formato bruto do controller ou de exceções.

## Objetivo
Padronizar as respostas HTTP da Video Management API conforme a skill de api-response-standardization: sucesso em `{ "success": true, "data": { ... }, "timestamp": "..." }` e erro em `{ "success": false, "error": { "code": "...", "message": "..." }, "timestamp": "..." }`, usando filtro para encapsular 200/201 e middleware (ou adaptação do existente) para exceções. **Esta story deve ser executada antes da Storie-06** para que todos os endpoints (incluindo PATCH e os demais) já utilizem o padrão.

## Escopo Técnico
- **Tecnologias:** .NET 10, ASP.NET Core, System.Text.Json
- **Arquivos criados/modificados:**
  - `src/VideoProcessing.VideoManagement.Api/Models/ApiResponse.cs` (novo)
  - `src/VideoProcessing.VideoManagement.Api/Models/ApiErrorResponse.cs` (novo) — inclui `ErrorDetail`
  - `src/VideoProcessing.VideoManagement.Api/Filters/ApiResponseFilter.cs` (novo)
  - `src/VideoProcessing.VideoManagement.Api/Middleware/GlobalExceptionMiddleware.cs` (novo) ou adaptação de `Infra.CrossCutting.Middleware.GlobalExceptionHandlerMiddleware` para retornar `ApiErrorResponse`
  - `src/VideoProcessing.VideoManagement.Api/DependencyInjection/ServiceCollectionExtensions.cs` (registro do filtro)
  - `src/VideoProcessing.VideoManagement.Api/Program.cs` (middleware, JSON camelCase, exclusão opcional de /health)
- **Componentes:** `ApiResponse<T>`, `ApiErrorResponse`, `ErrorDetail`, `ApiResponseFilter`, middleware de exceção global
- **Pacotes/Dependências:** Nenhum novo (uso de Microsoft.AspNetCore.Mvc e System.Text.Json já presentes)

## Dependências e Riscos (para estimativa)
- **Dependências:** Controllers existentes (Videos, Health); middleware atual `GlobalExceptionHandlerMiddleware` em Infra — pode ser adaptado ou substituído pelo contrato da skill. **Ordem:** executar após Storie-05 (GET listagem); antes de Storie-06.
- **Riscos:** Endpoints que hoje retornam corpo direto passarão a ser encapsulados; garantir que `/health` (e similares) possam ser excluídos do envelope se necessário para compatibilidade com load balancers/ferramentas.

## Subtasks
- [x] [Subtask 01: Modelos ApiResponse, ApiErrorResponse e ErrorDetail](./subtask/Subtask-01-Modelos_ApiResponse_ApiErrorResponse.md)
- [x] [Subtask 02: Filtro ApiResponseFilter (encapsular 200/201)](./subtask/Subtask-02-Filtro_ApiResponse_200_201.md)
- [x] [Subtask 03: Middleware de exceção global e ApiErrorResponse](./subtask/Subtask-03-Middleware_GlobalException_ApiErrorResponse.md)
- [x] [Subtask 04: Registro no DI, JSON camelCase e exclusões (health)](./subtask/Subtask-04-Registro_DI_JSON_Exclusoes.md)
- [x] [Subtask 05: Testes unitários (filtro e middleware)](./subtask/Subtask-05-Testes_Unitarios_Padronizacao_Resposta.md)

## Critérios de Aceite da História
- [x] Respostas de sucesso (200/201) retornam JSON no formato `{ "success": true, "data": <payload>, "timestamp": "<ISO8601 UTC>" }`.
- [x] Respostas de erro (4xx/5xx) retornam JSON no formato `{ "success": false, "error": { "code": "<string>", "message": "<string>" }, "timestamp": "<ISO8601 UTC>" }`.
- [x] Controllers continuam retornando `Ok(model)` ou `CreatedAtAction(..., model)`; o filtro encapsula automaticamente em `ApiResponse<T>`.
- [x] Exceções não tratadas são capturadas pelo middleware e mapeadas para (statusCode, code, message) com pelo menos: Unauthorized (401), BadRequest (400), NotFound (404), InternalServerError (500); domínio/SDK (ex.: Cognito, DynamoDB) podem ser estendidos em `MapException`.
- [x] JSON da API usa `PropertyNamingPolicy.CamelCase` de forma que o contrato saia como `success`, `data`, `error`, `timestamp`, `code`, `message`.
- [x] Endpoint `/health` (ou equivalente) pode ser excluído do envelope quando necessário (filtro não encapsula).
- [x] Testes unitários cobrindo o filtro de resposta e o mapeamento de exceções do middleware; `dotnet test` passa.

## Rastreamento (dev tracking)
- **Início:** 21/02/2026, às 17:50 (Brasília)
- **Fim:** 21/02/2026, às 17:56 (Brasília)
- **Tempo total de desenvolvimento:** 6min
