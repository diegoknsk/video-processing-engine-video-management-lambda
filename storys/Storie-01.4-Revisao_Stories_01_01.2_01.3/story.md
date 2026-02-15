# Storie-01.4: Revisão das Stories 01, 01.2 e 01.3

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** —

## Descrição
Como arquiteto/desenvolvedor, quero revisar tudo o que foi feito nas stories 01, 01.2 e 01.3 (e artefatos relacionados), para garantir que está correto, que as regras do projeto foram seguidas e que não há artefatos ou configurações estranhas ou incorretas, mesmo que o deploy esteja funcionando.

## Objetivo
Realizar uma revisão técnica completa do que foi entregue até aqui (bootstrap, health/gateway, deploy via GitHub), documentar findings (o que está correto, o que está estranho ou faltando) e listar itens para correção em stories/follow-ups, sem alterar código nesta story — apenas revisar e documentar.

## Escopo Técnico
- **Tecnologias:** N/A (revisão de documentação, código, workflow e convenções)
- **Arquivos revisados:** Program.cs, workflow deploy, docs, .gitignore, stories 01/01.2/01.3, artefatos no repositório (publish_dry_run, build_log.txt, etc.)
- **Entregável:** Este story.md contém a seção **Resultado da Revisão** com todos os achados; subtasks cobrem cada área revisada.

## Dependências e Riscos (para estimativa)
- **Dependências:** Stories 01, 01.2 e 01.3 (já executadas)
- **Riscos:** Nenhum (revisão não altera código)

---

## Resultado da Revisão (Findings)

### 1. Artefatos no repositório (estranhos ou desnecessários)

