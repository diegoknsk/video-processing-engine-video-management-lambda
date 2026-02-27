# Video Processing Engine - Video Management Lambda

## Descrição
Serviço Serverless (AWS Lambda) responsável pelo gerenciamento de vídeos (upload, consulta, status). Desenvolvido em .NET 10 seguindo Clean Architecture.

## Estrutura do Projeto
- **Api**: Entry point (Lambda Hosting), Controllers/Endpoints.
- **Application**: Casos de uso, interfaces (Ports), Models.
- **Domain**: Entidades, Value Objects, Regras de negócio.
- **Infra.Data**: Implementação de repositórios (DynamoDB, S3).
- **Infra.CrossCutting**: Configurações, Logging, IoC.
- **UnitTests**: Testes unitários com xUnit.

## Configuração Local
1. **Pré-requisitos**: .NET 10 SDK, Docker (opcional para emulação).
2. **Executar**:
   ```bash
   dotnet run --project src/VideoProcessing.VideoManagement.Api
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
