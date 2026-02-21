# Subtask 05: Criar testes unitários faltantes (validator, S3 service, 4º cenário UseCase)

## Descrição
Criar os arquivos de teste que estavam faltando da Story 04: `UploadVideoInputModelValidatorTests` (mínimo 6 testes cobrindo cada regra), `S3PresignedUrlServiceTests` (mínimo 1 teste mockando o SDK), e adicionar o 4º cenário faltante em `UploadVideoUseCaseTests` (erro no repository).

## Passos de Implementação
1. **UploadVideoInputModelValidatorTests** — criar em `tests/.../Application/Validators/UploadVideoInputModelValidatorTests.cs`:
   - Cenário válido: todos os campos corretos → `IsValid = true`
   - OriginalFileName vazio → falha com mensagem "OriginalFileName is required."
   - OriginalFileName > 255 chars → falha com mensagem de limite
   - ContentType inválido (ex.: "image/png") → falha com mensagem de whitelist
   - SizeBytes = 0 → falha com "SizeBytes must be greater than 0."
   - SizeBytes > 5 GB → falha com mensagem de limite
   - ClientRequestId = "not-a-guid" → falha com mensagem de UUID (após correção da Subtask 02)
   - DurationSec = -1 (quando presente) → falha com "DurationSec must be greater than 0 if provided."

2. **S3PresignedUrlServiceTests** — criar em `tests/.../Infra/Data/Services/S3PresignedUrlServiceTests.cs`:
   - Mock de `IAmazonS3`; chamar `GeneratePutPresignedUrl("bucket", "key", TimeSpan.FromMinutes(15), "video/mp4")`
   - Verificar que `GetPreSignedURL` foi chamado com `Verb = HttpVerb.PUT`, `BucketName`, `Key`, `ContentType` e `Expires` corretos
   - Verificar retorno da URL gerada pelo mock

3. **UploadVideoUseCaseTests — 4º cenário** — adicionar em `UploadVideoUseCaseTests.cs`:
   - `ExecuteAsync_RepositoryThrows_ShouldPropagateException`: configurar `_repositoryMock.Setup(r => r.CreateAsync(...)).ThrowsAsync(new Exception("DB error"))` → `await Assert.ThrowsAsync<Exception>(() => _useCase.ExecuteAsync(...))`

4. Executar `dotnet test` e verificar cobertura >= 80% para os componentes afetados

## Formas de Teste
1. `dotnet test --filter "UploadVideoInputModelValidator"` — todos os testes passam
2. `dotnet test --filter "S3PresignedUrlService"` — teste passa
3. `dotnet test --filter "UploadVideo"` — 4 testes passam
4. `dotnet test /p:CollectCoverage=true` — cobertura ≥ 80% para UseCase, validator e S3PresignedUrlService

## Critérios de Aceite da Subtask
- [ ] `UploadVideoInputModelValidatorTests` criado com mínimo 6 testes (1 válido + 5 inválidos), cada um cobrindo uma regra diferente
- [ ] `S3PresignedUrlServiceTests` criado com mínimo 1 teste verificando chamada ao SDK com parâmetros corretos (PUT, bucket, key, content-type, expiry)
- [ ] `UploadVideoUseCaseTests` com mínimo 4 testes (incluindo cenário de erro no repository)
- [ ] `dotnet test` passa sem falhas
- [ ] Cobertura ≥ 80% para `UploadVideoUseCase`, `UploadVideoInputModelValidator` e `S3PresignedUrlService`
