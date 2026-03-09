# Subtask 03: Domain — timestamps automáticos nas transições de status e logs estruturados

## Descrição
Alterar a lógica de transição de status na entidade `Video` para preencher automaticamente os timestamps do pipeline conforme as regras de transição. Adicionar logging estruturado no `UpdateVideoUseCase` para registrar toda mudança de status com `videoId`, `previousStatus` e `newStatus`, garantindo visibilidade no CloudWatch.

## Passos de Implementação
1. Alterar o método `UpdateStatus(VideoStatus status)` na entidade `Video` para, após validar a transição, preencher automaticamente:
   - Se status anterior era `ProcessingImages` e novo é `GeneratingZip` → atribuir `ImagesProcessingCompletedAt = DateTime.UtcNow`
   - Se status anterior era `GeneratingZip` e novo é `Completed` → atribuir `ProcessingCompletedAt = DateTime.UtcNow`
   - Se novo status é `Failed` → atribuir `LastFailedAt = DateTime.UtcNow`
   - Se novo status é `Cancelled` → atribuir `LastCancelledAt = DateTime.UtcNow`
2. Considerar extrair a lógica de preenchimento de timestamps em um método privado `ApplyTransitionTimestamps(VideoStatus previousStatus, VideoStatus newStatus)` para manter `UpdateStatus` limpo.
3. Atualizar `MarkAsFailed` para também preencher `LastFailedAt = DateTime.UtcNow`.
4. Atualizar `MarkAsCompleted` para preencher `ProcessingCompletedAt = DateTime.UtcNow`.
5. No `UpdateVideoUseCase`: injetar `ILogger<UpdateVideoUseCase>` via construtor primário. Antes de chamar `Video.FromMerge`, capturar o status anterior (`video.Status`). Após o merge e persistência, se o status mudou, registrar log estruturado:
   ```
   logger.LogInformation("Video status changed — {@StatusChange}",
       new { VideoId = videoId, PreviousStatus = previousStatus.ToString(), NewStatus = newStatus.ToString() });
   ```
6. Garantir que o log use structured logging (template com placeholder, não interpolação) para que os campos apareçam como propriedades no CloudWatch.

## Formas de Teste
1. Teste unitário: criar Video com status `ProcessingImages`, chamar `UpdateStatus(GeneratingZip)`; verificar que `ImagesProcessingCompletedAt` não é `null` e está próximo de `DateTime.UtcNow`.
2. Teste unitário: criar Video com status `GeneratingZip`, chamar `UpdateStatus(Completed)`; verificar que `ProcessingCompletedAt` foi preenchido.
3. Teste unitário: chamar `MarkAsFailed("erro")`; verificar que `LastFailedAt` foi preenchido.
4. Teste unitário: chamar `UpdateStatus(Cancelled)`; verificar que `LastCancelledAt` foi preenchido.
5. Teste unitário: criar Video com status `UploadPending`, chamar `UpdateStatus(ProcessingImages)`; verificar que nenhum timestamp de transição foi preenchido (apenas `UpdatedAt` mudou).
6. Teste do UseCase (com mock de ILogger): verificar que `LogInformation` é chamado quando o status muda; verificar que **não** é chamado quando o status permanece o mesmo.

## Critérios de Aceite da Subtask
- [ ] `UpdateStatus` preenche `ImagesProcessingCompletedAt` na transição `ProcessingImages → GeneratingZip`.
- [ ] `UpdateStatus` preenche `ProcessingCompletedAt` na transição `GeneratingZip → Completed`.
- [ ] Entrada em `Failed` (via `UpdateStatus` ou `MarkAsFailed`) preenche `LastFailedAt`.
- [ ] Entrada em `Cancelled` preenche `LastCancelledAt`.
- [ ] Transições que não se encaixam nas regras acima não preenchem nenhum timestamp extra.
- [ ] `UpdateVideoUseCase` registra log estruturado com `videoId`, `previousStatus` e `newStatus` em toda mudança de status.
- [ ] Log usa structured logging (template + objeto, não interpolação de string).
