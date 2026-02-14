# Subtask 02: Implementar GetVideoByIdUseCase (ownership validation)

## Descrição
Criar IGetVideoByIdUseCase e GetVideoByIdUseCase que chama IVideoRepository.GetByIdAsync(userId, videoId), valida ownership (userId do JWT corresponde ao userId do video), retorna VideoResponseModel ou null se não encontrado/não autorizado.

## Passos de Implementação
1. Interface: `Task<VideoResponseModel?> ExecuteAsync(string userId, string videoId, CancellationToken ct);`
2. Implementar: chamar `repository.GetByIdAsync(userId, videoId, ct)`, se null retornar null, mapear Video para VideoResponseModel, retornar
3. Ownership implícito: GetByIdAsync já filtra por userId (pk = USER#{userId}), então se retornar null é porque não encontrado ou não pertence ao usuário
4. Registrar no DI

## Formas de Teste
1. Found test: mock GetByIdAsync retorna video, validar que VideoResponseModel é retornado
2. Not found test: mock GetByIdAsync retorna null, validar que retorna null

## Critérios de Aceite da Subtask
- [ ] IGetVideoByIdUseCase e GetVideoByIdUseCase implementados
- [ ] GetByIdAsync chamado com userId e videoId
- [ ] Mapeia Video para VideoResponseModel
- [ ] Retorna null se video não encontrado ou ownership mismatch
- [ ] Testes unitários (cobertura >= 80%)
