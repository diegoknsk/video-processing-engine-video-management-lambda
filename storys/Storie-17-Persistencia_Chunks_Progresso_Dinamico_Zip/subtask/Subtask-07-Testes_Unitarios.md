# Subtask-07: Testes Unitários

## Descrição
Garantir cobertura de testes unitários ≥ 80% nos novos artefatos criados nesta story: `VideoChunkRepository`, `UpdateVideoUseCase` (fluxos novos), `GetVideoByIdUseCase` (cálculo de progresso e URL pré-assinada), `VideoMapper` (campos ZIP e retrocompatibilidade de `parallelChunks`). Consolidar e atualizar testes existentes afetados pelo rename e novos campos.

## Passos de Implementação
1. **VideoMapper:**
   - Teste: leitura de registro com atributo `maxParallelChunks` (legado) → `ParallelChunks` preenchido.
   - Teste: leitura de registro com atributo `parallelChunks` (novo) → `ParallelChunks` preenchido.
   - Teste: leitura/escrita de `ZipBucket`, `ZipKey`, `ZipFileName`.
2. **VideoChunkRepository:**
   - Teste `UpsertAsync`: mock `IAmazonDynamoDB`; verificar `PutItemAsync` chamado com pk/sk corretos.
   - Teste `CountProcessedAsync`: mock retorna `Count = 3`; verificar retorno `3`.
3. **UpdateVideoUseCase:**
   - Teste: payload com dados de chunk → `IVideoChunkRepository.UpsertAsync` chamado.
   - Teste: `IVideoChunkRepository.UpsertAsync` lança `Exception` → use case não propaga, resultado principal é sucesso.
   - Teste: payload com `ZipBucket`/`ZipKey`/`ZipFileName` → `IVideoRepository.UpdateAsync` chamado com campos ZIP.
4. **GetVideoByIdUseCase:**
   - Teste: `parallelChunks = 4`, chunks processados `= 2` → `progressPercent = 50`.
   - Teste: `parallelChunks = null`, chunks processados `= 1` → `progressPercent = 100`.
   - Teste: status `Completed` → `progressPercent = 100` independente da contagem.
   - Teste: `ZipKey` presente → `ZipUrl` não nulo no response.
   - Teste: `ZipKey` ausente → `ZipUrl = null`.
   - Teste: geração de URL pré-assinada falha → `ZipUrl = null`, sem exceção propagada.
5. Executar `dotnet test` e verificar cobertura ≥ 80% nos arquivos novos/modificados.

## Formas de Teste
1. `dotnet test` no projeto `UnitTests` — todos os testes passando (green).
2. Verificar cobertura de linha/branch nos novos arquivos via relatório de cobertura (`--collect:"XPlat Code Coverage"`).
3. Revisar testes existentes que referenciam `MaxParallelChunks` — atualizar para `ParallelChunks`.

## Critérios de Aceite
- [ ] Todos os testes unitários passam após `dotnet test`.
- [ ] Cobertura ≥ 80% nos novos artefatos desta story.
- [ ] Testes existentes que referenciavam `MaxParallelChunks` atualizados para `ParallelChunks` e passando.
- [ ] Cenário de falha tolerada no `UpdateVideoUseCase` (chunk repo) coberto por teste.
- [ ] Cenário de URL pré-assinada ausente (sem ZipKey) coberto por teste.
