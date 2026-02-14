# Quick Reference — Rules, Skills e Princípios

**Consulta rápida de 1 página** para decisões de skill, princípios-chave .NET e Clean Architecture.

---

## Tabela de Decisão Rápida

| Se a tarefa envolver... | Use a skill |
|------------------------|-------------|
| DB, repositório, EF Core, queries, migrations | **database-persistence** |
| API externa, Refit, HttpClient, consumir API | **external-api-refit** |
| Validação, FluentValidation, validators, InputModels | **validation-fluentvalidation** |
| Testes, xUnit, BDD, cobertura, build | **testing** |
| Segurança, JWT, autenticação, secrets | **security** |
| Performance, Span<T>, otimização, alocações | **performance-optimization** |
| Logging, métricas, tracing, health checks | **observability** |
| Lambda API, AddAWSLambdaHosting, API Gateway | **lambda-api-hosting** |
| Criar/desenvolver stories, subtasks | **technical-stories** |

---

## Princípios-Chave .NET

1. **Nomenclatura:** PascalCase (tipos/métodos), camelCase (locais/campos), UPPERCASE (constantes), prefixo `I` (interfaces)
2. **C# moderno:** Construtores primários para DI, collection expressions (`[item1, item2]`), async/await
3. **DI:** Serviços registrados no bootstrap (`Program.cs`), **nunca** instanciar manualmente
4. **JSON:** `System.Text.Json` (nunca Newtonsoft)
5. **Async:** Sempre para I/O, evitar `async void`, usar `CancellationToken`
6. **.NET 10+ com C# 13** (ou LTS mais recente)
7. **Imutabilidade:** Preferir `record` types para DTOs/InputModels/OutputModels

---

## Princípios-Chave Clean Architecture

1. **Camadas:** Api → Application → Domain; Infra implementa Ports
2. **Domain sem dependências:** Nenhum framework externo (EF, ASP.NET, HttpClient, etc.)
3. **Fluxo:** Controller recebe InputModel (body) → UseCase → Presenter → ResponseModel
4. **Não criar RequestModels:** InputModel é o contrato único (body)
5. **Dados de rota/auth:** Separados do InputModel (parâmetros do UseCase)
6. **Controller minimalista:** Receber input, chamar UseCase, retornar resultado (filtro padroniza resposta)
7. **Repositórios:** Interface na Application/Ports, implementação na Infra.Persistence
8. **Validators:** Validam formato (FluentValidation), UseCases validam negócio

---

## Quando Ler Skill vs Perguntar

**Ler skill:**
- Tarefa claramente envolve um dos gatilhos da tabela acima
- Precisa de exemplos de código ou configuração específica
- Quer entender padrões/convenções para um contexto

**Perguntar:**
- Há dúvida sobre abordagem ou trade-offs
- Múltiplas skills aplicáveis e não está claro qual usar
- Precisa de decisão arquitetural (não apenas implementação)

**Quick-ref:**
- Consultas rápidas durante implementação
- Verificar nomenclatura, princípios-chave
- Lembrar qual skill usar

---

## Hierarquia de Consulta

```
Rules (core-dotnet, core-clean-architecture)
  ↓ (princípios universais, agnósticos)
Quick Reference
  ↓ (tabela de decisão + princípios-chave)
Skills (database-persistence, external-api-refit, ...)
  ↓ (contexto específico: quando usar, princípios, checklist, exemplo)
Documentação oficial externa
  ↓ (detalhes avançados, referência completa)
```

---

## Anti-Patterns Críticos (Nunca Fazer)

### .NET
- ❌ **Nunca** instanciar serviços fora do bootstrap (DI container resolve)
- ❌ **Nunca** usar `Newtonsoft.Json` (preferir `System.Text.Json`)
- ❌ **Nunca** `async void` (exceto event handlers)
- ❌ **Nunca** queries síncronas (ToList(), First() — sempre async)

### Clean Architecture
- ❌ **Nunca** expor DbContext para Application ou API (usar repositórios)
- ❌ **Nunca** lógica de negócio em Controllers (pertence a UseCases)
- ❌ **Nunca** criar RequestModels (InputModel é o contrato único)
- ❌ **Nunca** acessar banco de dados em Validators (apenas formato/estrutura)

### Segurança
- ❌ **Nunca** commitar secrets no código/appsettings
- ❌ **Nunca** logar dados sensíveis (senhas, tokens, CPF, cartão)
- ❌ **Nunca** expor stack traces em produção

### Performance
- ❌ **Nunca** otimizar sem profiling (medição real)
- ❌ **Nunca** usar `Span<T>` em métodos async (usar `Memory<T>`)
- ❌ **Nunca** esquecer de devolver arrays ao `ArrayPool` (usar `try/finally`)

---

**Última atualização:** 14/02/2026  
**Versão:** 1.0 (pós-condensação)
