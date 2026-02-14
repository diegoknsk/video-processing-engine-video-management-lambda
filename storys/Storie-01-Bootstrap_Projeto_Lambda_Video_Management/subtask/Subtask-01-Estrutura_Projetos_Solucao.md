# Subtask 01: Criar estrutura de projetos e solução (.sln, .csproj)

## Descrição
Criar a estrutura completa de projetos da solution seguindo Clean Architecture: Api (entry point/Lambda), Application (use cases, ports, contracts), Domain (entidades, enums, value objects), Infra.Data (implementações DynamoDB/S3), Infra.CrossCutting (logging, config, extensions) e projeto de testes unitários.

## Passos de Implementação
1. **Criar a solution (.sln)**:
   - Executar `dotnet new sln -n VideoProcessing.VideoManagement` na raiz do repositório
   - Estrutura esperada: solution na raiz; projetos em `src/` e testes em `tests/`

2. **Criar projetos de domínio e aplicação (sem dependências externas)**:
   - `dotnet new classlib -n VideoProcessing.VideoManagement.Domain -f net10.0 -o src/VideoProcessing.VideoManagement.Domain`
   - `dotnet new classlib -n VideoProcessing.VideoManagement.Application -f net10.0 -o src/VideoProcessing.VideoManagement.Application`
   - Application referencia Domain: `dotnet add src/VideoProcessing.VideoManagement.Application reference src/VideoProcessing.VideoManagement.Domain`

3. **Criar projeto de infraestrutura (Data)**:
   - `dotnet new classlib -n VideoProcessing.VideoManagement.Infra.Data -f net10.0 -o src/VideoProcessing.VideoManagement.Infra.Data`
   - Infra.Data referencia Application e Domain
   - Adicionar packages AWS: `dotnet add src/VideoProcessing.VideoManagement.Infra.Data package AWSSDK.DynamoDBv2` e `dotnet add src/VideoProcessing.VideoManagement.Infra.Data package AWSSDK.S3`

4. **Criar projeto de infraestrutura (CrossCutting)**:
   - `dotnet new classlib -n VideoProcessing.VideoManagement.Infra.CrossCutting -f net10.0 -o src/VideoProcessing.VideoManagement.Infra.CrossCutting`
   - Adicionar packages: Serilog.AspNetCore, Serilog.Sinks.Console, FluentValidation.AspNetCore

5. **Criar projeto de API (entry point Lambda)**:
   - `dotnet new web -n VideoProcessing.VideoManagement.Api -f net10.0 -o src/VideoProcessing.VideoManagement.Api` (template minimal API)
   - Api referencia Application, Infra.Data, Infra.CrossCutting
   - Adicionar package: `dotnet add src/VideoProcessing.VideoManagement.Api package Amazon.Lambda.AspNetCoreServer.Hosting`

6. **Criar projeto de testes unitários**:
   - `dotnet new xunit -n VideoProcessing.VideoManagement.UnitTests -f net10.0 -o tests/VideoProcessing.VideoManagement.UnitTests`
   - Adicionar referências: Api, Application, Domain, Infra.Data
   - Adicionar packages: Moq, FluentAssertions, xunit.runner.visualstudio

7. **Adicionar todos os projetos à solution**:
   - `dotnet sln add` para cada projeto criado (6 projetos no total)
   - Verificar com `dotnet sln list`

8. **Validar estrutura e build**:
   - Executar `dotnet restore` e `dotnet build` na raiz; garantir 0 erros
   - Estrutura de pastas esperada:
     ```
     /
     ├── VideoProcessing.VideoManagement.sln
     ├── src/
     │   ├── VideoProcessing.VideoManagement.Api/
     │   ├── VideoProcessing.VideoManagement.Application/
     │   ├── VideoProcessing.VideoManagement.Domain/
     │   ├── VideoProcessing.VideoManagement.Infra.Data/
     │   └── VideoProcessing.VideoManagement.Infra.CrossCutting/
     └── tests/
         └── VideoProcessing.VideoManagement.UnitTests/
     ```

## Formas de Teste
1. **Build test**: executar `dotnet build` na raiz; garantir 0 warnings e 0 erros
2. **Referências**: executar `dotnet list reference` em cada projeto; validar que dependências estão corretas (Application → Domain; Infra.Data → Application; Api → todos)
3. **Packages**: executar `dotnet list package` em cada projeto; validar que packages AWS, Serilog, FluentValidation, xUnit, Moq estão instalados nos projetos corretos

## Critérios de Aceite da Subtask
- [ ] Solution (.sln) criada com 6 projetos (5 src + 1 tests)
- [ ] Todos os projetos em .NET 10 (net10.0)
- [ ] Grafo de dependências correto: Api → Application/Infra.*; Application → Domain; Infra.Data → Application/Domain; Infra.CrossCutting standalone
- [ ] Packages AWS SDK (DynamoDBv2, S3) instalados em Infra.Data
- [ ] Packages Serilog e FluentValidation instalados em Infra.CrossCutting
- [ ] Package Amazon.Lambda.AspNetCoreServer.Hosting instalado em Api
- [ ] Packages de teste (xUnit, Moq, FluentAssertions) instalados em UnitTests
- [ ] `dotnet build` executa sem erros; `dotnet restore` resolve todas as dependências
- [ ] Estrutura de pastas src/ e tests/ organizada conforme esperado
