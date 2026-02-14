# Storie-08: Smoke Test pós-Deploy (GET /health no Gateway)

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** —

## Descrição
Como DevOps/desenvolvedor, quero executar o deploy da Lambda e validar que a API responde corretamente atrás do API Gateway via smoke test (GET /health na URL pública considerando GATEWAY_PATH_PREFIX e GATEWAY_STAGE), para garantir que o deploy e a configuração de gateway estão corretos.

## Objetivo
Executar o deploy (workflow da Storie-07), construir a URL pública do smoke test com base em GATEWAY_PATH_PREFIX e GATEWAY_STAGE (ex.: `https://{API_GATEWAY_URL}/default/videos/health`), validar GET /health retornando 200 com JSON `{ "status": "healthy", ... }`, incluir step de smoke test no workflow (opcional ou obrigatório), e documentar o smoke test manual e a URL.

## Escopo Técnico
- **Tecnologias:** GitHub Actions (step smoke test), curl/Invoke-WebRequest, API Gateway HTTP API
- **Arquivos criados/modificados:**
  - `.github/workflows/deploy-lambda-video-management.yml` (step smoke test após deploy)
  - `docs/deploy-video-management-lambda.md` ou `docs/smoke-test.md` (seção ou doc do smoke test)
- **Componentes:**
  - Step "Smoke test" no workflow: chamar URL pública do API Gateway (montada com GATEWAY_PATH_PREFIX, GATEWAY_STAGE), validar status 200 e JSON
  - Documentação da URL do smoke test e validação manual
- **Pacotes/Dependências:** Nenhum novo

## Dependências e Riscos (para estimativa)
- **Dependências:**
  - Storie-07 concluída (workflow de deploy e Handler/env vars)
  - Storie-01.2 concluída (GET /health e GatewayPathBaseMiddleware; URL documentada)
  - Lambda deployada; API Gateway com rota para a Lambda; variáveis GATEWAY_PATH_PREFIX e GATEWAY_STAGE configuradas na Lambda
- **Riscos:** URL incorreta (stage ou prefix errado) causa 404; documentar fórmula da URL claramente

## Subtasks
- [Subtask 01: Executar deploy e validar via smoke test (GET /health na URL do gateway)](./subtask/Subtask-01-Executar_Deploy_Smoke_Test_Health.md)
- [Subtask 02: Incluir step smoke test no workflow GitHub Actions](./subtask/Subtask-02-Smoke_Test_No_Workflow.md)
- [Subtask 03: Documentar smoke test manual e URL (GATEWAY_PATH_PREFIX, GATEWAY_STAGE)](./subtask/Subtask-03-Documentar_Smoke_Test_Manual.md)

## Critérios de Aceite da História
- [ ] Deploy executado via workflow (Storie-07); smoke test executado após deploy
- [ ] URL do smoke test montada corretamente: `{API_GATEWAY_BASE_URL}/{stage?}/{path_prefix}/health` (stage omitido se $default)
- [ ] Smoke test (curl ou Invoke-WebRequest) para essa URL retorna 200 OK e JSON `{ "status": "healthy", "timestamp": "..." }`
- [ ] Step "Smoke test" no workflow (ou job separado) chama a URL pública e valida status 200 e corpo JSON; falha do smoke test falha o job
- [ ] Documentação descreve como obter a URL do API Gateway e montar a URL do smoke test; exemplos para diferentes combinações de stage/prefix
- [ ] Logs do CloudWatch revisados após deploy; validam que aplicação iniciou sem erros e respondeu ao request GET /health
- [ ] Se smoke test falhar (404, 500), troubleshooting documentado (Handler, GATEWAY_PATH_PREFIX, GATEWAY_STAGE, env vars)

## Rastreamento (dev tracking)
- **Início:** —
- **Fim:** —
- **Tempo total de desenvolvimento:** —
