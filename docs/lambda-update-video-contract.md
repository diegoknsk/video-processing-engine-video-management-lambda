# Contrato da Lambda Update Video (VideoProcessing.VideoManagement.LambdaUpdateVideo)

Para os contratos completos da API (POST criar vídeo, GET obter, PATCH atualizar) com exemplos, veja [api-video-contracts.md](api-video-contracts.md).

## Visão geral

A Lambda **Update Video** é uma função AWS Lambda pura (handler .NET padrão, sem AddAWSLambdaHosting) que atualiza estado e metadados de um vídeo no DynamoDB. O contrato de entrada é o **mesmo do PATCH** do VideoManagement: **UpdateVideoInputModel** (Application) com `videoId` no evento. A Lambda reutiliza o mesmo **Use Case** (`IUpdateVideoUseCase`) e o mesmo **validator** (`UpdateVideoInputModelValidator`); a borda expõe apenas o tipo de evento **UpdateVideoLambdaEvent** (InputModel + videoId) e a resposta **UpdateVideoLambdaResponse**.

**Formas de invocação:**

A Lambda aceita dois formatos de evento, detectados automaticamente pelo adapter de entrada:

1. **Invocação via SQS (produção):** o evento é o envelope SQS da AWS: `{ "Records": [ { "body": "<JSON do update>" } ] }`. O adapter extrai cada `body`, desserializa para o DTO de update e processa um evento por mensagem. O Step Function (ou outro produtor) envia para a fila SQS o mesmo JSON do DTO (`videoId`, `userId`, `status`, etc.).
2. **Invocação direta (testes locais / Lambda Test Tool / execução manual):** o evento é o próprio JSON do DTO, sem envelope: `{ "videoId": "...", "userId": "...", "status": 2, ... }`. O adapter trata o payload como um único evento e processa normalmente.

## Event shape (entrada)

### Invocação direta (JSON do DTO)

O evento é um JSON com os seguintes campos (um único objeto de update):

| Campo             | Tipo   | Obrigatório | Descrição |
|------------------|--------|-------------|-----------|
| `videoId`        | string (Guid) | Sim | Identificador do vídeo a atualizar. |
| `userId`         | string (Guid) | Sim | Identificador do usuário dono do vídeo. |
| `status`         | int    | Não | Status do processamento. Enum: UploadPending=0, ProcessingImages=1, GeneratingZip=2, Completed=3, Failed=4, Cancelled=5. |
| `progressPercent`| int    | Não | Percentual de progresso (0–100). |
| `errorMessage`   | string | Não | Mensagem de erro em caso de falha. |
| `errorCode`      | string | Não | Código de erro opcional. |
| `framesPrefix`   | string | Não | Prefixo dos frames no S3. |
| `s3KeyZip`       | string | Não | Chave S3 do arquivo ZIP de saída. |
| `s3BucketFrames` | string | Não | Bucket S3 dos frames. |
| `s3BucketZip`    | string | Não | Bucket S3 do ZIP. |
| `stepExecutionArn` | string | Não | ARN da execução Step Functions. |
| `maxParallelChunks` | int | Não | Máximo de chunks processados em paralelo (1–100). |
| `processingStartedAt` | string (ISO 8601) | Não | Data/hora de início do processamento. |
| `processingSummary` | object | Não | Resumo de processamento com chunks (merge incremental). Ver estrutura abaixo. |

**Estrutura de `processingSummary`:**
- `chunks`: objeto com chaves = ChunkId e valor = objeto com: `chunkId`, `startSec`, `endSec`, `intervalSec`, `manifestPrefix`, `framesPrefix`. Cada chunk enviado é mergeado ao existente (idempotente: chunk já existente não é sobrescrito).

**Regras de validação:**

- `userId` é obrigatório.
- Pelo menos um campo de atualização (além de `userId`) deve ser informado: `status`, `progressPercent`, `errorMessage`, `errorCode`, `framesPrefix`, `s3KeyZip`, `s3BucketFrames`, `s3BucketZip`, `stepExecutionArn`, `maxParallelChunks`, `processingStartedAt` ou `processingSummary.chunks` (com ao menos um chunk).
- Quando informado, `progressPercent` deve estar entre 0 e 100.
- Quando informado, `status` deve ser um valor válido do enum (0–5). Valores: UploadPending=0, ProcessingImages=1, GeneratingZip=2, Completed=3, Failed=4, Cancelled=5.
- Quando informado, `maxParallelChunks` deve estar entre 1 e 100.
- Quando `processingSummary.chunks` é informado: cada chunk deve ter `chunkId` não vazio, `startSec >= 0`, `endSec > startSec`, `intervalSec > 0`.

