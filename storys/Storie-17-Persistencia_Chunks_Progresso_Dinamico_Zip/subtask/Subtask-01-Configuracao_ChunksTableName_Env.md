# Subtask-01: Configuração ChunksTableName por Variável de Ambiente

## Descrição
Adicionar suporte à configuração do nome da tabela de chunks DynamoDB via variável de ambiente `DynamoDB__ChunksTableName`, propagando o valor pelos arquivos de configuração da aplicação e pelos dois pipelines de deploy do GitHub Actions.

## Passos de Implementação
1. Adicionar propriedade `ChunksTableName` em `DynamoDbOptions` com valor default `"video-processing-engine-dev-video-chunks"`.
2. Adicionar o campo `ChunksTableName` no `appsettings.json` (valor prod) e `appsettings.Development.json` (valor dev) de cada entrypoint que usar a tabela.
3. Atualizar `deploy-lambda-video-management.yml`: adicionar `DynamoDB__ChunksTableName=${{ vars.DYNAMODB_CHUNKS_TABLE_NAME }}` na etapa `update-function-configuration`.
4. Atualizar `deploy-lambda-update-video.yml`: idem, incluir a mesma variável na configuração da Lambda.
5. Garantir que `ServiceCollectionExtensions` registra corretamente o `DynamoDbOptions` via `IOptions<DynamoDbOptions>` — verificar se a nova propriedade é resolvida sem alteração estrutural adicional no DI.

## Formas de Teste
1. Build local: verificar que `dotnet build` passa sem erros em todos os projetos.
2. Verificação manual: inspecionar `DynamoDbOptions` com o campo `ChunksTableName` preenchido ao startar a aplicação em modo Development.
3. Teste unitário: instanciar `DynamoDbOptions` sem setar `ChunksTableName` e validar que o valor default é `"video-processing-engine-dev-video-chunks"`.
4. Validação do pipeline: confirmar que o YAML dos dois workflows contém `DynamoDB__ChunksTableName` na etapa de configuração da Lambda.

## Critérios de Aceite
- [ ] `DynamoDbOptions.ChunksTableName` existe com default `"video-processing-engine-dev-video-chunks"`.
- [ ] Variável `DynamoDB__ChunksTableName` está presente na etapa `update-function-configuration` em ambos os workflows.
- [ ] `dotnet build` passa sem erros após a alteração.
- [ ] Nenhum nome de tabela hardcoded no código para a tabela de chunks.
