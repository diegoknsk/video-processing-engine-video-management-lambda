# Subtask 01: Executar deploy e validar via smoke test (GET /health na URL do gateway)

## Descrição
Executar deploy manual via GitHub Actions (workflow_dispatch), aguardar conclusão, obter a URL pública do API Gateway, montar a URL do smoke test com base em GATEWAY_PATH_PREFIX e GATEWAY_STAGE (ex.: `https://xxx.../default/videos/health`), executar GET /health e validar resposta 200 com JSON esperado; revisar logs do CloudWatch.

## Passos de Implementação
1. Executar workflow: GitHub > Actions > Deploy Lambda Video Management > Run workflow (branch main, inputs padrão).
2. Aguardar conclusão; validar que todos os steps passaram (verde).
3. Obter URL base do API Gateway: AWS Console > API Gateway > HTTP API > Stages > URL base (ex.: https://xxx.execute-api.us-east-1.amazonaws.com).
4. Montar URL do smoke test: se GATEWAY_STAGE=default e GATEWAY_PATH_PREFIX=/videos, URL = `{base}/default/videos/health`; se $default, sem segmento de stage.
5. Executar smoke test: `curl -i https://.../default/videos/health` (ou PowerShell: `Invoke-WebRequest -Uri "URL" -Method GET`).
6. Validar resposta: status 200, Content-Type application/json, body contém "status":"healthy" e "timestamp".
7. Revisar logs: CloudWatch Logs > log group da Lambda > log stream mais recente; validar startup e request GET /health sem erros.

## Formas de Teste
1. Smoke test via curl: validar 200 e JSON.
2. Smoke test via browser: abrir URL/health no navegador; validar JSON.
3. Logs: revisar CloudWatch; validar request processado e 200.

## Critérios de Aceite da Subtask
- [ ] Deploy executado via workflow; todos os steps passaram
- [ ] URL do smoke test montada conforme GATEWAY_PATH_PREFIX e GATEWAY_STAGE
- [ ] Smoke test retorna 200 OK com JSON `{ "status": "healthy", "timestamp": "..." }`
- [ ] Logs do CloudWatch mostram startup sem erros e request GET /health com 200
- [ ] Se falhar (404/500), troubleshoot (Handler, GATEWAY_PATH_PREFIX, GATEWAY_STAGE) e re-deploy
