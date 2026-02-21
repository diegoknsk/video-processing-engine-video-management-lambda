# Subtask 03: Corrigir UseCase — DurationSec, resultado de CreateAsync e construtor primário

## Descrição
Corrigir três problemas no `UploadVideoUseCase`: (1) `DurationSec` do input não é propagado para a entidade Video; (2) o resultado de `CreateAsync` é descartado; (3) o UseCase usa construtor convencional em vez de construtor primário conforme convenção do projeto.

## Passos de Implementação
1. **DurationSec:** Avaliar se a entidade `Video` já expõe `SetDuration(double durationSec)` ou similar. Se não, adicionar o método na entidade em `src/VideoProcessing.VideoManagement.Domain/Entities/Video.cs`. Após criação do objeto `Video`, chamar `video.SetDuration(input.DurationSec.Value)` quando `input.DurationSec.HasValue`.
2. **Resultado de CreateAsync:** Alterar `await _repository.CreateAsync(video, input.ClientRequestId, cancellationToken)` para `video = await _repository.CreateAsync(video, input.ClientRequestId, cancellationToken)` — usar a instância retornada pela persistência.
3. **Construtor primário:** Refatorar `UploadVideoUseCase` para usar construtor primário:
   ```csharp
   public class UploadVideoUseCase(
       IVideoRepository repository,
       IS3PresignedUrlService s3PresignedUrlService,
       IOptions<S3Options> s3Options,
       IValidator<UploadVideoInputModel> validator,
       ILogger<UploadVideoUseCase> logger) : IUploadVideoUseCase
   ```
   Remover campos privados e usar os parâmetros diretamente.
4. Compilar e garantir que os testes existentes ainda passam.

## Formas de Teste
1. Teste unitário existente: `ExecuteAsync_ValidInput_ShouldCreateVideoAndReturnPresignedUrl` deve continuar passando após refatoração
2. Novo teste (Subtask 05): verificar que DurationSec é setado na entidade quando fornecido no input
3. `dotnet build` sem erros
4. `dotnet test --filter UploadVideo` — todos os testes passam

## Critérios de Aceite da Subtask
- [ ] `DurationSec` do input propagado para a entidade Video quando fornecido (campo não é ignorado)
- [ ] Resultado de `CreateAsync` atribuído de volta à variável `video`
- [ ] `UploadVideoUseCase` usa construtor primário (sem campos privados de injeção manual)
- [ ] `dotnet test --filter UploadVideo` passa sem regressões
