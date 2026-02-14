# Subtask 02: Implementar UpdateVideoUseCase (idempotente e condicional)

## Descrição
Criar IUpdateVideoUseCase e UpdateVideoUseCase que obtém video existente via GetByIdAsync (precisa userId do video para ownership no UpdateAsync), aplica atualizações aos campos presentes no UpdateVideoInputModel, chama repository.UpdateAsync (condicional), trata VideoUpdateConflictException (409), retorna VideoResponseModel atualizado.

## Passos de Implementação
1. Interface: `Task<VideoResponseModel> ExecuteAsync(string videoId, UpdateVideoInputModel input, CancellationToken ct);` (NÃO passa userId; usa videoId para buscar)
2. Buscar video: `var video = await repository.GetByIdAsync(videoId, ct);` (busca sem userId; precisa implementar GetByIdAsync sobrecarregado ou usar scan; decisão: adicionar método GetByVideoIdAsync no repository que faz scan ou query com GSI)
3. **Alternativa mais eficiente**: exigir userId no input (ou extrair de outro meio); decisão recomendada: adicionar campo `userId` no UpdateVideoInputModel como obrigatório (lambdas internas conhecem o userId do video)
4. Aplicar updates: se input.Status presente, video.Status = input.Status; idem para demais campos
5. Chamar `await repository.UpdateAsync(video, ct);` (com validações condicionais)
6. Capturar VideoUpdateConflictException, re-lançar ou converter em resultado esperado
7. Retornar VideoResponseModel do video atualizado

## Formas de Teste
1. Update sucesso: mock GetByIdAsync e UpdateAsync, validar VideoResponseModel retornado
2. Video não encontrado: mock GetByIdAsync retorna null, validar exception ou retorno null
3. Conflict: mock UpdateAsync lança VideoUpdateConflictException, validar que é propagada

## Critérios de Aceite da Subtask
- [ ] IUpdateVideoUseCase e UpdateVideoUseCase implementados
- [ ] Busca video existente (decisão sobre userId documentada)
- [ ] Aplica atualizações aos campos presentes no input
- [ ] Chama repository.UpdateAsync com validações condicionais
- [ ] VideoUpdateConflictException propagada ou tratada
- [ ] Retorna VideoResponseModel atualizado
- [ ] Testes unitários (cobertura >= 80%)
