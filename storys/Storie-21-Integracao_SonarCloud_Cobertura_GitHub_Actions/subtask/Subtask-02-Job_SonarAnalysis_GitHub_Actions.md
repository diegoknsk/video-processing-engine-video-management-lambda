# Subtask-02: Adicionar job sonar-analysis no workflow GitHub Actions

## Descrição
Modificar o workflow `.github/workflows/deploy-lambda-update-video.yml` para:
1. Adicionar o trigger `pull_request` com `branches: [main]` (além do push existente).
2. Inserir o job `sonar-analysis` **antes** do job `build-and-deploy`.
3. Fazer o job `build-and-deploy` declarar `needs: [sonar-analysis]` para que o deploy nunca ocorra sem aprovação do Quality Gate.

## Arquivo alvo
`.github/workflows/deploy-lambda-update-video.yml`

## Passos de implementação
1. Adicionar o trigger `pull_request` no bloco `on:` do workflow:
   ```yaml
   on:
     push:
       branches: [ main ]
     pull_request:
       branches: [ main ]
     workflow_dispatch:
   ```

2. Inserir o job `sonar-analysis` antes do job `build-and-deploy`:
   ```yaml
   sonar-analysis:
     name: SonarCloud Analysis
     runs-on: ubuntu-latest
     steps:
       - name: Checkout code
         uses: actions/checkout@v4
         with:
           fetch-depth: 0

       - name: Setup .NET
         uses: actions/setup-dotnet@v4
         with:
           dotnet-version: '10.0.x'

       - name: Install SonarScanner
         run: dotnet tool install --global dotnet-sonarscanner

       - name: Restore dependencies
         run: dotnet restore

       - name: Begin SonarCloud analysis
         run: |
           dotnet sonarscanner begin \
             /k:"${{ vars.SONAR_PROJECT_KEY }}" \
             /o:"${{ vars.SONAR_ORGANIZATION }}" \
             /d:sonar.token="${{ secrets.SONAR_TOKEN }}" \
             /d:sonar.host.url="https://sonarcloud.io" \
             /d:sonar.projectBaseDir="${{ github.workspace }}" \
             /d:sonar.sources="src/" \
             /d:sonar.tests="tests/" \
             /d:sonar.exclusions="**/bin/**,**/obj/**,**/*.Designer.cs" \
             /d:sonar.test.exclusions="tests/**/" \
             /d:sonar.coverage.exclusions="**/Program.cs,**/*Extensions.cs" \
             /d:sonar.cs.opencover.reportsPaths="tests/**/TestResults/**/coverage.opencover.xml"

       - name: Build solution
         run: dotnet build --configuration Release --no-restore

       - name: Run tests with coverage
         run: |
           dotnet test \
             --configuration Release \
             --no-build \
             --verbosity normal \
             /p:CollectCoverage=true \
             /p:CoverageReporter=opencover \
             /p:CoverletOutputFormat=opencover \
             /p:CoverletOutput=./TestResults/coverage.opencover.xml

       - name: End SonarCloud analysis
         run: |
           dotnet sonarscanner end \
             /d:sonar.token="${{ secrets.SONAR_TOKEN }}"
   ```

3. Adicionar `needs: [sonar-analysis]` ao job `build-and-deploy`:
   ```yaml
   build-and-deploy:
     name: Build and Deploy
     needs: [sonar-analysis]
     runs-on: ubuntu-latest
     ...
   ```

4. Garantir que o job `build-and-deploy` continua executando **apenas** em `push` e `workflow_dispatch` (não em PR), usando condição `if: github.event_name == 'push' || github.event_name == 'workflow_dispatch'` no nível do job (já existe nos steps individuais; mover para o job garante que os steps de build/testes não sejam duplicados desnecessariamente em PR):
   ```yaml
   build-and-deploy:
     name: Build and Deploy
     needs: [sonar-analysis]
     if: github.event_name == 'push' || github.event_name == 'workflow_dispatch'
     runs-on: ubuntu-latest
   ```

## Pontos críticos (armadilhas da skill)
- **NUNCA** usar `sonar.projectBaseDir="."` — o path relativo resolve para `.sonarqube/` no runner. Usar `${{ github.workspace }}` (caminho absoluto).
- **NUNCA** criar `sonar-project.properties` — o SonarScanner for .NET ignora esse arquivo; tudo vai via `/d:`.
- `fetch-depth: 0` é obrigatório para que o Sonar colete blame info e diff correto para análise de novo código em PRs.

## Formas de teste
1. Abrir um PR para `main` e verificar que o job `sonar-analysis` aparece na aba "Checks" do PR e conclui com sucesso.
2. Após o job Sonar passar, verificar que o job `build-and-deploy` **não** é executado no PR (apenas em push/merge).
3. Após merge na `main`, verificar que o deploy ocorre normalmente e que o SonarCloud exibe análise da branch `main` com cobertura.

## Critérios de aceite
- [ ] Trigger `pull_request` com `branches: [main]` adicionado ao workflow.
- [ ] Job `sonar-analysis` executa com `fetch-depth: 0` e `sonar.projectBaseDir="${{ github.workspace }}"`.
- [ ] Job `build-and-deploy` declara `needs: [sonar-analysis]` e tem condição `if` para não rodar em PRs.
- [ ] Arquivo de cobertura é localizado pelo glob `tests/**/TestResults/**/coverage.opencover.xml`.
- [ ] Pipeline conclui sem erros no GitHub Actions.
