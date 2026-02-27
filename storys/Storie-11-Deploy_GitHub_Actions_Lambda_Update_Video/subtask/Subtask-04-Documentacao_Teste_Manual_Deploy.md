# Subtask 04: Documentação e teste manual pós-deploy

## Descrição
Criar documentação do processo de deploy da Lambda Update Video (variáveis, como rodar o workflow, troubleshooting) e incluir exemplos de como testar a Lambda manualmente após o deploy: comando aws lambda invoke com arquivo de evento JSON (ex.: exemplo mínimo da Storie-10) e/ou instruções para uso no console AWS Lambda (test event). Garantir que um desenvolvedor consiga executar o deploy e validar a função sem ambiguidade.

## Passos de Implementação
1. Criar documento `docs/deploy-lambda-update-video.md` (ou equivalente) com seções: Objetivo, Pré-requisitos (conta AWS, função Lambda já criada, GitHub repo com secrets/variables), Variáveis e Secrets (lista completa com LAMBDA_FUNCTION_UPDATE_STATUS_NAME, AWS_ACCESS_KEY_ID, AWS_SECRET_ACCESS_KEY, AWS_SESSION_TOKEN, AWS_REGION)
2. Incluir seção "Como executar o deploy": disparo por push na branch configurada (ex.: main) e disparo manual (workflow_dispatch) com passos para acionar no GitHub Actions
3. Incluir seção "Teste manual após deploy": exemplo de arquivo de evento (event.json) com payload mínimo (userId, videoId, status, progressPercent) conforme documentação da Storie-10; comando aws lambda invoke --function-name <LAMBDA_FUNCTION_UPDATE_STATUS_NAME> --payload fileb://event.json response.json; como inspecionar response.json
4. Incluir referência ao teste no console AWS (Lambda → Test tab, criar evento com o JSON de exemplo)
5. Incluir seção de troubleshooting: erro de permissão (IAM), Handler incorreto, timeout, variável de ambiente DynamoDB faltando; e referência às limitações AWS Academy quando aplicável
6. Atualizar README.md do repositório com link para esta documentação (seção Deploy ou Lambdas)

## Formas de Teste
1. Seguir a documentação passo a passo: configurar variáveis/secrets, executar workflow_dispatch, validar que o deploy conclui
2. Executar o comando aws lambda invoke com o event.json de exemplo e validar que a resposta é gerada (sucesso ou erro esperado conforme estado do DynamoDB)
3. Revisão de pares: documento está claro para quem nunca executou o deploy

## Critérios de Aceite da Subtask
- [ ] Documento de deploy criado com pré-requisitos, variáveis, secrets e como executar o workflow (push e manual)
- [ ] Exemplo de evento JSON para teste manual (mínimo) e comando aws lambda invoke documentados
- [ ] Instruções para teste no console AWS (test event) incluídas ou referenciadas
- [ ] Seção de troubleshooting com erros comuns (permissão, Handler, timeout, env vars)
- [ ] README ou índice de documentação atualizado com link para o deploy da Lambda Update Video
