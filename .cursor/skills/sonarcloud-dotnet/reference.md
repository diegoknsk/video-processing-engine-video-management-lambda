# SonarCloud — Execução local (Windows, PowerShell)

Execute **todos os passos na mesma sessão PowerShell**, a partir da **raiz do repositório**.

---

## 0. Instalar o SonarScanner (uma vez)

```powershell
dotnet tool install --global dotnet-sonarscanner
```

---

## 1. Definir o token (uma vez por sessão)

Gere em: https://sonarcloud.io/account/security

```powershell
$env:SONAR_TOKEN = "seu_token_aqui"
```

---

## 2. Iniciar análise

```powershell
dotnet sonarscanner begin `
  /k:"SEU_PROJECT_KEY" `
  /o:"SUA_ORGANIZATION" `
  /d:sonar.token=$env:SONAR_TOKEN `
  /d:sonar.host.url="https://sonarcloud.io" `
  /d:sonar.projectBaseDir="$(Get-Location)" `
  /d:sonar.sources="src/" `
  /d:sonar.tests="tests/" `
  /d:sonar.exclusions="**/bin/**,**/obj/**,**/*.Designer.cs,**/TestResults/**,**/.sonarqube/**" `
  /d:sonar.cs.opencover.reportsPaths="tests/**/TestResults/**/coverage.opencover.xml"
```

> **Atenção:** Use `$(Get-Location)` (caminho absoluto) — nunca `"."` como `projectBaseDir`.

---

## 3. Build

```powershell
dotnet build --no-incremental
```

---

## 4. Testes com cobertura

```powershell
dotnet test `
  --configuration Release `
  --no-build `
  /p:CollectCoverage=true `
  /p:CoverageReporter=opencover `
  /p:CoverletOutputFormat=opencover `
  /p:CoverletOutput=./TestResults/coverage.opencover.xml
```

O relatório fica em `tests/<projeto>/TestResults/coverage.opencover.xml`.

---

## 5. Finalizar e enviar

```powershell
dotnet sonarscanner end /d:sonar.token=$env:SONAR_TOKEN
```

Aguarde 1–2 minutos e acesse o dashboard do projeto no SonarCloud.

---

## Troubleshooting

| Sintoma | Causa | Solução |
|---------|-------|---------|
| `sonar-project.properties files are not understood` | Arquivo `.properties` na raiz | Deletar o arquivo; passar tudo via `/d:` |
| `No analyzable projects were found` | `projectBaseDir` aponta para `.sonarqube/` | Usar `$(Get-Location)` ou caminho absoluto |
| Cobertura ausente no SonarCloud | Path do XML não bate com o glob | Verificar que o XML existe em `tests/**/TestResults/**/coverage.opencover.xml` |
| `Exit code: 1` no end | Token inválido ou projeto não encontrado | Verificar `SONAR_TOKEN`, `projectKey` e `organization` |
