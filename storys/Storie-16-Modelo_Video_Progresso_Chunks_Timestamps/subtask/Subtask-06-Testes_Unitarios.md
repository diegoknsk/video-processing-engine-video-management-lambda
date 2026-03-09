# Subtask 06: Testes unitários — merge de chunks, idempotência, transições e timestamps

## Descrição
Implementar testes unitários abrangentes para todas as novas funcionalidades introduzidas nesta story: merge incremental de `ProcessingSummary`, idempotência de chunks, transições de status com timestamps automáticos, regra de não regressão, e logs estruturados. Os testes devem cobrir cenários de sucesso, falha e edge cases conforme especificado nos requisitos.

## Passos de Implementação
1. Criar classe de teste `ProcessingSummaryTests` no projeto de testes com os cenários:
   - **Adição de novo chunk:** `Merge(existing, incoming)` com chunk novo → resultado contém chunk adicionado.
   - **Chunk duplicado:** `Merge(existing, incoming)` com chunk de mesmo `ChunkId` → chunk existente preservado, incoming ignorado.
   - **Tentativa de sobrescrever:** incoming com mesmo `ChunkId` mas valores diferentes → valor original mantido.
   - **Incoming null:** `Merge(existing, null)` → retorna existing inalterado.
   - **Existing null:** `Merge(null, incoming)` → retorna incoming.
   - **Ambos null:** `Merge(null, null)` → retorna null.
   - **Múltiplos chunks mistos:** merge com 3 chunks existing + 2 novos + 1 duplicado → resultado com 5 chunks (3 originais + 2 novos).
2. Criar classe de teste `VideoStatusTransitionTests` no projeto de testes com os cenários:
   - **ProcessingImages → GeneratingZip:** `ImagesProcessingCompletedAt` preenchido.
   - **GeneratingZip → Completed:** `ProcessingCompletedAt` preenchido.
   - **Qualquer → Failed:** `LastFailedAt` preenchido.
   - **Qualquer → Cancelled:** `LastCancelledAt` preenchido.
   - **UploadPending → ProcessingImages:** nenhum timestamp de transição preenchido.
   - **Tentativa de regressão (Completed → ProcessingImages):** exceção lançada.
   - **MarkAsFailed:** `LastFailedAt` preenchido e status é `Failed`.
   - **MarkAsCompleted:** `ProcessingCompletedAt` preenchido e status é `Completed`.
3. Criar testes para `UpdateVideoUseCase`:
   - **Com ProcessingSummary:** input com 1 chunk → Video persistido contém o chunk no ProcessingSummary.
   - **Com MaxParallelChunks:** input com `MaxParallelChunks = 10` → Video persistido com valor correto.
   - **Log de transição:** mock de ILogger; verificar que log é emitido quando status muda e que não é emitido quando status permanece igual.
4. Criar testes para `UpdateVideoInputModelValidator`:
   - **MaxParallelChunks inválido (0 ou negativo):** validação falha.
   - **MaxParallelChunks válido:** validação passa.
   - **ChunkInfo sem ChunkId:** validação falha.
   - **ChunkInfo com EndSec <= StartSec:** validação falha.
   - **ChunkInfo válido:** validação passa.
5. Garantir que todos os testes existentes continuam passando (`dotnet test` na solução completa).

## Formas de Teste
1. Executar `dotnet test` no projeto de testes unitários; todos os cenários acima devem passar.
2. Executar `dotnet test --collect:"XPlat Code Coverage"` e verificar cobertura dos novos arquivos (ProcessingSummary, ChunkInfo, transições de status, UseCase) ≥ 80%.
3. Executar `dotnet build` na solução completa sem erros.

## Critérios de Aceite da Subtask
- [ ] Teste de adição de novo chunk ao ProcessingSummary passando.
- [ ] Teste de recebimento duplicado do mesmo chunk (idempotência) passando — chunk existente preservado.
- [ ] Teste de tentativa de sobrescrever chunk existente passando — valor original mantido.
- [ ] Teste de regressão de status (Completed → ProcessingImages) passando — exceção lançada.
- [ ] Testes de timestamps automáticos para cada transição (ProcessingImages→GeneratingZip, GeneratingZip→Completed, →Failed, →Cancelled) passando.
- [ ] Teste de atualização parcial sem sobrescrever processingSummary (incoming null) passando.
- [ ] Teste de log estruturado no UseCase passando (log emitido na mudança, não emitido sem mudança).
- [ ] Testes de validação (MaxParallelChunks, ChunkInfo) passando.
- [ ] `dotnet build` e `dotnet test` passando na solução completa; nenhuma regressão.
- [ ] Cobertura dos novos componentes ≥ 80%.
