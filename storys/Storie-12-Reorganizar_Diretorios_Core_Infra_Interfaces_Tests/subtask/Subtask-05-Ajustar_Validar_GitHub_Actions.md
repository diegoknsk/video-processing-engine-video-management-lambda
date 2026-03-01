# Subtask-05: Ajustar e validar GitHub Actions

## Descrição
Identificar e corrigir todos os workflows do GitHub Actions que usam caminhos antigos dos projetos (ex.: `src/VideoProcessing.VideoManagement.Api`, `src/VideoProcessing.VideoManagement.LambdaUpdateVideo`). Após a reorganização, esses paths quebram nos steps de **publish** e em qualquer outro que referencie diretórios de projeto.

## O que pode quebrar
- **Publish da API:** step que faz `dotnet publish src/VideoProcessing.VideoManagement.Api/...` — path passa a ser `src/InterfacesExternas/VideoProcessing.VideoManagement.Api/...`.
- **Publish da Lambda Update Video:** step que faz `dotnet publish src/VideoProcessing.VideoManagement.LambdaUpdateVideo/...` — path passa a ser `src/InterfacesExternas/VideoProcessing.VideoManagement.LambdaUpdateVideo/...`.
- **Restore/Build/Test:** em geral usam a raiz da solution (`dotnet restore`, `dotnet build`, `dotnet test`) e podem continuar funcionando; validar que a solution está na raiz e que não há referência a .csproj por path antigo em nenhum step.
- Qualquer workflow que cite explicitamente `src/VideoProcessing.*` (paths antigos) em `run`, `working-directory` ou em scripts invocados.

## Passos de Implementação
1. Listar todos os arquivos em `.github/workflows/` (ex.: `deploy-lambda-video-management.yml`, `deploy-lambda-update-video.yml`).
2. Em cada workflow, buscar por ocorrências de:
   - `src/VideoProcessing.VideoManagement.Api`
   - `src/VideoProcessing.VideoManagement.LambdaUpdateVideo`
   - `src/VideoProcessing.VideoManagement.Application`
   - `src/VideoProcessing.VideoManagement.Domain`
   - `src/VideoProcessing.VideoManagement.Infra.Data`
   - `src/VideoProcessing.VideoManagement.Infra.CrossCutting`
3. Substituir pelos novos paths:
   - Api e LambdaUpdateVideo → `src/InterfacesExternas/<NomeDoProjeto>/...`
   - Application e Domain → `src/Core/<NomeDoProjeto>/...`
   - Infra.Data e Infra.CrossCutting → `src/Infra/<NomeDoProjeto>/...`
4. Salvar os arquivos e rodar os workflows (push ou workflow_dispatch) para validar que build, test e publish concluem com sucesso.

## Formas de Teste
1. Executar o workflow manualmente (workflow_dispatch) ou fazer push para a branch configurada e acompanhar a execução no GitHub Actions.
2. Verificar que os steps "Publish application" / "Publish Lambda Update Video" não falham por "project or solution not found".
3. Confirmar que os steps de deploy (se aplicável) continuam encontrando os artefatos gerados (zip, etc.).

## Critérios de Aceite
- [x] Todos os workflows em `.github/workflows/` que referenciam caminhos de projetos usam os novos paths (InterfacesExternas, Core, Infra).
- [x] Execução do workflow de deploy da API (video-management) conclui com sucesso no step de publish.
- [x] Execução do workflow de deploy da Lambda Update Video conclui com sucesso no step de publish.
- [x] Nenhum step falha por path inexistente ou projeto não encontrado.
