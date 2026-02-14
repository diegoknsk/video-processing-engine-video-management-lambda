# Subtask 03: Documentar smoke test manual e URL (GATEWAY_PATH_PREFIX, GATEWAY_STAGE)

## Descrição
Documentar como executar o smoke test manualmente após o deploy: como obter a URL base do API Gateway, como montar a URL do endpoint /health considerando GATEWAY_PATH_PREFIX e GATEWAY_STAGE, exemplos de curl e PowerShell, e troubleshooting quando o smoke test falha (404, 500).

## Passos de Implementação
1. Em `docs/deploy-video-management-lambda.md` (ou `docs/smoke-test.md`), criar seção "Smoke test" ou "Validação pós-deploy".
2. Descrever: obter URL base do API Gateway (Console ou AWS CLI); fórmula da URL: `{base}/{stage?}/{path_prefix}/health`; quando usar stage (GATEWAY_STAGE não $default) e path_prefix (valor de GATEWAY_PATH_PREFIX, ex.: videos para /videos).
3. Exemplos: curl e Invoke-WebRequest com URL de exemplo; validação do status 200 e do JSON.
4. Troubleshooting: 404 (verificar GATEWAY_PATH_PREFIX e GATEWAY_STAGE; rota no API Gateway); 500 (verificar logs CloudWatch; Handler; env vars).
5. Referenciar documentação da Storie-01.2 (URL do smoke test e variáveis de gateway) se já existir.

## Formas de Teste
1. Seguir a documentação para executar smoke test manual; validar que está claro e completo.
2. Revisão: outro dev consegue executar o smoke test apenas com o doc.

## Critérios de Aceite da Subtask
- [ ] Documentação descreve como obter a URL base do API Gateway
- [ ] Fórmula da URL do smoke test documentada (stage + path_prefix + /health)
- [ ] Exemplos de curl e PowerShell (Invoke-WebRequest) para GET /health
- [ ] Troubleshooting: 404 (gateway/path), 500 (Handler, env, logs)
- [ ] Referência a GATEWAY_PATH_PREFIX e GATEWAY_STAGE e quando usá-las