| Item | O que é | Problema | Recomendação |
|------|---------|----------|--------------|
| **publish_dry_run/** | Pasta de saída de um `dotnet publish` (provavelmente com `--output ./publish_dry_run`) usado como “dry run” para testar o publish sem sobrescrever a pasta `./publish` usada no CI. | Não deveria estar versionada; é artefato de build. | Adicionar `publish/` e `publish_dry_run/` ao `.gitignore` e remover a pasta do controle de versão (ou não commitar). |
| **build_log.txt**, **build_output.txt**, **test_output.txt**, **test_output_ascii.txt** | Logs de saída de comandos como `dotnet build` ou `dotnet test` redirecionados para arquivos (ex.: `dotnet build > build_output.txt`). | São logs temporários e não devem fazer parte do repositório. | Adicionar ao `.gitignore` (ex.: `build_output.txt`, `test_output.txt`, `*_log.txt` ou padrão equivalente) e não commitar esses arquivos. |

**Resumo:** “Dry run” aqui significa apenas um publish de teste para outra pasta; os arquivos `.txt` são logs de build/test. Nenhum deles deve ser versionado.

---

### 2. Program.cs e Lambda Hosting

| Item | Situação | Problema | Recomendação |
|------|----------|----------|--------------|
| **AddAWSLambdaHosting** | **Removido** do Program.cs. Comentário no código: *"AWS Lambda Hosting removed temporarily for diagnosis/compatibility on .NET 10"*. | Para rodar no AWS Lambda com API Gateway HTTP API, a aplicação precisa de `AddAWSLambdaHosting(LambdaEventSource.HttpApi)`. Sem isso, o entry point da Lambda não é o esperado e o deploy pode falhar ou se sustentar por uma versão anterior. | Reavaliar compatibilidade .NET 10 e **reintroduzir** `builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);` antes de considerar o deploy correto. |
| **Ordem do pipeline** | `GatewayPathBaseMiddleware` está registrado antes de `MapControllers()`; não há `UseRouting()` explícito. | O skill lambda-api-hosting recomenda middleware de path **antes** de `UseRouting()`. Em ASP.NET Core moderno, o roteamento pode ser implícito com `MapControllers()`, então a ordem atual pode funcionar, mas o skill sugere `UseRouting()` explícito para clareza. | Opcional: adicionar `app.UseRouting();` após o middleware e antes de `MapControllers()` para alinhar à documentação. |

---

### 3. Workflow de deploy (`.github/workflows/deploy-lambda-video-management.yml`)

| Item | Situação | Problema | Recomendação |
|------|----------|----------|--------------|
| **GATEWAY_STAGE** | O step "Update Lambda configuration" envia várias variáveis de ambiente para a Lambda (DynamoDB, S3, Cognito, GATEWAY_PATH_PREFIX, etc.), mas **não envia `GATEWAY_STAGE`**. | Stories 01.2 e 01.3 e a documentação (`docs/gateway-path-prefix.md`) exigem `GATEWAY_STAGE` quando o API Gateway usa stage nomeado (ex.: `default`). Sem essa variável, o middleware não remove o segmento do stage do path e pode ocorrer 404 em URLs como `.../default/videos/health`. | Incluir `GATEWAY_STAGE` nas variáveis de ambiente do workflow (ex.: a partir de `vars.GATEWAY_STAGE`), de forma análoga a `GATEWAY_PATH_PREFIX`. |
| **Handler** | Workflow usa `--handler VideoProcessing.VideoManagement.Api`. | Está alinhado ao skill (Handler = nome do assembly). Nenhuma alteração necessária desde que o assembly da API seja esse. | Manter. |
| **Build, test, publish, zip** | Fluxo: restore → build → test → publish (linux-x64) → zip. | Coerente com Storie-07 e documentação. | OK. |

---

### 4. Consistência das stories (01, 01.2, 01.3 e 07)

| Item | Situação | Problema | Recomendação |
|------|----------|----------|--------------|
| **Storie-01** | Status ainda **"Em desenvolvimento"**. | 01.2 e 01.3 dependem da 01 e estão marcadas como concluídas; há inconsistência de estado. | Quando o bootstrap estiver realmente fechado (incluindo AddAWSLambdaHosting e critérios de aceite), marcar Storie-01 como **Concluída** e preencher data e dev tracking. |
| **Storie-01.3** | Depende do workflow da **Storie-07**. Storie-07 continua **"Em desenvolvimento"**, mas o workflow já existe e foi usado na 01.3. | Dependência circular ou antecipada: 01.3 foi executada usando um workflow que formalmente pertence à 07 ainda não concluída. | Alinhar: ou concluir Storie-07 (workflow + docs + critérios) e então manter 01.3 como está, ou documentar na 01.3 que o workflow foi usado “em prévia” da 07. |
| **Storie-01.3 – Dev tracking** | Campos **Início**, **Fim** e **Tempo total** estão vazios. | Dificulta métricas e rastreabilidade. | Preencher quando houver registro (ou marcar "N/A" se não for recuperável). |

---

### 5. .gitignore e convenções

| Item | Situação | Recomendação |
|------|----------|--------------|
| **Pastas de publish** | `.gitignore` não ignora `publish/` nem `publish_dry_run/`. | Incluir `publish/` e `publish_dry_run/` para evitar commit de artefatos de publish. |
| **Logs de build/test** | Existe `[Bb]uild[Ll]og.*`, que cobre padrões como `BuildLog.*`, mas não `build_log.txt` ou `build_output.txt`. | Incluir entradas como `build_log.txt`, `build_output.txt`, `test_output.txt`, `test_output_ascii.txt` ou um padrão genérico (ex.: `*_output.txt`) para evitar versionar logs de build/test. |

---

### 6. Documentação e código (resumo)

- **docs/deploy-video-management-lambda.md:** Adequada; descreve secrets, variables, fluxo e troubleshooting.
- **README.md:** Contém seção Deploy e link para a documentação de deploy.
- **DynamoDbOptions.cs:** Record com `[Required]` em TableName e Region; adequado.
- **GatewayPathBaseMiddleware e Health:** Implementação e testes existentes; stories 01.2/01.3 referem-se corretamente a GATEWAY_PATH_PREFIX e GATEWAY_STAGE.

---

### 7. Resumo executivo

- **O que está correto:** Workflow de deploy (fluxo build/test/publish/zip/deploy), Handler, documentação de deploy, middleware de gateway, endpoint /health, uso de variáveis no workflow (exceto GATEWAY_STAGE), DynamoDbOptions e README.
- **O que deve ser corrigido ou ajustado:**  
  1) Reintroduzir **AddAWSLambdaHosting** no Program.cs (ou justificar e documentar se permanecer removido).  
  2) Incluir **GATEWAY_STAGE** nas variáveis de ambiente da Lambda no workflow.  
  3) **.gitignore:** adicionar `publish/`, `publish_dry_run/` e logs de build/test.  
  4) Remover ou não commitar: **publish_dry_run/**, **build_log.txt**, **build_output.txt**, **test_output.txt**, **test_output_ascii.txt**.  
  5) Alinhar status e dependências das **Stories 01 e 07** com o que foi realmente entregue; preencher dev tracking da **01.3** quando possível.

---

## Subtasks
- [x] [Subtask 01: Revisar artefatos no repositório (dry run, build txt, .gitignore)](./subtask/Subtask-01-Artefatos_Repositorio_Gitignore.md)
- [x] [Subtask 02: Revisar Program.cs e AddAWSLambdaHosting](./subtask/Subtask-02-Program_AddAWSLambdaHosting.md)
- [x] [Subtask 03: Revisar workflow de deploy e variável GATEWAY_STAGE](./subtask/Subtask-03-Workflow_GATEWAY_STAGE.md)
- [x] [Subtask 04: Revisar consistência das stories 01, 01.2, 01.3 e 07](./subtask/Subtask-04-Consistencia_Stories.md)
- [x] [Subtask 05: Documentar conclusões e lista de correções](./subtask/Subtask-05-Documentar_Conclusoes_Correcoes.md)

## Critérios de Aceite da História
- [x] Revisão documentada neste story.md (seção Resultado da Revisão) cobrindo artefatos, Program.cs, workflow, stories e .gitignore
- [x] Explicação do que é "dry run" e dos arquivos build/test .txt disponível na revisão
- [x] Lista clara do que está correto e do que deve ser corrigido (resumo executivo)
- [x] Subtasks criadas para cada área revisada; conclusões permitem priorizar correções em follow-ups
- [x] Nenhuma alteração de código ou configuração nesta story — apenas revisão e documentação

## Rastreamento (dev tracking)
- **Início:** 15/02/2026, às 15:45 (Brasília)
- **Fim:** —
- **Tempo total de desenvolvimento:** —
