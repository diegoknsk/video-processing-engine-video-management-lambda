# Subtask 04: Configurar logging estruturado (Serilog) e appsettings

## Descrição
Integrar Serilog como provider de logging estruturado, configurar sinks (console para Lambda; CloudWatch é responsabilidade da infra AWS), definir níveis de log por ambiente (Information em produção, Debug em desenvolvimento), criar appsettings.json e appsettings.Production.json com seções de configuração AWS/DynamoDB/S3/Cognito, e garantir que logs de startup, request e erros são emitidos com contexto rico (TraceId, UserId quando disponível, etc.).

## Passos de Implementação
1. **Adicionar Serilog ao Program.cs**:
   - No início do Program.cs, configurar Serilog: `Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger()`
   - Adicionar `builder.Host.UseSerilog((context, services, configuration) => configuration.ReadFrom.Configuration(context.Configuration).ReadFrom.Services(services).Enrich.FromLogContext().WriteTo.Console(new JsonFormatter()))`
   - Garantir que logs estruturados são emitidos em JSON (JsonFormatter) para facilitar ingestão no CloudWatch

2. **Configurar Serilog no appsettings.json**:
   - Adicionar seção `Serilog` no appsettings.json:
     ```json
     "Serilog": {
       "MinimumLevel": {
         "Default": "Information",
         "Override": {
           "Microsoft.AspNetCore": "Warning",
           "System": "Warning"
         }
       },
       "WriteTo": [
         { "Name": "Console", "Args": { "formatter": "Serilog.Formatting.Json.JsonFormatter, Serilog" } }
       ],
       "Enrich": ["FromLogContext"]
     }
     ```
   - Em appsettings.Production.json, manter MinimumLevel em Information; em appsettings.Development.json (criar se necessário), usar Debug

3. **Enriquecer logs com contexto de request**:
   - Adicionar middleware `SerilogRequestLoggingMiddleware`: `app.UseSerilogRequestLogging(options => { options.EnrichDiagnosticContext = (diagnosticContext, httpContext) => { diagnosticContext.Set("TraceId", httpContext.TraceIdentifier); diagnosticContext.Set("UserId", httpContext.User?.FindFirst("sub")?.Value); }; })`
   - Garantir que cada log de request inclui TraceId e UserId (quando autenticado)

4. **Criar appsettings.json com seções de configuração AWS**:
   - Estrutura esperada:
     ```json
     {
       "Serilog": { ... },
       "AWS": {
         "Region": "us-east-1"
       },
       "DynamoDB": {
         "TableName": "video-processing-videos",
         "Region": "us-east-1"
       },
       "S3": {
         "BucketVideo": "video-processing-videos",
         "BucketFrames": "video-processing-frames",
         "BucketZip": "video-processing-zip",
         "Region": "us-east-1",
         "PresignedUrlTtlMinutes": 15
       },
       "Cognito": {
         "UserPoolId": "us-east-1_XXXXXXXXX",
         "ClientId": "xxxxxxxxxxxxxxxxxx",
         "Region": "us-east-1"
       }
     }
     ```
   - Em appsettings.Production.json, sobrescrever valores com variáveis de ambiente ou valores reais de produção

5. **Criar classes de Options para configuration binding**:
   - `AwsOptions` (Region)
   - `DynamoDbOptions` (TableName, Region)
   - `S3Options` (BucketVideo, BucketFrames, BucketZip, Region, PresignedUrlTtlMinutes)
   - `CognitoOptions` (UserPoolId, ClientId, Region)
   - Usar record para imutabilidade: `public record AwsOptions(string Region);`
   - Registrar no DI: `builder.Services.Configure<AwsOptions>(builder.Configuration.GetSection("AWS"))`

6. **Testar logging**:
   - Executar `dotnet run`, validar que logs de startup são emitidos em JSON no console
   - Fazer requisição GET /health, validar que log de request é emitido com TraceId
   - Forçar exceção (rota de teste), validar que log de erro é emitido com stack trace e TraceId

7. **Validar configuration binding**:
   - Criar teste unitário que carrega appsettings.json, faz binding de `DynamoDbOptions` e valida que TableName é o esperado
   - Validar que variáveis de ambiente sobrescrevem valores do appsettings (teste com `DYNAMODB__TABLENAME=override`)

## Formas de Teste
1. **Startup logs test**: executar `dotnet run`, verificar console output; validar que logs são emitidos em formato JSON e incluem campos "Timestamp", "Level", "Message"
2. **Request logs test**: fazer requisição `curl http://localhost:5000/health`, verificar console; validar que log de request inclui "TraceId", "StatusCode", "Elapsed"
3. **Error logs test**: forçar exceção, verificar console; validar que log de erro inclui "Exception", "StackTrace", "TraceId"
4. **Configuration binding test**: teste unitário carrega appsettings.json e valida que `IOptions<DynamoDbOptions>.Value.TableName` retorna valor correto

## Critérios de Aceite da Subtask
- [ ] Serilog configurado no Program.cs com `UseSerilog` e `UseSerilogRequestLogging`
- [ ] Logs emitidos em formato JSON (JsonFormatter) no console
- [ ] Seção `Serilog` criada no appsettings.json com MinimumLevel, WriteTo (Console), Enrich (FromLogContext)
- [ ] appsettings.Development.json criado com MinimumLevel Debug; appsettings.Production.json com MinimumLevel Information
- [ ] appsettings.json contém seções AWS, DynamoDB, S3, Cognito com valores placeholder
- [ ] Classes de Options criadas: AwsOptions, DynamoDbOptions, S3Options, CognitoOptions
- [ ] Options registradas no DI e vinculadas às seções do configuration
- [ ] Logs de startup, request e erros são emitidos com contexto rico (TraceId, UserId quando disponível)
- [ ] Middleware `UseSerilogRequestLogging` enriquece logs com TraceId e UserId (claim "sub")
- [ ] Teste unitário valida configuration binding para pelo menos 1 Options (ex.: DynamoDbOptions)
- [ ] `dotnet run` emite logs estruturados em JSON no console; logs de request incluem TraceId e status code
