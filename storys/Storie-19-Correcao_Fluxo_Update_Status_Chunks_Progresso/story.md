# Storie-19: Correção do Fluxo Update Status — Chunks e ProgressPercent

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** [DD/MM/AAAA]

---

## Descrição
Como sistema de processamento de vídeo, quero que o fluxo de Update Status popule corretamente a tabela de chunks e reflita o percentual de progresso de forma precisa, para que os consumidores da API enxerguem o estado real do processamento.

---

## Objetivo
Corrigir o fluxo de `UpdateVideoUseCase` e `GetVideoByIdUseCase` para que:
1. A tabela de chunks seja populada mesmo quando o evento de finalização não inclui `processingSummary.chunks`.
2. O campo `progressPercent` retornado no GET use o valor armazenado na entidade como _floor_, evitando regressão e garantindo que um `progressPercent=100` enviado seja exibido corretamente.
3. O campo `finalize` recebido no evento Lambda seja mapeado e aproveitado para compor a entrada de finalização na tabela de chunks.

---

## Contexto

### Problema atual

O evento de finalização recebido pelo Lambda Update Status tem o seguinte formato (exemplo real):

```json
{
  "status": 2,
  "progressPercent": 100,
  "finalize": {
    "ordenaAutomaticamente": true,
    "outputBucket": "video-processing-engine-dev-zip",
    "framesBasePrefix": "videos/<userId>/<videoId>/frames/",
    "framesBucket": "video-processing-engine-dev-images",
    "outputBasePrefix": "<userId>/<videoId>",
    "videoId": "<videoId>"
  },
  "s3BucketFrames": "video-processing-engine-dev-images",
  "zipKey": "videos/<userId>/<videoId>/output.zip",
  "zipFileName": "<videoId>.zip",
  "videoId": "<videoId>",
  "zipBucket": "video-processing-engine-dev-zip",
  "framesPrefix": "videos/<userId>/<videoId>/frames/",
  "userId": "<userId>"
}
```

### Gap identificado — 3 pontos críticos

**1. Tabela de chunks não é populada no evento de finalização**

No `UpdateVideoUseCase.ExecuteAsync`, o bloco de upsert de chunks só é executado quando `merged.ProcessingSummary?.Chunks` é não nulo e não vazio:

```csharp
if (merged.ProcessingSummary?.Chunks is { } chunks && chunks.Count > 0)
{
    // upsert dos chunks
}
```

O evento de finalização acima **não inclui o campo `processingSummary`**. Por isso, `MapProcessingSummaryFromInput` retorna `null`, o dicionário de chunks nunca é preenchido e o bloco de upsert nunca é executado. A tabela de chunks (`video-processing-engine-dev-video-chunks`) permanece sem registros para o vídeo.

**2. ProgressPercent no GET ignora o valor armazenado na entidade**

O `GetVideoByIdUseCase` recomputa o progresso do zero:

```csharp
var progressPercent = video.Status == VideoStatus.Completed
    ? 100
    : ComputeProgressPercent(video.ParallelChunks ?? 0,
        await chunkRepository.CountProcessedAsync(videoId, ct));
```

Para vídeos com `status=Completed`, o GET já retorna 100 (caminho correto). Porém, para vídeos em estados intermediários, se a tabela de chunks estiver vazia (o caso atual), `CountProcessedAsync` retorna 0, `ComputeProgressPercent` retorna 0 — mesmo que o evento tenha enviado `progressPercent=50` e esse valor esteja armazenado na entidade `Video.ProgressPercent`. O campo armazenado é completamente ignorado.

Adicionalmente: se `ParallelChunks` for nulo, o divisor cai para 1, tornando qualquer chunk único = 100%, o que distorce o cálculo.

**3. Campo `finalize` silenciosamente ignorado**

O `UpdateVideoLambdaEvent` (que herda de `UpdateVideoInputModel`) não declara a propriedade `finalize`. O campo é desserializado e descartado silenciosamente. Isso impede qualquer uso futuro dos metadados de finalização (ex.: `framesBasePrefix`, `framesBucket`, `outputBucket`) para compor um chunk de finalização.

