# Contratos da API de Vídeos (Video Management)

Documentação dos endpoints **POST** (criar), **GET** (obter) e **PATCH** (atualizar) de vídeos, com exemplos de request/response.

---

## POST /videos — Criar vídeo e obter URL de upload

Registra um novo vídeo e retorna uma URL pré-assinada para upload no S3. Requer autenticação JWT (Cognito).

### Request (body)

| Campo              | Tipo   | Obrigatório | Descrição |
|--------------------|--------|-------------|-----------|
| `originalFileName` | string | Sim         | Nome original do arquivo de vídeo. |
| `contentType`      | string | Sim         | Tipo MIME (ex.: `video/mp4`). |
| `sizeKb`           | long   | Sim         | Tamanho do arquivo em **quilobytes** (KB). |
| `durationSec`      | double | Não         | Duração do vídeo em segundos. |
| `frameIntervalSec` | double | Não         | Intervalo em segundos para captura de frames (uso pelo processador). |
| `maxParallelChunks` | int   | Não         | Máximo de chunks em paralelo (1–100). Se omitido, **default 1**. |
| `clientRequestId`  | string | Não         | UUID para idempotência; mesmo valor + mesmo usuário = mesmo vídeo. |

### Exemplo de request (seu payload)

```json
{
  "originalFileName": "Final Fantasy XVI Ascension OST (Ifrit Risen vs Bahamut Theme).mp4",
  "contentType": "video/mp4",
  "sizeKb": 463000,
  "durationSec": 248,
  "frameIntervalSec": 5,
  "maxParallelChunks": 4
}
```

- `sizeKb`: 463000 KB ≈ 452 MB.  
- `durationSec`: 248 s.  
- `frameIntervalSec`: 5 s (um frame a cada 5 segundos).  
- `maxParallelChunks`: 4 (opcional; se omitido, usa **1**).

### Resposta 201 Created

```json
{
  "videoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "uploadUrl": "https://s3.amazonaws.com/bucket/videos/.../original?X-Amz-Signature=...",
  "expiresAt": "2026-03-08T22:00:00.0000000Z"
}
```

O `maxParallelChunks` pode ser informado já no POST (1–100). Os demais campos novos (timestamps do pipeline, `processingSummary`) são preenchidos durante o **processamento** e aparecem no **GET** e no **PATCH** (resposta).

---

## GET /videos/{id} — Obter vídeo

Retorna o vídeo pelo `id`. Na resposta entram todos os campos persistidos, incluindo os novos da Story 16 (timestamps do pipeline, `maxParallelChunks`, `processingSummary`).

### Resposta 200 OK (forma completa)

Campos que podem vir preenchidos após criação e ao longo do processamento:

```json
{
  "videoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "originalFileName": "Final Fantasy XVI Ascension OST (Ifrit Risen vs Bahamut Theme).mp4",
  "contentType": "video/mp4",
  "sizeBytes": 474112000,
  "durationSec": 248,
  "frameIntervalSec": 5,
  "status": 1,
  "statusDescription": "Processando imagens",
  "processingMode": 0,
  "progressPercent": 0,
  "s3BucketVideo": "my-bucket-video",
  "s3KeyVideo": "videos/7c9e6679-.../3fa85f64-.../original",
  "s3BucketZip": null,
  "s3KeyZip": null,
  "s3BucketFrames": null,
  "framesPrefix": null,
  "stepExecutionArn": null,
  "errorMessage": null,
  "errorCode": null,
  "clientRequestId": null,
  "chunkCount": null,
  "chunkDurationSec": null,
  "uploadIssuedAt": null,
  "uploadUrlExpiresAt": null,
  "framesProcessed": null,
  "finalizedAt": null,
  "maxParallelChunks": null,
  "processingSummary": null,
  "processingStartedAt": null,
  "imagesProcessingCompletedAt": null,
  "processingCompletedAt": null,
  "lastFailedAt": null,
  "lastCancelledAt": null,
  "createdAt": "2026-03-08T21:30:00.0000000Z",
  "updatedAt": "2026-03-08T21:30:00.0000000Z",
  "version": 1
}
```

**Campos novos (Story 16):**

- `maxParallelChunks`: máximo de chunks em paralelo (1–100). No POST, se omitido, **default 1**.  
- `processingSummary`: resumo com chunks (merge incremental); formato abaixo.  
- Timestamps do pipeline: `processingStartedAt`, `imagesProcessingCompletedAt`, `processingCompletedAt`, `lastFailedAt`, `lastCancelledAt` (ver tabela abaixo).

**Status (enum):** 0=UploadPending, 1=ProcessingImages, 2=GeneratingZip, 3=Completed, 4=Failed, 5=Cancelled.

### Quando os campos de pipeline são preenchidos e persistência (DynamoDB)

Todos os campos abaixo são **persistidos no DynamoDB** (Create e Update). Quando cada um é preenchido:

