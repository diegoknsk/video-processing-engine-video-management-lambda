# Subtask 01: Definir entidade Video (Domain) e enums (VideoStatus, ProcessingMode)

## Descrição
Criar a entidade agregado `Video` no Domain com todos os campos obrigatórios e recomendados (incluindo fan-out/fan-in), definir enums `VideoStatus` e `ProcessingMode`, garantir imutabilidade (record ou propriedades init-only), validações básicas no construtor (videoId/userId não nulos, progressPercent 0-100), e documentar cada campo com comentários XML.

## Passos de Implementação
1. **Criar enum VideoStatus**: Pending (cadastrado aguardando upload), Uploading (upload em andamento), Processing (processamento iniciado), Completed (sucesso), Failed (erro), Cancelled (cancelado pelo usuário ou timeout)
2. **Criar enum ProcessingMode**: SINGLE_LAMBDA (modo simples), FANOUT (processamento paralelo com chunks)
3. **Criar entidade Video como record**: campos obrigatórios (videoId, userId, originalFileName, contentType, sizeBytes, durationSec?, createdAt, updatedAt, status, progressPercent, s3BucketVideo, s3KeyVideo, s3BucketFrames?, framesPrefix?, s3BucketZip?, s3KeyZip?, stepExecutionArn?, errorMessage?, errorCode?) e campos recomendados (processingMode?, chunkCount?, chunkDurationSec?, uploadIssuedAt?, uploadUrlExpiresAt?, framesProcessed?, finalizedAt?)
4. **Adicionar validações no construtor**: lançar ArgumentException se videoId/userId vazios, progressPercent fora de 0-100, originalFileName vazio
5. **Documentar campos com XML comments**: descrever propósito de cada campo, especialmente os de fan-out/fan-in
6. **Criar método factory opcional**: `Video.Create(userId, originalFileName, contentType, sizeBytes, durationSec)` que gera videoId (Guid.NewGuid), define status Pending, progressPercent 0, timestamps, etc.

## Formas de Teste
1. **Instanciação válida**: criar Video com dados válidos, validar que propriedades são acessíveis
2. **Validação**: tentar criar Video com videoId vazio ou progressPercent = 150, validar que lança ArgumentException
3. **Factory method**: chamar Video.Create, validar que videoId é Guid válido, status é Pending, progressPercent é 0

## Critérios de Aceite da Subtask
- [ ] Enum `VideoStatus` criado com 6 valores (Pending, Uploading, Processing, Completed, Failed, Cancelled)
- [ ] Enum `ProcessingMode` criado com 2 valores (SINGLE_LAMBDA, FANOUT)
- [ ] Record `Video` criado com todos os campos obrigatórios e recomendados (total ~25 propriedades)
- [ ] Validações no construtor: videoId/userId não nulos, progressPercent 0-100, originalFileName não vazio
- [ ] XML comments em todas as propriedades públicas
- [ ] Método factory `Video.Create` implementado (opcional mas recomendado)
- [ ] Teste unitário valida instanciação válida, validação de campos e factory method
