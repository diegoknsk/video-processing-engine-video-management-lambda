# Subtask 04: Validar smoke test GET /health pós-deploy

## Descrição
Após o deploy concluído com sucesso, validar minimamente que a API responde na URL pública do API Gateway: realizar uma requisição GET no endpoint de health (ex.: `https://<api-id>.execute-api.<region>.amazonaws.com/default/videos/health`) e garantir que retorna 200 OK com JSON contendo "status": "healthy".

## Passos de Implementação
1. Obter a URL base do API Gateway (documentada em docs/gateway-path-prefix.md ou na Storie-01.2): formato esperado `{API_GATEWAY_BASE_URL}/{stage}/{path_prefix}/health` (ex.: `https://xxx.execute-api.us-east-1.amazonaws.com/default/videos/health`).
2. Executar GET nessa URL (curl, navegador ou Postman): `curl -i "https://<url-base>/default/videos/health"` (ajustar stage e path_prefix conforme GATEWAY_STAGE e GATEWAY_PATH_PREFIX).
3. Validar resposta: status HTTP 200 e corpo JSON com "status": "healthy" e "timestamp" (ISO 8601).
4. Se retornar 404 ou 502: verificar GATEWAY_PATH_PREFIX/GATEWAY_STAGE na Lambda, integração API Gateway ↔ Lambda e documentação da URL; corrigir e repetir o smoke test.

## Formas de Teste
1. curl: `curl -s "https://.../default/videos/health"` retorna JSON com status healthy.
2. Navegador: abrir a URL e conferir resposta 200 e conteúdo JSON.
3. Script ou step opcional no workflow: adicionar step "Smoke test" que faz GET na URL e falha o job se status != 200 (pode ser feito em story futura).

## Critérios de Aceite da Subtask
- [ ] GET na URL pública do API Gateway para /health (stage + path_prefix + /health) retorna HTTP 200.
- [ ] Corpo da resposta é JSON com "status": "healthy" e "timestamp" em formato ISO 8601.
- [ ] Smoke test executado após pelo menos um deploy bem-sucedido (Subtask 03).
- [ ] URL do smoke test documentada ou reproduzível a partir das variables GATEWAY_STAGE e GATEWAY_PATH_PREFIX.
