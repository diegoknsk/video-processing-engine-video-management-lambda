# Subtask 03: Implementar UploadVideoUseCase (lógica de idempotência e criação)

## Descrição
Implementar `UploadVideoUseCase` que: verifica idempotência via `IVideoRepository.GetByClientRequestIdAsync`, se existe retorna videoId existente e gera nova pre-signed URL, se não existe cria Video (videoId UUID, status Pending, s3KeyVideo imutável), persiste no DynamoDB via CreateAsync, gera pre-signed URL via IS3PresignedUrlService, retorna UploadVideoResponseModel.

## Passos de Implementação
1. Criar interface `IUploadVideoUseCase` com `Task<UploadVideoResponseModel> ExecuteAsync(UploadVideoInputModel input, string userId, CancellationToken ct);`
2. Implementar `UploadVideoUseCase` injetando `IVideoRepository`, `IS3PresignedUrlService`, `IOptions<S3Options>`
3. Verificar idempotência: `var existing = await repository.GetByClientRequestIdAsync(userId, input.ClientRequestId, ct);` se não null, usar existing.VideoId e existing.S3KeyVideo
4. Se null: gerar videoId = Guid.NewGuid().ToString(), s3KeyVideo = $"videos/{userId}/{videoId}/original", criar Video via Video.Create, chamar repository.CreateAsync
5. Gerar pre-signed URL: `var (url, expiresAt) = await presignedService.GenerateUploadUrlAsync(s3Options.BucketVideo, s3KeyVideo, input.ContentType, s3Options.PresignedUrlTtlMinutes, ct);`
6. Retornar `new UploadVideoResponseModel(videoId, url, expiresAt)`

## Formas de Teste
1. New video test: mock repository retorna null em GetByClientRequestId, validar que CreateAsync é chamado
2. Idempotency test: mock repository retorna video existente, validar que CreateAsync NÃO é chamado, videoId retornado é o existente
3. Pre-signed URL test: validar que IS3PresignedUrlService.GenerateUploadUrlAsync é chamado

## Critérios de Aceite da Subtask
- [ ] IUploadVideoUseCase e UploadVideoUseCase implementados
- [ ] Idempotência: GetByClientRequestIdAsync chamado; se retorna video, usa videoId existente
- [ ] Novo video: videoId UUID, s3KeyVideo = videos/{userId}/{videoId}/original, status Pending, progressPercent 0
- [ ] CreateAsync chamado com clientRequestId
- [ ] Pre-signed URL gerada para bucket/key corretos
- [ ] UploadVideoResponseModel retornado com videoId, uploadUrl, expiresAt
- [ ] Testes unitários cobrindo idempotência e criação (cobertura >= 80%)
