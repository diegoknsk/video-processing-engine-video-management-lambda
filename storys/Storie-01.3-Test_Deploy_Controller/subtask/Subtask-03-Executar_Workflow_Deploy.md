# Subtask 03: Executar workflow de deploy e validar conclusão

## Descrição
Executar o workflow de deploy (criado na Storie-07) a partir do GitHub, seja por disparo manual (workflow_dispatch) ou por push na branch configurada, e validar que todos os steps são concluídos com sucesso (build, test, publish, zip, update-function-code, update handler, update env vars, verify).

## Passos de Implementação
1. Garantir que as Subtasks 01 e 02 estão concluídas (secrets e variables configurados).
2. No GitHub: Actions → selecionar o workflow "Deploy Lambda Video Management" (ou nome equivalente).
3. Executar manualmente via "Run workflow" (workflow_dispatch), escolhendo branch e inputs se o workflow tiver (ex.: lambda_function_name, aws_region).
4. Acompanhar a execução: steps de checkout, setup .NET, restore, build, test, publish, zip, configure AWS, update-function-code, wait for Active, update handler, update env vars e verify devem concluir em verde.
5. Se algum step falhar: corrigir conforme logs (credenciais, nome da função, permissões IAM, etc.) e reexecutar até sucesso.

## Formas de Teste
1. Workflow executado manualmente conclui com status "Success" (verde).
2. Logs de cada step não apresentam erro de permissão ou configuração.
3. Na AWS Console: Lambda → função configurada → código e configuração (handler, env vars) atualizados conforme esperado.

## Critérios de Aceite da Subtask
- [ ] Workflow de deploy executado a partir do GitHub (manual ou por push).
- [ ] Todos os jobs/steps do workflow concluem com sucesso.
- [ ] Função Lambda na AWS está com código atualizado (última modificação condizente com o run).
- [ ] Handler e variáveis de ambiente da Lambda estão configurados conforme o workflow (VideoProcessing.VideoManagement.Api e env vars aplicadas).
