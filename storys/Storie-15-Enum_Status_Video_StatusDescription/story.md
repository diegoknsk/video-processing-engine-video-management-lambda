# Storie-15: Novo enum VideoStatus e statusDescription na API

## Status
- **Estado:** ✅ Concluída
- **Data de Conclusão:** 08/03/2026

## Descrição
Como desenvolvedor, quero que o enum de status do vídeo reflita o fluxo real do pipeline (UploadPending, ProcessingImages, GeneratingZip, Completed, Failed, Cancelled) e que a API exponha também uma descrição amigável (`statusDescription`) em listagem e detalhe, para melhor UX e alinhamento com o processamento.

## Objetivo (resumido)
- Substituir enum antigo (Pending, Uploading, Processing, …) pelo novo: **UploadPending**, **ProcessingImages**, **GeneratingZip**, **Completed**, **Failed**, **Cancelled**.
- Adicionar **statusDescription** ao model de saída (VideoResponseModel) em todos os pontos que retornam vídeo (listagem, detalhe, Lambda Update Video).
- Atualizar Domain, Application, Infra, Lambda Update Video, contratos e testes.

## Escopo Técnico (resumido)
- **Arquivos:** `VideoStatus.cs`, `Video.cs`, `VideoResponseModel.cs`, `VideoResponseModelMapper.cs`, `VideoMapper.cs`, `VideoEntity` (persistência string), `VideoStatusExtensions.cs` (novo), docs e testes.
- **Lambda Update Video:** contrato e exemplos com novos valores de status.

## Subtasks
- [Subtask 01: Enum VideoStatus e descrição amigável](./subtask/Subtask-01-Enum_Status_Extensions.md)
- [Subtask 02: ResponseModel, mapper, Lambda e testes](./subtask/Subtask-02-Response_Lambda_Testes.md)

## Critérios de Aceite
- [x] Enum `VideoStatus` com: UploadPending, ProcessingImages, GeneratingZip, Completed, Failed, Cancelled.
- [x] `VideoResponseModel` expõe `status` e `statusDescription`; mapper preenche descrição via extensão.
- [x] Entidade `Video` usa estado inicial `UploadPending`; persistência e Lambda Update Video usam novos valores.
- [x] Documentação (lambda-update-video-contract) e testes atualizados; build e testes passando.

## Rastreamento (dev tracking)
- **Início:** — (story criada após implementação)
- **Fim:** —
- **Tempo total de desenvolvimento:** —
