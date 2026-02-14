# Estratégia de Condensação — Rules, Documents e Skills

**Data:** 14/02/2026  
**Objetivo:** Reduzir ~70% do tamanho mantendo 100% da eficácia

---

## 1. Análise do Estado Atual

### Métricas Atuais

| Tipo | Arquivos | Linhas Totais | Observações |
|------|----------|---------------|-------------|
| Rules | 2 | ~30 (15 cada) | ✅ Já enxutos e agnósticos |
| Documents | 4 | ~600 | ⚠️ Redundância com rules/skills |
| Skills | 9 | ~4.520 | ⚠️ Muito extensos (300-788 linhas cada) |
| **TOTAL** | **15** | **~5.150** | 🎯 Meta: ~1.600 linhas |

### Detalhamento por Skill

| Skill | Linhas Atuais | Meta | Status |
|-------|---------------|------|--------|
| security | 788 | ~150 | 🔴 Reduzir 81% |
| testing | 747 | ~150 | 🔴 Reduzir 80% |
| observability | 653 | ~150 | 🔴 Reduzir 77% |
| performance-optimization | 638 | ~150 | 🔴 Reduzir 76% |
| validation-fluentvalidation | 555 | ~150 | 🔴 Reduzir 73% |
| external-api-refit | 488 | ~150 | 🔴 Reduzir 69% |
| database-persistence | 410 | ~150 | 🔴 Reduzir 63% |
| lambda-api-hosting | 136 | ~130 | 🟢 Manter (já enxuta) |
| technical-stories | 105 | 105 | 🟢 Não alterar |

---

## 2. Template Padrão de Skill Condensada

### Estrutura (máx ~150 linhas)

```markdown
---
name: skill-name
description: Descrição concisa com gatilhos principais (1 linha). Use quando...
---

# [Nome da Skill]

## Quando Usar

- Lista concisa de gatilhos (5-8 bullets máx)
- Palavras-chave principais

## Princípios Essenciais

### ✅ Fazer
- Princípio crítico 1
- Princípio crítico 2
- Princípio crítico 3 (máx 5-6 itens)

### ❌ Não Fazer
- Anti-pattern crítico 1
- Anti-pattern crítico 2
- Anti-pattern crítico 3 (máx 5-6 itens)

## Checklist Rápido

1. Passo essencial 1
2. Passo essencial 2
3. Passo essencial 3
4. Passo essencial 4
5. Passo essencial 5 (máx 8 passos)

## Exemplo Mínimo

**Cenário:** [descrição em 1 linha do problema]

```csharp
// Código essencial (20-40 linhas máx)
// Apenas o suficiente para entender o padrão
```

**Pontos-chave:**
- Explicação crítica 1
- Explicação crítica 2 (máx 3-4 pontos)

## Referências

- [Documentação oficial](URL)
- [Artigo relevante](URL) (máx 2-3 links)
```

### Seções a Remover

- ❌ Múltiplos exemplos completos (manter apenas 1)
- ❌ Troubleshooting extenso (apenas críticos)
- ❌ Explicações redundantes ou "nice to have"
- ❌ Código comentado linha por linha (apenas pontos-chave)
- ❌ Seções de "Melhores práticas" se já cobertas em princípios

### Seções a Manter

- ✅ Frontmatter (name, description, gatilhos)
- ✅ Quando usar (gatilhos expandidos)
- ✅ Princípios críticos (✅/❌)
- ✅ Checklist rápido (máx 8 passos)
- ✅ 1 exemplo minimal representativo
- ✅ Referências essenciais (links externos)

---

## 3. Estratégia por Skill

### Grupo 1: Validação, API Externa, Persistência (Subtask 02)

**validation-fluentvalidation (555 → 150 linhas)**
- Manter: princípios (o que validar/não validar), estrutura, 1 exemplo de validator
- Remover: múltiplos exemplos, validators complexos, troubleshooting extenso
- Adicionar: link para FluentValidation docs

**external-api-refit (488 → 150 linhas)**
- Manter: por que Refit, estrutura, 1 exemplo completo (interface + service + DI)
- Remover: exemplos de resilience patterns (apenas mencionar Polly), HTTP/3 detalhes
- Adicionar: link para Refit e Polly docs

**database-persistence (410 → 150 linhas)**
- Manter: princípios (repositórios, queries eficientes), estrutura, 1 exemplo de repositório
- Remover: múltiplos exemplos de queries, migrations extensas, troubleshooting
- Adicionar: link para EF Core docs

