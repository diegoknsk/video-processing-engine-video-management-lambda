# Subtask 05: Application e Lambda — InputModel, Validator, UseCase e evento Lambda

## Descrição
Adaptar a camada Application e a Lambda Update Video para suportar os novos campos. Atualizar `UpdateVideoInputModel` e `UpdateVideoLambdaEvent` com os novos campos (`maxParallelChunks`, `processingSummary`, `processingStartedAt`), ajustar o validator, adaptar o `UpdateVideoUseCase` para propagar os novos campos no patch e utilizar o merge de `ProcessingSummary`, e atualizar `VideoResponseModel` e mapper para expor os novos campos nas respostas.

## Passos de Implementação
1. Adicionar ao `UpdateVideoInputModel` as novas propriedades opcionais:
   - `int? MaxParallelChunks`
   - `DateTime? ProcessingStartedAt`
   - `ProcessingSummaryInputModel? ProcessingSummary` (novo DTO de entrada com `Dictionary<string, ChunkInfoInputModel>? Chunks`)
   - Criar `ProcessingSummaryInputModel` e `ChunkInfoInputModel` como records em `Application/Models/InputModels/`
2. Atualizar `UpdateVideoLambdaEvent` (herda de `UpdateVideoInputModel`) — como herda, os novos campos são herdados automaticamente; validar se a desserialização JSON do SQS body funciona com os novos campos.
3. Atualizar `UpdateVideoInputModelValidator`:
   - `MaxParallelChunks`: quando informado, deve ser > 0 e ≤ 100 (ou limite razoável)
   - `ProcessingSummary.Chunks`: quando informado, cada chunk deve ter `ChunkId` não vazio, `StartSec >= 0`, `EndSec > StartSec`, `IntervalSec > 0`
4. Adaptar `UpdateVideoUseCase.ExecuteAsync`:
   - Incluir `MaxParallelChunks` e `ProcessingStartedAt` no `VideoUpdateValues`
   - Converter `ProcessingSummaryInputModel` → `ProcessingSummary` do domínio (mapear `ChunkInfoInputModel` → `ChunkInfo`)
   - Incluir o `ProcessingSummary` convertido no `VideoUpdateValues`
   - O merge será feito no `Video.FromMerge` (já implementado na Subtask 02)
5. Atualizar `VideoResponseModel` adicionando:
   - `DateTime? ProcessingStartedAt`
   - `DateTime? ImagesProcessingCompletedAt`
   - `DateTime? ProcessingCompletedAt`
   - `DateTime? LastFailedAt`
   - `DateTime? LastCancelledAt`
   - `int? MaxParallelChunks`
   - `ProcessingSummaryResponseModel? ProcessingSummary` (novo DTO de saída com chunks)
6. Atualizar `VideoResponseModelMapper.ToResponseModel` para mapear os novos campos do domínio para o response model.
7. Atualizar `docs/lambda-update-video-contract.md` com os novos campos no contrato de entrada e exemplos de payload com `processingSummary`.

## Formas de Teste
1. Teste unitário: validar `UpdateVideoInputModel` com `MaxParallelChunks = 0` → erro; com `MaxParallelChunks = 10` → sucesso.
2. Teste unitário: validar `ProcessingSummary` com chunk sem `ChunkId` → erro; com chunk válido → sucesso.
3. Teste unitário: `UpdateVideoUseCase` com input contendo `ProcessingSummary` com 1 chunk; verificar que o Video resultante possui o chunk no `ProcessingSummary`.
4. Teste unitário: `VideoResponseModelMapper` com Video contendo novos campos; verificar que o response model expõe os valores corretos.
5. Verificar que `dotnet build` compila sem erros e `dotnet test` passa.

## Critérios de Aceite da Subtask
- [ ] `UpdateVideoInputModel` possui `MaxParallelChunks`, `ProcessingStartedAt` e `ProcessingSummary` como opcionais.
- [ ] DTOs `ProcessingSummaryInputModel` e `ChunkInfoInputModel` criados em `Application/Models/InputModels/`.
- [ ] Validator valida `MaxParallelChunks > 0` e regras de `ChunkInfo` (ChunkId não vazio, StartSec >= 0, EndSec > StartSec, IntervalSec > 0).
- [ ] `UpdateVideoUseCase` propaga os novos campos no `VideoUpdateValues` e converte `ProcessingSummaryInputModel` → `ProcessingSummary` do domínio.
- [ ] `VideoResponseModel` e `VideoResponseModelMapper` expõem todos os novos campos.
- [ ] Documentação `docs/lambda-update-video-contract.md` atualizada com novos campos e exemplos.
- [ ] `dotnet build` e `dotnet test` passam sem regressão.
