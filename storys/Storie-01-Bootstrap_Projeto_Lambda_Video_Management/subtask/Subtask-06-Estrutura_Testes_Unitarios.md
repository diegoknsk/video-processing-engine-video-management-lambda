# Subtask 06: Estrutura inicial de testes unitários (xUnit + Moq + FluentAssertions)

## Descrição
Criar a estrutura de testes unitários no projeto `VideoProcessing.VideoManagement.UnitTests`, espelhar a estrutura de pastas de `src/` (Application/, Infra/, etc.), adicionar packages necessários (xUnit, Moq, FluentAssertions, xunit.runner.visualstudio), criar pelo menos 1 teste exemplo funcionando (ex.: teste do middleware de exception handler ou configuration binding), e garantir que `dotnet test` executa com sucesso.

## Passos de Implementação
1. **Criar estrutura de pastas no projeto de testes**:
   - `tests/VideoProcessing.VideoManagement.UnitTests/Application/` (testes de use cases)
   - `tests/VideoProcessing.VideoManagement.UnitTests/Infra/Data/` (testes de repositories)
   - `tests/VideoProcessing.VideoManagement.UnitTests/Infra/CrossCutting/` (testes de middleware, extensions)
   - `tests/VideoProcessing.VideoManagement.UnitTests/Api/` (testes de endpoints, se aplicável)

2. **Adicionar packages de teste**:
   - xUnit já adicionado pelo template `dotnet new xunit`
   - Adicionar Moq: `dotnet add tests/VideoProcessing.VideoManagement.UnitTests package Moq`
   - Adicionar FluentAssertions: `dotnet add tests/VideoProcessing.VideoManagement.UnitTests package FluentAssertions`
   - Adicionar xunit.runner.visualstudio (para integração com IDEs): `dotnet add tests/VideoProcessing.VideoManagement.UnitTests package xunit.runner.visualstudio`
   - Adicionar coverlet.collector (para cobertura de código): `dotnet add tests/VideoProcessing.VideoManagement.UnitTests package coverlet.collector`

3. **Criar teste exemplo 1: Configuration binding (DynamoDbOptions)**:
   - Arquivo: `tests/VideoProcessing.VideoManagement.UnitTests/Infra/CrossCutting/Configuration/DynamoDbOptionsTests.cs`
   - Teste: carregar appsettings.json em IConfiguration, fazer binding de DynamoDbOptions, validar que TableName e Region estão corretos
   - Usar FluentAssertions: `options.TableName.Should().Be("video-processing-videos")`

4. **Criar teste exemplo 2: Middleware de exception handler**:
   - Arquivo: `tests/VideoProcessing.VideoManagement.UnitTests/Infra/CrossCutting/Middleware/GlobalExceptionHandlerMiddlewareTests.cs`
   - Teste: criar mock de HttpContext, simular exceção, invocar middleware, validar que response StatusCode é 500 e body contém ErrorResponse
   - Usar Moq para criar mocks: `var mockHttpContext = new Mock<HttpContext>();`

5. **Criar teste exemplo 3: Health check endpoint (opcional, pode ser integração)**:
   - Arquivo: `tests/VideoProcessing.VideoManagement.UnitTests/Api/HealthCheckTests.cs`
   - Teste: validar que função do endpoint retorna objeto com status "healthy" e timestamp não nulo
   - Se usar minimal API, testar a função diretamente (sem HTTP); se usar controller, testar o método do controller

6. **Configurar execução de testes**:
   - Criar arquivo `xunit.runner.json` (opcional) para configurar xUnit (ex.: desabilitar paralelização se necessário)
   - Executar `dotnet test` na raiz; garantir que todos os testes passam
   - Executar com cobertura: `dotnet test /p:CollectCoverage=true`; validar que relatório de cobertura é gerado

7. **Criar arquivo README no projeto de testes**:
   - Documentar estrutura de pastas, convenções de nomenclatura de testes (ex.: `[MethodName]_[Scenario]_[ExpectedResult]`)
   - Documentar como executar testes: `dotnet test`, `dotnet test --filter Category=Unit`
   - Documentar como gerar relatório de cobertura

## Formas de Teste
1. **Testes passam**: executar `dotnet test`; validar que todos os testes (mínimo 2) passam
2. **Coverage test**: executar `dotnet test /p:CollectCoverage=true`; validar que relatório de cobertura é gerado (coverage.json ou similar)
3. **IDE integration**: abrir projeto no Visual Studio ou Rider; validar que Test Explorer detecta os testes e permite execução individual
4. **Fail test**: modificar um teste para falhar; executar `dotnet test`; validar que falha é reportada com mensagem clara (FluentAssertions)

## Critérios de Aceite da Subtask
- [ ] Estrutura de pastas criada em UnitTests espelhando src/ (Application/, Infra/Data/, Infra/CrossCutting/, Api/)
- [ ] Packages adicionados: xUnit, Moq, FluentAssertions, xunit.runner.visualstudio, coverlet.collector
- [ ] Mínimo 2 testes criados e passando: 1 para configuration binding (DynamoDbOptionsTests), 1 para middleware de exception handler (GlobalExceptionHandlerMiddlewareTests)
- [ ] Testes usam FluentAssertions para asserções (ex.: `.Should().Be()`, `.Should().NotBeNull()`)
- [ ] Testes usam Moq para criar mocks de dependências (ex.: HttpContext, ILogger)
- [ ] `dotnet test` executa com sucesso; output mostra "Passed! - Failed: 0, Passed: X, Skipped: 0, Total: X"
- [ ] `dotnet test /p:CollectCoverage=true` gera relatório de cobertura
- [ ] README.md criado em tests/ documentando estrutura, convenções e comandos de execução
- [ ] Testes são independentes (não compartilham estado) e podem ser executados em qualquer ordem
- [ ] Nomenclatura de testes segue padrão `[MethodName]_[Scenario]_[ExpectedResult]` (ex.: `Bind_ValidConfiguration_ReturnsOptions`)
