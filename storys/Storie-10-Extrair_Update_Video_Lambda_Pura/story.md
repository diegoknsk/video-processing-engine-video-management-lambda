# Storie-10: Extrair Update de Vídeo para Lambda Pura (VideoManagement.LambdaUpdateVideo)

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** —

## Descrição
Como arquiteto do Video Processing Engine, quero extrair a responsabilidade de atualização de estado/metadados de vídeo (hoje no PATCH do VideoManagement) para uma nova Lambda pura (.NET handler padrão, sem AddAWSLambdaHosting), para que o update possa ser invocado diretamente (API Gateway futura, SQS, chamada direta) e o VideoManagement deixe de implementar o PATCH ou apenas encaminhe para essa Lambda, mantendo o mesmo contrato para clientes e evitando breaking change.

## Objetivo
Criar um novo projeto/lambda **VideoProcessing.VideoManagement.LambdaUpdateVideo** que receba o mesmo input do PATCH atual (update parcial), permita alterar todos os campos que o PATCH permite hoje (UserId, Status, ProgressPercent, ErrorMessage, ErrorCode, FramesPrefix, S3KeyZip, S3BucketFrames, S3BucketZip, StepExecutionArn), persista no DynamoDB com a mesma modelagem e regras condicionais; documentar o contrato de entrada (event shape) e exemplos de payload JSON (mínimo e completo); e adaptar o VideoManagement: remover a implementação do PATCH e/ou encaminhar a chamada para a nova Lambda, de forma que o comportamento esperado e os impactos fiquem claros.

**Sugestão de nome do projeto:** `VideoProcessing.VideoManagement.LambdaUpdateVideo`. Justificativa: mantém o prefixo de domínio (VideoManagement), deixa explícito que é a Lambda de update de vídeo e alinha com o padrão do repositório (VideoProcessing.VideoManagement.*). Alternativa considerada: `VideoProcessing.LambdaUpdateVideo` (mais curto, mas menos explícito no contexto do módulo).

## Escopo Técnico
- **Tecnologias:** .NET 10, AWS Lambda (handler padrão, sem AddAWSLambdaHosting), DynamoDB (mesma tabela e modelo do VideoManagement). **Contrato:** reutilização de `UpdateVideoInputModel` (Application); evento da Lambda = `UpdateVideoLambdaEvent` (estende InputModel com `videoId`).
- **Arquivos/projetos:**
  - Projeto: `src/VideoProcessing.VideoManagement.LambdaUpdateVideo/` — apenas **borda** (Function, Handler, evento/saída). Lógica de negócio: **mesmo Use Case** (`IUpdateVideoUseCase`) e mesmo validator (`UpdateVideoInputModelValidator`) da Application; sem duplicação de regras.
  - Documentação do contrato (event shape = InputModel + videoId) e exemplos JSON em `docs/lambda-update-video-contract.md`
  - VideoManagement: PATCH mantido como proxy que invoca a Lambda (mesmo contrato para o cliente).
- **Componentes:** Lambda Function handler, `UpdateVideoLambdaEvent` (interface externa), `UpdateVideoLambdaResponse`; Use Case e validação compartilhados com a API.
- **Pacotes/Dependências:** AWS Lambda Core, Application/Infra referenciados; AWSSDK.Lambda apenas na API (proxy).

## Dependências e Riscos (para estimativa)
- **Dependências:** Storie-02 (modelo DynamoDB e repositório), Storie-06 (contrato PATCH e UpdateVideoInputModel), solução atual do VideoManagement compilando e testada
- **Riscos:**
  - Quebra de contrato para clientes que hoje chamam PATCH no VideoManagement: mitigar definindo claramente se o PATCH será removido (e clientes passam a chamar a nova Lambda/rota) ou se o VideoManagement mantém um proxy para a Lambda (sem breaking change)
  - Permissões IAM da nova Lambda: necessidade de acesso à mesma tabela DynamoDB do VideoManagement; não presumir permissões além do necessário (leitura/escrita na tabela de vídeos)
  - Duplicação de código (Domain/Application): avaliar referência a projetos compartilhados vs. cópia do mínimo necessário para o handler

