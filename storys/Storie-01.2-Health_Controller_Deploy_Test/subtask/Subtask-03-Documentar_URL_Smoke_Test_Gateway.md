# Subtask 03: Documentar URL do smoke test e variáveis de gateway

## Descrição
Documentar como montar a URL pública do smoke test (GET /health) quando a API está atrás do API Gateway, usando GATEWAY_PATH_PREFIX e GATEWAY_STAGE, para que o pipeline de deploy e validação manual saibam qual URL chamar.

## Passos de Implementação
1. Atualizar ou criar seção em `docs/gateway-path-prefix.md` (ou em `docs/deploy-video-management-lambda.md` se já existir) com "URL do smoke test".
2. Fórmula da URL: `{API_GATEWAY_BASE_URL}/{stage?}/{path_prefix}/health` — stage omitido se for $default; path_prefix é o valor de GATEWAY_PATH_PREFIX sem barra inicial se necessário (ex.: /videos → segmento "videos" na URL).
3. Exemplos: stage default, prefix /videos → `https://xxx.execute-api.us-east-1.amazonaws.com/default/videos/health`; $default e prefix /videos → `https://xxx.../videos/health`.
4. Incluir tabela ou lista das variáveis GATEWAY_PATH_PREFIX e GATEWAY_STAGE com descrição e quando usar.

## Formas de Teste
1. Revisão da documentação: outro dev consegue montar a URL do smoke test apenas lendo o doc.
2. Teste real: após deploy, usar a URL documentada e validar 200.

## Critérios de Aceite da Subtask
- [x] Documentação descreve como montar a URL do smoke test (base + stage + prefix + /health)
- [x] Exemplos para cenários: stage nomeado + prefixo; $default + prefixo
- [x] Variáveis GATEWAY_PATH_PREFIX e GATEWAY_STAGE documentadas (descrição, quando definir, exemplos)
