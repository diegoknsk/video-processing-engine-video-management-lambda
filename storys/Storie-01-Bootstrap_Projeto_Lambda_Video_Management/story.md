# Storie-01: Bootstrap do Projeto Lambda Video Management

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** —

## Descrição
Como arquiteto de software, quero criar a estrutura inicial do projeto Lambda Video Management seguindo Clean Architecture e padrões do ecossistema, para garantir base sólida, manutenível e evolutiva para todas as funcionalidades de gerenciamento de vídeos.

## Objetivo
Criar a estrutura completa do projeto .NET 10 com Clean Architecture (Api → Application → Domain; Infra.* implementa Ports), configurar packages essenciais (AWS Lambda Hosting, DynamoDB, S3, Logging estruturado), definir modelos de configuração, health check básico e preparar a base para desenvolvimento das demais stories.

## Escopo Técnico
- **Tecnologias:** .NET 10, C# 13, ASP.NET Core Minimal API, AWS Lambda
- **Arquivos criados:**
  - `src/VideoProcessing.VideoManagement.Api/Program.cs` (entry point com AddAWSLambdaHosting)
  - `src/VideoProcessing.VideoManagement.Api/appsettings.json` e `appsettings.Production.json`
  - `src/VideoProcessing.VideoManagement.Application/` (UseCases, Ports, InputModels, ResponseModels)
  - `src/VideoProcessing.VideoManagement.Domain/` (Entities, ValueObjects, Enums)
  - `src/VideoProcessing.VideoManagement.Infra.Data/` (implementação DynamoDB, S3)
  - `src/VideoProcessing.VideoManagement.Infra.CrossCutting/` (logging, config, extensions)
  - `tests/VideoProcessing.VideoManagement.UnitTests/` (estrutura inicial de testes)
  - `.csproj` para cada projeto
- **Componentes:** 
  - Health check endpoint GET /health
  - Middleware de tratamento de erros (exception handler)
  - Logging estruturado (Serilog)
  - Configuration binding (AWS, DynamoDB, S3 options)
  - DI container com registro de serviços
- **Pacotes/Dependências:**
  - Amazon.Lambda.AspNetCoreServer.Hosting (1.0.0 ou latest para .NET 10)
  - AWSSDK.DynamoDBv2 (3.7.x)
  - AWSSDK.S3 (3.7.x)
  - Serilog.AspNetCore (8.0.x)
  - Serilog.Sinks.Console (5.0.x)
  - FluentValidation.AspNetCore (11.3.x)
  - xUnit (2.6.x)
  - xUnit.runner.visualstudio (2.5.x)
  - Moq (4.20.x)
  - FluentAssertions (6.12.x)

## Dependências e Riscos (para estimativa)
- **Dependências:** 
  - Nenhuma outra story (é a primeira)
  - Pré-condição: conhecimento da estrutura Clean Architecture definida nas rules (.cursor/rules/core-clean-architecture.mdc)
- **Riscos:** 
  - Mudanças no package Amazon.Lambda.AspNetCoreServer.Hosting para .NET 10 (verificar versão estável)
  - Definição de convenções de nomenclatura (já mitigado pelas rules/skills do projeto)

## Subtasks
- [Subtask 01: Criar estrutura de projetos e solução (.sln, .csproj)](./subtask/Subtask-01-Estrutura_Projetos_Solucao.md)
- [Subtask 02: Configurar Program.cs com AddAWSLambdaHosting e DI](./subtask/Subtask-02-Program_DI_LambdaHosting.md)
- [Subtask 03: Implementar health check e middleware de erros](./subtask/Subtask-03-HealthCheck_Middleware_Erros.md)
- [Subtask 04: Configurar logging estruturado (Serilog) e appsettings](./subtask/Subtask-04-Logging_Serilog_AppSettings.md)
- [Subtask 05: Criar modelos de configuração AWS (DynamoDB, S3, Cognito)](./subtask/Subtask-05-Modelos_Configuracao_AWS.md)
- [Subtask 06: Estrutura inicial de testes unitários (xUnit + Moq + FluentAssertions)](./subtask/Subtask-06-Estrutura_Testes_Unitarios.md)

## Critérios de Aceite da História
- [ ] Solução (.sln) com 5 projetos (Api, Application, Domain, Infra.Data, Infra.CrossCutting) + 1 projeto de testes
- [ ] Program.cs configurado com `AddAWSLambdaHosting(LambdaEventSource.HttpApi)` e registra todos os serviços de infraestrutura no DI
- [ ] GET /health retorna 200 OK com JSON `{ "status": "healthy", "timestamp": "..." }`
- [ ] Middleware de exception handler captura exceções não tratadas e retorna 500 com estrutura padronizada
- [ ] Logging estruturado com Serilog configurado (console sink); logs de startup, request e erros funcionando
- [ ] appsettings.json e appsettings.Production.json criados com seções AWS, DynamoDB, S3, Cognito (valores placeholder)
- [ ] Classes de configuração (Options) criadas e registradas no DI: `AwsOptions`, `DynamoDbOptions`, `S3Options`, `CognitoOptions`
- [ ] Projeto de testes com estrutura de pastas espelhando src (Application/, Infra/, etc.) e pelo menos 1 teste exemplo rodando
- [ ] `dotnet build` passa sem erros; `dotnet test` executa com sucesso
- [ ] Documentação inline (comentários XML em classes públicas) para tipos principais (Program, health check, middleware)

## Rastreamento (dev tracking)
- **Início:** dia 14/02/2026, às 17:48 (Brasília)
- **Fim:** —
- **Tempo total de desenvolvimento:** —
