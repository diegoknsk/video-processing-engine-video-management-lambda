# Subtask-02: Atualizar Validator para Reconhecer `Chunk` Singular como Campo Válido

## Descrição
Atualizar `UpdateVideoInputModelValidator` para que a regra "pelo menos um campo de atualização deve ser informado" passe a reconhecer `Chunk != null` como condição suficiente. Além disso, adicionar as regras de validação dos campos internos do chunk singular (`ChunkId`, `StartSec`, `EndSec`, `IntervalSec`).

## Passos de Implementação
1. Na regra `Must(x => ...)` que verifica se pelo menos um campo está preenchido, adicionar `|| x.Chunk != null` à expressão.
2. Adicionar bloco `When(x => x.Chunk != null)` com regras para os campos do chunk singular:
   - `Chunk.ChunkId` não pode ser vazio
   - `Chunk.StartSec >= 0`
   - `Chunk.EndSec > Chunk.StartSec`
   - `Chunk.IntervalSec > 0`
3. Garantir que as regras de `ProcessingSummary.Chunks` (dicionário) continuem inalteradas e independentes.

## Formas de Teste
- Payload com apenas `userId` + `chunk` válido: validação deve passar (sem erros).
- Payload com `chunk` e `chunkId` vazio: deve retornar erro "ChunkId não pode ser vazio".
- Payload com `chunk.endSec <= chunk.startSec`: deve retornar erro sobre EndSec.
- Payload sem nenhum campo de atualização (nem `chunk`, nem `status`, nem outros): deve retornar erro "Pelo menos um campo de atualização deve ser informado".
- Payload com `processingSummary.chunks` válido, sem `chunk` singular: validação deve passar normalmente (regressão).

## Critérios de Aceite
- [ ] `x.Chunk != null` adicionado à expressão da regra de campo mínimo obrigatório
- [ ] Regras de validação dos campos internos do `Chunk` singular aplicadas quando `Chunk != null`
- [ ] Testes de validação cobrindo cenários de chunk válido, inválido e ausente passando com `dotnet test`
