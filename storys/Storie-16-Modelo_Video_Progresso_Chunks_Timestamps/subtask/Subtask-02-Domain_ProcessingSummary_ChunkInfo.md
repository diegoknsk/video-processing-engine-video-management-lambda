# Subtask 02: Domain — ProcessingSummary e ChunkInfo com merge incremental idempotente

## Descrição
Criar os value objects `ChunkInfo` e `ProcessingSummary` no Domain para representar o progresso incremental de chunks. Implementar a lógica de merge que garante: novos chunks são adicionados, chunks existentes são preservados (idempotência), e `processingSummary` null na entrada não modifica o objeto existente. Integrar `ProcessingSummary` na entidade `Video`.

## Passos de Implementação
1. Criar o record `ChunkInfo` em `Domain/Entities/` (ou `Domain/ValueObjects/`) com as propriedades:
   - `string ChunkId`
   - `double StartSec`
   - `double EndSec`
   - `double IntervalSec`
   - `string ManifestPrefix`
   - `string FramesPrefix`
2. Criar a classe `ProcessingSummary` em `Domain/Entities/` (ou `Domain/ValueObjects/`) com:
   - Propriedade `IReadOnlyDictionary<string, ChunkInfo> Chunks` (dicionário indexado por `ChunkId`)
   - Método estático `Merge(ProcessingSummary? existing, ProcessingSummary? incoming)` que:
     - Se `incoming` for `null`, retorna `existing` (sem modificação)
     - Se `existing` for `null`, retorna `incoming`
     - Se ambos existirem, faz merge dos chunks: para cada chunk no `incoming`, só adiciona se o `ChunkId` não existir no `existing` (idempotência)
   - Construtor que aceita dicionário de chunks
3. Adicionar a propriedade `ProcessingSummary? ProcessingSummary { get; private set; }` na entidade `Video`.
4. Atualizar `VideoRehydrationData` para incluir `ProcessingSummary?`.
5. Atualizar construtor `Video(VideoRehydrationData)` para atribuir `ProcessingSummary`.
6. Atualizar `VideoUpdateValues` para incluir `ProcessingSummary?` como campo opcional do patch.
7. Atualizar `Video.FromMerge` para chamar `ProcessingSummary.Merge(existing.ProcessingSummary, patch.ProcessingSummary)` em vez de simples null-coalesce.

## Formas de Teste
1. Teste unitário: chamar `ProcessingSummary.Merge(null, incoming)` com 2 chunks; verificar que o resultado possui os 2 chunks.
2. Teste unitário: chamar `ProcessingSummary.Merge(existing, null)` com existing contendo 1 chunk; verificar que o resultado mantém o chunk existente inalterado.
3. Teste unitário: chamar `ProcessingSummary.Merge(existing, incoming)` onde `incoming` contém um chunk com `ChunkId` já presente em `existing`; verificar que o chunk existente é preservado (não sobrescrito).
4. Teste unitário: chamar `ProcessingSummary.Merge(existing, incoming)` onde `incoming` contém 1 chunk novo e 1 duplicado; verificar que o resultado tem o chunk existente + o novo, sem o duplicado sobrescrever.
5. Teste unitário: `Video.FromMerge` com patch contendo `ProcessingSummary` com novo chunk; verificar que o Video resultante possui o chunk adicionado sem perder chunks existentes.

## Critérios de Aceite da Subtask
- [ ] `ChunkInfo` é um record imutável com os campos: `ChunkId`, `StartSec`, `EndSec`, `IntervalSec`, `ManifestPrefix`, `FramesPrefix`.
- [ ] `ProcessingSummary` possui `Chunks` como `IReadOnlyDictionary<string, ChunkInfo>` e método `Merge` com lógica incremental idempotente.
- [ ] `Merge(existing, null)` retorna `existing` sem modificação; `Merge(null, incoming)` retorna `incoming`.
- [ ] Chunk com `ChunkId` já existente no `existing` nunca é sobrescrito pelo `incoming`.
- [ ] Entidade `Video` possui `ProcessingSummary?`; `FromMerge` usa `ProcessingSummary.Merge` para combinar.
- [ ] `dotnet build` compila sem erros; testes unitários do merge passando.
