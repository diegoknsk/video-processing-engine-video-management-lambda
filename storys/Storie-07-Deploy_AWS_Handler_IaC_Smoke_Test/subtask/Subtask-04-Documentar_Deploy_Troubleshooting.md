# Subtask 04: Documentar processo de deploy, troubleshooting e variáveis de ambiente

## Descrição
Criar documentação completa em `docs/deploy-video-management-lambda.md` descrevendo pré-requisitos, configuração de GitHub Secrets/Variables, execução do workflow (manual e automático), validação pós-deploy (smoke test), troubleshooting comum (Handler incorreto, Timeout, GATEWAY_PATH_PREFIX, env vars faltando, erros de DynamoDB/S3), e atualizar README.md com seção "Deploy" linkando para a documentação.

## Passos de Implementação
1. Criar `docs/deploy-video-management-lambda.md` seguindo estrutura de docs/deploy-github-actions.md
2. Seções: Visão Geral, Pré-requisitos AWS (Lambda provisionada, API Gateway, DynamoDB, S3, Cognito), Configuração GitHub (Secrets, Variables com descrição completa), Como Funciona o Workflow (steps detalhados), Execução Manual (workflow_dispatch), Smoke Test (curl, validação), Troubleshooting (Handler, Timeout, GATEWAY_PATH_PREFIX, env vars, logs)
3. Tabela de variáveis de ambiente obrigatórias: nome, descrição, exemplo, padrão
4. Exemplos de comandos: aws lambda get-function-configuration, curl, aws logs tail
5. Seção de troubleshooting com erros comuns: "Handler not found" (solução: Handler = VideoProcessing.VideoManagement.Api), "Sandbox.Timedout" (solução: Timeout >= 30s), "404 Not Found" (solução: verificar GATEWAY_PATH_PREFIX e rota do API Gateway), "OptionsValidationException" (solução: verificar env vars obrigatórias)
6. Atualizar README.md: adicionar seção "Deploy" com link para docs/deploy-video-management-lambda.md, listar variáveis de ambiente obrigatórias em tabela

## Formas de Teste
1. Documentation review: ler documentação, validar que está clara e completa
2. Follow-the-docs test: seguir passo a passo da documentação para executar deploy; validar que funciona
3. README clarity: validar que seção "Deploy" no README é clara e linkada corretamente

## Critérios de Aceite da Subtask
- [ ] Documentação criada em `docs/deploy-video-management-lambda.md` com seções: Visão Geral, Pré-requisitos, Configuração GitHub, Workflow, Execução Manual, Smoke Test, Troubleshooting
- [ ] Tabela de variáveis de ambiente obrigatórias com nome, descrição, exemplo, padrão
- [ ] Seção de troubleshooting cobre: Handler incorreto, Timeout, GATEWAY_PATH_PREFIX, env vars, DynamoDB/S3 errors
- [ ] Exemplos de comandos (curl, aws cli) para smoke test e validação
- [ ] README.md atualizado com seção "Deploy" linkando para docs/deploy-video-management-lambda.md
- [ ] README lista variáveis de ambiente obrigatórias em tabela ou lista
- [ ] Documentação revisada e validada por execução real do processo
