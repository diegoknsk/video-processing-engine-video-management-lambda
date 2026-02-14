# Subtask 02: Configurar Handler e variáveis de ambiente da Lambda (via workflow ou manual)

## Descrição
Garantir que Handler da função Lambda é `VideoProcessing.VideoManagement.Api` (nome do assembly) e que variáveis de ambiente estão configuradas corretamente (DynamoDB TableName/Region, S3 Buckets/Region, Cognito UserPoolId/ClientId/Region, GATEWAY_PATH_PREFIX, GATEWAY_STAGE, API_PUBLIC_BASE_URL opcional), validar timeout >= 30s e memory >= 512 MB (configuração via IaC ou Console).

## Passos de Implementação
1. Via workflow: step "Update Lambda handler" e "Update environment variables" (já criados na Subtask 01)
2. Via AWS Console (manual, para validação inicial): Lambda > Configuration > General configuration: editar Handler para `VideoProcessing.VideoManagement.Api`, Timeout para 30s, Memory para 512 MB
3. Via AWS Console (manual): Lambda > Configuration > Environment variables: adicionar/editar variáveis conforme lista de GitHub Variables
4. Validar env vars obrigatórias: Cognito__UserPoolId, Cognito__ClientId, Cognito__Region, DynamoDB__TableName, DynamoDB__Region, S3__BucketVideo, S3__BucketFrames, S3__BucketZip, S3__Region, GATEWAY_PATH_PREFIX (se aplicável), GATEWAY_STAGE (se não for $default)
5. Testar localmente com env vars mockadas (dotnet run com variáveis definidas) antes de deploy

## Formas de Teste
1. Console test: abrir Lambda no Console, validar Handler e env vars corretos
2. CLI test: `aws lambda get-function-configuration --function-name NOME`, validar Handler e Environment.Variables
3. Startup test: após deploy, verificar logs do CloudWatch; validar que configuração é lida sem erros (OptionsValidationException se inválida)

## Critérios de Aceite da Subtask
- [ ] Handler configurado como `VideoProcessing.VideoManagement.Api` (via workflow ou manual)
- [ ] Timeout >= 30s, Memory >= 512 MB (configuração via IaC recomendado)
- [ ] Env vars configuradas: Cognito (UserPoolId, ClientId, Region), DynamoDB (TableName, Region), S3 (BucketVideo, BucketFrames, BucketZip, Region), GATEWAY_PATH_PREFIX, GATEWAY_STAGE (se aplicável)
- [ ] Validação: `aws lambda get-function-configuration` retorna Handler e env vars corretos
- [ ] Logs do CloudWatch após startup não mostram OptionsValidationException
