# Storie-06: PATCH /videos/{id} — Update Interno Idempotente

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** —

## Descrição
Como componente interno do sistema (orchestrator, processor, finalizer), quero atualizar o estado de um vídeo (status, progressPercent, framesPrefix, s3KeyZip, errorMessage, etc.) via PATCH /videos/{id} de forma idempotente e com validações condicionais no DynamoDB, para garantir que múltiplos writers não causem regressão de progresso ou transições inválidas de status.

## Objetivo
Implementar endpoint PATCH /videos/{id} que recebe UpdateVideoInputModel (campos opcionais: status, progressPercent, errorMessage, errorCode, framesPrefix, s3KeyZip, s3BucketFrames, s3BucketZip, stepExecutionArn), valida que pelo menos 1 campo está presente, **NÃO exige autenticação via JWT** (rota interna, protegida via API Gateway ou network; decisão: deixar público por enquanto mas documentar como interna), implementa UpdateVideoUseCase que chama IVideoRepository.UpdateAsync (com validações condicionais: ownership implícito via userId do video, progressPercent monotônico, status transitions), trata ConflictException (409) quando condição falha, e retorna VideoResponseModel atualizado.

## Escopo Técnico
- **Tecnologias:** .NET 10, DynamoDB UpdateItem condicional, idempotência
- **Arquivos criados/modificados:**
  - `src/VideoProcessing.VideoManagement.Application/UseCases/UpdateVideo/UpdateVideoUseCase.cs`
  - `src/VideoProcessing.VideoManagement.Application/Validators/UpdateVideoInputModelValidator.cs`
  - `src/VideoProcessing.VideoManagement.Api/Endpoints/VideosEndpoints.cs` (adicionar MapPatch)
- **Componentes:** UpdateVideoUseCase, UpdateVideoInputModelValidator
- **Pacotes/Dependências:** Nenhum novo

## Dependências e Riscos (para estimativa)
- **Dependências:** Story 01, 02 (IVideoRepository.UpdateAsync condicional), Story 03 (UpdateVideoInputModel)
- **Riscos:** 
  - Segurança: PATCH sendo interno mas sem autenticação pode ser exposto publicamente se API Gateway não configurar; documentar claramente
  - Conflitos de update: múltiplos writers simultâneos podem causar ConditionalCheckFailedException; idempotência via valores iguais (update com mesmo progressPercent não falha)

## Subtasks
- [Subtask 01: Criar validator de UpdateVideoInputModel (pelo menos 1 campo presente)](./subtask/Subtask-01-Validator_UpdateVideoInputModel.md)
- [Subtask 02: Implementar UpdateVideoUseCase (idempotente e condicional)](./subtask/Subtask-02-UpdateVideoUseCase_Idempotente_Condicional.md)
- [Subtask 03: Criar endpoint PATCH /videos/{id} (sem autenticação JWT por enquanto)](./subtask/Subtask-03-Endpoint_PATCH_Videos_Update_Interno.md)
- [Subtask 04: Testes unitários (use case, validator, conflitos)](./subtask/Subtask-04-Testes_Unitarios_PATCH_Videos.md)

## Critérios de Aceite da História
- [ ] Endpoint PATCH /videos/{id} implementado; aceita UpdateVideoInputModel no body
- [ ] Validação: pelo menos 1 campo presente (status, progressPercent, errorMessage, errorCode, framesPrefix, s3KeyZip, s3BucketFrames, s3BucketZip, stepExecutionArn); todos opcionais mas pelo menos 1 obrigatório
- [ ] Se progressPercent presente, validar 0–100
- [ ] UpdateVideoUseCase obtém video existente via GetByIdAsync (para ter userId e videoId), aplica atualizações aos campos presentes, chama repository.UpdateAsync
- [ ] IVideoRepository.UpdateAsync implementa validações condicionais (já implementado na Story 02 Subtask 05): ownership, progressPercent monotônico, status transitions
- [ ] Se ConditionalCheckFailedException (repository lança VideoUpdateConflictException), retornar 409 Conflict com ErrorResponse
- [ ] Idempotência: update com mesmos valores (ex.: mesmo progressPercent) não falha; DynamoDB permite `<=` para monotonia
- [ ] Response 200 OK com VideoResponseModel atualizado (usar ReturnValues ALL_NEW do DynamoDB)
- [ ] Endpoint **NÃO** usa .RequireAuthorization() (interno; decisão: deixar público por ora; documentar no OpenAPI description que é rota interna)
- [ ] Documentado no OpenAPI com description "Internal route for orchestrator/processor/finalizer"; responses (200, 400, 404, 409, 500)
- [ ] Testes unitários cobrindo: update de status, update de progressPercent, update múltiplos campos, conflito (409), video não encontrado (404)
- [ ] Cobertura >= 80% para UpdateVideoUseCase
- [ ] Scalar UI "Try it" funciona: PATCH /videos/{id} com body válido retorna 200

## Rastreamento (dev tracking)
- **Início:** —
- **Fim:** —
- **Tempo total de desenvolvimento:** —
