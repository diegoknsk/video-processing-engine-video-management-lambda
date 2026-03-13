# Storie-21: Integração SonarCloud com Cobertura de Código via GitHub Actions

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** [DD/MM/AAAA]

## Descrição
Como desenvolvedor do projeto, quero integrar o SonarCloud com cobertura de código (OpenCover) ao pipeline de CI/CD via GitHub Actions, para garantir análise estática automática, visibilidade de cobertura e Quality Gate obrigatório antes de todo merge na `main`.

## Objetivo
Configurar análise estática contínua com SonarCloud: coleta de cobertura via coverlet (OpenCover), job `sonar-analysis` no workflow do GitHub Actions disparado em PRs para `main` e em push direto na `main`, dependência do job de deploy em relação ao job Sonar, secrets/variables no GitHub, `.gitignore` atualizado, Branch Protection Rule com Quality Gate obrigatório e badges no README.

## Escopo Técnico
- Tecnologias: .NET 10, GitHub Actions, SonarCloud, coverlet
- Arquivos afetados:
  - `tests/VideoProcessing.VideoManagement.UnitTests/VideoProcessing.VideoManagement.UnitTests.csproj`
  - `.github/workflows/deploy-lambda-update-video.yml`
  - `.gitignore`
  - `README.md`
- Componentes/Recursos:
  - Job `sonar-analysis` no workflow existente
  - SonarScanner for .NET (`dotnet-sonarscanner`)
  - coverlet coleta de cobertura em formato OpenCover
- Pacotes/Dependências:
  - coverlet.collector (6.0.2)
  - coverlet.msbuild (6.0.2)

## Dependências e Riscos (para estimativa)
- Dependências: Conta SonarCloud criada; repositório conectado à organização SonarCloud; token gerado em sonarcloud.io/account/security.
- Riscos:
  - **Armadilha crítica:** `sonar.projectBaseDir="."` resolve para `.sonarqube/` no runner — usar `${{ github.workspace }}` (caminho absoluto).
  - **Armadilha crítica:** Automatic Analysis ativada ao mesmo tempo que CI causa `exit code 1` — desativar em Administration → Analysis Method.
  - Cobertura não aparece se `CoverletOutput` relativo não bater com glob Sonar — manter `./TestResults/coverage.opencover.xml`.
  - Overview do SonarCloud mostra "No data available" até o primeiro push/merge na `main`.

## Subtasks
- [Subtask 01: Adicionar pacotes coverlet no projeto de testes](./subtask/Subtask-01-Pacotes_Coverlet_Projeto_Testes.md)
- [Subtask 02: Adicionar job sonar-analysis no workflow GitHub Actions](./subtask/Subtask-02-Job_SonarAnalysis_GitHub_Actions.md)
- [Subtask 03: Configurar Secrets e Variables no GitHub e criar projeto SonarCloud](./subtask/Subtask-03-Secrets_Variables_GitHub_SonarCloud.md)
- [Subtask 04: Atualizar .gitignore com entradas do Sonar](./subtask/Subtask-04-Gitignore_Sonar.md)
- [Subtask 05: Branch Protection Rule e badges no README](./subtask/Subtask-05-Branch_Protection_Badges_README.md)

## Critérios de Aceite da História
- [ ] `coverlet.collector` e `coverlet.msbuild` (6.0.2) presentes no `.csproj` de testes com `PrivateAssets=all`
- [ ] Job `sonar-analysis` no workflow com `fetch-depth: 0`, `sonar.projectBaseDir="${{ github.workspace }}"` e glob de cobertura `tests/**/TestResults/**/coverage.opencover.xml`
- [ ] Workflow disparado em `pull_request` com `branches: [main]` além do push existente; job `build-and-deploy` declara `needs: [sonar-analysis]`
- [ ] Secret `SONAR_TOKEN` e variables `SONAR_PROJECT_KEY` / `SONAR_ORGANIZATION` documentados e configurados no repositório GitHub
- [ ] Automatic Analysis desativada no SonarCloud (Administration → Analysis Method) para evitar conflito com CI
- [ ] `.gitignore` contém entradas `.sonarqube/`, `.scannerwork/`, `coverage.opencover.xml`
- [ ] Branch Protection Rule para `main` com check `SonarCloud Analysis` obrigatório antes de merge
- [ ] Badges de Quality Gate e Coverage adicionados ao `README.md`
- [ ] Pipeline executa sem erros; Quality Gate passa para código existente

## Rastreamento (dev tracking)
- **Início:** dia 13/03/2026, às 12:42 (Brasília)
- **Fim:** —
- **Tempo total de desenvolvimento:** —