### Grupo 2: Observabilidade, Performance, Segurança (Subtask 03)

**observability (653 → 150 linhas)**
- Manter: logging estruturado (princípios), health checks, 1 exemplo de logging
- Remover: OpenTelemetry completo (apenas mencionar), métricas customizadas extensas
- Adicionar: link para Serilog/OpenTelemetry docs
- Princípio 80/20: logging é 80% do uso

**performance-optimization (638 → 150 linhas)**
- Manter: Span<T>, Memory<T>, ArrayPool, ValueTask (mais comuns)
- Remover: técnicas avançadas (SIMD, unsafe code), benchmarks extensos
- Adicionar: link para Performance best practices Microsoft
- Princípio 80/20: esses 4 padrões cobrem 80% dos casos

**security (788 → 150 linhas)**
- Manter: JWT (config + geração), secrets management, rate limiting básico
- Remover: CORS detalhado, security headers extensos, múltiplos exemplos
- Adicionar: link para Microsoft Security docs
- Princípio 80/20: JWT + secrets são 80% do uso

### Grupo 3: Testes, Lambda API (Subtask 04)

**testing (747 → 150 linhas)**
- Manter: estrutura (xUnit), padrão AAA, 1 exemplo unitário, BDD mínimo, cobertura
- Remover: múltiplos exemplos de testes, mocking avançado, mutation testing extenso
- Adicionar: link para xUnit e SpecFlow docs

**lambda-api-hosting (136 → 130 linhas)**
- Já enxuta! Apenas remover redundâncias mínimas
- Manter: AddAWSLambdaHosting, GATEWAY_PATH_PREFIX, GATEWAY_STAGE, OpenAPI
- Revisão leve

**technical-stories (105 linhas)**
- 🚫 **Não alterar** (já otimizada e específica)

---

## 4. Documents: Eliminar ou Mesclar

### Decisão por Document

| Document | Decisão | Justificativa |
|----------|---------|---------------|
| `skills-index.md` | ❌ **Eliminar** | Tabela será inline nas rules (core-dotnet/clean-arch) |
| `dotnet-conventions.md` | ❌ **Eliminar** | Redundante com rules + quick-ref será suficiente |
| `clean-architecture-spec.md` | ❌ **Eliminar** | Redundante com rules + quick-ref será suficiente |
| `README-estrategia-rules-docs.md` | ❌ **Eliminar** | Estratégia antiga, substituída por este doc |
| `estrategia-condensacao.md` | ⚠️ **Mover** | Pode ir para storys/Storie-14/docs/ (referência histórica) |

**Nenhum conteúdo crítico será perdido:**
- Princípios essenciais de `dotnet-conventions.md` → `quick-reference.md`
- Princípios essenciais de `clean-architecture-spec.md` → `quick-reference.md`
- Detalhes técnicos → já estão nas skills ou são acessíveis via links externos

### Novo Document a Criar

**`quick-reference.md` (~80 linhas)**
- Tabela de decisão: gatilhos → skill (9 skills)
- Princípios-chave .NET (5-8 itens mais críticos)
- Princípios-chave Clean Architecture (5-8 itens mais críticos)
- Quando ler skill vs quando perguntar
- Máximo 80 linhas (1 página escaneável)

---

## 5. Rules: Atualização

### Mudanças nas Rules

**core-dotnet.mdc (~15 → ~25 linhas)**
- Remover referência a `skills-index.md`
- Adicionar tabela inline de gatilhos → skills (formato compacto)
- Adicionar referência a `quick-reference.md`
- Permanecer agnóstica e reutilizável

**core-clean-architecture.mdc (~15 → ~25 linhas)**
- Mesma abordagem: tabela inline + quick-ref
- Permanecer agnóstica e reutilizável

**Exemplo de tabela inline nas rules:**

```markdown
**Skills por contexto:** consultar quick-reference.md ou:
- DB/repositórios → database-persistence
- APIs externas → external-api-refit
- Validação → validation-fluentvalidation
- Testes → testing
- Segurança → security
- Performance → performance-optimization
- Observabilidade → observability
- Lambda API → lambda-api-hosting
```

---

## 6. Quick-Reference: Estrutura

### Conteúdo (máx 80 linhas)

