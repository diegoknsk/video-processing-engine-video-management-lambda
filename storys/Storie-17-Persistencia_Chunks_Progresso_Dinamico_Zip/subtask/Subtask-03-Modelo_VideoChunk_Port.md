# Subtask-03: Modelo VideoChunk (Domain) e Port IVideoChunkRepository (Application)

## Descrição
Criar a entidade de domínio `VideoChunk` e a port `IVideoChunkRepository` que define o contrato de acesso à tabela de chunks. Esses artefatos são a base para as subtasks de implementação e persistência.

## Passos de Implementação
1. Criar `VideoChunk` em `Domain/Entities/VideoChunk.cs` como record imutável com propriedades:
   - `ChunkId` (string) — identificador único do chunk (ex.: `"chunk-01"`)
   - `VideoId` (string) — FK para o vídeo pai
   - `Status` (string) — estado reportado pelo pipeline (ex.: `"completed"`, `"failed"`)
   - `StartSec` (double) — segundo de início no vídeo original
   - `EndSec` (double) — segundo de término
   - `IntervalSec` (double) — intervalo entre frames capturados
   - `ManifestPrefix` (string?) — prefixo S3 do manifesto do chunk
   - `FramesPrefix` (string?) — prefixo S3 dos frames extraídos
   - `ProcessedAt` (DateTime?) — quando o chunk foi concluído pelo pipeline
   - `CreatedAt` (DateTime) — quando o registro foi criado na tabela
2. Criar `IVideoChunkRepository` em `Application/Ports/IVideoChunkRepository.cs` com os métodos:
   - `Task UpsertAsync(VideoChunk chunk, CancellationToken ct = default)`
   - `Task<int> CountProcessedAsync(string videoId, CancellationToken ct = default)` — conta chunks com `Status = "completed"` (ou equivalente concluído)
3. Garantir que `VideoChunk` não tem dependências externas (apenas tipos primitivos e `DateTime`).

## Formas de Teste
1. Compilar `Domain` e `Application` de forma isolada (`dotnet build`) — devem compilar sem dependências de Infra.
2. Teste unitário: instanciar `VideoChunk` com todos os campos e verificar imutabilidade (record).
3. Verificar que `IVideoChunkRepository` não referencia tipos de Infra (DynamoDB, EF Core, etc.).

## Critérios de Aceite
- [ ] `VideoChunk` existe em `Domain/Entities/` sem dependências externas de Infra.
- [ ] `IVideoChunkRepository` existe em `Application/Ports/` com métodos `UpsertAsync` e `CountProcessedAsync`.
- [ ] `dotnet build` passa em `Domain` e `Application` após os novos arquivos.
- [ ] Record `VideoChunk` tem ao menos as propriedades: `ChunkId`, `VideoId`, `Status`, `StartSec`, `EndSec`, `CreatedAt`.
