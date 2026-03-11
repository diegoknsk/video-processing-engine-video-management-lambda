# Subtask 02: VideoResponseModel statusDescription, Lambda e testes

## Objetivo
Expor `statusDescription` na API (listagem e detalhe), atualizar Lambda Update Video (contrato e fluxo) e ajustar todos os testes que referenciam status.

## Entregas
- [x] `VideoResponseModel`: propriedade `StatusDescription`; `VideoResponseModelMapper` preenche via `ToFriendlyName()`.
- [x] Contrato e exemplos em `docs/lambda-update-video-contract.md` com novo enum.
- [x] Testes: VideoMapper, VideoRepository, UpdateVideoHandler, UpdateVideoUseCase, validators, adapter, deserialization — status atualizados para novos valores; asserção de `StatusDescription` onde aplicável.
