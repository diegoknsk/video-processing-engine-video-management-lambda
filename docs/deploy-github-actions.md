# Deploy via GitHub Actions - Documentação Completa

Este documento descreve o processo de deploy automatizado da aplicação Video Processing Auth API para AWS Lambda via GitHub Actions.

## 📋 Índice

- [Visão Geral](#visão-geral)
- [Pré-requisitos AWS](#pré-requisitos-aws)
- [Configuração GitHub](#configuração-github)
- [Variáveis de Ambiente Lambda](#variáveis-de-ambiente-lambda)
- [Como Funciona o Workflow](#como-funciona-o-workflow)
- [Execução Manual](#execução-manual)
- [Troubleshooting](#troubleshooting)

## 🎯 Visão Geral

O workflow `.github/workflows/deploy-lambda.yml` automatiza o processo de build, teste e deploy da aplicação .NET 10 para AWS Lambda. Ele é executado automaticamente em push/PR para a branch `main` e pode ser executado manualmente em qualquer branch.

### Fluxo do Processo

```
1. Checkout do código
2. Setup .NET 10
3. Restore de dependências
4. Build da solution (Release)
5. Execução dos testes unitários
6. Publish para linux-x64
7. Criação do ZIP de deployment
8. Deploy no Lambda via AWS CLI
9. Wait for Lambda update to complete
10. Verificação do deploy
11. Upload do artifact (ZIP)
```

## ☁️ Pré-requisitos AWS

### 1. Lambda Function Provisionada

A função Lambda deve estar previamente criada na AWS (via Terraform/CloudFormation/IaC):

- **Nome padrão:** `video-processing-engine-dev-auth`
- **Runtime:** `dotnet8` ou `dotnet6` com bootstrap customizado
- **Arquitetura:** x86_64
- **Handler:** Definido via IaC (ex.: `VideoProcessing.Auth.Api`)

### 2. IAM User/Role para Deploy

O workflow precisa de credenciais AWS com as seguintes permissões:

#### Permissões Necessárias (IAM Policy)

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "lambda:UpdateFunctionCode",
        "lambda:GetFunction",
        "lambda:UpdateFunctionConfiguration",
        "lambda:GetFunctionConfiguration"
      ],
      "Resource": "arn:aws:lambda:REGION:ACCOUNT_ID:function/video-processing-engine-dev-auth"
    }
  ]
}
```

**Nota:** O workflow atualiza as variáveis de ambiente do Lambda quando as GitHub Variables `COGNITO_USER_POOL_ID` e `COGNITO_CLIENT_ID` estão setadas. Por isso a policy inclui `lambda:UpdateFunctionConfiguration` e `lambda:GetFunctionConfiguration`.

#### Criar IAM User para CI/CD

```bash
# Via AWS CLI
aws iam create-user --user-name github-actions-lambda-deploy

# Anexar política (substitua ACCOUNT_ID e REGION)
aws iam attach-user-policy \
  --user-name github-actions-lambda-deploy \
  --policy-arn arn:aws:iam::ACCOUNT_ID:policy/LambdaDeployPolicy

# Criar Access Key
aws iam create-access-key --user-name github-actions-lambda-deploy
```

Guarde o `AccessKeyId` e `SecretAccessKey` retornados - serão usados nos GitHub Secrets.

### 3. Lambda - Variáveis de Ambiente

A função Lambda deve ter as seguintes variáveis de ambiente configuradas (configure manualmente via AWS Console ou IaC após o primeiro deploy):

| Variável | Descrição | Exemplo |
|----------|-----------|---------|
| `Cognito__Region` | Região AWS do Cognito User Pool | `us-east-1` |
| `Cognito__UserPoolId` | ID do Cognito User Pool | `us-east-1_XXXXXXXXX` |
| `Cognito__ClientId` | Client ID da aplicação no Cognito | `xxxxxxxxxxxxxxxxxx` |
| `ASPNETCORE_ENVIRONMENT` | Ambiente de execução (.NET) | `Production` ou `Development` |

**Exemplo de configuração via AWS CLI:**

```bash
aws lambda update-function-configuration \
  --function-name video-processing-engine-dev-auth \
  --environment "Variables={
    Cognito__Region=us-east-1,
    Cognito__UserPoolId=us-east-1_XXXXXXXXX,
    Cognito__ClientId=xxxxxxxxxxxxxxxxxx,
    ASPNETCORE_ENVIRONMENT=Production
  }"
```

**Nota:** O deploy via GitHub Actions **não sobrescreve** essas variáveis. Elas devem ser configuradas uma vez e permanecem entre deploys.

## 🔐 Configuração GitHub

### GitHub Secrets (Obrigatórios)

Configure os seguintes **Secrets** no repositório GitHub: `Settings > Secrets and variables > Actions > Secrets`

| Secret Name | Descrição | Como Obter |
|-------------|-----------|------------|
| `AWS_ACCESS_KEY_ID` | Access Key ID do IAM User (ou credenciais temporárias STS) para deploy | Criado via `aws iam create-access-key` ou obtido de sessão STS |
| `AWS_SECRET_ACCESS_KEY` | Secret Access Key correspondente | Idem |
| `AWS_SESSION_TOKEN` | Token de sessão (obrigatório quando usar credenciais temporárias/STS) | Retornado por `AssumeRole`, `GetSessionToken`, etc. |

**Região:** use a **Variable** `AWS_REGION` (ou o input manual no workflow). Ordem: input manual → variable `AWS_REGION` → `us-east-1`.

**⚠️ Segurança:**
- **Nunca** commite essas credenciais no código
- Use IAM User dedicado com permissões mínimas necessárias
- Rotacione as Access Keys periodicamente
- Considere usar OIDC (OpenID Connect) para autenticação sem credenciais (melhoria futura)

### GitHub Variables (Opcionais)

Configure as seguintes **Variables** no repositório GitHub: `Settings > Secrets and variables > Actions > Variables`

| Variable Name | Descrição | Valor Padrão | Quando Alterar |
|---------------|-----------|--------------|----------------|
| `AWS_REGION` | Região AWS do Lambda | `us-east-1` | Se o Lambda estiver em outra região |
| `LAMBDA_FUNCTION_NAME` | Nome da função Lambda | `video-processing-engine-dev-auth` | Se a função tiver nome diferente |
| `COGNITO_USER_POOL_ID` | ID do Cognito User Pool (injetado no Lambda como `Cognito__UserPoolId`) | — | Para o workflow atualizar env vars do Lambda |
| `COGNITO_CLIENT_ID` | App Client ID do Cognito (injetado no Lambda como `Cognito__ClientId`) | — | Idem |
| `GATEWAY_PATH_PREFIX` | Prefixo de path do API Gateway (ex.: `/auth`). Injetado no Lambda; ver [gateway-path-prefix.md](gateway-path-prefix.md). | — (vazio) | Quando a Lambda estiver atrás de um gateway com prefixo (ex.: rotas `/auth/*`) |

**Nota:** O workflow atualiza as variáveis de ambiente do Lambda em todo deploy: mescla Cognito (se `COGNITO_USER_POOL_ID` e `COGNITO_CLIENT_ID` estiverem configurados) e **GATEWAY_PATH_PREFIX** (valor da Variable; vazio = path inalterado). Processo completo: [processo-subida-deploy.md](./processo-subida-deploy.md).

## 🚀 Como Funciona o Workflow

### Triggers Automáticos

O workflow é executado automaticamente em:

1. **Push para `main`:** Deploy direto após merge
2. **Pull Request para `main`:** Build e testes para validação (sem deploy real*)

*Nota: Atualmente o workflow faz deploy em PRs. Considere adicionar condição `if: github.event_name == 'push'` no step de deploy para evitar deploys em PRs.

### Triggers Manuais (`workflow_dispatch`)

Você pode executar o workflow manualmente em **qualquer branch**:

1. Vá para: `Actions > Deploy Lambda Auth API > Run workflow`
2. Selecione a branch desejada
3. (Opcional) Preencha os inputs: `lambda_function_name`, `aws_region`, `gateway_path_prefix` (ex.: `/auth`)
4. Clique em `Run workflow`

**Casos de uso:**
- Deploy de hotfix de uma branch de bugfix
- Deploy de feature branch para ambiente de teste/staging
- Re-deploy após mudança manual no Lambda

### Jobs e Steps

#### Job: `build-and-deploy`

| Step | Descrição | Falhará se... |
|------|-----------|---------------|
| Checkout code | Clone do repositório | Falha de rede/permissão |
| Setup .NET | Instala .NET 10 SDK | Versão inválida |
| Restore dependencies | `dotnet restore` | Dependências quebradas/indisponíveis |
| Build solution | `dotnet build --configuration Release` | Erros de compilação |
| Run tests | `dotnet test` | Testes unitários falharem |
| Publish application | `dotnet publish` para linux-x64 | Erros de publicação |
| Create deployment package | Cria ZIP com binários publicados | Falha ao criar ZIP |
| Configure AWS credentials | Configura AWS CLI com secrets | Credenciais inválidas |
| Deploy to Lambda | `aws lambda update-function-code` | Permissões IAM, função não existe |
| Wait for update | Aguarda Lambda ficar em estado `Active` | Timeout (função não atualiza) |
| Verify deployment | Mostra informações da função (nome, última modificação, runtime, estado) | Falha de leitura (não crítico) |
| Update Lambda environment variables (Cognito + GATEWAY_PATH_PREFIX) | Mescla Cognito (se Variables setadas) e GATEWAY_PATH_PREFIX nas env vars do Lambda | Erro ao obter/atualizar configuração |
| Upload artifact | Salva ZIP como artifact do workflow | Falha de upload (não crítico) |

#### Job: `test-coverage` (Comentado - Futura)

Estrutura preparada para story futura que bloqueará PRs com cobertura de testes < 70%.

## 📦 Conteúdo do Pacote de Deploy

O ZIP de deployment (`deployment-package.zip`) contém:

```
deployment-package.zip
├── VideoProcessing.Auth.Api.dll
├── VideoProcessing.Auth.Application.dll
├── VideoProcessing.Auth.Infra.dll
├── appsettings.json
├── appsettings.Production.json
├── (dependências .NET e AWS SDK)
└── ...
```

**Tamanho típico:** ~5-15 MB comprimido

**Limites AWS Lambda:**
- Comprimido: 50 MB
- Descomprimido: 250 MB

## 🔧 Execução Manual

### Via GitHub Actions UI

1. Acesse: https://github.com/OWNER/REPO/actions/workflows/deploy-lambda.yml
2. Clique em `Run workflow`
3. Selecione a branch
4. Aguarde a conclusão

### Via GitHub CLI (`gh`)

```bash
# Deploy da branch atual
gh workflow run deploy-lambda.yml

# Deploy de branch específica
gh workflow run deploy-lambda.yml --ref feature/minha-feature

# Com input customizado
gh workflow run deploy-lambda.yml --ref develop -f branch=hotfix/critical-fix
```

## 🩺 Troubleshooting

### Erro: "Access Denied" no Deploy

**Causa:** IAM User não tem permissões suficientes

**Solução:**
1. Verifique a policy IAM anexada ao usuário
2. Confirme que o ARN da função Lambda está correto na policy
3. Teste as permissões manualmente:
   ```bash
   aws lambda get-function --function-name video-processing-engine-dev-auth
   ```

### Erro: "Function not found"

**Causa:** Lambda não existe ou nome incorreto

**Solução:**
1. Verifique o nome da função no AWS Console
2. Atualize a GitHub Variable `LAMBDA_FUNCTION_NAME`
3. Ou corrija o nome no IaC e re-provisione

### Erro: "InvalidParameterValueException: Unzipped size must be smaller than..."

**Causa:** ZIP descomprimido excede 250 MB

**Solução:**
1. Revise dependências no `.csproj`
2. Remova pacotes não usados
3. Use `--self-contained false` no publish (já configurado)
4. Considere Lambda Layers para dependências grandes

### Erro: "Tests failed" no Workflow

**Causa:** Testes unitários falharam

**Solução:**
1. Execute os testes localmente: `dotnet test`
2. Corrija as falhas
3. Commit e push novamente

### Deploy Bem-Sucedido mas Lambda Não Funciona

**Causa:** Variáveis de ambiente não configuradas no Lambda

**Solução:**
1. Verifique as variáveis via AWS Console: Lambda > Configuration > Environment variables
2. Configure `Cognito__Region`, `Cognito__UserPoolId`, `Cognito__ClientId`
3. Teste o endpoint: `curl https://API_GATEWAY_URL/health`

### Workflow Lento

**Otimizações:**
- Cache de dependências .NET (adicionar step `actions/cache`)
- Executar testes em job paralelo
- Usar `dotnet test --no-build` (já configurado)

## 📊 Monitoramento

### GitHub Actions

- **Histórico de execuções:** `Actions > Deploy Lambda Auth API`
- **Logs detalhados:** Clique em uma execução > Job > Step
- **Artifacts:** Baixe o `deployment-package.zip` de qualquer execução

### AWS Lambda

- **Logs:** CloudWatch Logs > `/aws/lambda/video-processing-engine-dev-auth`
- **Métricas:** Lambda Console > Monitor > View metrics in CloudWatch
- **Last Modified:** Console mostra data/hora do último deploy

## 🔮 Melhorias Futuras

- [ ] Adicionar job de verificação de cobertura de testes (>= 70%)
- [ ] Deploy condicional: só em push (não em PR)
- [ ] Usar OIDC em vez de Access Keys para autenticação AWS
- [ ] Cache de dependências .NET para acelerar builds
- [ ] Deploy multi-ambiente (dev/staging/prod) com matrix strategy
- [ ] Notificações de deploy (Slack/Discord)
- [ ] Rollback automático em caso de falha nos health checks

## 📞 Suporte

Em caso de problemas:
1. Verifique os logs do workflow no GitHub Actions
2. Verifique os logs do Lambda no CloudWatch
3. Consulte esta documentação
4. Entre em contato com a equipe de DevOps/Infra
