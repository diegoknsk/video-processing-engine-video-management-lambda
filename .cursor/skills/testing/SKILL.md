---
name: testing
description: Guia para testes (unitários, BDD, integração), cobertura, build e qualidade. Use quando a tarefa envolver testes, xUnit, BDD, SpecFlow, cobertura, mutation testing ou validação de build.
---

# Testing — Qualidade e Cobertura

## Quando Usar

- Criar ou modificar **testes** (unitários, BDD, integração)
- **xUnit**, **SpecFlow**, **BDD**, cobertura de código
- **Build**, **CI/CD**, validação de qualidade
- Palavras-chave: "teste", "test", "BDD", "cobertura", "coverage", "xUnit", "build"

## Princípios Essenciais

### ✅ Fazer

- Usar **padrão AAA** (Arrange, Act, Assert) em todos os testes
- **FluentAssertions** para assertions legíveis
- **Moq** para mockar dependências (interfaces)
- **Fact** para teste único, **Theory** para múltiplos casos
- Nomear testes: `MethodName_Scenario_ExpectedResult`
- Testar **comportamento**, não implementação
- Cobertura mínima: **80%** (UseCases, Domain, Validators)

### ❌ Não Fazer

- **Nunca** testar detalhes de implementação (mockar campos privados)
- **Nunca** testes dependentes uns dos outros (ordem importa)
- **Nunca** lógica complexa dentro de testes
- **Nunca** ignorar testes falhando (comentar `[Fact]`)
- **Nunca** esquecer de testar casos de erro (exceções, validações)

**Regra de ouro:** AAA + FluentAssertions + Moq = 80% dos testes unitários.

## Checklist Rápido

1. Criar projeto `<Projeto>.Tests.Unit` (xUnit)
2. Instalar: `xunit`, `FluentAssertions`, `Moq`
3. Estrutura de teste: Arrange → Act → Assert
4. Nomear: `MethodName_Scenario_ExpectedResult`
5. Mockar dependências (interfaces) com Moq
6. Usar `[Fact]` (teste único) ou `[Theory]` (múltiplos casos)
7. Cobertura: `dotnet test --collect:"XPlat Code Coverage"`
8. Meta: 80% de cobertura em UseCases, Domain, Validators

## Exemplo Mínimo

**Cenário:** Teste unitário de UseCase com mocks

### UseCase Sendo Testado

```csharp
public class CreateUserUseCase(IUserRepository repository) : ICreateUserUseCase
{
    public async Task<CreateUserOutputModel> ExecuteAsync(CreateUserInput input, CancellationToken ct = default)
    {
        if (await repository.ExistsAsync(input.Email, ct))
            throw new InvalidOperationException("Email já está em uso.");

        var user = await repository.CreateAsync(new User { Email = input.Email, Name = input.Name }, ct);
        return new CreateUserOutputModel(user.Id, user.Email);
    }
}
```

### Testes (xUnit + Moq + FluentAssertions)

```csharp
using Xunit;
using FluentAssertions;
using Moq;

public class CreateUserUseCaseTests
{
    private readonly Mock<IUserRepository> _repositoryMock = new();
    private readonly CreateUserUseCase _sut; // System Under Test

    public CreateUserUseCaseTests()
    {
        _sut = new CreateUserUseCase(_repositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenInputIsValid_ShouldCreateUser()
    {
        // Arrange
        var input = new CreateUserInput("test@test.com", "Test User");
        var expectedUser = new User { Id = Guid.NewGuid(), Email = input.Email, Name = input.Name };
        
        _repositoryMock.Setup(x => x.ExistsAsync(input.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repositoryMock.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _sut.ExecuteAsync(input);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(input.Email);
        _repositoryMock.Verify(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenEmailAlreadyExists_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var input = new CreateUserInput("existing@test.com", "Test");
        _repositoryMock.Setup(x => x.ExistsAsync(input.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        await _sut.Invoking(x => x.ExecuteAsync(input))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Email já está em uso.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public async Task ExecuteAsync_WhenEmailIsEmpty_ShouldThrowArgumentException(string email)
    {
        // Arrange
        var input = new CreateUserInput(email, "Test");

        // Act & Assert
        await _sut.Invoking(x => x.ExecuteAsync(input))
            .Should().ThrowAsync<ArgumentException>();
    }
}
```

