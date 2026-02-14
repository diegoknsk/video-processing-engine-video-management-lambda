# Subtask 01: Criar workflow GitHub Actions de deploy (build, test, publish, zip, deploy)

## Descrição
Criar workflow `.github/workflows/deploy-lambda-video-management.yml` baseado no existente (docs/deploy-github-actions.md), com jobs de build (.NET 10), test (dotnet test; falha aborta deploy), publish (linux-x64), create ZIP, deploy via aws lambda update-function-code, wait for update, e validação pós-deploy.

## Passos de Implementação
1. Criar arquivo `.github/workflows/deploy-lambda-video-management.yml`
2. Triggers: on push/PR para main, workflow_dispatch (manual) com inputs (lambda_function_name, aws_region, gateway_path_prefix)
3. Job build-and-deploy: steps: checkout, setup .NET 10, restore, build (Release), test (abort se falhar), publish (linux-x64, --self-contained false), create ZIP (cd publish && zip -r deployment-package.zip .), configure AWS credentials (secrets), update-function-code, wait for Active state, update handler, update env vars, verify deployment (get-function), upload artifact (ZIP)
4. Usar variáveis: ${{ vars.AWS_REGION }}, ${{ vars.LAMBDA_FUNCTION_NAME }}, etc.; inputs do workflow_dispatch sobrescrevem
5. Step "Update Lambda handler": `aws lambda update-function-configuration --function-name $FUNCTION_NAME --handler VideoProcessing.VideoManagement.Api`
6. Step "Update environment variables": `aws lambda get-function-configuration`, mesclar env vars existentes com novas (Cognito, DynamoDB, S3, GATEWAY_PATH_PREFIX), `aws lambda update-function-configuration --environment Variables=...`

## Formas de Teste
1. Workflow syntax: validar YAML com GitHub Actions linter
2. Dry-run: executar workflow manualmente (workflow_dispatch) em branch de teste
3. Validar steps: cada step logado e bem-sucedido

## Critérios de Aceite da Subtask
- [ ] Workflow criado em `.github/workflows/deploy-lambda-video-management.yml`
- [ ] Triggers: push/PR main, workflow_dispatch com inputs
- [ ] Steps: checkout, setup .NET 10, restore, build, test, publish, zip, configure AWS, update-function-code, wait, update handler, update env vars, verify, upload artifact
- [ ] Testes abortam deploy se falharem
- [ ] Handler configurado como `VideoProcessing.VideoManagement.Api`
- [ ] Env vars mescladas corretamente (preserva existentes, adiciona/atualiza novas)
- [ ] Workflow valida sintaxe (sem erros YAML)
