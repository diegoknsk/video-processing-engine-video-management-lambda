# Subtask 01: Enum VideoStatus e extensão de descrição amigável

## Objetivo
Definir o novo enum `VideoStatus` (UploadPending, ProcessingImages, GeneratingZip, Completed, Failed, Cancelled) no Domain e criar extensão em Application para descrição amigável (ex.: "Aguardando upload", "Processando imagens").

## Entregas
- [x] `Domain/Enums/VideoStatus.cs`: novos valores com numeração explícita 0–5.
- [x] `Application/Extensions/VideoStatusExtensions.cs`: método `ToFriendlyName()` para cada status.
- [x] Entidade `Video`: estado inicial do novo vídeo = `UploadPending`.