**Pontos-chave:**
- **Padrão AAA:** Arrange (setup), Act (executar), Assert (verificar)
- **Moq:** Setup (mock comportamento), Verify (verificar chamadas)
- **FluentAssertions:** `.Should().Be()`, `.Should().ThrowAsync<>()`

## FluentAssertions Comuns

```csharp
// Valores
result.Should().Be(expected);
result.Should().NotBeNull();

// Strings
result.Should().Contain("substring");
result.Should().StartWith("prefix");

// Números
result.Should().BeGreaterThan(0);
result.Should().BeInRange(1, 10);

// Coleções
list.Should().HaveCount(3);
list.Should().Contain(x => x.Id == expectedId);
list.Should().BeEmpty();

// Exceções
await action.Should().ThrowAsync<InvalidOperationException>()
    .WithMessage("Error message");
```

## Moq — Mocking

```csharp
// Setup (mock comportamento)
_mock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(user);

// Setup com condição
_mock.Setup(x => x.GetByIdAsync(specificId, It.IsAny<CancellationToken>()))
    .ReturnsAsync(user);

// Setup throw exception
_mock.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
    .ThrowsAsync(new InvalidOperationException("Error"));

// Verify (verificar chamadas)
_mock.Verify(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
_mock.Verify(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
```

## Estrutura de Pastas

```
<Projeto>.Tests.Unit/
  Application/
    UseCases/
      User/
        CreateUserUseCaseTests.cs
    Validators/
      User/
        CreateUserInputValidatorTests.cs
  Domain/
    Entities/
      UserTests.cs
```

## BDD com SpecFlow (Opcional)

Para testes baseados em comportamento (Gherkin):

```bash
dotnet add package SpecFlow.xUnit
dotnet add package SpecFlow.Tools.MsBuild.Generation
```

### Feature (CreateUser.feature)

```gherkin
Feature: Create User
  As an API consumer
  I want to create a new user
  So that they can access the system

Scenario: Create user successfully
  Given I have valid user data
  When I submit the create user request
  Then the user should be created
  And the response should contain the user ID

Scenario: Create user with existing email
  Given a user with email "test@test.com" already exists
  When I submit the create user request with email "test@test.com"
  Then I should receive an error "Email já está em uso."
```

### Steps (CreateUser.steps.cs)

```csharp
[Binding]
public class CreateUserSteps(TestContext context)
{
    [Given("I have valid user data")]
    public void GivenIHaveValidUserData()
    {
        context.Input = new CreateUserInput("test@test.com", "Test User");
    }

    [When("I submit the create user request")]
    public async Task WhenISubmitTheCreateUserRequest()
    {
        context.Result = await context.UseCase.ExecuteAsync(context.Input);
    }

    [Then("the user should be created")]
    public void ThenTheUserShouldBeCreated()
    {
        context.Result.Should().NotBeNull();
    }
}
```

## Cobertura de Código

```bash
# Rodar testes com cobertura
dotnet test --collect:"XPlat Code Coverage"

# Gerar relatório HTML (instalar reportgenerator)
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"**\coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html

# Abrir relatório
start coveragereport\index.html
```

**Meta de cobertura:**
- UseCases: 90%+
- Domain: 85%+
- Validators: 90%+
- Infra: 70%+

## Build e Validação

```bash
# Rodar todos os testes
dotnet test

# Build e teste (CI/CD)
dotnet build --configuration Release
dotnet test --configuration Release --no-build --verbosity normal
```

## Referências

- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions](https://fluentassertions.com/)
- [Moq Quickstart](https://github.com/moq/moq4/wiki/Quickstart)
- [SpecFlow Documentation](https://docs.specflow.org/)