## Subtasks
- [Subtask 01: Criar projeto Lambda pura e contrato de entrada (event shape)](./subtask/Subtask-01-Projeto_Lambda_Contrato_Event.md)
- [Subtask 02: Documentar contrato e exemplos de payload JSON (mínimo e completo)](./subtask/Subtask-02-Documentacao_Contrato_Exemplos_JSON.md)
- [Subtask 03: Implementar handler e persistência DynamoDB (update condicional)](./subtask/Subtask-03-Handler_Persistencia_DynamoDB.md)
- [Subtask 04: Validação de payload e testes unitários (contrato e handler)](./subtask/Subtask-04-Validacao_Payload_Testes_Unitarios.md)
- [Subtask 05: Adaptar VideoManagement (remover ou encaminhar PATCH)](./subtask/Subtask-05-Adaptar_VideoManagement_PATCH.md)

## Exemplos de payload (contrato da Lambda)

Para testes manuais (Postman, AWS Lambda test event). Campos obrigatórios: `userId`, `videoId` (no evento); pelo menos um campo de atualização além de `userId`. Enum `VideoStatus`: Pending=0, Uploading=1, Processing=2, Completed=3, Failed=4, Cancelled=5.

**Exemplo mínimo (status e progress):**
```json
{
  "videoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "status": 2,
  "progressPercent": 50
}
```

**Exemplo completo (todos os campos possíveis do payload):**
```json
{
  "videoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "status": 3,
  "progressPercent": 100,
  "errorMessage": null,
  "errorCode": null,
  "framesPrefix": "videos/7c9e6679-7425-40de-944b-e07fc1f90ae7/3fa85f64-5717-4562-b3fc-2c963f66afa6/frames/",
  "s3KeyZip": "videos/7c9e6679-7425-40de-944b-e07fc1f90ae7/3fa85f64-5717-4562-b3fc-2c963f66afa6/out.zip",
  "s3BucketFrames": "my-bucket-frames",
  "s3BucketZip": "my-bucket-zip",
  "stepExecutionArn": "arn:aws:states:us-east-1:123456789012:execution:MyStateMachine:exec-123"
}
```

## Critérios de Aceite da História
- [ ] Novo projeto **VideoProcessing.VideoManagement.LambdaUpdateVideo** criado, compilando, sem uso de AddAWSLambdaHosting (handler padrão .NET para Lambda)
- [ ] Contrato de entrada da Lambda documentado (event shape): como o evento é recebido (ex.: invocação direta com `videoId` + body igual ao PATCH atual; ou wrapper para API Gateway/SQS) e onde cada campo do `UpdateVideoInputModel` é mapeado (UserId, Status, ProgressPercent, ErrorMessage, ErrorCode, FramesPrefix, S3KeyZip, S3BucketFrames, S3BucketZip, StepExecutionArn)
- [ ] Documentação inclui **dois exemplos de body JSON**: (1) exemplo mínimo (ex.: status e progressPercent) e (2) exemplo completo (todos os campos possíveis do payload), utilizáveis em testes manuais (Postman, AWS Lambda test event)
- [ ] Campos e modelagem alinhados ao DynamoDB e ao domínio do projeto (VideoStatus, progressPercent 0–100, buckets/keys S3, stepExecutionArn, errorMessage, errorCode)
- [ ] Lambda executa update no DynamoDB com as mesmas regras condicionais/idempotentes que o UseCase atual (ownership, progressão monotônica, transições de status); retorno adequado (sucesso com vídeo atualizado ou erro com código/mensagem)
- [ ] VideoManagement: PATCH removido **ou** substituído por chamada à nova Lambda (proxy); comportamento esperado e impacto para clientes documentados (evitar breaking change se optar por proxy)
- [ ] Testes unitários mínimos: validação de payload/contrato e handler (mock de repositório); cobertura ≥ 80% para o handler/use case da nova Lambda
- [ ] `dotnet build` e `dotnet test` passam na solução (incluindo o novo projeto)
- [ ] Formas de invocação da Lambda documentadas (chamada direta, API Gateway route futura, SQS) sem implementar a borda se ainda não existir

## Rastreamento (dev tracking)
- **Início:** 22/02/2026, às 20:17 (Brasília)
- **Fim:** —
- **Tempo total de desenvolvimento:** —