```markdown
# Quick Reference — Rules, Skills e Princípios

## Tabela de Decisão Rápida

| Se a tarefa envolver... | Use a skill |
|------------------------|-------------|
| DB, repositório, EF Core, queries | database-persistence |
| API externa, Refit, HttpClient | external-api-refit |
| Validação, FluentValidation | validation-fluentvalidation |
| Testes, xUnit, BDD, cobertura | testing |
| Segurança, JWT, secrets | security |
| Performance, Span<T>, otimização | performance-optimization |
| Logging, métricas, tracing | observability |
| Lambda API, API Gateway, OpenAPI | lambda-api-hosting |
| Criar/desenvolver stories | technical-stories |

## Princípios-Chave .NET

1. **Nomenclatura:** PascalCase (tipos/métodos), camelCase (locais), UPPERCASE (constantes)
2. **C# moderno:** Construtores primários, collection expressions, async/await
3. **DI:** Serviços registrados no bootstrap (Program.cs), nunca instanciar manualmente
4. **JSON:** System.Text.Json (nunca Newtonsoft)
5. **Async:** Sempre para I/O, evitar async void, usar CancellationToken
6. **.NET 10+** com C# 13 (ou LTS mais recente)

## Princípios-Chave Clean Architecture

1. **Camadas:** Api → Application → Domain; Infra implementa Ports
2. **Domain sem dependências:** Nenhum framework externo (EF, ASP.NET, etc.)
3. **Fluxo:** Controller recebe InputModel → UseCase → Presenter → ResponseModel
4. **Não criar RequestModels:** InputModel é o contrato único (body)
5. **Dados de rota/auth:** Separados do InputModel (parâmetros do UseCase)
6. **Controller minimalista:** Receber input, chamar UseCase, retornar resultado

## Quando Ler Skill vs Perguntar

- **Ler skill:** Quando a tarefa claramente envolve um dos gatilhos acima
- **Perguntar:** Quando há dúvida sobre abordagem ou trade-offs
- **Quick-ref:** Para consultas rápidas durante implementação
```

---

## 7. Validação da Estratégia

### Checklist de Validação

- ✅ Template de skill condensada definido (máx ~150 linhas)
- ✅ Decisão clara sobre documents (4 a eliminar, 1 a criar)
- ✅ Estrutura do quick-reference definida (~80 linhas)
- ✅ Nenhuma informação crítica será perdida:
  - Princípios essenciais → quick-reference
  - Decisões técnicas críticas → mantidas nas skills condensadas
  - Detalhes avançados → links para docs oficiais
- ✅ Estratégia por skill definida (grupos 1, 2, 3)
- ✅ Rules permanecerão agnósticas (~25 linhas com tabela inline)

### Métricas Esperadas (Antes → Depois)

| Item | Antes | Depois | Redução |
|------|-------|--------|---------|
| Rules | 30 linhas | ~50 linhas | +66% (tabela inline) |
| Documents | 600 linhas | ~80 linhas | -87% |
| Skills | 4.520 linhas | ~1.350 linhas | -70% |
| **TOTAL** | **~5.150 linhas** | **~1.480 linhas** | **-71%** |

**Redução de tokens estimada:** ~70% mantendo 100% da eficácia

---

## 8. Plano de Execução

1. **Subtask 02:** Condensar validation, external-api, database-persistence
2. **Subtask 03:** Condensar observability, performance, security
3. **Subtask 04:** Condensar testing, revisar lambda-api-hosting
4. **Subtask 05:** Criar quick-reference, atualizar rules
5. **Subtask 06:** Eliminar obsoletos, validar estrutura, criar README

---

## 9. Princípios de Condensação

### Regra 80/20
- Focar nas técnicas/padrões que cobrem 80% dos casos de uso
- Remover técnicas avançadas que são <20% de uso
- Links externos para detalhes avançados

### Gatilhos Claros
- Frontmatter descritivo com palavras-chave
- Seção "Quando Usar" concisa (5-8 bullets)
- Agente deve reconhecer rapidamente quando aplicar

### Decisões Críticas Visíveis
- Formato ✅/❌ para princípios e anti-patterns
- Máximo 10-12 itens (5-6 cada)
- Foco em decisões que impactam arquitetura/segurança/performance

### Checklist Acionável
- Máximo 8 passos
- Cada passo deve ser claro e acionável
- Ordem lógica de execução

### Exemplo Minimal Representativo
- 1 exemplo apenas (20-40 linhas de código)
- Cobrir o cenário mais comum (80%)
- Pontos-chave explicados (máx 3-4 bullets)

### Referências Externas
- Máximo 2-3 links
- Docs oficiais priorizados
- Artigos/guias quando apropriado

---

**Próximo passo:** Executar Subtask 02 — Condensar as 3 primeiras skills.
