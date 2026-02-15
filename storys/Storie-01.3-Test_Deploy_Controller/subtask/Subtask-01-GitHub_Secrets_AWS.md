# Subtask 01: Configurar GitHub Secrets (AWS_ACCESS_KEY_ID, AWS_SECRET_ACCESS_KEY, AWS_SESSION_TOKEN)

## Descrição
Configurar no repositório GitHub os secrets necessários para o workflow de deploy autenticar na AWS: Access Key ID, Secret Access Key e, quando for o caso de credenciais temporárias (ex.: SSO, assumed role), o Session Token. O workflow da Storie-07 utiliza esses secrets no step de "Configure AWS credentials".

## Passos de Implementação
1. Acessar o repositório no GitHub → Settings → Secrets and variables → Actions.
2. Criar secret **AWS_ACCESS_KEY_ID** com o valor da Access Key da IAM (usuário ou role temporária).
3. Criar secret **AWS_SECRET_ACCESS_KEY** com o valor da Secret Access Key correspondente.
4. Se estiver usando credenciais temporárias (ex.: `aws sso login` + export de session): criar secret **AWS_SESSION_TOKEN** com o valor do token; caso use usuário IAM permanente, esse secret pode ser omitido se o workflow estiver preparado para uso opcional.
5. Garantir que o workflow `.github/workflows/deploy-lambda-video-management.yml` referencia esses secrets (ex.: `aws-actions/configure-aws-credentials` com `aws-access-key-id`, `aws-secret-access-key`, `aws-session-token`).

## Formas de Teste
1. Verificar em Settings → Secrets and variables → Actions que os secrets aparecem (valores mascarados).
2. Executar o workflow manualmente; o step "Configure AWS credentials" deve concluir sem erro de autenticação.
3. Em caso de credenciais temporárias: renovar token e atualizar secret AWS_SESSION_TOKEN quando expirar.

## Critérios de Aceite da Subtask
- [ ] Secret **AWS_ACCESS_KEY_ID** configurado no repositório.
- [ ] Secret **AWS_SECRET_ACCESS_KEY** configurado no repositório.
- [ ] Secret **AWS_SESSION_TOKEN** configurado quando forem usadas credenciais temporárias; ou documentado que não é necessário para usuário IAM permanente.
- [ ] Workflow de deploy utiliza esses secrets no step de configuração da AWS e o step conclui com sucesso.
