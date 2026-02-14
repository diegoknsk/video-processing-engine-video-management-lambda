# Storie-01.2: Health Controller para Teste de Deploy (Gateway)

## Status
- **Estado:** ✅ Concluída
- **Data de Conclusão:** 14/02/2026

## Descrição
Como desenvolvedor/DevOps, quero ter um endpoint GET /health e o middleware de gateway (GATEWAY_PATH_PREFIX, GATEWAY_STAGE) configurados assim que tivermos o mínimo da API, para validar o deploy atrás do API Gateway e garantir que a URL pública do smoke test funcione corretamente.

## Objetivo
Garantir endpoint GET /health pronto para teste de deploy e configurar o pipeline de request para funcionar atrás do API Gateway: implementar ou consolidar o endpoint /health, implementar o GatewayPathBaseMiddleware (ordem: antes de UseRouting()), documentar variáveis GATEWAY_PATH_PREFIX e GATEWAY_STAGE e como montar a URL do smoke test (ex.: `https://.../default/videos/health`).

## Escopo Técnico
- **Tecnologias:** .NET 10, ASP.NET Core, AWS Lambda (AddAWSLambdaHosting)
- **Arquivos criados/modificados:**
  - `src/VideoProcessing.VideoManagement.Api/Program.cs` (registro do middleware antes de UseRouting)
  - `src/VideoProcessing.VideoManagement.Api/Controllers/HealthController.cs` ou endpoint mínimo GET /health (se ainda não existir como controller)
  - `src/VideoProcessing.VideoManagement.Infra.CrossCutting/Middleware/GatewayPathBaseMiddleware.cs` (novo)
  - `docs/gateway-path-prefix.md` (atualizar ou referenciar; URL do smoke test)
- **Componentes:**
  - Endpoint GET /health (controller ou minimal API) retornando 200 com `{ "status": "healthy", "timestamp": "..." }`
  - GatewayPathBaseMiddleware: lê GATEWAY_STAGE e GATEWAY_PATH_PREFIX do ambiente; remove stage do path; define PathBase/Path quando prefixo presente
  - Documentação da URL de smoke test (stage + prefixo + /health)
- **Pacotes/Dependências:** Nenhum novo (usa apenas ASP.NET Core e variáveis de ambiente)

## Dependências e Riscos (para estimativa)
- **Dependências:** Story 01 concluída (mínimo da API com Program.cs, DI e pipeline básico)
- **Riscos:** Ordem do middleware incorreta causa 404 (middleware deve ser antes de UseRouting); variáveis vazias não devem quebrar (path inalterado quando não definidas)

## Subtasks
- [Subtask 01: Garantir endpoint GET /health (controller ou minimal API)](./subtask/Subtask-01-Endpoint_Health_Controller.md)
- [Subtask 02: Implementar GatewayPathBaseMiddleware e variáveis GATEWAY_PATH_PREFIX/GATEWAY_STAGE](./subtask/Subtask-02-GatewayPathBaseMiddleware_Env_Vars.md)
- [Subtask 03: Documentar URL do smoke test e variáveis de gateway](./subtask/Subtask-03-Documentar_URL_Smoke_Test_Gateway.md)

## Critérios de Aceite da História
- [x] GET /health retorna 200 OK com JSON `{ "status": "healthy", "timestamp": "<UTC ISO 8601>" }` (local e atrás do gateway)
- [x] GatewayPathBaseMiddleware implementado: lê GATEWAY_STAGE (opcional) e GATEWAY_PATH_PREFIX (opcional); remove stage do path; define PathBase e Path quando prefixo presente; ordem de aplicação: primeiro stage, depois prefix
- [x] Middleware registrado no pipeline **antes** de `UseRouting()` (conforme skill lambda-api-hosting)
- [x] Sem GATEWAY_PATH_PREFIX/GATEWAY_STAGE definidas: path não é alterado; GET /health funciona em `http://localhost:PORT/health`
- [x] Com GATEWAY_PATH_PREFIX=/videos e GATEWAY_STAGE=default: request `rawPath = "/default/videos/health"` resulta em PathBase=`/videos`, Path=`/health` e rota /health é encontrada
- [x] Documentação (docs ou README) descreve como montar a URL do smoke test: `{API_GATEWAY_BASE_URL}/{stage}/{path_prefix}/health` (ex.: `https://xxx.execute-api.region.amazonaws.com/default/videos/health`)
- [x] Teste local com variáveis GATEWAY_PATH_PREFIX e GATEWAY_STAGE setadas valida que GET na URL simulada (ex.: /default/videos/health) retorna 200
- [x] Testes unitários para o middleware (opcional nesta story): validar reescrita do path para combinações stage/prefix

## Rastreamento (dev tracking)
- **Início:** 14/02/2026 20:08
- **Fim:** 14/02/2026 20:41
- **Tempo total de desenvolvimento:** 33min