### Invocação via SQS (produção)

O evento é o envelope padrão SQS da AWS. Cada mensagem na fila deve ter no **body** o mesmo JSON do DTO de update (campos acima).

**Exemplo de evento SQS completo (um record):**

```json
{
  "Records": [
    {
      "messageId": "msg-id-123",
      "receiptHandle": "...",
      "body": "{\"videoId\":\"3fa85f64-5717-4562-b3fc-2c963f66afa6\",\"userId\":\"7c9e6679-7425-40de-944b-e07fc1f90ae7\",\"status\":2,\"progressPercent\":50}",
      "attributes": {},
      "messageAttributes": {},
      "md5OfBody": "...",
      "eventSource": "aws:sqs",
      "eventSourceARN": "arn:aws:sqs:us-east-1:123456789012:my-queue",
      "awsRegion": "us-east-1"
    }
  ]
}
```

O conteúdo de `body` é uma **string** contendo o JSON do DTO (videoId, userId, status, etc.). Em produção, o Step Function (ou outro produtor) publica na fila SQS uma mensagem cujo body é esse JSON.

**Múltiplos records:** quando o evento contém mais de um item em `Records`, a Lambda processa cada um em ordem. Retorna a **primeira resposta de erro (4xx)** encontrada; se todos forem sucesso (200), retorna a **última resposta**. Assim, um record inválido falha o batch e permite retry pela SQS.

## Modelagem DynamoDB

A Lambda utiliza a **mesma tabela** do VideoManagement:

- **PK:** `USER#{userId}`
- **SK:** `VIDEO#{videoId}`

Atributos atualizados conforme o patch: `status`, `progressPercent`, `errorMessage`, `errorCode`, `framesPrefix`, `s3KeyZip`, `s3BucketZip`, `s3BucketFrames`, `maxParallelChunks`, `processingSummary` (JSON), `processingStartedAt`, `imagesProcessingCompletedAt`, `processingCompletedAt`, `lastFailedAt`, `lastCancelledAt`, `updatedAt`. As mesmas condições do repositório (ownership, progressão monotônica, transições de status) são aplicadas.

## Resposta

- **200:** sucesso; corpo inclui `statusCode: 200` e `video` (objeto com todos os campos do vídeo atualizado, incluindo `status`, `statusDescription`, `processingStartedAt`, `imagesProcessingCompletedAt`, `processingCompletedAt`, `lastFailedAt`, `lastCancelledAt`, `maxParallelChunks`, `processingSummary` quando aplicável).
- **400:** validação falhou; `errorCode`, `errorMessage`.
- **404:** vídeo não encontrado; `errorCode: "NotFound"`.
- **409:** conflito (regressão de progresso, transição de status inválida, etc.); `errorCode: "UpdateConflict"`.

## Exemplo mínimo (status e progresso)

Campos estritamente necessários para um update válido: `videoId`, `userId` e pelo menos um campo de atualização. Exemplo com `status` e `progressPercent`:

```json
{
  "videoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "status": 1,
  "progressPercent": 50
}
```

- `status`: 1 = ProcessingImages.  
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

- `status`: 3 = Completed. O objeto `video` na resposta inclui também `statusDescription` (ex.: "Concluído") para exibição amigável.  
- `progressPercent`: 100.  
- Demais campos opcionais preenchidos conforme uso (S3, Step Functions).

## Exemplo com processingSummary (merge de chunks)

```json
{
  "videoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "maxParallelChunks": 4,
  "processingStartedAt": "2026-03-08T21:00:00Z",
  "processingSummary": {
    "chunks": {
      "chunk-0": {
        "chunkId": "chunk-0",
        "startSec": 0,
        "endSec": 30,
        "intervalSec": 1,
        "manifestPrefix": "manifests/chunk-0/",
        "framesPrefix": "frames/chunk-0/"
      }
    }
  }
}
```

- Os chunks em `processingSummary.chunks` são mergeados de forma incremental e idempotente: envios repetidos do mesmo `chunkId` não sobrescrevem o valor já persistido.

## Uso em testes

### AWS Lambda Console (test event) — invocação direta

1. Crie um **test event** no console da função.
2. Use o **JSON direto do DTO** (exemplo mínimo ou completo abaixo) como corpo do evento — **não** use o envelope SQS.
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
