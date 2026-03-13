# Subtask-06: Atualizar e ampliar testes unitários

## Descrição
Atualizar os testes existentes de `GetVideoByIdUseCase` e `ListVideosUseCase` para refletir as novas dependências e comportamentos, criar os testes do `ChunkProgressCalculator` com tabela de decisão completa e ampliar os testes do `VideoChunkRepository` para o novo método `GetStatusSummaryAsync`.

---

## Contexto
Arquivos de teste existentes a serem ajustados ou ampliados:
- `tests/.../UseCases/GetVideoById/GetVideoByIdUseCaseTests.cs` — 9 casos existentes; precisa mockar `IChunkProgressCalculator` e `GetStatusSummaryAsync`
- `tests/.../UseCases/ListVideos/ListVideosUseCaseTests.cs` — 5 casos existentes; precisa mockar novas dependências
- `tests/.../Repositories/VideoChunkRepositoryTests.cs` — precisa de novos testes para `GetStatusSummaryAsync`

Novos arquivos de teste:
- `tests/.../Services/ChunkProgressCalculatorTests.cs` — tabela de decisão completa

---

## Passos de Implementação

### 1. Atualizar `GetVideoByIdUseCaseTests.cs`
- Substituir mock de `CountProcessedAsync` por mock de `GetStatusSummaryAsync`
- Adicionar mock de `IChunkProgressCalculator` (injetado via construtor)
- Casos novos a adicionar:
  - Vídeo FanOut com chunks: `chunksSummary` preenchido corretamente
  - `FinalizeStatus = "completed"` → `progressPercent = 100`
  - `FinalizeStatus = null` (finalize não completou) + base = 100% → retorna 97
  - Vídeo sem chunks → `chunksSummary = null`, `progressPercent = video.ProgressPercent`
  - `chunks` (lista) preenchida corretamente nos casos com chunks

### 2. Atualizar `ListVideosUseCaseTests.cs`
- Adicionar mocks de `IVideoChunkRepository` e `IChunkProgressCalculator`
- Casos novos a adicionar:
  - Apenas vídeos elegíveis (FanOut + não Completed/Cancelled) têm chunks consultados
  - `Task.WhenAll` chamado com todos os vídeos elegíveis simultaneamente
  - Vídeos não elegíveis retornam sem `chunksSummary`
  - `Chunks` nunca preenchido na listagem (sempre `null`)
  - Paginação (`NextToken`) não é afetada pelo enriquecimento

### 3. Criar `ChunkProgressCalculatorTests.cs`
Cobrir todos os cenários da tabela de decisão:

| Cenário | VideoStatus | Summary | ProgressPercent esperado | CurrentStage esperado |
|---|---|---|---|---|
| Completed always 100 | Completed | qualquer | 100 | "Concluído" |
| Sem chunks (null summary) | ProcessingImages | null | -1 (usar valor salvo) | "Processando" |
| Sem chunks (total = 0) | ProcessingImages | Total=0 | -1 | "Processando" |
| 1 de 3 completed | ProcessingImages | T=3,C=1,P=1,F=0 | 33 | "Processando chunks" |
| 2 de 3 completed | ProcessingImages | T=3,C=2,P=1,F=0 | 66 | "Processando chunks" |
| 3 de 3 completed, finalize null | GeneratingZip | T=3,C=3, fin=null | 97 | "Gerando ZIP" |
| 3 de 3 + finalize completed | Completed | T=3,C=3, fin="completed" | 100 | "Concluído" |
| Failed | Failed | T=3,C=1 | 33 | "Falhou" |
| Upload pendente | UploadPending | null | -1 | "Upload pendente" |
| Cancelled | Cancelled | qualquer | -1 | "Cancelado" |

### 4. Ampliar `VideoChunkRepositoryTests.cs`
- Teste de `GetStatusSummaryAsync` com itens misturados:
  - 2 completed, 1 processing, 1 failed, 1 pending + 1 finalize completed
  - Esperado: `Total=5, Completed=2, Processing=1, Failed=1, Pending=1, FinalizeStatus="completed"`
- Teste sem nenhum chunk: `Total=0, FinalizeStatus=null`
- Teste com apenas o item finalize: `Total=0, FinalizeStatus="processing"`
- Teste com paginação (múltiplas páginas de `LastEvaluatedKey`)

---

## Formas de Teste

1. **Executar `dotnet test`** — todos os testes devem passar sem falhas

2. **Verificar cobertura** — com `dotnet test --collect:"XPlat Code Coverage"` e relatório coverlet:
   - `ChunkProgressCalculator`: ≥ 90% de cobertura de linhas
   - `GetVideoByIdUseCase` e `ListVideosUseCase`: ≥ 80% de cobertura de linhas

3. **Revisão manual dos cenários de borda** — rodar os testes em modo verbose e confirmar que cada caso de borda está coberto (sem chunks, com finalize, status completed, status failed)

---

## Critérios de Aceite

- [ ] `dotnet test` executa sem falhas em todos os projetos de teste
- [ ] `ChunkProgressCalculatorTests.cs` criado com no mínimo 10 casos de teste cobrindo a tabela de decisão completa
- [ ] `GetVideoByIdUseCaseTests.cs` atualizado: mocks de `GetStatusSummaryAsync` e `IChunkProgressCalculator`; casos novos para `chunksSummary`, `chunks`, e compatibilidade sem chunks
- [ ] `ListVideosUseCaseTests.cs` atualizado: casos para elegibilidade de enriquecimento, paralelismo e `Chunks = null`
- [ ] `VideoChunkRepositoryTests.cs` ampliado: 4+ casos para `GetStatusSummaryAsync` incluindo paginação
- [ ] Cobertura de linhas ≥ 80% nos use cases e ≥ 90% no `ChunkProgressCalculator`
