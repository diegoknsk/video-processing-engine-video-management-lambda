# Subtask 02: Incluir step smoke test no workflow GitHub Actions

## Descrição
Adicionar um step (ou job) no workflow de deploy que, após atualizar a Lambda (update-function-code e configuração), chama a URL pública do API Gateway (GET /health) montada com as variáveis GATEWAY_PATH_PREFIX e GATEWAY_STAGE, e valida status 200 e corpo JSON; se falhar, o job falha.

## Passos de Implementação
1. No workflow `.github/workflows/deploy-lambda-video-management.yml`, após o step "Wait for Lambda update" (e opcionalmente após update handler/env vars), adicionar step "Smoke test".
2. Montar URL: usar GitHub Variables ou inputs (API_GATEWAY_BASE_URL ou URL completa do health); ou construir a partir de AWS_REGION, API_ID, GATEWAY_STAGE, GATEWAY_PATH_PREFIX (ex.: `https://${{ vars.API_GATEWAY_ID }}.execute-api.${{ vars.AWS_REGION }}.amazonaws.com/${{ vars.GATEWAY_STAGE }}/videos/health` — ajustar conforme convenção).
3. Executar: `curl -sf -o /dev/null -w "%{http_code}" URL` e validar 200; ou usar action como curl e checar body por "healthy".
4. Se o smoke test falhar (status != 200 ou body inválido), falhar o step (exit 1) para abortar o deploy como "não validado".

## Formas de Teste
1. Executar workflow manualmente; validar que step smoke test roda e passa quando Lambda está OK.
2. Simular falha (ex.: URL errada); validar que o job falha.
3. Revisar logs do step: URL chamada e resposta exibidas (sem expor secrets).

## Critérios de Aceite da Subtask
- [ ] Step "Smoke test" adicionado ao workflow após deploy
- [ ] URL do health montada a partir de variáveis (API Gateway base, stage, path prefix)
- [ ] Step valida status HTTP 200 e (opcionalmente) corpo contém "healthy"
- [ ] Falha do smoke test falha o job (deploy considerado não validado)
- [ ] Documentação ou comentário no workflow explicam como configurar URL (vars)
