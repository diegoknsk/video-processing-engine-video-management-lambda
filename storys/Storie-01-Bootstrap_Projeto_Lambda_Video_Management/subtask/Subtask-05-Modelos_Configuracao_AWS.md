# Subtask 05: Criar modelos de configuração AWS (DynamoDB, S3, Cognito)

## Descrição
Criar classes de Options (records imutáveis) para configuration binding das seções AWS, DynamoDB, S3 e Cognito, registrar no DI, validar valores obrigatórios (DataAnnotations ou FluentValidation), e garantir que configuração inválida gera erro claro no startup (fail-fast).

## Passos de Implementação
1. **Criar record AwsOptions**:
   - Arquivo: `src/VideoProcessing.VideoManagement.Infra.CrossCutting/Configuration/AwsOptions.cs`
   - Definição: `public record AwsOptions(string Region);`
   - Adicionar DataAnnotations: `[Required]` na propriedade Region

2. **Criar record DynamoDbOptions**:
   - Arquivo: `src/VideoProcessing.VideoManagement.Infra.CrossCutting/Configuration/DynamoDbOptions.cs`
   - Definição: `public record DynamoDbOptions([Required] string TableName, [Required] string Region);`
   - TableName e Region são obrigatórios

3. **Criar record S3Options**:
   - Arquivo: `src/VideoProcessing.VideoManagement.Infra.CrossCutting/Configuration/S3Options.cs`
   - Definição:
     ```csharp
     public record S3Options(
         [Required] string BucketVideo,
         [Required] string BucketFrames,
         [Required] string BucketZip,
         [Required] string Region,
         int PresignedUrlTtlMinutes = 15
     );
     ```
   - Buckets e Region obrigatórios; PresignedUrlTtlMinutes com valor padrão 15

4. **Criar record CognitoOptions**:
   - Arquivo: `src/VideoProcessing.VideoManagement.Infra.CrossCutting/Configuration/CognitoOptions.cs`
   - Definição: `public record CognitoOptions([Required] string UserPoolId, [Required] string ClientId, [Required] string Region);`
   - Todos os campos obrigatórios

5. **Registrar Options no DI com validação**:
   - No extension method `AddAwsServices` (Infra.CrossCutting):
     ```csharp
     services.AddOptions<AwsOptions>().BindConfiguration("AWS").ValidateDataAnnotations().ValidateOnStart();
     services.AddOptions<DynamoDbOptions>().BindConfiguration("DynamoDB").ValidateDataAnnotations().ValidateOnStart();
     services.AddOptions<S3Options>().BindConfiguration("S3").ValidateDataAnnotations().ValidateOnStart();
     services.AddOptions<CognitoOptions>().BindConfiguration("Cognito").ValidateDataAnnotations().ValidateOnStart();
     ```
   - `ValidateOnStart()` garante que configuração inválida causa falha no startup (fail-fast)

6. **Criar testes unitários de validação**:
   - Teste 1: configuração válida; instanciar ServiceProvider, resolver `IOptions<DynamoDbOptions>`, validar que Value está populado
   - Teste 2: configuração inválida (TableName vazio); validar que `ValidateOnStart` lança exceção no build do ServiceProvider
   - Teste 3: variável de ambiente sobrescreve appsettings; definir env var `DYNAMODB__TABLENAME=override`, validar que Value.TableName retorna "override"

7. **Documentar configuração esperada**:
   - Criar README.md (ou atualizar docs/) listando todas as variáveis de ambiente esperadas:
     - `AWS__REGION` (padrão: us-east-1)
     - `DYNAMODB__TABLENAME` (obrigatório)
     - `DYNAMODB__REGION` (padrão: valor de AWS__REGION)
     - `S3__BUCKETVIDEO`, `S3__BUCKETFRAMES`, `S3__BUCKETZIPOUTPUT` (obrigatórios)
     - `S3__PRESIGNEDURLTTLMINUTES` (padrão: 15)
     - `COGNITO__USERPOOLID`, `COGNITO__CLIENTID`, `COGNITO__REGION` (obrigatórios)

## Formas de Teste
1. **Valid configuration test**: criar appsettings.json válido, instanciar ServiceProvider, resolver `IOptions<DynamoDbOptions>`, validar que `Value.TableName` retorna valor esperado
2. **Invalid configuration test (missing required)**: criar appsettings.json com TableName vazio, instanciar ServiceProvider com `ValidateOnStart`, validar que lança `OptionsValidationException`
3. **Environment variable override test**: definir env var `DYNAMODB__TABLENAME=env-override`, instanciar ServiceProvider, validar que `Value.TableName == "env-override"`
4. **Fail-fast test**: executar `dotnet run` com configuração inválida (ex.: remover TableName do appsettings.json), validar que aplicação falha no startup com mensagem clara

## Critérios de Aceite da Subtask
- [ ] Records criados: `AwsOptions`, `DynamoDbOptions`, `S3Options`, `CognitoOptions` em Infra.CrossCutting/Configuration/
- [ ] Todos os records usam DataAnnotations `[Required]` em propriedades obrigatórias
- [ ] S3Options.PresignedUrlTtlMinutes tem valor padrão 15
- [ ] Options registradas no DI com `.BindConfiguration`, `.ValidateDataAnnotations()`, `.ValidateOnStart()`
- [ ] Configuração inválida causa falha no startup (fail-fast) com `OptionsValidationException`
- [ ] Teste unitário valida configuration binding com valores válidos
- [ ] Teste unitário valida que configuração inválida lança exceção
- [ ] Teste unitário valida que variáveis de ambiente sobrescrevem valores do appsettings.json
- [ ] Documentação (README ou docs/) lista todas as variáveis de ambiente esperadas, valores padrão e quais são obrigatórias
- [ ] `dotnet run` com appsettings.json válido inicia sem erros; com appsettings inválido (campo obrigatório faltando), falha no startup com mensagem clara
