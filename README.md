# Video Processing Engine - Video Management Lambda

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=SONAR_PROJECT_KEY&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=SONAR_PROJECT_KEY)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=SONAR_PROJECT_KEY&metric=coverage)](https://sonarcloud.io/summary/new_code?id=SONAR_PROJECT_KEY)

> **SonarCloud:** substitua `SONAR_PROJECT_KEY` nas URLs acima pelo Project Key do seu projeto no SonarCloud (ex.: `minha-org_video-processing-engine-video-management-lambda`). Configuração completa: [SonarCloud Setup](docs/sonarcloud-setup.md).

## Descrição
Serviço Serverless (AWS Lambda) responsável pelo gerenciamento de vídeos (upload, consulta, status). Desenvolvido em .NET 10 seguindo Clean Architecture.

## Estrutura do Projeto
Diretórios seguem Clean Architecture (~70%): Core, Infra, InterfacesExternas, Tests (detalhes em `.cursor/skills/architecture-clean-70-dotnet/SKILL.md`).
- **src/Core**: Domain, Application (casos de uso, Ports, Models).
- **src/Infra**: Infra.Data (DynamoDB, S3), Infra.CrossCutting (config, logging).
- **src/InterfacesExternas**: Api (Lambda Hosting, Controllers), LambdaUpdateVideo.
- **tests**: UnitTests (xUnit).

## Configuração Local
1. **Pré-requisitos**: .NET 10 SDK, Docker (opcional para emulação).
2. **Executar**:
   ```bash
   dotnet run --project src/InterfacesExternas/VideoProcessing.VideoManagement.Api
   ```
3. **Testes**:
   ```bash
   dotnet test
   ```

## Deploy
O deploy é realizado via GitHub Actions:
- **Video Management (Lambda API):** [Documentação de Deploy](docs/deploy-video-management-lambda.md)
- **Lambda Update Video (ZIP):** [Documentação de Deploy Lambda Update Video](docs/deploy-lambda-update-video.md)

## Variáveis de Ambiente
Consulte `appsettings.json` para ver as chaves de configuração esperadas (AWS, DynamoDB, S3, Cognito).
