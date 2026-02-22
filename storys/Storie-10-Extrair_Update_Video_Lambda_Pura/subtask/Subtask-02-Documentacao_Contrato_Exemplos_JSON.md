# Subtask 02: Documentar contrato e exemplos de payload JSON (mínimo e completo)

## Descrição
Criar documentação do contrato de entrada da Lambda de update de vídeo com dois exemplos de body JSON: (1) exemplo mínimo (ex.: status e progressPercent) e (2) exemplo completo com todos os campos possíveis do payload, para facilitar testes manuais (Postman, AWS Lambda test event). Manter coerência com a modelagem do DynamoDB e com os campos do vídeo do projeto (VideoStatus, progressPercent 0–100, buckets/keys S3, stepExecutionArn, errorMessage, errorCode).

## Passos de Implementação
1. Criar documento em `docs/lambda-update-video-contract.md` (ou pasta da story) descrevendo o contrato: campos obrigatórios (UserId, VideoId no evento), opcionais e regras (ProgressPercent 0–100, Status como enum numérico ou string)
2. Incluir **exemplo mínimo**: JSON com apenas os campos estritamente necessários para um update válido (ex.: userId, videoId, status e progressPercent), com valores de exemplo (ex.: status = 2 para Processing, progressPercent = 50)
3. Incluir **exemplo completo**: JSON com todos os campos possíveis do payload (userId, videoId, status, progressPercent, errorMessage, errorCode, framesPrefix, s3KeyZip, s3BucketFrames, s3BucketZip, stepExecutionArn), com valores de exemplo coerentes com o domínio (VideoStatus: Pending=0, Uploading=1, Processing=2, Completed=3, Failed=4, Cancelled=5)
4. Referenciar na documentação a tabela DynamoDB e atributos utilizados (PK/SK ou modelo atual do repositório) sem alterar a infra
5. Incluir instruções de como usar os exemplos no AWS Lambda console (test event) e em ferramentas como Postman (se a borda HTTP existir no futuro)

## Formas de Teste
1. Revisar documento: exemplo mínimo contém apenas campos necessários e é válido para deserialização
2. Revisar exemplo completo: todos os campos do UpdateVideoInputModel (exceto UserId/VideoId conforme event shape) presentes com tipos corretos
3. Validar que os valores de enum (VideoStatus) estão alinhados ao código (ex.: 0–5) e que progressPercent está entre 0 e 100

## Critérios de Aceite da Subtask
- [ ] Documento de contrato criado com descrição clara do event shape
- [ ] Exemplo mínimo (ex.: status + progress) presente e válido para testes manuais
- [ ] Exemplo completo com todos os campos (status, progressPercent, errorMessage, errorCode, framesPrefix, s3KeyZip, s3BucketFrames, s3BucketZip, stepExecutionArn) presente e coerente com o domínio
- [ ] Referência à modelagem DynamoDB e campos do vídeo do projeto
- [ ] Instruções para uso em AWS Lambda test event e/ou Postman documentadas
