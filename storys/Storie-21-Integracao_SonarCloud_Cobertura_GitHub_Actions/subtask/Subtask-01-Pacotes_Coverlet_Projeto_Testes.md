# Subtask-01: Adicionar pacotes coverlet no projeto de testes

## Descrição
Adicionar os pacotes NuGet `coverlet.collector` e `coverlet.msbuild` ao projeto de testes unitários para habilitar a coleta de cobertura de código em formato OpenCover, que será consumida pelo SonarCloud.

## Arquivo alvo
`tests/VideoProcessing.VideoManagement.UnitTests/VideoProcessing.VideoManagement.UnitTests.csproj`

## Passos de implementação
1. Abrir o arquivo `.csproj` do projeto de testes.
2. Localizar o `<ItemGroup>` com os `<PackageReference>` existentes.
3. Adicionar as duas referências a seguir, mantendo `PrivateAssets=all` (pacotes de build, não de runtime):
   ```xml
   <PackageReference Include="coverlet.collector" Version="6.0.2">
     <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
     <PrivateAssets>all</PrivateAssets>
   </PackageReference>
   <PackageReference Include="coverlet.msbuild" Version="6.0.2">
     <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
     <PrivateAssets>all</PrivateAssets>
   </PackageReference>
   ```
4. Executar `dotnet restore` para validar que os pacotes são resolvidos.
5. Executar `dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput=./TestResults/coverage.opencover.xml` localmente para confirmar que o arquivo de cobertura é gerado.

## Formas de teste
1. `dotnet restore` retorna sem erros; pacotes aparecem no lockfile.
2. `dotnet test` com os parâmetros de cobertura gera `tests/VideoProcessing.VideoManagement.UnitTests/TestResults/coverage.opencover.xml`.
3. O XML gerado contém elementos `<Module>` com dados de cobertura (não está vazio).

## Critérios de aceite
- [ ] `coverlet.collector` 6.0.2 presente no `.csproj` com `PrivateAssets=all`.
- [ ] `coverlet.msbuild` 6.0.2 presente no `.csproj` com `PrivateAssets=all`.
- [ ] `dotnet test` com flags de cobertura gera `coverage.opencover.xml` no diretório `TestResults/` do projeto de testes.
- [ ] `dotnet build` continua sem erros após a adição dos pacotes.
