# Subtask 02: Implementar UpdateVideoUseCase (idempotente e condicional)

## Descrição
Criar `IUpdateVideoUseCase` e `UpdateVideoUseCase` que busca o vídeo existente via `GetByIdAsync(input.UserId, videoId)`, aplica apenas os campos presentes no `UpdateVideoInputModel`, chama `repository.UpdateAsync` (com validações condicionais do DynamoDB), trata `VideoUpdateConflictException` relançando-a para o controller tratar como 409, e retorna `VideoResponseModel` atualizado.

> **Decisão de design:** `UserId` vem do `UpdateVideoInputModel` (não do JWT), pois a rota é interna sem autenticação. O caller (orchestrator/processor) sempre conhece o `userId` do vídeo que está processando.

## Passos de Implementação
1. Criar interface:
   ```csharp
   public interface IUpdateVideoUseCase
   {
       Task<VideoResponseModel?> ExecuteAsync(Guid videoId, UpdateVideoInputModel input, CancellationToken ct = default);
   }
   ```
2. Implementar `UpdateVideoUseCase(IVideoRepository repository)` com construtor primário
3. Buscar vídeo existente:
   ```csharp
   var video = await repository.GetByIdAsync(input.UserId.ToString(), videoId.ToString(), ct);
   if (video is null) return null; // controller retorna 404
   ```
4. Aplicar apenas campos presentes no input (campos `null` mantêm o valor atual do vídeo):
   ```csharp
   var updated = video with
   {
       Status = input.Status ?? video.Status,
       ProgressPercent = input.ProgressPercent ?? video.ProgressPercent,
       ErrorMessage = input.ErrorMessage ?? video.ErrorMessage,
       ErrorCode = input.ErrorCode ?? video.ErrorCode,
       FramesPrefix = input.FramesPrefix ?? video.FramesPrefix,
       S3KeyZip = input.S3KeyZip ?? video.S3KeyZip,
       S3BucketFrames = input.S3BucketFrames ?? video.S3BucketFrames,
       S3BucketZip = input.S3BucketZip ?? video.S3BucketZip,
       StepExecutionArn = input.StepExecutionArn ?? video.StepExecutionArn,
       UpdatedAt = DateTime.UtcNow
   };
   ```
5. Chamar `repository.UpdateAsync(updated, ct)` — lança `VideoUpdateConflictException` se condição DynamoDB falhar
6. Mapear video retornado para `VideoResponseModel` e retornar
7. `VideoUpdateConflictException` **não** é capturada aqui — propaga para o controller que retorna 409
8. Registrar no DI

## Formas de Teste
1. Update de status: mock `GetByIdAsync` retorna video, mock `UpdateAsync` retorna video atualizado; validar `VideoResponseModel` com status correto
2. Update de progressPercent: idem, validar campo atualizado
3. Múltiplos campos: input com status + progressPercent + errorMessage; validar que todos aplicados
4. Vídeo não encontrado: mock `GetByIdAsync` retorna `null`; validar que UseCase retorna `null`
5. Conflito: mock `UpdateAsync` lança `VideoUpdateConflictException`; validar que exceção propaga

## Critérios de Aceite da Subtask
- [ ] `IUpdateVideoUseCase` e `UpdateVideoUseCase` implementados com construtor primário
- [ ] `GetByIdAsync` chamado com `input.UserId.ToString()` e `videoId.ToString()`
- [ ] Retorna `null` se vídeo não encontrado (controller trata como 404)
- [ ] Apenas campos não-nulos do input sobrescrevem o video existente
- [ ] `UpdateAsync` chamado com a entidade resultante após merge
- [ ] `VideoUpdateConflictException` propagada sem captura (controller trata como 409)
- [ ] Retorna `VideoResponseModel` mapeado do resultado de `UpdateAsync`
- [ ] Registrado no DI
- [ ] Testes unitários (cobertura >= 80%)
