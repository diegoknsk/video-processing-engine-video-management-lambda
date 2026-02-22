# Subtask 02: Steps de deploy AWS (update-function-code e Handler)

## Descrição
Adicionar ao workflow os steps de deploy na AWS: configurar credenciais AWS (usando secrets), atualizar o código da função Lambda com o ZIP (aws lambda update-function-code), aguardar a função ficar atualizada (aws lambda wait function-updated), e atualizar a configuração da função incluindo o Handler correto para o projeto .NET LambdaUpdateVideo (aws lambda update-function-configuration --handler). O nome da função deve vir de LAMBDA_FUNCTION_UPDATE_STATUS_NAME.

## Passos de Implementação
1. Adicionar step "Configure AWS credentials" (condicionado a push ou workflow_dispatch): usar aws-actions/configure-aws-credentials@v4 com aws-access-key-id, aws-secret-access-key, aws-session-token e aws-region (variável ou default us-east-1)
2. Adicionar step "Deploy to Lambda": aws lambda update-function-code --function-name ${{ vars.LAMBDA_FUNCTION_UPDATE_STATUS_NAME }} --zip-file fileb://deployment-package-update-video.zip (caminho do ZIP gerado no job)
3. Adicionar step "Wait for Lambda update": aws lambda wait function-updated --function-name ${{ vars.LAMBDA_FUNCTION_UPDATE_STATUS_NAME }}
4. Adicionar step "Update Lambda configuration": aws lambda update-function-configuration --function-name ${{ vars.LAMBDA_FUNCTION_UPDATE_STATUS_NAME }} --handler <valor do handler>. O handler para Lambda .NET (handler padrão) deve ser obtido do template do projeto (ex.: Assembly::Namespace.Class::Method); documentar o valor no step ou em variável
5. (Opcional) Step "Verify deployment": aws lambda get-function --function-name ${{ vars.LAMBDA_FUNCTION_UPDATE_STATUS_NAME }} --query 'Configuration.[FunctionName,LastModified,State,Handler]' --output table
6. Garantir que os steps de deploy só rodem em push ou workflow_dispatch (não em pull_request), para evitar deploy em PRs

## Formas de Teste
1. Executar workflow em branch com credenciais e variável LAMBDA_FUNCTION_UPDATE_STATUS_NAME configuradas; validar que update-function-code e update-function-configuration retornam sucesso
2. Após deploy, na AWS Console (ou CLI) verificar que a função mostra LastModified atualizado e Handler correto
3. Invocar a função com evento de teste e validar que o handler responde (comportamento esperado conforme Storie-10)

## Critérios de Aceite da Subtask
- [ ] Step de configuração de credenciais AWS presente (secrets: AWS_ACCESS_KEY_ID, AWS_SECRET_ACCESS_KEY, AWS_SESSION_TOKEN; região via variável ou default)
- [ ] Step update-function-code com --zip-file apontando para o ZIP gerado no build
- [ ] Step wait function-updated para garantir que a função está ativa antes de alterar configuração
- [ ] Step update-function-configuration com --handler atualizado para o valor correto do projeto LambdaUpdateVideo
- [ ] Nome da função em todos os comandos usando LAMBDA_FUNCTION_UPDATE_STATUS_NAME (variável ou secret)
- [ ] Deploy não executado em pull_request (apenas em push ou workflow_dispatch)
