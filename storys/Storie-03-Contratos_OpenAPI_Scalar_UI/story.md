# Storie-03: Contratos OpenAPI + Scalar UI (Documentação Completa)

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** —

## Descrição
Como desenvolvedor de API, quero configurar OpenAPI (Swagger/OAS 3.0) com documentação completa de todas as rotas (incluindo autenticação Cognito, InputModels, ResponseModels, erros padronizados) e integrar Scalar UI como interface de documentação interativa, para garantir que a API seja autodocumentada, testável via "Try it" e alinhada com padrões do mercado desde o primeiro deploy.

## Objetivo
Configurar Swashbuckle.AspNetCore (ou alternativa para .NET 10) para geração automática de OpenAPI, definir todos os contratos de InputModels e ResponseModels para as 5 rotas obrigatórias (GET /health, POST /videos, GET /videos, GET /videos/{id}, PATCH /videos/{id}), documentar esquemas de autenticação (Cognito/JWT), integrar Scalar UI para substituir Swagger UI padrão, garantir que exemplos e descrições de erros estão presentes, e validar que documentação JSON está acessível via GET /openapi/v1.json.

## Escopo Técnico
- **Tecnologias:** .NET 10, Swashbuckle.AspNetCore (6.x), Scalar UI, OpenAPI 3.0
- **Arquivos criados/modificados:**
  - `src/VideoProcessing.VideoManagement.Api/Extensions/OpenApiExtensions.cs` (configuração Swashbuckle + Scalar)
  - `src/VideoProcessing.VideoManagement.Application/Models/InputModels/` (UploadVideoInputModel, UpdateVideoInputModel)
  - `src/VideoProcessing.VideoManagement.Application/Models/ResponseModels/` (VideoResponseModel, VideoListResponseModel, UploadVideoResponseModel)
  - `src/VideoProcessing.VideoManagement.Api/Program.cs` (registrar AddSwaggerGen, UseSwagger, UseScalarApiReference)
  - Documentação inline nos InputModels/ResponseModels com XML comments e atributos [Description], [Required], [Example]
- **Componentes:** 
  - OpenAPI configuration (info, security schemes, servers)
  - Scalar UI integration
  - InputModels (UploadVideoInputModel, UpdateVideoInputModel)
  - ResponseModels (VideoResponseModel, UploadVideoResponseModel, VideoListResponseModel, ErrorResponse)
- **Pacotes/Dependências:**
  - Swashbuckle.AspNetCore (6.5.x)
  - Scalar.AspNetCore (1.x ou latest) — UI alternativa moderna ao Swagger UI

## Dependências e Riscos (para estimativa)
- **Dependências:** 
  - Story 01 concluída (Program.cs, rotas base)
  - Story 02 concluída (entidade Video para mapear em ResponseModels)
- **Riscos:** 
  - Compatibilidade de Swashbuckle.AspNetCore com .NET 10 (verificar versão estável)
  - Scalar UI pode não ter package oficial para .NET; alternativa: servir Scalar HTML estático apontando para /swagger/v1/swagger.json

## Subtasks
- [Subtask 01: Configurar Swashbuckle e gerar OpenAPI JSON básico](./subtask/Subtask-01-Configurar_Swashbuckle_OpenAPI_Basico.md)
- [Subtask 02: Criar InputModels e ResponseModels com documentação XML](./subtask/Subtask-02-InputModels_ResponseModels_Documentacao.md)
- [Subtask 03: Documentar esquemas de segurança (Cognito/JWT) no OpenAPI](./subtask/Subtask-03-Documentar_Seguranca_Cognito_OpenAPI.md)
- [Subtask 04: Integrar Scalar UI e configurar servers/PathBase](./subtask/Subtask-04-Integrar_Scalar_UI_Servers_PathBase.md)
- [Subtask 05: Adicionar exemplos e descrições de erros padronizados](./subtask/Subtask-05-Exemplos_Erros_Padronizados_OpenAPI.md)

## Critérios de Aceite da História
- [ ] Swashbuckle.AspNetCore configurado no Program.cs; GET /swagger/v1/swagger.json retorna JSON do OpenAPI válido
- [ ] OpenAPI info preenchido: título ("Video Management API"), versão ("1.0.0"), descrição, contact/license
- [ ] Scalar UI acessível via GET /scalar (ou rota configurada); interface carrega e exibe todas as rotas
- [ ] InputModels criados: UploadVideoInputModel (originalFileName, contentType, sizeBytes, durationSec?, clientRequestId), UpdateVideoInputModel (status?, progressPercent?, errorMessage?, errorCode?, framesPrefix?, s3KeyZip?, etc.)
- [ ] ResponseModels criados: VideoResponseModel (todos os campos de Video), UploadVideoResponseModel (videoId, uploadUrl, expiresAt), VideoListResponseModel (videos: VideoResponseModel[], nextToken?), ErrorResponse (já existe da Story 01)
- [ ] Todos os InputModels e ResponseModels têm XML comments e atributos [Required], [Description], [Example] onde aplicável
- [ ] Esquema de segurança "BearerAuth" (JWT) documentado no OpenAPI com securitySchemes e security aplicada às rotas protegidas
- [ ] Rota GET /health documentada como pública (sem security); demais rotas (POST /videos, GET /videos, GET /videos/{id}, PATCH /videos/{id}) documentadas como protegidas
- [ ] Exemplos de request/response presentes no OpenAPI para pelo menos POST /videos e GET /videos/{id}
- [ ] Descrições de erros padronizados documentadas (400 Bad Request, 401 Unauthorized, 404 Not Found, 409 Conflict, 500 Internal Server Error) com schema ErrorResponse
- [ ] Scalar UI "Try it" funciona localmente (consegue chamar GET /health sem auth e recebe 401 ao tentar POST /videos sem token)
- [ ] Documentação OpenAPI valida em validator.swagger.io (ou equivalente)

## Rastreamento (dev tracking)
- **Início:** 15/02/2026, às 18:03 (Brasília)
- **Fim:** —
- **Tempo total de desenvolvimento:** —
