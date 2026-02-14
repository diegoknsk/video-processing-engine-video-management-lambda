# Subtask 02: Criar InputModels e ResponseModels com documentação XML

## Descrição
Definir todos os InputModels (UploadVideoInputModel, UpdateVideoInputModel) e ResponseModels (VideoResponseModel, UploadVideoResponseModel, VideoListResponseModel) com propriedades documentadas via XML comments, adicionar atributos [Required], [Description], [Example] do System.ComponentModel.DataAnnotations, garantir que contratos são consistentes com entidade Video do Domain e que Swashbuckle gera schemas corretos no OpenAPI.

## Passos de Implementação
1. Criar UploadVideoInputModel: originalFileName, contentType, sizeBytes, durationSec (opcional), clientRequestId
2. Criar UpdateVideoInputModel: status, progressPercent, errorMessage, errorCode, framesPrefix, s3KeyZip, s3BucketFrames, s3BucketZip, stepExecutionArn (todos opcionais; pelo menos 1 deve estar presente)
3. Criar VideoResponseModel: mapear todos os campos de Video (videoId, userId, status, progressPercent, createdAt, updatedAt, s3KeyVideo, etc.)
4. Criar UploadVideoResponseModel: videoId, uploadUrl, expiresAt
5. Criar VideoListResponseModel: videos (List<VideoResponseModel>), nextToken
6. Adicionar XML comments e atributos [Required], [Description] em cada propriedade
7. Registrar filtros Swashbuckle para incluir exemplos (OperationFilter ou SchemaFilter)

## Formas de Teste
1. Schema validation: acessar /swagger/v1/swagger.json, validar que schemas dos InputModels/ResponseModels estão presentes e corretos
2. XML comments test: validar que descrições aparecem no Swagger UI
3. Validation test: tentar fazer POST com InputModel inválido (campo Required faltando), validar que 400 é retornado (após implementação de rotas)

## Critérios de Aceite da Subtask
- [ ] InputModels criados: UploadVideoInputModel, UpdateVideoInputModel
- [ ] ResponseModels criados: VideoResponseModel, UploadVideoResponseModel, VideoListResponseModel
- [ ] Todos os models têm XML comments e atributos [Required]/[Description]
- [ ] Schemas aparecem corretamente no OpenAPI JSON
- [ ] Swashbuckle gera documentação com descrições dos campos
