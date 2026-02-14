# Subtask 04: Integrar Scalar UI e configurar servers/PathBase

## Descrição
Substituir Swagger UI padrão por Scalar UI (interface moderna e melhor UX), configurar servers no OpenAPI para incluir URL base pública (API_PUBLIC_BASE_URL quando definido; caso contrário usar request base), garantir que Scalar UI respeita PathBase (GATEWAY_PATH_PREFIX) e funciona tanto localmente quanto atrás do API Gateway, e validar que "Try it" funciona corretamente.

## Passos de Implementação
1. Instalar package Scalar.AspNetCore: `dotnet add src/VideoProcessing.VideoManagement.Api package Scalar.AspNetCore` (se disponível; caso contrário, servir HTML estático)
2. Registrar UseScalarApiReference no pipeline (após UseSwagger): `app.MapScalarApiReference();` ou `app.UseStaticFiles(); app.MapGet("/scalar", () => Results.Content(scalarHtml, "text/html"))`
3. Configurar servers no OpenAPI: se API_PUBLIC_BASE_URL definido, incluir; caso contrário, Scalar usa o request base automaticamente
4. Validar que Scalar UI carrega em http://localhost:5000/scalar
5. Testar "Try it": fazer request para GET /health via Scalar, validar que funciona

## Formas de Teste
1. Scalar UI test: abrir http://localhost:5000/scalar, validar que interface carrega e exibe rotas
2. Try it test: usar Scalar para chamar GET /health, validar que retorna 200
3. PathBase test: definir GATEWAY_PATH_PREFIX=/videos-api, validar que Scalar ajusta URLs corretamente

## Critérios de Aceite da Subtask
- [ ] Scalar UI integrado (package ou HTML estático)
- [ ] GET /scalar acessível e exibe interface Scalar com todas as rotas
- [ ] Servers configurados no OpenAPI (API_PUBLIC_BASE_URL ou request base)
- [ ] "Try it" funciona para GET /health localmente
- [ ] Scalar respeita PathBase (GATEWAY_PATH_PREFIX) quando definido
