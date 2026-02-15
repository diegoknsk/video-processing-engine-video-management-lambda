# Subtask 02: Configurar GitHub Variables do workflow de deploy

## Descrição
Configurar no repositório GitHub as variables (Variables) exigidas pelo workflow de deploy da Storie-07, como AWS_REGION, LAMBDA_FUNCTION_NAME, GATEWAY_PATH_PREFIX, GATEWAY_STAGE e demais variáveis de ambiente da Lambda (DynamoDB, S3, Cognito, etc.), para que o workflow possa executar sem falha por variável ausente.

## Passos de Implementação
1. Consultar o workflow `.github/workflows/deploy-lambda-video-management.yml` e a documentação da Storie-07 para listar todas as variables utilizadas (ex.: `vars.AWS_REGION`, `vars.LAMBDA_FUNCTION_NAME`).
2. Acessar o repositório no GitHub → Settings → Secrets and variables → Actions → aba Variables.
3. Criar cada variable necessária com o valor correspondente ao ambiente (ex.: AWS_REGION=us-east-1, LAMBDA_FUNCTION_NAME=video-processing-engine-dev-video-management, GATEWAY_PATH_PREFIX=/videos, GATEWAY_STAGE=default).
4. Incluir variables para env vars da Lambda quando o workflow as injeta a partir de variables (DYNAMODB_TABLE_NAME, S3_BUCKET_VIDEO, S3_BUCKET_FRAMES, S3_BUCKET_ZIP, COGNITO_USER_POOL_ID, COGNITO_CLIENT_ID, etc.), conforme definido na Storie-07.
5. Documentar em docs ou README a lista de variables obrigatórias e exemplos de valor.

## Formas de Teste
1. Listar em Settings → Variables que todas as variables esperadas pelo workflow existem.
2. Executar o workflow; nenhum step deve falhar por "variable not set" ou valor vazio indevido.
3. Após deploy, conferir na console AWS (Lambda → Configuration → Environment variables) se as variáveis foram aplicadas corretamente.

## Critérios de Aceite da Subtask
- [ ] Todas as GitHub Variables exigidas pelo workflow da Storie-07 estão configuradas (AWS_REGION, LAMBDA_FUNCTION_NAME e demais usadas no YAML).
- [ ] Valores estão corretos para o ambiente alvo (dev/staging).
- [ ] Documentação (docs ou README) lista as variables obrigatórias e, quando útil, exemplos de valor.
- [ ] Execução do workflow não falha por falta ou valor incorreto de variable.
