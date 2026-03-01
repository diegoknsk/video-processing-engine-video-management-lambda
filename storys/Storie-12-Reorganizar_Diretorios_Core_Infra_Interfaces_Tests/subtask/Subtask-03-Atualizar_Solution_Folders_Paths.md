# Subtask-03: Atualizar Solution (.slnx) com Solution Folders e paths

## Descrição
Atualizar o arquivo `VideoProcessing.VideoManagement.slnx` para refletir a estrutura de Solution Folders (Core, Infra, InterfacesExternas, Tests) e os novos caminhos dos projetos, conforme skill architecture-clean-70-dotnet.

## Passos de Implementação
1. Abrir `VideoProcessing.VideoManagement.slnx` e substituir a estrutura atual (Folder `/src/` com todos os projetos) pela estrutura com Solution Folders:
   - **Core:** projetos em `src/Core/VideoProcessing.VideoManagement.Domain` e `src/Core/VideoProcessing.VideoManagement.Application`
   - **Infra:** projetos em `src/Infra/VideoProcessing.VideoManagement.Infra.Data` e `src/Infra/VideoProcessing.VideoManagement.Infra.CrossCutting`
   - **InterfacesExternas:** projetos em `src/InterfacesExternas/VideoProcessing.VideoManagement.Api` e `src/InterfacesExternas/VideoProcessing.VideoManagement.LambdaUpdateVideo`
   - **Tests:** projeto em `tests/VideoProcessing.VideoManagement.UnitTests`
2. Garantir que cada `<Project Path="...">` use o caminho relativo correto à raiz da solution.
3. Validar no editor/IDE que a solution carrega sem erros e que os projetos aparecem nas pastas lógicas corretas.

## Formas de Teste
1. Abrir a solution no Visual Studio ou Rider e verificar que as pastas Core, Infra, InterfacesExternas e Tests aparecem com os projetos corretos.
2. Executar `dotnet sln list` (ou equivalente) para confirmar que todos os projetos são listados e os paths existem.
3. Rodar `dotnet build` na raiz para garantir que a solution compila.

## Critérios de Aceite
- [x] O arquivo .slnx contém Solution Folders Core, Infra, InterfacesExternas e Tests (não mais uma única pasta /src/ com tudo).
- [x] Cada projeto está referenciado com o path correto (ex.: `src/Core/VideoProcessing.VideoManagement.Domain/VideoProcessing.VideoManagement.Domain.csproj`).
- [x] A solution carrega e compila sem erros; estrutura na IDE reflete a arquitetura Core | Infra | InterfacesExternas | Tests.
