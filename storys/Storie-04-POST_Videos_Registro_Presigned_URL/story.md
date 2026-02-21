# Storie-04: POST /videos — Registro de Vídeo e Pre-signed URL

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** —

## Descrição
Como usuário autenticado, quero registrar um novo vídeo no sistema fornecendo metadados (nome do arquivo, content type, tamanho, duração) e receber uma URL pré-assinada do S3 para upload direto, para que eu possa fazer upload do vídeo sem passar pela API (melhor performance e custo).

## Objetivo
Implementar endpoint POST /videos que recebe UploadVideoInputModel, valida input com FluentValidation, extrai userId do JWT (claim "sub"), implementa idempotência com clientRequestId (deduplicação de retries), gera videoId (UUID), cria registro no DynamoDB com status Pending, gera s3KeyVideo imutável (videos/{userId}/{videoId}/original), gera pre-signed URL de PUT no S3 com TTL configurável, retorna UploadVideoResponseModel (videoId, uploadUrl, expiresAt) e garante que múltiplas chamadas com mesmo clientRequestId retornam mesmo videoId.

## Escopo Técnico
- **Tecnologias:** .NET 10, FluentValidation, AWSSDK.S3 (GetPreSignedURL), DynamoDB (via IVideoRepository)
- **Arquivos criados/modificados:**
  - `src/VideoProcessing.VideoManagement.Application/UseCases/UploadVideo/UploadVideoUseCase.cs`
  - `src/VideoProcessing.VideoManagement.Application/UseCases/UploadVideo/IUploadVideoUseCase.cs` (port)
  - `src/VideoProcessing.VideoManagement.Application/Validators/UploadVideoInputModelValidator.cs` (FluentValidation)
  - `src/VideoProcessing.VideoManagement.Api/Endpoints/VideosEndpoints.cs` (MapPost)
  - `src/VideoProcessing.VideoManagement.Infra.Data/Services/S3PresignedUrlService.cs` (geração de URL)
  - `src/VideoProcessing.VideoManagement.Infra.Data/Services/IS3PresignedUrlService.cs` (port)
- **Componentes:** 
  - UploadVideoUseCase (lógica de negócio)
  - UploadVideoInputModelValidator (validação)
  - S3PresignedUrlService (adapter S3)
  - VideosEndpoints (controller/route handler)
- **Pacotes/Dependências:**
  - FluentValidation.AspNetCore (11.3.x) — já adicionado
  - AWSSDK.S3 (3.7.x) — já adicionado

## Dependências e Riscos (para estimativa)
- **Dependências:** 
  - Story 01 (bootstrap, DI, logging)
  - Story 02 (IVideoRepository, Video entity)
  - Story 03 (InputModels, ResponseModels, OpenAPI)
- **Riscos:** 
  - Idempotência via clientRequestId: precisa query rápido (GSI no DynamoDB ou scan; decisão: GSI gsi1pk=USER#{userId}, gsi1sk=CLIENT_REQUEST#{clientRequestId})
  - Pre-signed URL TTL curto (15 min padrão) pode causar timeout em uploads grandes; documentar limites
  - Validação de contentType: whitelist (video/mp4, video/quicktime, etc.); documentar tipos aceitos

## Uso do clientRequestId (múltiplos vídeos por usuário)
- **Idempotência:** Quando informado, o backend trata requisições com o **mesmo** `clientRequestId` para o mesmo usuário como a **mesma** operação: retorna o mesmo `videoId` e uma nova presigned URL (sem criar outro vídeo). Serve para **retries** do mesmo upload (rede/timeout).
- **Novo vídeo por upload:** Para cada **novo** arquivo/vídeo o cliente deve enviar um **ClientRequestId diferente** (ex.: novo UUID por arquivo) ou **omitir** o campo. Se o cliente enviar sempre o mesmo valor (ex.: o próprio `userId`) em todos os POSTs, apenas o primeiro vídeo será criado; as demais chamadas serão tratadas como idempotência e devolverão o mesmo `videoId`.
- **Resumo:** Um `clientRequestId` = um vídeo (por usuário). Múltiplos vídeos = múltiplos `clientRequestId` distintos (ou campo omitido para cada novo upload).

## Subtasks
- [Subtask 01: Criar validator de UploadVideoInputModel (FluentValidation)](./subtask/Subtask-01-Validator_UploadVideoInputModel.md)
- [Subtask 02: Implementar S3PresignedUrlService (geração de URL de PUT)](./subtask/Subtask-02-S3PresignedUrlService_Presigned_URL.md)
- [Subtask 03: Implementar UploadVideoUseCase (lógica de idempotência e criação)](./subtask/Subtask-03-UploadVideoUseCase_Logica_Idempotencia.md)
- [Subtask 04: Criar endpoint POST /videos (extração de userId do JWT)](./subtask/Subtask-04-Endpoint_POST_Videos_Extracao_UserId.md)
- [Subtask 05: Testes unitários (use case, validator, service)](./subtask/Subtask-05-Testes_Unitarios_POST_Videos.md)

## Critérios de Aceite da História
- [ ] Endpoint POST /videos implementado; aceita UploadVideoInputModel no body
- [ ] Validação com FluentValidation: originalFileName obrigatório (não vazio, max 255 chars), contentType whitelist (video/mp4, video/quicktime, video/x-msvideo, etc.), sizeBytes > 0 e <= limite configurado (ex.: 5 GB), durationSec opcional (se presente, > 0), clientRequestId obrigatório (UUID)
- [ ] userId extraído do JWT (claim "sub"); se ausente ou inválido, retorna 401 Unauthorized
- [ ] Idempotência: se clientRequestId já existe para o userId, retorna videoId e s3KeyVideo existentes; gera nova pre-signed URL (pode ter expirado)
- [ ] videoId gerado como UUID v4 (Guid.NewGuid)
- [ ] s3KeyVideo imutável definido como `videos/{userId}/{videoId}/original` (ou original.{ext}; decisão: sem extensão para simplicidade)
- [ ] Registro criado no DynamoDB com status Pending, progressPercent 0, createdAt/updatedAt com timestamp UTC
- [ ] Pre-signed URL gerada com: método PUT, bucket configurado (S3Options.BucketVideo), TTL configurável (S3Options.PresignedUrlTtlMinutes padrão 15), Content-Type header obrigatório no upload
- [ ] Response 201 Created com UploadVideoResponseModel: videoId, uploadUrl, expiresAt (timestamp UTC)
- [ ] Erros documentados: 400 (validação), 401 (sem token/userId), 409 (erro de idempotência se detecção falhar), 500 (erro interno)
- [ ] Testes unitários cobrindo: validação (campos obrigatórios, whitelist contentType, limites), idempotência (mesmo clientRequestId retorna mesmo videoId), geração de pre-signed URL, criação de registro no DynamoDB
- [ ] Cobertura de testes >= 80% para UploadVideoUseCase e validator
- [ ] Scalar UI "Try it" funciona: POST /videos com token válido retorna 201; sem token retorna 401

## Rastreamento (dev tracking)
- **Início:** —
- **Fim:** —
- **Tempo total de desenvolvimento:** —
