# Subtask 01: Criar projeto Lambda pura e contrato de entrada (event shape)

## Descrição
Criar o novo projeto .NET 10 para a Lambda de update de vídeo (VideoProcessing.VideoManagement.LambdaUpdateVideo) como Lambda pura: handler padrão .NET (Function.cs com método Handler), sem AddAWSLambdaHosting. Definir o contrato de entrada (event shape) que a Lambda receberá: para invocação direta, um wrapper que inclua `videoId` (Guid) e um body com os mesmos campos do `UpdateVideoInputModel` (UserId, Status, ProgressPercent, ErrorMessage, ErrorCode, FramesPrefix, S3KeyZip, S3BucketFrames, S3BucketZip, StepExecutionArn), de forma a manter compatibilidade com o PATCH atual.

## Passos de Implementação
1. Criar pasta `src/VideoProcessing.VideoManagement.LambdaUpdateVideo/` e projeto de tipo AWS Lambda (.NET 10), configurado para handler padrão (não Minimal API / AddAWSLambdaHosting)
2. Definir tipo de evento que **reutiliza** `UpdateVideoInputModel`: record `UpdateVideoLambdaEvent` que estende `UpdateVideoInputModel` e adiciona apenas `VideoId` (Guid), para deserialização do JSON do evento sem duplicar propriedades
3. Documentar no código ou em doc o event shape: formato esperado para invocação direta (ex.: `{ "videoId": "guid", "userId": "guid", "status": 2, "progressPercent": 50, ... }`) e, se aplicável, como seria mapeado a partir de API Gateway (body + path parameter) ou SQS (message body)
4. Adicionar o projeto à solução (.sln) e referências mínimas (Domain ou contratos compartilhados) para tipos VideoStatus e nomes de campos alinhados ao DynamoDB

## Formas de Teste
1. Compilar o novo projeto com `dotnet build` sem erros
2. Executar um evento de teste local (invoke local) com JSON mínimo e validar que o handler é invocado (stub retornando OK)
3. Validar que o evento JSON é deserializado corretamente para o tipo de entrada (UserId, Status, ProgressPercent preenchidos conforme JSON)

## Critérios de Aceite da Subtask
- [ ] Projeto VideoProcessing.VideoManagement.LambdaUpdateVideo criado e incluído na solução
- [ ] Lambda configurada como handler padrão .NET (sem AddAWSLambdaHosting)
- [ ] Contrato de entrada (event shape) definido e documentado com todos os campos do update (UserId obrigatório; demais opcionais)
- [ ] Event shape documentado para invocação direta e mencionado para API Gateway/SQS (sem implementar borda)
- [ ] Build da solução passa incluindo o novo projeto
