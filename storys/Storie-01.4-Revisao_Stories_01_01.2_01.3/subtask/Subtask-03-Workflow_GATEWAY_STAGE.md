# Subtask 03: Revisar workflow de deploy e variável GATEWAY_STAGE

## Descrição
Revisar o workflow `.github/workflows/deploy-lambda-video-management.yml` em relação às variáveis de ambiente enviadas à Lambda, em especial GATEWAY_STAGE (exigida pelas stories 01.2/01.3 e pela documentação do gateway).

## Passos de implementação
1. Abrir o workflow e listar todas as variáveis de ambiente passadas no step "Update Lambda configuration".
2. Comparar com a documentação (docs/gateway-path-prefix.md) e com as stories 01.2 e 01.3: GATEWAY_PATH_PREFIX e GATEWAY_STAGE são necessárias quando a API está atrás do API Gateway com stage nomeado.
3. Verificar se GATEWAY_STAGE está presente no bloco de environment Variables do workflow; documentar a ausência.
4. Registrar no Resultado da Revisão a recomendação de incluir GATEWAY_STAGE (ex.: vars.GATEWAY_STAGE) no workflow.
5. Confirmar que o Handler e o fluxo build/test/publish/zip estão corretos e documentar que estão OK.

## Formas de teste
1. Conferir na revisão a tabela "Workflow de deploy" com o finding sobre GATEWAY_STAGE.
2. Validar que a recomendação é acionável (incluir variável no workflow em follow-up).

## Critérios de aceite da subtask
- [x] Revisão documenta que GATEWAY_STAGE não está sendo enviada ao Lambda no workflow
- [x] Recomendação registrada: incluir GATEWAY_STAGE nas env vars do workflow (vars.GATEWAY_STAGE)
- [x] Handler e fluxo do workflow confirmados como corretos
- [x] Nenhuma alteração no arquivo YAML nesta story — apenas revisão
