# Subtask-03: Configurar Secrets e Variables no GitHub e criar projeto SonarCloud

## Descrição
Configuração manual (fora do código) necessária para que o job `sonar-analysis` funcione: criar o projeto no SonarCloud, obter o token de autenticação, desativar Automatic Analysis e registrar as credenciais no repositório GitHub.

## Passos de implementação

### No SonarCloud (sonarcloud.io)

1. **Criar o projeto:** acessar [sonarcloud.io](https://sonarcloud.io), fazer login com a conta GitHub, ir em **+** → **Analyze new project** → selecionar o repositório `video-processing-engine-video-management-lambda`.
   - Anotar o **Project Key** gerado (ex.: `org_video-processing-engine-video-management-lambda`).
   - Anotar o **Organization slug** (ex.: `minha-org`).

2. **Desativar Automatic Analysis (OBRIGATÓRIO):**
   - No projeto criado, acessar **Administration → Analysis Method**.
   - Desativar o toggle **Automatic Analysis** (toggle OFF).
   - Sem isso, o CI falha com `exit code 1` e a mensagem `You are running CI analysis while Automatic Analysis is enabled`.

3. **Gerar o token de autenticação:**
   - Acessar [sonarcloud.io/account/security](https://sonarcloud.io/account/security).
   - Criar um token de tipo **Project Analysis Token** ou **Global Analysis Token**.
   - Copiar o valor gerado (exibido apenas uma vez).

4. **Configurar Quality Gate recomendado** (opcional, mas recomendado):
   - **Project Settings → Quality Gate** → selecionar ou criar gate com:
     - Cobertura em novo código: **≥ 70%**
     - Sem bugs/vulnerabilidades `CRITICAL` / `BLOCKER` em novo código
     - Maintainability Rating em novo código: **A**

5. **Ativar webhook do SonarCloud para reportar Quality Gate no PR:**
   - **Project Settings → GitHub** → ativar webhook para que o status do Quality Gate apareça como check no PR.

### No GitHub (repositório)

6. **Criar Secret `SONAR_TOKEN`:**
   - **Settings → Secrets and variables → Actions → Secrets → New repository secret**
   - Nome: `SONAR_TOKEN`
   - Valor: token gerado no passo 3.

7. **Criar Variable `SONAR_PROJECT_KEY`:**
   - **Settings → Secrets and variables → Actions → Variables → New repository variable**
   - Nome: `SONAR_PROJECT_KEY`
   - Valor: Project Key anotado no passo 1.

8. **Criar Variable `SONAR_ORGANIZATION`:**
   - **Settings → Secrets and variables → Actions → Variables → New repository variable**
   - Nome: `SONAR_ORGANIZATION`
   - Valor: Organization slug anotado no passo 1.

## Formas de teste
1. Executar o workflow manualmente via `workflow_dispatch` após as configurações e verificar que o step "Begin SonarCloud analysis" não retorna erro de autenticação.
2. Acessar o projeto no SonarCloud e confirmar que a análise apareceu na aba **Activity**.
3. Confirmar que o status do Quality Gate é exibido no PR como check do GitHub.

## Critérios de aceite
- [ ] Projeto criado no SonarCloud com Automatic Analysis **desativada**.
- [ ] Token `SONAR_TOKEN` salvo como Secret no repositório GitHub.
- [ ] `SONAR_PROJECT_KEY` e `SONAR_ORGANIZATION` salvos como Variables no repositório GitHub.
- [ ] Job `sonar-analysis` conclui com sucesso após as configurações (sem erros de autenticação ou conflito com Automatic Analysis).
- [ ] Quality Gate aparece como check no PR após o webhook ser ativado.