| Campo | Quando é preenchido | Persistido no DynamoDB |
|-------|---------------------|-------------------------|
| `maxParallelChunks` | **POST (criar):** valor enviado ou **default 1** se omitido. **PATCH:** valor enviado (opcional). | Sim (atributo `maxParallelChunks`) |
| `processingStartedAt` | **PATCH:** quando o orquestrador envia `processingStartedAt` no body. | Sim (atributo `processingStartedAt`, ISO 8601) |
| `imagesProcessingCompletedAt` | **Automático:** na transição de status `ProcessingImages` → `GeneratingZip` (via PATCH com `status`). | Sim (atributo `imagesProcessingCompletedAt`) |
| `processingCompletedAt` | **Automático:** na transição `GeneratingZip` → `Completed` ou ao chamar `MarkAsCompleted`. | Sim (atributo `processingCompletedAt`) |
| `lastFailedAt` | **Automático:** ao entrar em status `Failed` (PATCH com `status: 4` ou `MarkAsFailed`). | Sim (atributo `lastFailedAt`) |
| `lastCancelledAt` | **Automático:** ao entrar em status `Cancelled` (PATCH com `status: 5`). | Sim (atributo `lastCancelledAt`) |
| `processingSummary` | **PATCH:** quando o body envia `processingSummary.chunks` (merge incremental, idempotente). | Sim (atributo `processingSummary`, JSON) |

**Formato de `processingSummary` (quando preenchido):**

```json
{
  "processingSummary": {
    "chunks": {
      "chunk-0": {
        "chunkId": "chunk-0",
        "startSec": 0,
        "endSec": 62,
        "intervalSec": 5,
        "manifestPrefix": "manifests/chunk-0/",
        "framesPrefix": "frames/chunk-0/"
      }
    }
  }
}
```

---

## PATCH /videos/{id} — Atualizar vídeo (parcial)

Atualização parcial: apenas os campos enviados são considerados. Usado pelo orquestrador/processador (rota interna). O **body** é o mesmo da Lambda Update Video (sem `videoId` no body; o `id` vai no path). O `userId` é obrigatório no body.

### Campos permitidos no body

Além de `userId` (obrigatório), pelo menos um campo de atualização:

| Campo                  | Tipo   | Descrição |
|------------------------|--------|-----------|
| `userId`               | string (Guid) | Obrigatório. Dono do vídeo. |
| `status`               | int    | 0–5 (enum). |
| `progressPercent`      | int    | 0–100. |
| `errorMessage`         | string | Mensagem de erro. |
| `errorCode`            | string | Código de erro. |
| `framesPrefix`         | string | Prefixo dos frames no S3. |
| `s3KeyZip`             | string | Chave S3 do ZIP. |
| `s3BucketFrames`       | string | Bucket dos frames. |
| `s3BucketZip`          | string | Bucket do ZIP. |
| `stepExecutionArn`     | string | ARN da Step Function. |
| `maxParallelChunks`    | int    | 1–100. Máximo de chunks em paralelo. |
| `processingStartedAt`  | string (ISO 8601) | Início do processamento. |
| `processingSummary`    | object | Chunks a serem mergeados (idempotente). |

### Exemplo 1 — Só status e progresso

```json
{
  "userId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "status": 1,
  "progressPercent": 25
}
```

### Exemplo 2 — Novos campos (paralelismo + início + chunk)

```json
{
  "userId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "maxParallelChunks": 4,
  "processingStartedAt": "2026-03-08T21:35:00Z",
  "processingSummary": {
    "chunks": {
      "chunk-0": {
        "chunkId": "chunk-0",
        "startSec": 0,
        "endSec": 62,
        "intervalSec": 5,
        "manifestPrefix": "manifests/chunk-0/",
        "framesPrefix": "frames/chunk-0/"
      }
    }
  }
}
```

### Exemplo 3 — Conclusão (status Completed)

```json
{
  "userId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "status": 3,
  "progressPercent": 100,
  "s3BucketZip": "my-bucket-zip",
  "s3KeyZip": "videos/.../out.zip",
  "framesPrefix": "videos/.../frames/"
}
```

A resposta **200** do PATCH traz o objeto `video` completo (mesmo formato do GET), incluindo os novos campos quando existirem.

---

## Resumo

| Operação | Contrato |
|----------|----------|
| **POST /videos** | `originalFileName`, `contentType`, `sizeKb`, `durationSec`, `frameIntervalSec`, `maxParallelChunks` (1–100, opcional; **default 1**). Resposta: `videoId`, `uploadUrl`, `expiresAt`. |
| **GET /videos/{id}** | Inclui os novos campos: `maxParallelChunks`, `processingSummary`, `processingStartedAt`, `imagesProcessingCompletedAt`, `processingCompletedAt`, `lastFailedAt`, `lastCancelledAt`. |
| **PATCH /videos/{id}** | Pode enviar `maxParallelChunks`, `processingStartedAt`, `processingSummary`; a resposta traz o vídeo no mesmo formato do GET. |

Para o contrato detalhado da **Lambda Update Video** (evento direto, SQS, validações), veja [lambda-update-video-contract.md](lambda-update-video-contract.md).
