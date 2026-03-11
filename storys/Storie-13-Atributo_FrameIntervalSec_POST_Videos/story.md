# Storie-13: Atributo frameIntervalSec no POST /videos

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** —

## Descrição
Como usuário do sistema, quero informar o intervalo em segundos (`frameIntervalSec`) no registro de vídeo (POST /videos), para que outro microserviço saiba em qual intervalo tirar prints do vídeo e armazená-los; esse valor deve ser persistido no DynamoDB e refletido em todas as camadas (API e Lambda).

## Objetivo
Adicionar o atributo **frameIntervalSec** ao fluxo POST /videos: aceitar no body, validar (obrigatório ou opcional conforme decisão; quando informado junto com durationSec, não pode ser superior a 50% da duração do vídeo — ex.: vídeo 60s → máximo 30s), persistir na entidade Video e no DynamoDB, e expor em respostas de consulta (GET/listagem). Garantir que a mesma lógica valha tanto na API (Kestrel) quanto quando a API é hospedada no Lambda (AddAWSLambdaHosting).

## Escopo Técnico
- **Tecnologias:** .NET 10, FluentValidation, DynamoDB (IVideoRepository), existente Story 04 (POST /videos)
- **Arquivos criados/modificados:**
  - `src/Core/VideoProcessing.VideoManagement.Application/Models/InputModels/UploadVideoInputModel.cs` — adicionar `FrameIntervalSec`
  - `src/Core/VideoProcessing.VideoManagement.Application/Validators/UploadVideoInputModelValidator.cs` — regra 50% de durationSec
  - `src/Core/VideoProcessing.VideoManagement.Domain/Entities/Video.cs` — propriedade e setter
  - `src/Core/VideoProcessing.VideoManagement.Domain/Entities/VideoRehydrationData.cs` — parâmetro
  - `src/Core/VideoProcessing.VideoManagement.Application/UseCases/UploadVideo/UploadVideoUseCase.cs` — propagar para entidade
  - `src/Infra/VideoProcessing.VideoManagement.Infra.Data/Repositories/VideoEntity.cs` — campo DynamoDB
  - `src/Infra/VideoProcessing.VideoManagement.Infra.Data/Repositories/VideoRepository.cs` — leitura/escrita do atributo
  - `src/Infra/VideoProcessing.VideoManagement.Infra.Data/Mappers/VideoMapper.cs` — mapeamento
  - `src/Core/VideoProcessing.VideoManagement.Application/Models/ResponseModels/VideoResponseModel.cs` — expor em GET
  - `src/Core/VideoProcessing.VideoManagement.Application/Models/Mappers/VideoResponseModelMapper.cs` — mapear FrameIntervalSec
  - `src/InterfacesExternas/VideoProcessing.VideoManagement.Api/Filters/OpenApiExamplesAndErrorsFilter.cs` — exemplo e documentação
- **Componentes:** UploadVideoInputModel, UploadVideoInputModelValidator, Video (Domain), VideoEntity, VideoRepository, VideoMapper, UploadVideoUseCase, VideoResponseModel, VideoResponseModelMapper, OpenAPI
- **Pacotes/Dependências:** Nenhum novo; FluentValidation e AWSSDK já utilizados

## Regra de validação (frameIntervalSec vs durationSec)
- **frameIntervalSec** opcional no input. Se informado: deve ser > 0.
- Quando **ambos** `durationSec` e `frameIntervalSec` forem informados: **frameIntervalSec não pode ser superior a 50% da duração** do vídeo.
  - Exemplo: vídeo de 60s → `frameIntervalSec` máximo = 30s.
  - Fórmula: `frameIntervalSec <= Math.Floor(durationSec * 0.5)` (ou equivalente).
- Se apenas `frameIntervalSec` for informado (sem durationSec), validar apenas que seja > 0 (a regra dos 50% só se aplica quando durationSec estiver presente).

## Dependências e Riscos (para estimativa)
- **Dependências:** Story 04 (POST /videos), Story 02 (entidade Video e DynamoDB)
- **Riscos:** Nenhum crítico; alteração aditiva em modelo e persistência

## Subtasks
- [Subtask 01: Domain — Video e VideoRehydrationData com FrameIntervalSec](./subtask/Subtask-01-Domain_Video_FrameIntervalSec.md)
- [Subtask 02: InputModel e Validator — frameIntervalSec e regra 50%](./subtask/Subtask-02-InputModel_Validator_FrameIntervalSec.md)
- [Subtask 03: Infra — VideoEntity, VideoRepository e VideoMapper](./subtask/Subtask-03-Infra_VideoEntity_Repository_Mapper_FrameIntervalSec.md)
- [Subtask 04: UseCase e Response — UploadVideoUseCase e VideoResponseModel](./subtask/Subtask-04-UseCase_Response_FrameIntervalSec.md)
- [Subtask 05: API/OpenAPI e documentação](./subtask/Subtask-05-OpenAPI_Exemplos_FrameIntervalSec.md)
- [Subtask 06: Testes unitários (validator e use case)](./subtask/Subtask-06-Testes_Unitarios_FrameIntervalSec.md)

## Critérios de Aceite da História
- [ ] `UploadVideoInputModel` possui propriedade opcional `FrameIntervalSec` (double?, segundos)
- [ ] Validação FluentValidation: quando presente, `FrameIntervalSec` > 0; quando `DurationSec` e `FrameIntervalSec` presentes, `FrameIntervalSec <= Floor(DurationSec * 0.5)` com mensagem clara
- [ ] Entidade `Video` (Domain) possui `FrameIntervalSec` e método de set (ex.: `SetFrameIntervalSec`); `VideoRehydrationData` inclui o campo
- [ ] DynamoDB: atributo `frameIntervalSec` persistido e lido em `VideoEntity` e `VideoRepository`; `VideoMapper` mapeia em ambas as direções
- [ ] `UploadVideoUseCase` propaga `FrameIntervalSec` do input para a entidade ao criar novo vídeo (e em idempotência o valor já persistido é mantido)
- [ ] `VideoResponseModel` e `VideoResponseModelMapper` expõem `FrameIntervalSec` em GET /videos e listagem
- [ ] Documentação OpenAPI/Scalar: exemplo de POST /videos inclui `frameIntervalSec`; exemplo de GET inclui o campo quando aplicável
- [ ] Comportamento idêntico na API (Kestrel) e na API hospedada no Lambda (mesmo código)
- [ ] Testes unitários: validator (casos 50%, ausência de durationSec, valor inválido); use case (valor persistido)

## Rastreamento (dev tracking)
- **Início:** 07/03/2026, às 13:24 (Brasília)
- **Fim:** —
- **Tempo total de desenvolvimento:** —
