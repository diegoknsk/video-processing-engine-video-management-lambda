# Subtask 01: Implementar ListVideosUseCase (paginação cursor-based)

## Descrição
Criar IListVideosUseCase e ListVideosUseCase que chama IVideoRepository.GetByUserIdAsync com userId, limit e nextToken, mapeia Videos para VideoResponseModel, e retorna VideoListResponseModel com items e nextToken.

## Passos de Implementação
1. Interface: `Task<VideoListResponseModel> ExecuteAsync(string userId, int limit, string? nextToken, CancellationToken ct);`
2. Implementar: chamar `repository.GetByUserIdAsync(userId, limit, nextToken, ct)`, mapear Videos para VideoResponseModel (criar mapper ou extension), retornar VideoListResponseModel
3. Validar limit: se > 100, ajustar para 100 (limite máximo)
4. Registrar no DI

## Formas de Teste
1. Mock test: validar que GetByUserIdAsync chamado com parâmetros corretos
2. Pagination test: mock retorna nextToken, validar que é incluído no response

## Critérios de Aceite da Subtask
- [ ] IListVideosUseCase e ListVideosUseCase implementados
- [ ] Limit máximo 100; se maior, ajusta para 100
- [ ] Mapeia Videos para VideoResponseModel
- [ ] Retorna VideoListResponseModel com videos e nextToken
- [ ] Testes unitários (cobertura >= 80%)
