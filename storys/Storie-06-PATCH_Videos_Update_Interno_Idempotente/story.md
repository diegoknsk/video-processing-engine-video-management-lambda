# Storie-06: PATCH /videos/{id} — Update Interno Idempotente

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** —

## Descrição
Como componente interno do sistema (orchestrator, processor, finalizer), quero atualizar o estado de um vídeo (status, progressPercent, framesPrefix, s3KeyZip, errorMessage, etc.) via PATCH /videos/{id} de forma idempotente e com validações condicionais no DynamoDB, para garantir que múltiplos writers não causem regressão de progresso ou transições inválidas de status.

## Objetivo
Implementar o action `UpdateVideo` já declarado como stub em `VideosController`, adicionando `[AllowAnonymous]` para sobrescrever o `[Authorize]` da classe (rota interna). O `UpdateVideoInputModel` deve incluir `UserId` (Guid, obrigatório) pois o caller (orchestrator) sempre conhece o userId e não há JWT disponível nessa rota. O UseCase usa `GetByIdAsync(userId, videoId)` para buscar o vídeo existente, aplica os campos presentes do input, chama `UpdateAsync` (condições DynamoDB), trata `VideoUpdateConflictException` (409) e retorna `VideoResponseModel` atualizado.

## Escopo Técnico
- **Tecnologias:** .NET 10, DynamoDB UpdateItem condicional, idempotência
- **Arquivos criados/modificados:**
  - `src/VideoProcessing.VideoManagement.Api/Controllers/VideosController.cs` (implementar stub UpdateVideo com `[AllowAnonymous]`)
  - `src/VideoProcessing.VideoManagement.Application/Models/InputModels/UpdateVideoInputModel.cs` (adicionar campo `UserId`)
  - `src/VideoProcessing.VideoManagement.Application/UseCases/UpdateVideo/UpdateVideoUseCase.cs` (novo)
  - `src/VideoProcessing.VideoManagement.Application/Validators/UpdateVideoInputModelValidator.cs` (novo)
  - `src/VideoProcessing.VideoManagement.Api/DependencyInjection/ServiceCollectionExtensions.cs` (registrar UseCase e Validator)
  - `tests/VideoProcessing.VideoManagement.UnitTests/Application/UseCases/UpdateVideo/UpdateVideoUseCaseTests.cs` (novo)
  - `tests/VideoProcessing.VideoManagement.UnitTests/Application/Validators/UpdateVideoInputModelValidatorTests.cs` (novo)
- **Componentes:** `VideosController`, `UpdateVideoUseCase`, `UpdateVideoInputModelValidator`
- **Pacotes/Dependências:** Nenhum novo

## Dependências e Riscos (para estimativa)
- **Dependências:** Story 01, 02 (`IVideoRepository.UpdateAsync` condicional), Story 03 (`UpdateVideoInputModel`), Story 04 + 04.2 (`VideosController` com `[Authorize]` na classe), **Storie-05.1** (padronização de respostas — envelope success/data/error/timestamp). A 05.1 deve ser executada **antes** da 06 para que este endpoint já utilize o mesmo padrão.
- **Riscos:**
  - `[AllowAnonymous]` no action sobrescreve o `[Authorize]` da classe — verificar que apenas `UpdateVideo` fica público, demais actions permanecem protegidos
  - Conflitos de update: múltiplos writers simultâneos podem causar `ConditionalCheckFailedException`; idempotência via valores iguais (update com mesmo progressPercent não falha)
  - `UserId` no body: responsabilidade do caller fornecer o UUID correto; sem JWT não há validação adicional — documentar claramente como rota interna

## Subtasks
- [Subtask 01: Atualizar UpdateVideoInputModel e criar validator](./subtask/Subtask-01-Validator_UpdateVideoInputModel.md)
- [Subtask 02: Implementar UpdateVideoUseCase (idempotente e condicional)](./subtask/Subtask-02-UpdateVideoUseCase_Idempotente_Condicional.md)
- [Subtask 03: Implementar action PATCH /videos/{id} no VideosController](./subtask/Subtask-03-Endpoint_PATCH_Videos_Update_Interno.md)
- [Subtask 04: Testes unitários (use case, validator)](./subtask/Subtask-04-Testes_Unitarios_PATCH_Videos.md)

## Critérios de Aceite da História
- [ ] `UpdateVideoInputModel` possui campo `UserId` (Guid, obrigatório) — caller interno sempre fornece o userId
- [ ] Validator rejeita input sem `UserId`; rejeita se todos os demais campos forem nulos; rejeita `ProgressPercent` fora de 0–100
- [ ] Action `UpdateVideo` em `VideosController` possui `[AllowAnonymous]` explícito — sobrescreve o `[Authorize]` da classe; `PATCH /videos/{id}` acessível sem token JWT
- [ ] `UpdateVideoUseCase` chama `GetByIdAsync(input.UserId.ToString(), videoId)` para buscar o vídeo existente; retorna 404 se não encontrado
- [ ] UseCase aplica apenas os campos presentes no input (campos `null` não sobrescrevem o valor existente)
- [ ] UseCase chama `repository.UpdateAsync(video)` com validações condicionais (ownership, progressPercent monotônico, transições de status)
- [ ] `VideoUpdateConflictException` capturada no controller e retornada como 409 Conflict no **padrão de erro da Storie-05.1** (`{ "success": false, "error": { "code": "...", "message": "..." }, "timestamp": "..." }`).
- [ ] Response 200 OK no **padrão de sucesso da Storie-05.1** (`{ "success": true, "data": <VideoResponseModel>, "timestamp": "..." }`) com `VideoResponseModel` atualizado (via `ReturnValues = ALL_NEW` no DynamoDB); 404 e 400 no envelope de erro da 05.1 quando aplicável.
- [ ] Endpoint documentado no OpenAPI com description "Internal route for orchestrator/processor/finalizer"; responses (200, 400, 404, 409, 500)
- [ ] Demais actions do `VideosController` (POST, GET) **não** são afetados pelo `[AllowAnonymous]` do PATCH
- [ ] Testes unitários cobrindo: update de status, update de progressPercent, múltiplos campos, conflito (409), vídeo não encontrado (404)
- [ ] Cobertura >= 80% para `UpdateVideoUseCase`
- [ ] `dotnet test` passa sem erros após a implementação

## Rastreamento (dev tracking)
- **Início:** 21/02/2026, às 17:58 (Brasília)
- **Fim:** —
- **Tempo total de desenvolvimento:** —
