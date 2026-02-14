# Subtask 05: Adicionar exemplos e descrições de erros padronizados

## Descrição
Enriquecer documentação OpenAPI com exemplos de request/response para rotas principais (POST /videos, GET /videos/{id}), documentar respostas de erro padronizadas (400, 401, 404, 409, 500) com schema ErrorResponse, adicionar descrições claras de cada código de status, e garantir que OpenAPI valida em validator.swagger.io.

## Passos de Implementação
1. Criar SchemaFilter ou OperationFilter para adicionar exemplos de request: UploadVideoInputModel com valores realistas
2. Adicionar ProducesResponseType attributes nos endpoints (ou metadata no MapPost/MapGet) para documentar status codes: 200, 400, 401, 404, 409, 500
3. Documentar ErrorResponse schema para códigos de erro: 400 (validation), 401 (unauthorized), 404 (not found), 409 (conflict/idempotency), 500 (internal error)
4. Adicionar descrições de cada response: ex.: "200 OK: Video created successfully", "400 Bad Request: Invalid input", etc.
5. Validar JSON do OpenAPI em validator.swagger.io; corrigir warnings/erros

## Formas de Teste
1. Examples test: abrir Scalar UI, validar que exemplos de request aparecem em POST /videos
2. Error schemas test: acessar /swagger/v1/swagger.json, validar que responses 400/401/404/409/500 estão documentadas com schema ErrorResponse
3. Validation test: copiar JSON do OpenAPI e validar em validator.swagger.io; garantir 0 erros

## Critérios de Aceite da Subtask
- [ ] Exemplos de request adicionados para POST /videos (UploadVideoInputModel)
- [ ] Exemplos de response adicionados para POST /videos (UploadVideoResponseModel) e GET /videos/{id} (VideoResponseModel)
- [ ] ProducesResponseType ou equivalente documenta status codes: 200, 400, 401, 404, 409, 500
- [ ] Todas as responses de erro (400–500) referenciam schema ErrorResponse
- [ ] Descrições claras de cada status code presentes no OpenAPI
- [ ] OpenAPI JSON valida em validator.swagger.io sem erros
