# Subtask-01: Mapear campo `finalize` no UpdateVideoLambdaEvent

## Descrição
O evento recebido pelo Lambda Update Status contém um campo `finalize` com metadados de finalização (`framesBasePrefix`, `framesBucket`, `outputBucket`, `outputBasePrefix`, `videoId`, `ordenaAutomaticamente`). Atualmente esse campo não está mapeado em `UpdateVideoLambdaEvent` e é silenciosamente descartado na desserialização. Esta subtask cria o record `UpdateVideoFinalizeInfo` e adiciona a propriedade `FinalizeInfo` ao `UpdateVideoLambdaEvent`.

---

## Passos de implementação

1. **Criar record `UpdateVideoFinalizeInfo`** em `src/InterfacesExternas/.../LambdaUpdateVideo/Models/`:
   ```csharp
   public record UpdateVideoFinalizeInfo
   {
       [JsonPropertyName("videoId")]
       public Guid? VideoId { get; init; }

       [JsonPropertyName("framesBucket")]
       public string? FramesBucket { get; init; }

       [JsonPropertyName("framesBasePrefix")]
       public string? FramesBasePrefix { get; init; }

       [JsonPropertyName("outputBucket")]
       public string? OutputBucket { get; init; }

       [JsonPropertyName("outputBasePrefix")]
       public string? OutputBasePrefix { get; init; }

       [JsonPropertyName("ordenaAutomaticamente")]
       public bool OrdenaAutomaticamente { get; init; }
   }
   ```

2. **Adicionar propriedade `FinalizeInfo` ao `UpdateVideoLambdaEvent`:**
   ```csharp
   [JsonPropertyName("finalize")]
   public UpdateVideoFinalizeInfo? FinalizeInfo { get; init; }
   ```
   A propriedade deve ser nullable para não quebrar eventos sem o campo `finalize`.

3. **Verificar que o `UpdateVideoEventAdapter`** desserializa corretamente com `PropertyNameCaseInsensitive = true` e `CamelCase` — o `[JsonPropertyName]` explícito garante o mapeamento independente da política.

---

## Formas de teste

1. **Teste unitário no `UpdateVideoEventAdapterTests`:** serializar um JSON com `finalize` preenchido e verificar que `FinalizeInfo` é populado corretamente (campos não nulos).
2. **Teste unitário com evento sem `finalize`:** verificar que `FinalizeInfo` é `null` e não lança exceção.
3. **Teste manual (Lambda invoke):** invocar a Lambda diretamente com o payload de exemplo da story e verificar nos logs que o evento foi desserializado sem `null`/`empty` warnings.

---

## Critérios de aceite da subtask

- [ ] `UpdateVideoFinalizeInfo` existe como record imutável; todos os campos são nullable (exceto `OrdenaAutomaticamente` que tem default `false`).
- [ ] `UpdateVideoLambdaEvent.FinalizeInfo` é preenchido ao desserializar o payload de exemplo da story.
- [ ] Evento sem campo `finalize` desserializa sem erro; `FinalizeInfo` é `null`.
- [ ] `dotnet build` e `dotnet test` passam sem falhas ou warnings introduzidos por esta subtask.
