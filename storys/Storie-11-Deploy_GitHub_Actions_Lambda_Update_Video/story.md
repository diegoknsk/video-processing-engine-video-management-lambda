# Storie-11: Deploy via GitHub Actions do Lambda Update Video (ZIP)

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** —

## Descrição
Como DevOps/desenvolvedor, quero ter um pipeline de deploy via GitHub Actions para a Lambda **VideoProcessing.VideoManagement.LambdaUpdateVideo** (criada na Storie-10), realizando build e testes, empacotamento em ZIP, atualização da função na AWS via AWS CLI (update-function-code e update-function-configuration incluindo Handler), usando variável/secret para o nome da função e credenciais padrão do AWS Academy, para que a Lambda de update de vídeo seja implantada de forma automatizada e reproduzível.

## Objetivo
Criar workflow GitHub Actions dedicado ao deploy da Lambda Update Video: restore → build → test → publish do projeto LambdaUpdateVideo (runtime linux-x64) → criar ZIP do pacote de deploy → atualizar a função Lambda na AWS (update-function-code com o ZIP) → atualizar configuração (Handler e variáveis de ambiente quando necessário). Utilizar **LAMBDA_FUNCTION_UPDATE_STATUS_NAME** (GitHub Variable ou Secret) para o nome da função Lambda na AWS; credenciais AWS via secrets (AWS_ACCESS_KEY_ID, AWS_SECRET_ACCESS_KEY, AWS_SESSION_TOKEN para AWS Academy). Incluir documentação com exemplos de como rodar o workflow (manual e por push) e como testar a Lambda manualmente após o deploy (invoke via AWS CLI ou console).

## Escopo Técnico
- **Tecnologias:** GitHub Actions, AWS CLI, .NET 10, dotnet publish, zip
- **Arquivos criados/modificados:**
  - `.github/workflows/deploy-lambda-update-video.yml` (novo workflow)
  - Documentação em `docs/deploy-lambda-update-video.md` (ou equivalente): variáveis/secrets necessários, como executar o workflow, como testar a Lambda manualmente (exemplo de evento JSON e comando aws lambda invoke)
- **Componentes:** Job de build/test, job ou steps de publish e zip, steps de deploy (update-function-code, update-function-configuration para Handler)
- **Pacotes/Dependências:** Nenhum novo no código; workflow usa actions oficiais (checkout, setup-dotnet, configure-aws-credentials) e AWS CLI

## Dependências e Riscos (para estimativa)
- **Dependências:** Storie-10 concluída (projeto LambdaUpdateVideo existente e compilando)
- **Riscos e mitigações:**
  - **AWS Academy / LabRole:** permissões IAM podem ser limitadas (ex.: apenas certas ações Lambda, sem criação de função). Mitigação: documentar que a função Lambda deve já existir na AWS (provisionada via IaC ou manualmente); o workflow apenas faz update-function-code e update-function-configuration. Se o Academy não permitir `lambda:UpdateFunctionCode` ou `lambda:UpdateFunctionConfiguration`, registrar na story como risco e listar permissões mínimas necessárias na documentação.
  - Handler do .NET Lambda: o handler padrão para projeto .NET Lambda (assembly) deve ser documentado (ex.: `Assembly::Namespace.Class::Method` ou formato esperado pelo AWS Lambda .NET); o step de deploy deve atualizar o Handler no update-function-configuration para o valor correto do projeto LambdaUpdateVideo.
  - Variáveis de ambiente da Lambda (DynamoDB__TableName, AWS__Region, etc.): o workflow pode incluir step para atualizar env vars da função usando as mesmas GitHub Variables do VideoManagement quando aplicável, ou documentar configuração manual.

## Subtasks
- [Subtask 01: Workflow build, test, publish e ZIP do projeto LambdaUpdateVideo](./subtask/Subtask-01-Workflow_Build_Test_Publish_Zip.md)
- [Subtask 02: Steps de deploy AWS (update-function-code e Handler)](./subtask/Subtask-02-Deploy_AWS_Update_Code_Handler.md)
- [Subtask 03: Variáveis e secrets (LAMBDA_FUNCTION_UPDATE_STATUS_NAME e AWS Academy)](./subtask/Subtask-03-Variaveis_Secrets_AWS_Academy.md)
- [Subtask 04: Documentação e teste manual pós-deploy](./subtask/Subtask-04-Documentacao_Teste_Manual_Deploy.md)

## Critérios de Aceite da História
- [ ] Workflow `.github/workflows/deploy-lambda-update-video.yml` criado com job(s): checkout, setup .NET 10, restore, build, test (dotnet test — falha aborta deploy), publish do projeto LambdaUpdateVideo (linux-x64), criar ZIP do conteúdo do publish
- [ ] Workflow executa deploy: configure AWS credentials (secrets AWS_ACCESS_KEY_ID, AWS_SECRET_ACCESS_KEY, AWS_SESSION_TOKEN), aws lambda update-function-code --function-name $LAMBDA_FUNCTION_UPDATE_STATUS_NAME --zip-file fileb://deployment-package.zip
- [ ] Workflow atualiza o Handler da função: aws lambda update-function-configuration --function-name $LAMBDA_FUNCTION_UPDATE_STATUS_NAME --handler <handler correto do projeto .NET LambdaUpdateVideo> (valor documentado na Storie-10 ou no README do projeto)
- [ ] Nome da função Lambda obtido de variável/secret **LAMBDA_FUNCTION_UPDATE_STATUS_NAME** (GitHub Variable ou Secret); demais credenciais AWS: AWS_ACCESS_KEY_ID, AWS_SECRET_ACCESS_KEY, AWS_SESSION_TOKEN (e AWS_REGION se necessário)
- [ ] Documentação criada com: lista de variáveis e secrets necessários, como rodar o workflow (workflow_dispatch e push na branch configurada), exemplo de teste manual da Lambda após deploy (ex.: aws lambda invoke com arquivo de evento JSON ou link para console)
- [ ] Riscos/mitigações AWS Academy documentados: permissões mínimas necessárias (lambda:UpdateFunctionCode, lambda:UpdateFunctionConfiguration, lambda:GetFunction); suposição de que a função já existe
- [ ] Build e testes do repositório (incluindo LambdaUpdateVideo) passam no workflow; deploy executado com sucesso em ambiente de teste (ou documentado como manual quando Academy não permitir)

## Rastreamento (dev tracking)
- **Início:** —
- **Fim:** —
- **Tempo total de desenvolvimento:** —
