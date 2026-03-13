# Subtask-06: GET Dinâmico — progressPercent Calculado e URL Pré-assinada ZIP

## Descrição
Evoluir o `GetVideoByIdUseCase` no Lambda API para calcular o `progressPercent` dinamicamente com base na contagem de chunks processados na tabela de chunks, e retornar a URL pré-assinada S3 para download do arquivo ZIP quando disponível.

## Regras de Negócio
- `progressPercent = (chunksProcessados / parallelChunks) * 100`, arredondado para baixo, máximo 100.
- Se `parallelChunks` for `null`, `0` ou ausente: usar valor padrão `1`.
- Se o status do vídeo for `Completed`: retornar `progressPercent = 100` independentemente da contagem (regra de segurança para evitar 99% em vídeos já concluídos).
- URL pré-assinada: gerada somente se `ZipKey` e `ZipBucket` estiverem preenchidos no vídeo; TTL lido de `S3Options.PresignedUrlTtlMinutes`; se a geração falhar, retornar `ZipUrl = null` com log de warning (não falha o GET).

## Passos de Implementação
1. Injetar `IVideoChunkRepository` e `IAmazonS3` (ou port dedicada `IS3PresignedUrlService`) no `GetVideoByIdUseCase`.
2. Após buscar o vídeo principal, chamar `IVideoChunkRepository.CountProcessedAsync(videoId)` para obter a contagem de chunks processados.
3. Calcular `progressPercent` conforme as regras acima.
4. Se `video.ZipKey` e `video.ZipBucket` estiverem preenchidos, gerar URL pré-assinada S3 (`GetPreSignedURL`) com TTL de `S3Options.PresignedUrlTtlMinutes` minutos.
5. Atualizar `VideoResponseModel` para incluir:
   - `ParallelChunks` (renomeado)
   - `ZipFileName?`
   - `ZipUrl?` (URL pré-assinada)
   - `ProgressPercent` calculado (não mais o valor gravado no DynamoDB)
6. Garantir que o campo `progressPercent` gravado no DynamoDB não seja mais lido diretamente — o valor retornado no response é sempre calculado.

## Formas de Teste
1. Teste unitário: vídeo com `parallelChunks = 4`, `CountProcessedAsync` retorna `2` → `progressPercent = 50`.
2. Teste unitário: vídeo com `parallelChunks = null`, `CountProcessedAsync` retorna `1` → `progressPercent = 100` (default = 1).
3. Teste unitário: vídeo com status `Completed`, qualquer contagem → `progressPercent = 100`.
4. Teste unitário: vídeo com `ZipKey` e `ZipBucket` preenchidos → `ZipUrl` retornado não nulo.
5. Teste unitário: vídeo sem `ZipKey` → `ZipUrl = null`, sem exceção.
6. Teste unitário: `IAmazonS3.GetPreSignedURL` lança exceção → use case retorna `ZipUrl = null` e não propaga erro.

## Critérios de Aceite
- [ ] `GetVideoByIdUseCase` chama `IVideoChunkRepository.CountProcessedAsync` para calcular progresso.
- [ ] Regra `parallelChunks = null/0 → default 1` aplicada corretamente.
- [ ] Regra `status = Completed → progressPercent = 100` aplicada independente da contagem.
- [ ] `VideoResponseModel` inclui `ZipUrl?` e `ZipFileName?`.
- [ ] URL pré-assinada gerada com TTL de `S3Options.PresignedUrlTtlMinutes`.
- [ ] Falha na geração da URL pré-assinada não retorna erro HTTP — retorna `ZipUrl = null`.
- [ ] Testes unitários de todos os cenários passando.
