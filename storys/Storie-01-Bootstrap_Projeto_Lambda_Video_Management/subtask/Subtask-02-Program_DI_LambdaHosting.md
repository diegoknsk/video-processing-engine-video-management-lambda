# Subtask 02: Configurar Program.cs com AddAWSLambdaHosting e DI

## Descrição
Implementar o entry point da API Lambda no `Program.cs` usando `AddAWSLambdaHosting(LambdaEventSource.HttpApi)`, configurar o DI container registrando todos os serviços de infraestrutura (DynamoDB client, S3 client, repositories, use cases), configurar pipeline de request (exception handler, CORS se necessário) e preparar a aplicação para receber requisições via API Gateway HTTP API v2.

## Passos de Implementação
1. **Configurar Program.cs com WebApplication builder**:
   - Remover código default do template `dotnet new web`
   - Criar `WebApplicationBuilder` via `WebApplication.CreateBuilder(args)`
   - Adicionar `builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi)`
   - Configurar `builder.Services.AddControllers()` ou usar minimal API routing (decisão: usar minimal API com MapGroup)

2. **Registrar AWS clients no DI**:
   - Criar extension method `AddAwsServices(this IServiceCollection services, IConfiguration configuration)` em Infra.CrossCutting
   - Registrar `IAmazonDynamoDB` como singleton: `services.AddSingleton<IAmazonDynamoDB>(new AmazonDynamoDBClient())`
   - Registrar `IAmazonS3` como singleton: `services.AddSingleton<IAmazonS3>(new AmazonS3Client())`
   - Registrar options: `services.Configure<DynamoDbOptions>(configuration.GetSection("DynamoDB"))`
   - Registrar options: `services.Configure<S3Options>(configuration.GetSection("S3"))`
   - Chamar `builder.Services.AddAwsServices(builder.Configuration)` no Program.cs

3. **Registrar repositories e use cases**:
   - Criar extension method `AddRepositories(this IServiceCollection services)` em Infra.Data
   - Registrar `IVideoRepository` como scoped: `services.AddScoped<IVideoRepository, VideoRepository>`
   - Criar extension method `AddUseCases(this IServiceCollection services)` em Application
   - Registrar todos os use cases como scoped (ainda não existem; preparar estrutura)
   - Chamar no Program.cs: `builder.Services.AddRepositories()` e `builder.Services.AddUseCases()`

4. **Configurar pipeline de request**:
   - Adicionar `app.UseExceptionHandler("/error")` (endpoint de erro será criado na Subtask 03)
   - Se necessário CORS: adicionar `app.UseCors()` com policy configurada
   - **Middleware GATEWAY_PATH_PREFIX**: adicionar middleware customizado `GatewayPathBaseMiddleware` que lê env var `GATEWAY_PATH_PREFIX` e `GATEWAY_STAGE` e ajusta `Request.PathBase` e `Request.Path` conforme documentado em docs/gateway-path-prefix.md (isso permite que rotas funcionem tanto localmente quanto atrás do API Gateway com prefixo)

5. **Definir rotas base (placeholder para próximas stories)**:
   - Criar grupo de rotas: `var videosGroup = app.MapGroup("/videos")`
   - Placeholder: `videosGroup.MapGet("/", () => Results.Ok(new { message = "Videos API - ready" }))` (será substituído nas próximas stories)
   - Adicionar rota raiz: `app.MapGet("/", () => Results.Ok(new { service = "Video Management API", version = "1.0.0" }))`

6. **Configurar variáveis de ambiente esperadas**:
   - Documentar no appsettings.json (placeholders): DynamoDB (TableName, Region), S3 (BucketVideo, BucketFrames, BucketZip, Region), Cognito (UserPoolId, ClientId, Region)
   - Garantir que configuration binding funciona: `builder.Configuration.AddEnvironmentVariables()`

7. **Executar a aplicação localmente (teste manual)**:
   - `dotnet run --project src/VideoProcessing.VideoManagement.Api`
   - Verificar startup logs (Serilog; será configurado na Subtask 04; por ora, logs default do ASP.NET Core)
   - Validar que aplicação inicia sem exceções

## Formas de Teste
1. **Startup test**: executar `dotnet run --project src/VideoProcessing.VideoManagement.Api`; verificar que aplicação inicia sem exceções e exibe URL de listening (ex.: http://localhost:5000)
2. **DI resolution test**: criar teste unitário que cria um `ServiceCollection`, chama os extension methods (AddAwsServices, AddRepositories, AddUseCases) e valida que serviços foram registrados (`serviceProvider.GetService<IAmazonDynamoDB>() != null`)
3. **Request test**: fazer requisição HTTP `GET http://localhost:5000/` e validar resposta 200 com JSON `{ "service": "Video Management API", ... }`

## Critérios de Aceite da Subtask
- [ ] Program.cs configurado com `AddAWSLambdaHosting(LambdaEventSource.HttpApi)`
- [ ] Extension methods criados: `AddAwsServices`, `AddRepositories`, `AddUseCases`
- [ ] AWS clients (IAmazonDynamoDB, IAmazonS3) registrados no DI como singleton
- [ ] Options (DynamoDbOptions, S3Options) registradas e vinculadas a seções do appsettings.json
- [ ] Middleware `GatewayPathBaseMiddleware` implementado e registrado no pipeline (lê GATEWAY_PATH_PREFIX e GATEWAY_STAGE)
- [ ] Grupo de rotas `/videos` criado com placeholder (GET /videos retorna 200 com mensagem)
- [ ] Rota raiz GET / retorna 200 com JSON identificando o serviço
- [ ] `dotnet run` inicia a aplicação sem exceções; aplicação responde em http://localhost:5000/
- [ ] Teste unitário valida que DI está configurado corretamente (resolve IAmazonDynamoDB, IAmazonS3)
