# Subtask 05: Testes unitários (use case, validator, service)

## Descrição
Criar suite completa de testes unitários para UploadVideoUseCase, UploadVideoInputModelValidator, S3PresignedUrlService e endpoint POST /videos, cobrindo cenários de sucesso, idempotência, validação, erros, e garantir cobertura >= 80%.

## Passos de Implementação
1. Criar UploadVideoUseCaseTests: mock IVideoRepository e IS3PresignedUrlService; testar novo video, idempotência, pre-signed URL gerada
2. Criar UploadVideoInputModelValidatorTests: testar cada regra de validação (valid, originalFileName vazio, contentType inválido, sizeBytes > limite, clientRequestId inválido)
3. Criar S3PresignedUrlServiceTests: mock IAmazonS3, validar GetPreSignedURL chamado com parâmetros corretos
4. Criar VideosEndpointsTests (opcional, integração): testar endpoint com mock de use case, validar status codes 201/400/401
5. Executar `dotnet test /p:CollectCoverage=true`, validar cobertura >= 80%

## Formas de Teste
1. All tests pass: `dotnet test --filter UploadVideo*`
2. Coverage: `dotnet test /p:CollectCoverage=true`; validar >= 80%

## Critérios de Aceite da Subtask
- [ ] UploadVideoUseCaseTests criado com mínimo 4 testes (novo video, idempotência, pre-signed URL, erro repository)
- [ ] UploadVideoInputModelValidatorTests criado com mínimo 6 testes (1 válido, 5 inválidos)
- [ ] S3PresignedUrlServiceTests criado com mínimo 1 teste (mock GetPreSignedURL)
- [ ] Todos os testes passam
- [ ] Cobertura >= 80% para UploadVideoUseCase, validator, S3PresignedUrlService
