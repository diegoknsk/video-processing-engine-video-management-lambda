# Subtask-01: Adicionar campo `Chunk` ao UpdateVideoInputModel

## Descrição
Adicionar a propriedade `Chunk` (singular, top-level) ao `UpdateVideoInputModel`, anotada com `[JsonPropertyName("chunk")]`, do tipo `ChunkInfoInputModel?`, de modo que o `System.Text.Json` reconheça e desserialize o campo `chunk` presente no payload do Step Functions.

## Passos de Implementação
1. Em `UpdateVideoInputModel.cs`, adicionar a propriedade:
   ```csharp
   [JsonPropertyName("chunk")]
   [Description("Chunk singular processado (enviado pelo Step Functions por iteração do Map)")]
   public ChunkInfoInputModel? Chunk { get; init; }
   ```
2. Verificar que `ChunkInfoInputModel` já possui todos os campos necessários: `ChunkId`, `StartSec`, `EndSec`, `IntervalSec`, `FramesPrefix`, `ManifestPrefix` — nenhuma alteração nesse modelo deve ser necessária.
3. Confirmar que `UpdateVideoLambdaEvent` herda `UpdateVideoInputModel` e, portanto, já expõe `Chunk` sem alteração adicional.

## Formas de Teste
- Deserializar o JSON de exemplo do Step Functions (`{ "videoId": "...", "userId": "...", "status": 2, "chunk": { ... } }`) para `UpdateVideoLambdaEvent` e verificar que `evt.Chunk` não é `null` e contém os valores corretos.
- Confirmar que payloads sem o campo `chunk` continuam funcionando normalmente (`evt.Chunk == null`).
- Confirmar que payloads com `processingSummary` continuam sendo mapeados corretamente sem interferência do novo campo.

## Critérios de Aceite
- [ ] `UpdateVideoInputModel` possui propriedade `Chunk` do tipo `ChunkInfoInputModel?` com `[JsonPropertyName("chunk")]`
- [ ] Deserialização de payload com `chunk` resulta em `Chunk != null` com todos os campos preenchidos corretamente
- [ ] Deserialização de payload sem `chunk` resulta em `Chunk == null` (campo opcional, sem quebra de contrato)
