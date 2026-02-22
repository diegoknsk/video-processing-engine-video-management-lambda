# Subtask 01: Workflow build, test, publish e ZIP do projeto LambdaUpdateVideo

## Descrição
Criar o arquivo de workflow GitHub Actions que faz checkout, configura .NET 10, restaura dependências, build da solução, execução dos testes (dotnet test — falha deve abortar o pipeline), publish do projeto VideoProcessing.VideoManagement.LambdaUpdateVideo para runtime linux-x64 (self-contained false), e criação do ZIP do pacote de deploy (conteúdo da pasta de publish) para uso no deploy da Lambda.

## Passos de Implementação
1. Criar arquivo `.github/workflows/deploy-lambda-update-video.yml` com trigger on push (branch main ou developer conforme padrão do repo), pull_request e workflow_dispatch para execução manual
2. Definir job (ex.: build-and-deploy) com steps: actions/checkout@v4, actions/setup-dotnet@v4 com dotnet-version '10.0.x'
3. Adicionar steps: dotnet restore, dotnet build --configuration Release --no-restore, dotnet test --configuration Release --no-build --verbosity normal (falha aborta o job)
4. Adicionar step de publish: dotnet publish src/VideoProcessing.VideoManagement.LambdaUpdateVideo/VideoProcessing.VideoManagement.LambdaUpdateVideo.csproj (ou caminho correto do projeto) --configuration Release --output ./publish-update-video --runtime linux-x64 --self-contained false
5. Adicionar step para criar ZIP: entrar na pasta de publish, zip -r ../deployment-package-update-video.zip . (ou nome que será usado no step de deploy)
6. (Opcional) Upload artifact do ZIP para permitir download do pacote após o job

## Formas de Teste
1. Validar sintaxe YAML do workflow (linter ou execução em branch de teste)
2. Executar workflow manualmente (workflow_dispatch) e verificar que todos os steps de build, test e publish concluem com sucesso
3. Verificar que o artifact (ZIP) contém os arquivos esperados (DLL do projeto, deps, runtimeconfig) quando o artifact for baixado

## Critérios de Aceite da Subtask
- [ ] Workflow criado em `.github/workflows/deploy-lambda-update-video.yml`
- [ ] Triggers configurados: push (branch principal), pull_request e workflow_dispatch
- [ ] Steps de restore, build e test presentes; falha em dotnet test aborta o job
- [ ] Publish do projeto LambdaUpdateVideo para linux-x64; pasta de saída definida
- [ ] ZIP do pacote de deploy criado com conteúdo da pasta de publish
- [ ] Build e testes passam quando executados pelo workflow
