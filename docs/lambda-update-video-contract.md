# Contrato da Lambda Update Video (VideoProcessing.VideoManagement.LambdaUpdateVideo)

## Visão geral

A Lambda **Update Video** é uma função AWS Lambda pura (handler .NET padrão, sem AddAWSLambdaHosting) que atualiza estado e metadados de um vídeo no DynamoDB. O contrato de entrada é o **mesmo do PATCH** do VideoManagement: **UpdateVideoInputModel** (Application) com `videoId` no evento. A Lambda reutiliza o mesmo **Use Case** (`IUpdateVideoUseCase`) e o mesmo **validator** (`UpdateVideoInputModelValidator`); a borda expõe apenas o tipo de evento **UpdateVideoLambdaEvent** (InputModel + videoId) e a resposta **UpdateVideoLambdaResponse**.

**Formas de invocação:**

- **Invocação direta:** payload JSON com `videoId`, `userId` e campos opcionais de atualização.
- **API Gateway (futuro):** body da requisição + `videoId` no path podem ser montados no mesmo event shape.
- **SQS:** o body da mensagem pode ser o JSON do evento.

## Event shape (entrada)

O evento é um JSON com os seguintes campos:

| Campo             | Tipo   | Obrigatório | Descrição |
|------------------|--------|-------------|-----------|
| `videoId`        | string (Guid) | Sim | Identificador do vídeo a atualizar. |
| `userId`         | string (Guid) | Sim | Identificador do usuário dono do vídeo. |
| `status`         | int    | Não | Status do processamento. Enum: Pending=0, Uploading=1, Processing=2, Completed=3, Failed=4, Cancelled=5. |
| `progressPercent`| int    | Não | Percentual de progresso (0–100). |
| `errorMessage`   | string | Não | Mensagem de erro em caso de falha. |
| `errorCode`      | string | Não | Código de erro opcional. |
| `framesPrefix`   | string | Não | Prefixo dos frames no S3. |
| `s3KeyZip`       | string | Não | Chave S3 do arquivo ZIP de saída. |
| `s3BucketFrames` | string | Não | Bucket S3 dos frames. |
| `s3BucketZip`    | string | Não | Bucket S3 do ZIP. |
| `stepExecutionArn` | string | Não | ARN da execução Step Functions. |

**Regras de validação:**

- `userId` é obrigatório.
- Pelo menos um campo de atualização (além de `userId`) deve ser informado: `status`, `progressPercent`, `errorMessage`, `errorCode`, `framesPrefix`, `s3KeyZip`, `s3BucketFrames`, `s3BucketZip` ou `stepExecutionArn`.
- Quando informado, `progressPercent` deve estar entre 0 e 100.
- Quando informado, `status` deve ser um valor válido do enum (0–5).

## Modelagem DynamoDB

A Lambda utiliza a **mesma tabela** do VideoManagement:

- **PK:** `USER#{userId}`
- **SK:** `VIDEO#{videoId}`

Atributos atualizados conforme o patch: `status`, `progressPercent`, `errorMessage`, `errorCode`, `framesPrefix`, `s3KeyZip`, `s3BucketZip`, `s3BucketFrames`, `updatedAt`. As mesmas condições do repositório (ownership, progressão monotônica, transições de status) são aplicadas.

## Resposta

- **200:** sucesso; corpo inclui `statusCode: 200` e `video` (objeto com todos os campos do vídeo atualizado).
- **400:** validação falhou; `errorCode`, `errorMessage`.
- **404:** vídeo não encontrado; `errorCode: "NotFound"`.
- **409:** conflito (regressão de progresso, transição de status inválida, etc.); `errorCode: "UpdateConflict"`.

## Exemplo mínimo (status e progresso)

Campos estritamente necessários para um update válido: `videoId`, `userId` e pelo menos um campo de atualização. Exemplo com `status` e `progressPercent`:

```json
{
  "videoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "status": 2,
  "progressPercent": 50
}
```

- `status`: 2 = Processing.  
- `progressPercent`: 50 (0–100).

## Exemplo completo (todos os campos)

Todos os campos possíveis do payload, com valores coerentes com o domínio:

```json
{
  "videoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "status": 3,
  "progressPercent": 100,
  "errorMessage": null,
  "errorCode": null,
  "framesPrefix": "videos/7c9e6679-7425-40de-944b-e07fc1f90ae7/3fa85f64-5717-4562-b3fc-2c963f66afa6/frames/",
  "s3KeyZip": "videos/7c9e6679-7425-40de-944b-e07fc1f90ae7/3fa85f64-5717-4562-b3fc-2c963f66afa6/out.zip",
  "s3BucketFrames": "my-bucket-frames",
  "s3BucketZip": "my-bucket-zip",
  "stepExecutionArn": "arn:aws:states:us-east-1:123456789012:execution:MyStateMachine:exec-123"
}
```

- `status`: 3 = Completed.  
- `progressPercent`: 100.  
- Demais campos opcionais preenchidos conforme uso (S3, Step Functions).

## Uso em testes

### AWS Lambda Console (test event)

1. Crie um **test event** no console da função.
2. Cole o **exemplo mínimo** ou **exemplo completo** no corpo do evento.
3. Ajuste `videoId` e `userId` para registros existentes na tabela DynamoDB (se quiser testar persistência).
4. Execute a função e verifique a resposta (200 com `video` ou 400/404/409 com `errorCode` e `errorMessage`).

### Variáveis de ambiente

Configure na função Lambda:

- `DynamoDB__TableName`: nome da tabela de vídeos.
- `DynamoDB__Region` (ou `AWS_REGION`): região do DynamoDB.

### Postman / HTTP (quando houver borda API Gateway)

Se a Lambda for exposta via API Gateway, use o mesmo JSON no **body** da requisição; o `videoId` pode vir no path e ser injetado no evento pelo mapeamento do Gateway.

---

## Integração com VideoManagement (proxy PATCH)

O **VideoManagement** (Lambda API com AddAWSLambdaHosting) mantém o endpoint **PATCH /videos/{id}** como **proxy** para a Lambda Update Video. O contrato para o cliente permanece o mesmo (body, respostas 200/400/404/409), sem breaking change.

- **Configuração:** em appsettings ou variáveis de ambiente, defina `Lambda:UpdateVideo:FunctionName` com o nome da função Lambda de update (ex.: `video-management-update-video`).
- **Comportamento:** o controller valida o body (FluentValidation), invoca a Lambda com o payload (videoId + body), mapeia a resposta e retorna 200 (com vídeo), 400 (validação), 404 (não encontrado) ou 409 (conflito).
- **Impacto para clientes:** nenhum; a rota e o contrato continuam iguais. Para invocar a Lambda diretamente (SQS, outra Lambda, API Gateway futura), use o event shape descrito neste documento.
