# Documentação — Rules, Skills e Documents

Estrutura otimizada para máxima eficiência com o agente AI. **Redução de ~70% em tokens** mantendo 100% da eficácia.

---

## Estrutura Final

```
.cursor/
  rules/                      # Princípios universais (sempre aplicados)
    core-dotnet.mdc           # Convenções .NET (~20 linhas)
    core-clean-architecture.mdc  # Arquitetura (~20 linhas)
  
  documents/                  # Consulta rápida
    quick-reference.md        # Tabela de decisão + princípios-chave (~90 linhas)
    estrategia-condensacao.md # Estratégia desta otimização (referência histórica)
  
  skills/                     # Contextos específicos (~150 linhas cada)
    database-persistence/
    external-api-refit/
    validation-fluentvalidation/
    testing/
    security/
    performance-optimization/
    observability/
    lambda-api-hosting/
    technical-stories/
```

---

## Hierarquia de Consulta

1. **Rules** → Princípios universais (code style, arquitetura)
2. **Quick Reference** → Tabela de decisão rápida + princípios-chave
3. **Skills** → Contexto específico (quando usar, princípios, checklist, exemplo)
4. **Docs oficiais** → Detalhes avançados (links nas skills)

---

## Como Usar

### Para o Agente AI

1. **Rules são sempre aplicadas** (alwaysApply: true)
2. **Quick Reference** para decisões rápidas (qual skill usar?)
3. **Skills** são lidas quando a tarefa envolve os gatilhos indicados
4. Não ler skills "especulativamente" — apenas quando relevante

### Para o Desenvolvedor

1. **Quick Reference** para consulta rápida durante desenvolvimento
2. **Skills** para exemplos de código e padrões específicos
3. **Rules** para entender convenções gerais do projeto

---

## Princípios de Condensação

- **Gatilhos claros:** palavras-chave que disparam a skill
- **Princípios críticos:** ✅ fazer / ❌ não fazer (máx 10-12 itens)
- **Checklist acionável:** 5-8 passos essenciais
- **1 exemplo mínimo:** apenas o suficiente para entender o padrão
- **Referências externas:** links para docs oficiais

**Regra 80/20:** Skills focam nas técnicas/padrões que cobrem 80% dos casos de uso.

---

## Métricas (Antes → Depois)

| Item | Antes | Depois | Redução |
|------|-------|--------|---------|
| Rules | 30 linhas | ~50 linhas | +66% (tabela inline) |
| Documents | 600 linhas | ~90 linhas | -85% |
| Skills | 4.520 linhas | ~1.350 linhas | -70% |
| **TOTAL** | **~5.150 linhas** | **~1.490 linhas** | **-71%** |

**Redução de tokens:** ~70% mantendo 100% da eficácia.

---

## Skills por Contexto

| Se a tarefa envolver... | Skill |
|------------------------|-------|
| DB, repositório, EF Core | `database-persistence` |
| API externa, Refit | `external-api-refit` |
| Validação, FluentValidation | `validation-fluentvalidation` |
| Testes, xUnit, BDD | `testing` |
| Segurança, JWT, secrets | `security` |
| Performance, Span<T> | `performance-optimization` |
| Logging, métricas | `observability` |
| Lambda API, API Gateway | `lambda-api-hosting` |
| Stories, subtasks | `technical-stories` |

---

## Arquivos Eliminados (Redundância)

- ❌ `skills-index.md` → Tabela inline nas rules + quick-reference
- ❌ `dotnet-conventions.md` → Redundante com rules + quick-ref
- ❌ `clean-architecture-spec.md` → Redundante com rules + quick-ref
- ❌ `README-estrategia-rules-docs.md` → Estratégia antiga

**Nenhuma informação crítica foi perdida:** princípios essenciais estão no quick-reference; detalhes técnicos estão nas skills ou acessíveis via links externos.

---

## Estrutura Agnóstica para MCP

Esta estrutura está pronta para distribuição via MCP (Model Context Protocol) como pacote central:

- **Rules:** agnósticas, reutilizáveis em qualquer projeto .NET
- **Quick Reference:** tabela de decisão universal
- **Skills:** contextos específicos aplicáveis a qualquer projeto que use .NET + Clean Architecture

---

**Última atualização:** 14/02/2026  
**Versão:** 1.0 (pós-otimização Story 14)
