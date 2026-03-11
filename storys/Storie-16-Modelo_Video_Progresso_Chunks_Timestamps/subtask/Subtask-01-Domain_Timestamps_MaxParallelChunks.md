# Subtask 01: Domain — novos campos de timestamps e maxParallelChunks na entidade Video

## Descrição
Adicionar à entidade `Video` os campos de timestamps do pipeline (`processingStartedAt`, `imagesProcessingCompletedAt`, `processingCompletedAt`, `lastFailedAt`, `lastCancelledAt`) e o campo de controle de paralelismo (`maxParallelChunks`). Atualizar `VideoRehydrationData` e `VideoUpdateValues` para suportar os novos campos, e adaptar `FromMerge` para aplicá-los no patch.

## Passos de Implementação
1. Adicionar na entidade `Video` as propriedades com `private set`:
   - `DateTime? ProcessingStartedAt`
   - `DateTime? ImagesProcessingCompletedAt`
   - `DateTime? ProcessingCompletedAt`
   - `DateTime? LastFailedAt`
   - `DateTime? LastCancelledAt`
   - `int? MaxParallelChunks`
2. Atualizar o record `VideoRehydrationData` incluindo os 6 novos parâmetros, mantendo a ordem lógica (timestamps agrupados, maxParallelChunks junto com campos fan-out).
3. Atualizar o construtor `Video(VideoRehydrationData d)` para atribuir os novos campos a partir de `d`.
4. Atualizar `VideoUpdateValues` incluindo `MaxParallelChunks` e `ProcessingStartedAt` como campos opcionais no record (os demais timestamps serão preenchidos automaticamente na Subtask 03).
5. Atualizar `Video.FromMerge(Video existing, VideoUpdateValues patch)` para aplicar os novos campos do patch quando não nulos (padrão `patch.Campo ?? existing.Campo`).

## Formas de Teste
1. Teste unitário: criar Video via `FromPersistence` com `VideoRehydrationData` contendo os novos campos preenchidos; verificar que as propriedades são acessíveis e possuem os valores corretos.
2. Teste unitário: chamar `FromMerge` com patch contendo `MaxParallelChunks = 10` e `ProcessingStartedAt` preenchido; verificar que o Video resultante possui os valores do patch.
3. Teste unitário: chamar `FromMerge` com patch onde `MaxParallelChunks` é `null`; verificar que o valor existente do Video original é preservado.
4. Verificar que `dotnet build` compila sem erros na solução completa.

## Critérios de Aceite da Subtask
- [ ] Entidade `Video` possui os 6 novos campos como `DateTime?` / `int?` com `private set`.
- [ ] `VideoRehydrationData` inclui os 6 novos parâmetros; construtor de `Video(VideoRehydrationData)` os atribui corretamente.
- [ ] `VideoUpdateValues` inclui `MaxParallelChunks` e `ProcessingStartedAt` como opcionais.
- [ ] `FromMerge` aplica os novos campos do patch quando não nulos e preserva os valores existentes quando nulos.
- [ ] `dotnet build` compila sem erros; nenhuma regressão nos testes existentes.
