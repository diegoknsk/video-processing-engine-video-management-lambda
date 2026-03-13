# Storie-22 — Melhorar Cobertura de Testes

**Status:** ✅ Concluída
**Data de Início:** 13/03/2026
**Data de Conclusão:** 13/03/2026
**Prioridade:** Alta

## Descrição

Como desenvolvedor, quero aumentar a cobertura de testes do projeto para que os componentes críticos tenham cobertura adequada e o quality gate do SonarCloud seja atendido.

## Critérios de Aceite

- [x] `VideosController` cobertura >= 80% (de 27%)
- [x] `UpdateVideoLambdaProxyUseCase` cobertura >= 80% (de 0%)
- [x] `VideosInternalController` cobertura >= 80% (de 0%)
- [x] `S3PresignedUrlService` cobertura >= 80% (de 46%)
- [x] `VideoMapper` cobertura >= 85% (de 77%)
- [x] `VideoRepository` cobertura >= 80% (de 72%)
- [x] `Video` entity cobertura >= 95% (de 91%)
- [x] `UploadVideoUseCase` cobertura >= 95% (de 92%)
- [x] Build passa sem erros após adição dos testes

## Escopo Técnico

**Pacotes utilizados (já presentes no projeto):**
- `xunit` 2.9.2
- `FluentAssertions` 6.12.0
- `Moq` 4.20.72

## Subtasks

- [x] [Subtask-01 — Testes VideosController completos](./subtask/Subtask-01-Testes_VideosController.md)
- [x] [Subtask-02 — Testes UpdateVideoLambdaProxyUseCase](./subtask/Subtask-02-Testes_UpdateVideoLambdaProxy.md)
- [x] [Subtask-03 — Testes VideosInternalController](./subtask/Subtask-03-Testes_VideosInternalController.md)
- [x] [Subtask-04 — Estender S3PresignedUrlService, VideoMapper, VideoRepository](./subtask/Subtask-04-Estender_Testes_Infra.md)
- [x] [Subtask-05 — Estender Video entity e UploadVideoUseCase](./subtask/Subtask-05-Estender_Testes_Domain_Application.md)

## Rastreamento (dev tracking)

- **Início:** dia 13/03/2026, às 19:13 (Brasília)
- **Fim:** dia 13/03/2026, às 19:30 (Brasília)
- **Tempo total de desenvolvimento:** 17min
