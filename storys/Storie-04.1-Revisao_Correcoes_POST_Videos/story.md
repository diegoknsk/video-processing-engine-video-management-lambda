# Storie-04.1: Revisão e Correções — POST /videos

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** —

## Descrição
Como time de desenvolvimento, queremos corrigir os problemas identificados na revisão da Story 04, para garantir que o endpoint POST /videos seja seguro, correto e com cobertura de testes adequada conforme os critérios de aceite originais.

## Objetivo
Corrigir todos os itens críticos e graves identificados na revisão da Story 04: remover backdoor de segurança no endpoint, corrigir validação de UUID no ClientRequestId, propagar DurationSec na entidade, adicionar UseAuthentication no pipeline, corrigir documentação OpenAPI do endpoint e criar os testes unitários faltantes (validator, S3 service, e 4º cenário do UseCase).

## Escopo Técnico
- **Tecnologias:** .NET 10, C# 13, FluentValidation 11, AWSSDK.S3 3.7.x, xUnit, Moq
- **Arquivos afetados:**
  - `src/VideoProcessing.VideoManagement.Api/Endpoints/VideosEndpoints.cs`
  - `src/VideoProcessing.VideoManagement.Api/Program.cs`
  - `src/VideoProcessing.VideoManagement.Application/Validators/UploadVideoInputModelValidator.cs`
  - `src/VideoProcessing.VideoManagement.Application/UseCases/UploadVideo/UploadVideoUseCase.cs`
  - `src/VideoProcessing.VideoManagement.Domain/Entities/Video.cs` (se necessário SetDuration)
  - `tests/VideoProcessing.VideoManagement.UnitTests/Application/Validators/UploadVideoInputModelValidatorTests.cs` (novo)
  - `tests/VideoProcessing.VideoManagement.UnitTests/Infra/Data/Services/S3PresignedUrlServiceTests.cs` (novo)
  - `tests/VideoProcessing.VideoManagement.UnitTests/Application/UseCases/UploadVideo/UploadVideoUseCaseTests.cs` (complementar)
- **Componentes:** VideosEndpoints, UploadVideoInputModelValidator, UploadVideoUseCase, Video (domain), pipeline de autenticação
- **Pacotes/Dependências:**
  - xUnit (2.9.x) — já adicionado
  - Moq (4.20.x) — já adicionado
  - coverlet.collector — já adicionado

## Dependências e Riscos (para estimativa)
- **Dependências:**
  - Story 04 (implementação base do POST /videos)
  - Story 02 (entidade Video, IVideoRepository)
- **Riscos:**
  - Adição de `UseAuthentication` pode exigir verificar se `AddAuthentication`/`AddJwtBearer` já está configurado (Story 01 ou DI); caso contrário, a chamada é no-op mas não quebra
  - `SetDuration` na entidade Video: avaliar se o construtor ou um método separado é a melhor abordagem sem impactar outros fluxos

## Subtasks
- [Subtask 01: Corrigir endpoint — remover backdoor e corrigir .Produces duplicado](./subtask/Subtask-01-Corrigir_Endpoint_Backdoor_Produces.md)
- [Subtask 02: Corrigir validator — validação UUID em ClientRequestId](./subtask/Subtask-02-Corrigir_Validator_UUID_ClientRequestId.md)
- [Subtask 03: Corrigir UseCase — DurationSec, resultado de CreateAsync e construtor primário](./subtask/Subtask-03-Corrigir_UseCase_DurationSec_CreateAsync.md)
- [Subtask 04: Adicionar UseAuthentication no pipeline (Program.cs)](./subtask/Subtask-04-Pipeline_UseAuthentication.md)
- [Subtask 05: Criar testes unitários faltantes (validator, S3 service, 4º cenário UseCase)](./subtask/Subtask-05-Testes_Unitarios_Faltantes.md)

## Critérios de Aceite da História
- [ ] Backdoor `x-user-id` removido do endpoint; sem token JWT válido com claim "sub", resposta é 401 Unauthorized
- [ ] `ClientRequestId` validado como UUID quando preenchido (`.Must(id => Guid.TryParse(id, out _))` no validator)
- [ ] `DurationSec` do input propagado corretamente para a entidade Video ao criar novo vídeo
- [ ] Resultado de `CreateAsync` atribuído de volta à variável `video` para usar a versão persistida
- [ ] `.Produces(401)` duplicado corrigido para `.Produces<ErrorResponse>(500)` no endpoint
- [ ] `app.UseAuthentication()` presente no pipeline em `Program.cs` antes de `app.UseRouting()`
- [ ] `UploadVideoInputModelValidatorTests` criado com mínimo 6 testes (1 válido + 5 inválidos cobrindo cada regra)
- [ ] `S3PresignedUrlServiceTests` criado com mínimo 1 teste validando chamada ao SDK com parâmetros corretos
- [ ] `UploadVideoUseCaseTests` com mínimo 4 testes (adicionado cenário de erro no repository)
- [ ] `dotnet test` passa sem erros; cobertura ≥ 80% para UseCase, validator e S3PresignedUrlService
- [ ] `UploadVideoUseCase` refatorado para usar construtor primário conforme convenção do projeto

## Rastreamento (dev tracking)
- **Início:** dia 18/02/2026, às 21:27 (Brasília)
- **Fim:** —
- **Tempo total de desenvolvimento:** —
