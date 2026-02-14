# Subtask 03: Executar deploy e validar via smoke test (GET /health)

## Descrição
Executar deploy manual via GitHub Actions (workflow_dispatch), aguardar conclusão, validar que função Lambda foi atualizada, executar smoke test (curl ou Invoke-WebRequest para GET /health via URL pública do API Gateway), validar resposta 200 com JSON `{ "status": "healthy", "timestamp": "..." }`, e revisar logs do CloudWatch para confirmar sucesso.

## Passos de Implementação
1. Executar workflow: GitHub > Actions > Deploy Lambda Video Management > Run workflow (branch main, inputs padrão)
2. Aguardar conclusão; validar que todos os steps passaram (verde)
3. Obter URL pública do API Gateway: AWS Console > API Gateway > HTTP API > Stages > URL base (ex.: https://xxx.execute-api.us-east-1.amazonaws.com/dev)
4. Construir URL de teste: se GATEWAY_PATH_PREFIX = /videos e rota é /health, URL final = `https://.../dev/videos/health` (ou `https://.../dev/health` se sem prefixo; depende da configuração do gateway)
5. Executar smoke test: `curl -i https://.../dev/videos/health` (ou PowerShell: `Invoke-WebRequest -Uri "https://..." -Method GET`)
6. Validar resposta: status 200, Content-Type application/json, body contém `"status":"healthy"` e `"timestamp"`
7. Revisar logs: CloudWatch Logs > /aws/lambda/video-processing-engine-dev-video-management > log stream mais recente; validar startup logs, request log de GET /health, sem erros

## Formas de Teste
1. Smoke test via curl: `curl -i URL/health`; validar 200 e JSON
2. Smoke test via browser: abrir URL/health no navegador; validar JSON exibido
3. Logs test: revisar CloudWatch; validar que request foi processado e respondido com 200

## Critérios de Aceite da Subtask
- [ ] Deploy executado via workflow; todos os steps passaram
- [ ] Smoke test `curl URL/health` retorna 200 OK com JSON `{ "status": "healthy", "timestamp": "..." }`
- [ ] Logs do CloudWatch mostram: startup sem erros, request GET /health, response 200, TraceId correlaciona
- [ ] Se smoke test falhar (404, 500), troubleshoot (Handler, GATEWAY_PATH_PREFIX, env vars) e re-deploy
- [ ] Deploy bem-sucedido validado e documentado
