# Subtask 02: Implementar S3PresignedUrlService (geração de URL de PUT)

## Descrição
Criar port `IS3PresignedUrlService` e implementação `S3PresignedUrlService` que usa `IAmazonS3.GetPreSignedURL` para gerar URL de PUT no S3 com TTL configurável, bucket/key especificados, método PUT, headers obrigatórios (Content-Type), e retornar URL assinada e timestamp de expiração.

## Passos de Implementação
1. Criar interface `IS3PresignedUrlService` com método `Task<(string Url, DateTime ExpiresAt)> GenerateUploadUrlAsync(string bucket, string key, string contentType, int ttlMinutes, CancellationToken ct = default);`
2. Implementar `S3PresignedUrlService` injetando `IAmazonS3` e `IOptions<S3Options>`
3. Usar `GetPreSignedURLRequest`: Verb = HttpVerb.PUT, BucketName = bucket, Key = key, Expires = DateTime.UtcNow.AddMinutes(ttlMinutes), ContentType = contentType
4. Chamar `s3Client.GetPreSignedURL(request)`, retornar (Url, ExpiresAt)
5. Registrar no DI: `services.AddScoped<IS3PresignedUrlService, S3PresignedUrlService>()`

## Formas de Teste
1. Mock test: mockar IAmazonS3, validar que GetPreSignedURL é chamado com parâmetros corretos
2. URL format test: validar que URL retornada contém bucket, key, assinatura AWS

## Critérios de Aceite da Subtask
- [ ] IS3PresignedUrlService criado
- [ ] S3PresignedUrlService implementado com GetPreSignedURL
- [ ] URL gerada com método PUT, Content-Type header, TTL correto
- [ ] ExpiresAt calculado corretamente (UtcNow + TTL)
- [ ] Registrado no DI
- [ ] Teste unitário valida geração de URL (mock IAmazonS3)
