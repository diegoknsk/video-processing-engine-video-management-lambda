---
description: Convenções .NET 10, C# 13, nomenclatura, DI, System.Text.Json e padrões idiomáticos (max 250 chars)
---

# Core .NET — Convenções Universais

- Código **conciso e idiomático**, seguindo convenções Microsoft; **.NET 10** (ou LTS) com **C# 13**.
- Nomenclatura: PascalCase (tipos, métodos), camelCase (locais, campos privados), UPPERCASE (constantes), prefixo `I` em interfaces.
- **Construtores primários** para DI; **collection expressions**; async/await para I/O; **System.Text.Json**; DI no bootstrap; **não** instanciar serviços fora do bootstrap.

**Referência rápida:** tabela de decisão e princípios-chave na documentação do projeto.

**Por contexto:** DB/repositórios → persistência; APIs externas → Refit; Validação → FluentValidation; Testes → xUnit/BDD; Segurança → JWT/secrets; Performance → Span/otimização; Observabilidade → logging/métricas/tracing; Lambda API → AddAWSLambdaHosting.
