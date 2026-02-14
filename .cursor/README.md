# Cursor Rules & Skills — Estrutura Modular

Este projeto utiliza uma estrutura **híbrida** de regras e skills para desenvolvimento .NET com Clean Architecture.

## 📁 Estrutura

```
.cursor/
  rules/
    ├── core-dotnet.mdc                     # ✅ Sempre aplicada
    └── core-clean-architecture.mdc         # ✅ Sempre aplicada
  skills/
    ├── database-persistence/
    │   └── SKILL.md
    ├── external-api-refit/
    │   └── SKILL.md
    ├── validation-fluentvalidation/
    │   └── SKILL.md
    ├── testing/
    │   └── SKILL.md
    ├── observability/
    │   └── SKILL.md
    ├── performance-optimization/
    │   └── SKILL.md
    └── security/
        └── SKILL.md
```

## 🎯 Como Funciona

### Rules Core (Sempre Aplicadas)

**Arquivos pequenos (~100-150 linhas)** com convenções essenciais que se aplicam a **todos** os contextos:

- **`core-dotnet.mdc`**: Convenções de código C#/.NET (nomenclatura, sintaxe moderna, async, JSON, DI, API design)
- **`core-clean-architecture.mdc`**: Estrutura de projetos, fluxo de dados, separação de camadas

### Skills Especializadas (Chamadas sob Demanda)

**Arquivos detalhados** com guias completos para contextos específicos:

| Skill | Quando Usar | Gatilhos |
|-------|-------------|----------|
| **database-persistence** | Trabalho com banco de dados | "banco", "repositório", "EF Core", "migration", "DbContext" |
| **external-api-refit** | Integração com APIs externas | "API externa", "Refit", "HttpClient", "consumir API" |
| **validation-fluentvalidation** | Validação de inputs | "validar", "FluentValidation", "InputModel", "regras" |
| **testing** | Testes e qualidade | "teste", "BDD", "cobertura", "xUnit", "build" |
| **observability** | Logging, métricas, tracing | "logging", "métricas", "OpenTelemetry", "health check" |
| **performance-optimization** | Otimização de performance | "performance", "otimizar", "Span", "alocações" |
| **security** | Segurança e proteção | "segurança", "JWT", "autenticação", "secrets" |

## 🚀 Como Usar

### Para Desenvolvedores

Quando trabalhar em uma tarefa:

1. **Rules core** são aplicadas automaticamente (sempre ativas)
2. **Skills** são consultadas quando você trabalha em contextos específicos:
   - Criando repositório? → Consulte `database-persistence`
   - Integrando API externa? → Consulte `external-api-refit`
   - Criando testes? → Consulte `testing`

### Para o Cursor AI

O Cursor AI automaticamente:
- **Aplica rules core** em todas as interações
- **Identifica contexto** baseado em palavras-chave da tarefa
- **Lê a skill apropriada** quando necessário

Exemplo:
```
User: "Criar repositório de usuários com EF Core"
AI: 
  1. Aplica core-dotnet.mdc e core-clean-architecture.mdc
  2. Identifica gatilho: "repositório", "EF Core"
  3. Lê skill: database-persistence/SKILL.md
  4. Implementa seguindo as três fontes
```

## 📊 Comparação: Antes vs Depois

### Antes (Monolítico)

```
❌ dotnet-development-universal.mdc (451 linhas)
❌ clean-architecture-universal.mdc (395 linhas)
Total: 846 linhas em 2 arquivos sempre carregados
```

**Problemas:**
- Arquivos grandes e difíceis de manter
- Duplicação de conteúdo (Refit, testes, observabilidade em ambos)
- Tudo carregado sempre, mesmo quando não relevante
- Difícil de distribuir e reutilizar

### Depois (Modular)

```
✅ Rules Core (2 arquivos, ~250 linhas total, sempre aplicadas)
  - core-dotnet.mdc (150 linhas)
  - core-clean-architecture.mdc (100 linhas)

✅ Skills Especializadas (7 arquivos, ~2800 linhas total, chamadas sob demanda)
  - database-persistence (400 linhas)
  - external-api-refit (350 linhas)
  - validation-fluentvalidation (400 linhas)
  - testing (450 linhas)
  - observability (450 linhas)
  - performance-optimization (400 linhas)
  - security (350 linhas)
```

**Benefícios:**
- ✅ Rules core leves (sempre carregadas, mas pequenas)
- ✅ Skills detalhadas (carregadas apenas quando relevante)
- ✅ Zero duplicação (cada tópico em um único lugar)
- ✅ Fácil manutenção (1 skill = 1 responsabilidade)
- ✅ Agnóstico e reutilizável (pode ser distribuído via MCP)
- ✅ Escalável (fácil adicionar novas skills sem impactar existentes)

## 🔄 Migração

Os arquivos antigos foram **removidos** (conteúdo versionado no histórico do repositório):

- ~~`dotnet-development-universal.mdc`~~ → Substituído por `core-dotnet.mdc` + skills
- ~~`clean-architecture-universal.mdc`~~ → Substituído por `core-clean-architecture.mdc` + skills

## 📦 Distribuição via MCP

Esta estrutura foi projetada para ser **distribuída via MCP** (Model Context Protocol):

```yaml
# Projeto centralizado de rules e skills
cursor-rules-dotnet/
  .cursor/
    rules/
      - core-dotnet.mdc
      - core-clean-architecture.mdc
    skills/
      - database-persistence/
      - external-api-refit/
      - validation-fluentvalidation/
      - testing/
      - observability/
      - performance-optimization/
      - security/

# Projetos consumidores
meu-projeto-1/  → consome via MCP
meu-projeto-2/  → consome via MCP
meu-projeto-3/  → consome via MCP
```

**Vantagens:**
- ✅ Um único repositório centralizado
- ✅ Atualizações propagadas para todos os projetos
- ✅ Consistência garantida entre projetos
- ✅ Fácil adicionar novos projetos

## 🎓 Boas Práticas

### Quando Adicionar Nova Skill

Crie uma nova skill quando:
- ✅ Tópico é **especializado** (não se aplica a todos os contextos)
- ✅ Conteúdo é **detalhado** (≥ 200 linhas com exemplos completos)
- ✅ Tem **gatilhos claros** (palavras-chave específicas)

**Não** crie skill para:
- ❌ Convenções básicas (vão nas rules core)
- ❌ Tópicos muito pequenos (< 100 linhas)
- ❌ Conteúdo genérico (vão nas rules core)

### Quando Atualizar Rules Core

Atualize rules core quando:
- ✅ Convenção se aplica a **todos** os contextos
- ✅ Mudança em versão do .NET/C# (ex.: C# 14 features)
- ✅ Padrão universal (nomenclatura, estrutura de projetos)

## 📝 Changelog

### 2026-02-09 — Refatoração Modular

- ✅ Criadas rules core (`core-dotnet.mdc`, `core-clean-architecture.mdc`)
- ✅ Criadas 7 skills especializadas
- ✅ Redução de duplicação: 0%
- ✅ Estrutura preparada para distribuição via MCP
- ✅ Seção "Quando Usar Skills" revisada com tabela de gatilhos e caminhos nas duas rules
- ✅ Arquivos antigos removidos (`dotnet-development-universal.mdc`, `clean-architecture-universal.mdc`)

---

**Mantido por:** Diego  
**Versão:** 1.0.0  
**Data:** 09/02/2026
