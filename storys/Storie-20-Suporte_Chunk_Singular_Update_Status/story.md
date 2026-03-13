# Storie-20: Suporte ao Campo `chunk` Singular no Update Status

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** [DD/MM/AAAA]

## Descrição
Como orquestrador (Step Functions), quero enviar um payload de atualização parcial com o campo `chunk` singular no topo do JSON, para que cada iteração do Map registre individualmente o chunk processado na tabela de chunks do DynamoDB.

## Objetivo
Adaptar a Lambda de Update Status para reconhecer, deserializar e persistir o campo `chunk` (singular, top-level) enviado pelo Step Functions a cada conclusão de chunk dentro do Map, gravando um item real na tabela `video-processing-engine-dev-video-chunks` com os dados corretos do chunk (chunkId, startSec, endSec, intervalSec, framesPrefix, manifestPrefix).

## Escopo Técnico
- Tecnologias: .NET 10, C# 13, AWS Lambda, Amazon DynamoDB, System.Text.Json, FluentValidation
- Arquivos afetados:
  - `src/Core/VideoProcessing.VideoManagement.Application/Models/InputModels/UpdateVideoInputModel.cs`
  - `src/Core/VideoProcessing.VideoManagement.Application/Validators/UpdateVideoInputModelValidator.cs`
  - `src/Core/VideoProcessing.VideoManagement.Application/UseCases/UpdateVideo/UpdateVideoUseCase.cs`
  - `tests/VideoProcessing.VideoManagement.UnitTests/Application/UseCases/UpdateVideo/UpdateVideoUseCaseTests.cs`
  - `tests/VideoProcessing.VideoManagement.UnitTests/LambdaUpdateVideo/UpdateVideoEventAdapterTests.cs`
- Componentes/Recursos: `UpdateVideoInputModel`, `UpdateVideoInputModelValidator`, `UpdateVideoUseCase`, `VideoChunkRepository`
- Pacotes/Dependências: nenhum pacote externo novo (tudo já presente)

## Dependências e Riscos (para estimativa)
- Dependências: Storie-17 (persistência de chunks via `IVideoChunkRepository`), Storie-16 (modelo de domínio com `VideoChunk`)
- Riscos:
  - O campo `chunk` singular **não pode conflitar** com o fluxo existente de `processingSummary.chunks` (dicionário); os dois caminhos devem coexistir
  - O fallback de chunk `"finalize"` não deve ser disparado quando `chunk` singular estiver presente no payload

## Subtasks
- [Subtask 01: Adicionar campo `Chunk` ao UpdateVideoInputModel](./subtask/Subtask-01-Campo_Chunk_Singular_InputModel.md)
- [Subtask 02: Atualizar validator para reconhecer `chunk` como campo válido](./subtask/Subtask-02-Validator_Chunk_Singular.md)
- [Subtask 03: Adaptar UseCase para persistir chunk singular via repositório](./subtask/Subtask-03-UseCase_Persistencia_Chunk_Singular.md)
- [Subtask 04: Testes unitários — adapter, handler e use case](./subtask/Subtask-04-Testes_Unitarios_Chunk_Singular.md)

## Critérios de Aceite da História
- [ ] O payload `{ "videoId": "...", "userId": "...", "status": 2, "chunk": { "chunkId": "...", "startSec": 23, "endSec": 45, "intervalSec": 5, "framesPrefix": "...", "manifestPrefix": "..." } }` é deserializado sem erros pela Lambda
- [ ] A propriedade `Chunk` (singular) existe em `UpdateVideoInputModel` com `[JsonPropertyName("chunk")]` e todos os campos de `ChunkInfoInputModel`
- [ ] O validator aceita um payload com apenas `userId` + `chunk` como campos de atualização (sem exigir `status` ou outros campos)
- [ ] O use case persiste o item correto na tabela de chunks via `IVideoChunkRepository.UpsertAsync` com os dados do `chunk` singular (chunkId real, startSec, endSec, intervalSec, framesPrefix, manifestPrefix)
- [ ] O fallback de chunk `"finalize"` **não** é inserido quando `chunk` singular estiver presente no payload
- [ ] Os dois caminhos coexistem: quando `processingSummary.chunks` estiver presente, o comportamento existente é mantido sem regressão
- [ ] Testes unitários cobrindo os cenários: chunk singular persistido, fallback "finalize" não disparado com chunk, coexistência com processingSummary; `dotnet test` passando com cobertura ≥ 80% nos métodos afetados

## Rastreamento (dev tracking)
- **Início:** 13/03/2026, às 04:19 (Brasília)
- **Fim:** —
- **Tempo total de desenvolvimento:** —
