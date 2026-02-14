---
name: validation-fluentvalidation
description: Guia para validação de InputModels com FluentValidation — regras, configuração, filtros e integração com Clean Architecture. Use quando a tarefa envolver validação, FluentValidation, validators, InputModels ou regras de validação.
---

# Validation — FluentValidation

## Quando Usar

- Criar ou modificar **validators**
- Validação de **InputModels** com **FluentValidation**
- Regras de validação, mensagens de erro
- Palavras-chave: "validar", "validação", "FluentValidation", "validator", "InputModel"

## Princípios Essenciais

### ✅ Fazer

- Validar **formato e consistência local** do InputModel (required, tamanho, formato, ranges, enums)
- Validar apenas dados do **body** (propriedades do InputModel)
- Usar mensagens de erro descritivas em português
- Criar validator para cada InputModel (mesmo que vazio)
- Deixar regras de negócio complexas para UseCase/Domain

### ❌ Não Fazer

- **Nunca** acessar banco de dados no validator
- **Nunca** chamar services externos no validator
- **Nunca** validar route parameters ou headers (validados por binding/filtros)
- **Nunca** colocar regras de negócio complexas (ex.: email único)

**Regra de ouro:** Validators validam forma; UseCases validam negócio.

## Checklist Rápido

1. Criar validator herdando `AbstractValidator<TInputModel>`
2. Adicionar regras no construtor: `RuleFor(x => x.Campo).Regra().WithMessage("Mensagem")`
3. Registrar validators no `Program.cs`: `AddValidatorsFromAssemblyContaining<Validator>()`
4. Estrutura: `Application/Validators/<Contexto>/<InputName>Validator.cs`
5. Testar validators com `FluentValidation.TestHelper`

## Exemplo Mínimo

**Cenário:** Validar criação de usuário (email, senha, telefone)

### InputModel

```csharp
public record CreateUserInput(
    string Email,
    string Password,
    string PhoneNumber
);
```

### Validator

```csharp
using FluentValidation;

public class CreateUserInputValidator : AbstractValidator<CreateUserInput>
{
    public CreateUserInputValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório.")
            .EmailAddress().WithMessage("Email em formato inválido.")
            .MaximumLength(200).WithMessage("Email não pode ter mais de 200 caracteres.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Senha é obrigatória.")
            .MinimumLength(8).WithMessage("Senha deve ter pelo menos 8 caracteres.")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)")
            .WithMessage("Senha deve conter letra maiúscula, minúscula e número.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Telefone é obrigatório.")
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .WithMessage("Telefone em formato inválido (use E.164).");
    }
}
```

### Configuração (Program.cs)

```csharp
using FluentValidation;

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateUserInputValidator>();
```

### Teste

```csharp
using FluentValidation.TestHelper;
using Xunit;

public class CreateUserInputValidatorTests
{
    private readonly CreateUserInputValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Email_Is_Empty()
    {
        var input = new CreateUserInput("", "Pass123", "+5511999999999");
        var result = _validator.TestValidate(input);
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email é obrigatório.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Input_Is_Valid()
    {
        var input = new CreateUserInput("test@test.com", "Password123", "+5511999999999");
        var result = _validator.TestValidate(input);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
```

**Pontos-chave:**
- **Formato no validator** (email válido, senha forte)
- **Regras de negócio no UseCase** (email único, usuário existe)
- FluentValidation integra automaticamente (valida antes do controller chamar UseCase)

## Regras Mais Comuns

```csharp
// Required
RuleFor(x => x.Email).NotEmpty();        // string não vazia
RuleFor(x => x.Id).NotEmpty();           // Guid não vazio
RuleFor(x => x.User).NotNull();          // objeto não nulo

// Strings
.MinimumLength(3), .MaximumLength(200), .EmailAddress(), .Matches(regex)

// Números
.GreaterThan(0), .LessThan(150), .InclusiveBetween(1, 100)

// Enum
.IsInEnum()

// Listas
RuleFor(x => x.Items).NotEmpty();
RuleForEach(x => x.Items).ChildRules(item => { /* validar cada item */ });

// Condicionais
.When(x => x.IsCompany)  // validar apenas se condição verdadeira

// Datas
.LessThan(DateTime.Now), .GreaterThanOrEqualTo(DateTime.Today)
```

## Separação de Responsabilidades

| Camada | Validação |
|--------|-----------|
| **Validator** | Formato, estrutura, required, ranges (ex.: email válido) |
| **UseCase** | Regras de negócio com banco (ex.: email único, saldo suficiente) |
| **Domain** | Invariantes de entidade (ex.: pedido não pode ter valor negativo) |

## Referências

- [FluentValidation Documentation](https://docs.fluentvalidation.net/)
- [ASP.NET Core Integration](https://docs.fluentvalidation.net/en/latest/aspnet.html)