---

## Proposta Técnica

### Abordagem: pragmática, sem novo Lambda, sem refatoração ampla

#### Fix 1 — Mapear `finalize` no `UpdateVideoLambdaEvent`

Adicionar a propriedade `FinalizeInfo` (novo record `UpdateVideoFinalizeInfo`) ao `UpdateVideoLambdaEvent`. Campos mínimos necessários: `VideoId`, `FramesBucket`, `FramesBasePrefix`, `OutputBucket`, `OutputBasePrefix`.

O `UpdateVideoHandler` deve repassar esses dados ao `UpdateVideoInputModel` de forma que o use case possa usá-los.

Alternativa mais simples (preferida): mapear `finalize` apenas no `UpdateVideoLambdaEvent` e passar `framesBasePrefix` e `framesBucket` como campos adicionais no `UpdateVideoInputModel` — ou criar um campo `FinalizeInfo` opaco que o use case interpreta.

#### Fix 2 — Criar chunk de finalização quando `processingSummary` ausente e `status=Completed`

No `UpdateVideoUseCase.ExecuteAsync`, adicionar lógica de fallback: quando o status resultante for `Completed` e `merged.ProcessingSummary?.Chunks` for nulo/vazio, criar um único `VideoChunk` de finalização usando o `videoId` como `ChunkId` (ou uma chave sintética como `"finalize"`), com `Status="completed"` e os metadados S3 disponíveis no evento.

Isso garante que a tabela de chunks tenha pelo menos um registro "completed" para cada vídeo finalizado, tornando `CountProcessedAsync` retornar ≥ 1.

#### Fix 3 — Usar `video.ProgressPercent` como _floor_ no GET

No `GetVideoByIdUseCase.ExecuteAsync`, aplicar `Math.Max` entre o progresso computado pelos chunks e o valor armazenado na entidade:

```csharp
var computedFromChunks = ComputeProgressPercent(
    video.ParallelChunks ?? 0,
    await chunkRepository.CountProcessedAsync(videoId, ct));

var progressPercent = video.Status == VideoStatus.Completed
    ? 100
    : Math.Max(computedFromChunks, video.ProgressPercent);
```

Isso garante:
- Se `progressPercent=100` foi enviado e persistido → GET retorna 100 (mesmo antes de Completed)
- Se chunks são processados mas progresso enviado é maior → prevalece o maior
- Progresso nunca regride na exibição

---

## Escopo Técnico

- **Tecnologias:** .NET 10, C# 13, AWS DynamoDB, AWS Lambda, System.Text.Json
- **Arquivos afetados:**
  - `src/InterfacesExternas/VideoProcessing.VideoManagement.LambdaUpdateVideo/Models/UpdateVideoLambdaEvent.cs`
  - `src/InterfacesExternas/VideoProcessing.VideoManagement.LambdaUpdateVideo/Models/UpdateVideoFinalizeInfo.cs` _(novo record, se necessário)_
  - `src/Core/VideoProcessing.VideoManagement.Application/UseCases/UpdateVideo/UpdateVideoUseCase.cs`
  - `src/Core/VideoProcessing.VideoManagement.Application/UseCases/GetVideoById/GetVideoByIdUseCase.cs`
  - `tests/VideoProcessing.VideoManagement.UnitTests/Application/UseCases/UpdateVideo/UpdateVideoUseCaseTests.cs`
  - `tests/VideoProcessing.VideoManagement.UnitTests/Application/UseCases/GetVideoById/GetVideoByIdUseCaseTests.cs` _(novo ou existente)_
  - `tests/VideoProcessing.VideoManagement.UnitTests/LambdaUpdateVideo/UpdateVideoEventAdapterTests.cs`
- **Componentes/Recursos:**
  - `UpdateVideoLambdaEvent` — novo campo `FinalizeInfo`
  - `UpdateVideoUseCase` — fallback de chunk na finalização
  - `GetVideoByIdUseCase` — floor de `ProgressPercent` via `Math.Max`
- **Pacotes/Dependências:** nenhum pacote novo (usa apenas dependências já presentes)

---

