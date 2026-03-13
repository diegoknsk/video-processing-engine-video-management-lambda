# Subtask-04: Testes Unitários — Adapter, Handler e UseCase para Chunk Singular

## Descrição
Criar e/ou atualizar os testes unitários existentes para cobrir os novos cenários introduzidos pelo suporte ao campo `chunk` singular: deserialização pelo adapter, validação pelo validator/handler e persistência pelo use case.

## Passos de Implementação
1. **`UpdateVideoEventAdapterTests`** — adicionar cenários:
   - Payload direto com campo `chunk` preenchido → `evt.Chunk != null` com dados corretos
   - Payload SQS com `body` contendo campo `chunk` → mesmo comportamento

2. **`UpdateVideoUseCaseTests`** — adicionar cenários:
   - `input.Chunk` preenchido, `status = ProcessingImages` → `chunkRepository.UpsertAsync` chamado com status `"processing"` e chunkId correto; chunk `"finalize"` **não** inserido
   - `input.Chunk` preenchido, `status = Completed` → `chunkRepository.UpsertAsync` chamado com status `"completed"` e `ProcessedAt` preenchido
   - `input.Chunk` preenchido, falha no `UpsertAsync` → log de warning emitido; atualização principal (vídeo) mantida sem exceção propagada
   - `input.Chunk == null`, `processingSummary == null`, `status = GeneratingZip` → fallback `"finalize"` inserido (regressão)
   - `input.Chunk == null`, `processingSummary.chunks` preenchido → comportamento existente preservado (regressão)

3. **`UpdateVideoInputModelValidatorTests`** (criar ou atualizar) — adicionar cenários:
   - Payload com `userId` + `chunk` válido → validação passa
   - Payload com `chunk.chunkId` vazio → erro de validação
   - Payload com `chunk.endSec <= chunk.startSec` → erro de validação
   - Payload sem nenhum campo de atualização e `chunk == null` → erro "Pelo menos um campo de atualização"

4. Executar `dotnet test` e garantir que todos os testes passam sem regressão.

## Formas de Teste
- `dotnet test` na solução completa passando sem falhas
- Cobertura dos novos métodos/branches verificada via `dotnet test --collect:"XPlat Code Coverage"`
- Revisar cenários de regressão manualmente nos testes existentes de `UpdateVideoUseCaseTests`

## Critérios de Aceite
- [ ] `dotnet test` passa sem falhas em todos os projetos de teste
- [ ] Cenários de chunk singular cobertos por testes no adapter, validator e use case
- [ ] Cenários de regressão (`processingSummary`, fallback `"finalize"`) mantidos e passando
- [ ] Cobertura dos métodos afetados ≥ 80%
