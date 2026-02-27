# Deploy via GitHub Actions - Lambda Update Video

Este documento descreve o processo de deploy automatizado da **Lambda Update Video** (VideoProcessing.VideoManagement.LambdaUpdateVideo) para AWS via GitHub Actions. O pipeline faz build, testes, empacotamento em ZIP e atualização da função Lambda na AWS (código + Handler).

## Índice

- [Visão geral](#visão-geral)
- [Pré-requisitos](#pré-requisitos)
- [Variáveis e Secrets](#variáveis-e-secrets)
- [Como executar o deploy](#como-executar-o-deploy)
- [Teste manual após o deploy](#teste-manual-após-o-deploy)
- [Troubleshooting](#troubleshooting)
- [AWS Academy / credenciais temporárias](#aws-academy--credenciais-temporárias)

## Visão geral

O workflow `.github/workflows/deploy-lambda-update-video.yml`:

1. **Build & Test**: restore, build e `dotnet test` (falha aborta o deploy).
2. **Publish**: publica o projeto LambdaUpdateVideo para `linux-x64` (self-contained false).
3. **Package**: cria o ZIP do conteúdo da pasta de publish.
4. **Deploy** (apenas em `push` ou `workflow_dispatch`): configura credenciais AWS, `update-function-code` com o ZIP, `wait function-updated`, `update-function-configuration` com Handler e **variáveis de ambiente** (DynamoDB__TableName, DynamoDB__Region, AWS_REGION).
5. **Artifact**: faz upload do ZIP para download opcional.

O nome da função Lambda é obtido da variável **LAMBDA_FUNCTION_UPDATE_STATUS_NAME**. Credenciais AWS vêm dos secrets (incluindo **AWS_SESSION_TOKEN** para AWS Academy). O workflow usa **DYNAMODB_TABLE_NAME** (a mesma do deploy Video Management) para preencher `DynamoDB__TableName` na Lambda.

## Pré-requisitos

- **Função Lambda já criada** na AWS (via IaC ou manualmente). O workflow **não** cria a função; apenas atualiza código e configuração (Handler).
- Conta AWS com permissões suficientes (veja [AWS Academy](#aws-academy--credenciais-temporárias)).
- Repositório com **Secrets** e **Variables** configurados em `Settings > Secrets and variables > Actions`.

## Variáveis e Secrets

Configurar em **Settings → Secrets and variables → Actions**.

**Parâmetro novo:** apenas **LAMBDA_FUNCTION_UPDATE_STATUS_NAME**. Região, credenciais e **DYNAMODB_TABLE_NAME** são os mesmos do deploy Video Management; o workflow grava na Lambda as env vars `DynamoDB__TableName`, `DynamoDB__Region` e `AWS_REGION`.

### Variables (Actions → Variables)

| Nome | Descrição | Exemplo |
|------|-----------|---------|
| `LAMBDA_FUNCTION_UPDATE_STATUS_NAME` | **(novo)** Nome da função Lambda Update Video na AWS | `video-management-update-video` |
| `AWS_REGION` | Mesmo do deploy Video Management (opcional; default: `us-east-1`) | `us-east-1` |
| `DYNAMODB_TABLE_NAME` | Mesmo do deploy Video Management; usado para `DynamoDB__TableName` na Lambda | `video-processing-engine-dev-videos` |

### Secrets (Actions → Secrets)

| Nome | Descrição |
|------|-----------|
| `AWS_ACCESS_KEY_ID` | Mesmo do deploy Video Management |
| `AWS_SECRET_ACCESS_KEY` | Mesmo do deploy Video Management |
| `AWS_SESSION_TOKEN` | Mesmo do deploy Video Management (**obrigatório** para AWS Academy) |

## Como executar o deploy

### Por push

- Faça push (ou merge) para a branch **main**. O workflow será disparado; build e testes rodam em todo push/PR; os steps de deploy (AWS) só rodam em **push** ou **workflow_dispatch**, não em pull_request.

### Manual (workflow_dispatch)

1. No GitHub, vá em **Actions**.
2. Selecione o workflow **Deploy Lambda Update Video**.
3. Clique em **Run workflow** e escolha a branch (ex.: `main`).
4. Aguarde a conclusão; em caso de sucesso, a função Lambda estará atualizada.

## Teste manual após o deploy

### Evento JSON mínimo (exemplo)

Salve como `event.json` (conforme [lambda-update-video-contract.md](lambda-update-video-contract.md)):

```json
{
  "videoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "status": 2,
  "progressPercent": 50
}
```

### Invocar via AWS CLI

```bash
aws lambda invoke \
  --function-name "<LAMBDA_FUNCTION_UPDATE_STATUS_NAME>" \
  --payload fileb://event.json \
  response.json
```

Substitua `<LAMBDA_FUNCTION_UPDATE_STATUS_NAME>` pelo nome real da função (mesmo valor da variável do GitHub). Para inspecionar a resposta:

```bash
cat response.json
```

Resposta de sucesso inclui `statusCode: 200` e o objeto `video`; erros de validação/negócio retornam 400/404/409 com `errorCode` e `errorMessage`.

### Console AWS Lambda

1. No console AWS, abra **Lambda** → sua função.
2. Aba **Test** → crie um **Test event**.
3. Cole o JSON do exemplo acima (ou o [exemplo completo](lambda-update-video-contract.md#exemplo-completo-todos-os-campos)).
4. Ajuste `videoId` e `userId` para itens existentes na tabela DynamoDB, se quiser testar persistência.
5. Execute o teste e verifique a resposta.

### Variáveis de ambiente na função

O **workflow** já define na Lambda: `DynamoDB__TableName` (de `DYNAMODB_TABLE_NAME`), `DynamoDB__Region` e `AWS_REGION`. Não é necessário configurá-las manualmente no console após o deploy. (O `update-function-configuration` substitui o conjunto de variáveis de ambiente da função pelo que o workflow envia.)

## Troubleshooting

| Problema | Possível causa | Ação |
|----------|----------------|------|
| **Access Denied** ao atualizar Lambda | Permissões IAM insuficientes ou **AWS_SESSION_TOKEN** expirado (Academy) | Verifique políticas IAM (veja abaixo). Se usar Academy, renove as credenciais e atualize os secrets. |
| **Handler inválido** após deploy | Handler não corresponde ao assembly .NET | O workflow define: `VideoProcessing.VideoManagement.LambdaUpdateVideo::VideoProcessing.VideoManagement.LambdaUpdateVideo.Function::Handler`. Confira no csproj (LambdaHandler) e na documentação da Storie-10. |
| **Timeout** na execução da Lambda | Cold start ou lógica lenta | Aumente o timeout da função na AWS (ex.: 30 s). |
| **DynamoDB / env vars** | Tabela ou região incorretas | Configure `DynamoDB__TableName` e região nas variáveis de ambiente da função no console AWS. |
| **TableName must have length >= 1** (AmazonDynamoDBException) | A aplicação lê o nome da tabela da chave **DynamoDB__TableName**, não `TABLE_NAME`. | Na Lambda, adicione a variável **DynamoDB__TableName** com o valor da tabela (ex.: `video-processing-engine-dev-videos`). O nome exato da chave é obrigatório. |
| Workflow falha com variável vazia | `LAMBDA_FUNCTION_UPDATE_STATUS_NAME` não configurada | Defina a variable no repositório (Actions → Variables). |

## AWS Academy / credenciais temporárias

- **Session Token**: O uso de **AWS_SESSION_TOKEN** é necessário quando se usam credenciais temporárias (ex.: AWS Academy / LabRole). Sem ele, a autenticação pode falhar.
- **Permissões mínimas**: A role/usuário deve ter pelo menos:
  - `lambda:UpdateFunctionCode`
  - `lambda:UpdateFunctionConfiguration`
  - `lambda:GetFunction`
- **Premissa**: A função Lambda **já deve existir**. O workflow não cria função; apenas atualiza código e configuração (Handler). Se a LabRole não permitir criação de função, provisione a função via IaC ou manualmente antes de rodar o deploy.
- **Risco**: Se o Academy restringir essas permissões, o deploy falhará com erro de acesso; nesse caso, documente o erro e as permissões necessárias para o administrador do ambiente.