## Dependências e Riscos

- **Dependências:**
  - Tabela DynamoDB de chunks deve existir (`DynamoDbOptions.ChunksTableName`) — já confirmado.
  - `IVideoChunkRepository.CountProcessedAsync` já implementado.
- **Riscos/Pré-condições:**
  - O campo `finalize` no evento é enviado apenas em eventos de finalização; o mapeamento deve ser opcional (nullable) para não quebrar outros eventos.
  - O chunk sintético de finalização usa `ChunkId = "finalize"` — garantir que esse valor não conflite com ChunkIds reais.
  - A lógica de `Math.Max` no GET não afeta a persistência; apenas a apresentação.
  - Nenhuma alteração no schema DynamoDB é necessária.

---

## Subtasks

- [Subtask 01: Mapear campo `finalize` no UpdateVideoLambdaEvent](./subtask/Subtask-01-Mapear_Finalize_LambdaEvent.md)
- [Subtask 02: Criar chunk de finalização no UpdateVideoUseCase](./subtask/Subtask-02-Chunk_Finalizacao_UpdateUseCase.md)
- [Subtask 03: Corrigir fallback de ProgressPercent no GetVideoByIdUseCase](./subtask/Subtask-03-Fallback_ProgressPercent_GET.md)
- [Subtask 04: Testes unitários e validação de build](./subtask/Subtask-04-Testes_Unitarios_Validacao.md)

---

## Critérios de Aceite da História

- [ ] Ao receber evento de finalização com `status=Completed` e sem `processingSummary.chunks`, o use case insere pelo menos um registro `"completed"` na tabela de chunks para o `videoId` correspondente.
- [ ] O campo `finalize` presente no evento Lambda é desserializado sem erro; campos `framesBasePrefix` e `framesBucket` estão disponíveis para uso no use case.
- [ ] O GET de um vídeo com `status=Completed` retorna `progressPercent=100`.
- [ ] O GET de um vídeo em estado intermediário, cujo evento enviou `progressPercent=50`, retorna `progressPercent` ≥ 50 (nunca regride abaixo do valor armazenado).
- [ ] O GET de um vídeo em estado intermediário com chunks processados retorna o maior entre o progresso calculado por chunks e o `ProgressPercent` armazenado na entidade.
- [ ] Testes unitários cobrindo os três fixes implementados; `dotnet test` passa sem falhas; cobertura dos casos novos ≥ 80%.
- [ ] Nenhuma regressão nos testes existentes de `UpdateVideoUseCase`, `GetVideoByIdUseCase` e `UpdateVideoEventAdapter`.

---

## Observações Técnicas

1. **ChunkId sintético para finalização:** usar `"finalize"` como `ChunkId` do chunk de finalização. Como a tabela usa SK = `CHUNK#{chunkId}`, não há colisão com ChunkIds numéricos gerados pelo processador.

2. **`ParallelChunks` nulo distorce o cálculo:** quando `ParallelChunks` é nulo, `ComputeProgressPercent` usa divisor=1. Isso já está contornado pelo `Math.Max` com `video.ProgressPercent`, mas convém registrar que um vídeo com `parallelChunks=null` e 1 chunk processado calcularia 100% pelo chunks — o `Math.Max` manteria o maior, o que é correto.

3. **Sem alteração no schema:** o chunk sintético de finalização usa os mesmos campos da tabela de chunks existente. Nenhuma migration ou alteração de IaC é necessária.

4. **Fire-and-warn mantido:** o bloco de upsert do chunk sintético deve seguir o mesmo padrão de `fire-and-warn` já presente no use case (falha no chunk não aborta o update principal).

5. **`finalize` vs `processingSummary`:** se o evento trouxer ambos (`processingSummary.chunks` preenchido E `finalize`), o fluxo normal de chunk upsert já acontece; o chunk sintético de finalização só deve ser criado como fallback quando `processingSummary.chunks` estiver ausente ou vazio.

---

## Rastreamento (dev tracking)
- **Início:** dia 13/03/2026, às 00:26 (Brasília)
- **Fim:** —
- **Tempo total de desenvolvimento:** —
