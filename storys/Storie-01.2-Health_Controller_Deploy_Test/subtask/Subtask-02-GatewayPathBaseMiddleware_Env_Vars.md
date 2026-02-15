# Subtask 02: Implementar GatewayPathBaseMiddleware e variáveis GATEWAY_PATH_PREFIX/GATEWAY_STAGE

## Descrição
Implementar o middleware que reescreve o path da requisição quando a API está atrás do API Gateway HTTP API (v2), lendo GATEWAY_STAGE e GATEWAY_PATH_PREFIX do ambiente, e registrá-lo no pipeline **antes** de UseRouting(), conforme documentação (docs/gateway-path-prefix.md e skill lambda-api-hosting).

## Passos de Implementação
1. Criar classe `GatewayPathBaseMiddleware` em Infra.CrossCutting (ex.: `Middleware/GatewayPathBaseMiddleware.cs`).
2. Comportamento: (a) Se GATEWAY_STAGE definida: remover primeiro segmento do path se coincidir com o stage (case-insensitive). (b) Se GATEWAY_PATH_PREFIX definida: se path começar com o prefixo, definir Request.PathBase = prefixo e Request.Path = restante (case-insensitive).
3. Ordem de aplicação no middleware: primeiro remoção do stage, depois remoção do prefixo. Variáveis não definidas ou vazias: path não é alterado.
4. No Program.cs: registrar `app.UseMiddleware<GatewayPathBaseMiddleware>();` **antes** de `app.UseRouting();`.
5. Testar localmente sem variáveis: GET /health deve funcionar. Testar com GATEWAY_PATH_PREFIX=/videos e GATEWAY_STAGE=default: simular request com path "/default/videos/health" e validar que a rota /health é encontrada.

## Formas de Teste
1. Local sem env vars: GET /health retorna 200.
2. Local com GATEWAY_PATH_PREFIX=/videos: GET /videos/health retorna 200 (PathBase=/videos, Path=/health).
3. Local com GATEWAY_STAGE=default e path /default/health: Path fica /health e rota responde 200.
4. Teste unitário do middleware: injetar path e variáveis de ambiente; validar Path/PathBase resultantes.

## Critérios de Aceite da Subtask
- [x] GatewayPathBaseMiddleware implementado e registrado antes de UseRouting()
- [x] GATEWAY_STAGE: remove primeiro segmento do path quando coincide (case-insensitive)
- [x] GATEWAY_PATH_PREFIX: define PathBase e Path quando path começa com o prefixo (case-insensitive)
- [x] Variáveis vazias ou não definidas: path inalterado
- [x] Documentação de referência (docs/gateway-path-prefix.md ou lambda-api-hosting) respeitada
