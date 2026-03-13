# Permissão DynamoDB — Tabela de Chunks (Lambda Exec Role)

A função **Video Management Lambda** usa a tabela DynamoDB de chunks (`video-processing-engine-dev-video-chunks`) para:

- **GET /videos/{id}**: contar chunks processados (`dynamodb:Query`) via `VideoChunkRepository.CountProcessedAsync`
- **Upsert de chunks** (por outros componentes): gravar/atualizar chunks (`dynamodb:PutItem`) via `VideoChunkRepository.UpsertAsync`

A **role de execução da Lambda** (ex.: `video-processing-engine-dev-lambda-exec-role`) precisa ter permissão para essas ações nessa tabela.

## Erro típico

Se a role não tiver permissão, o GET individual retorna 500 e nos logs aparece:

```text
User: arn:aws:sts::ACCOUNT:assumed-role/ROLE_NAME/FUNCTION_NAME is not authorized to perform:
dynamodb:Query on resource: arn:aws:dynamodb:REGION:ACCOUNT:table/video-processing-engine-dev-video-chunks
because no identity-based policy allows the dynamodb:Query action
```

## Solução

Anexe à role de execução da Lambda uma policy que permita **Query** e **PutItem** na tabela de chunks (e no índice, se usar GSI). Exemplo de policy (substitua `ACCOUNT_ID`, `REGION` e o nome da tabela se for diferente):

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Sid": "DynamoDBVideoChunksTable",
      "Effect": "Allow",
      "Action": [
        "dynamodb:Query",
        "dynamodb:PutItem",
        "dynamodb:GetItem",
        "dynamodb:BatchWriteItem"
      ],
      "Resource": [
        "arn:aws:dynamodb:REGION:ACCOUNT_ID:table/video-processing-engine-dev-video-chunks",
        "arn:aws:dynamodb:REGION:ACCOUNT_ID:table/video-processing-engine-dev-video-chunks/index/*"
      ]
    }
  ]
}
```

Para ambiente dev típico (conta `380894490298`, região `us-east-1`):

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Sid": "DynamoDBVideoChunksTable",
      "Effect": "Allow",
      "Action": [
        "dynamodb:Query",
        "dynamodb:PutItem",
        "dynamodb:GetItem",
        "dynamodb:BatchWriteItem"
      ],
      "Resource": [
        "arn:aws:dynamodb:us-east-1:380894490298:table/video-processing-engine-dev-video-chunks",
        "arn:aws:dynamodb:us-east-1:380894490298:table/video-processing-engine-dev-video-chunks/index/*"
      ]
    }
  ]
}
```

## Como aplicar no console AWS

1. **IAM** → **Roles** → role da Lambda (ex.: `video-processing-engine-dev-lambda-exec-role`).
2. **Add permissions** → **Create inline policy** (ou anexe uma policy gerenciada).
3. Aba **JSON**, cole o JSON acima (ajustando conta/região/tabela se necessário).
4. **Review policy** → nome sugerido: `DynamoDBVideoChunksTable` → **Create policy**.

Se a infraestrutura for gerenciada por IaC (Terraform, CDK, SAM), adicione essa policy na definição da role da Lambda no projeto de infra.
