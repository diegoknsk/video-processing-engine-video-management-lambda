# Regras globais — .NET 10 + Clean Architecture

Exportado a partir das rules do Cursor para uso no Antigravity (Gemini).  
Use como regras globais: copie o conteúdo para `~/.gemini/GEMINI.md` (ou no Antigravity: Customizations → + Global).

---

## Core .NET — Convenções Universais

- Código **conciso e idiomático**, seguindo convenções Microsoft; **.NET 10** (ou LTS) com **C# 13**.
- Nomenclatura: PascalCase (tipos, métodos), camelCase (locais, campos privados), UPPERCASE (constantes), prefixo `I` em interfaces.
- **Construtores primários** para DI; **collection expressions**; async/await para I/O; **System.Text.Json**; DI no bootstrap; **não** instanciar serviços fora do bootstrap.

**Referência rápida:** tabela de decisão e princípios-chave em documentação do projeto (quick-reference).

**Skills por contexto:** aplicar conforme a tarefa:
- DB/repositórios → persistência (EF Core, repositórios, migrations)
- APIs externas → Refit / HttpClient
- Validação → FluentValidation, InputModels
- Testes → xUnit, BDD, cobertura
- Segurança → JWT, secrets, autenticação
- Performance → Span<T>, otimização, alocações
- Observabilidade → logging, métricas, tracing, health checks
- Lambda API → AddAWSLambdaHosting, API Gateway

---

## Clean Architecture Core — Estrutura Universal

- **Camadas:** Api → Application → Domain; Infra.* implementa Ports; Domain sem dependências externas.
- **Fluxo:** Controller recebe InputModel no body; chama UseCase com dados de rota/auth separados; UseCase chama Ports, Presenter e retorna ResponseModel. **Não** criar RequestModels; InputModel é contrato único.
- **Controllers:** apenas receber InputModel, extrair rota/auth, chamar UseCase, retornar resultado; filtro/middleware padronizam respostas e erros.

**Referência rápida:** princípios-chave e tabela de decisão na documentação do projeto (quick-reference).

**Skills por contexto:** aplicar conforme a tarefa:
- Persistência → repositórios, EF Core, DbContext
- API externa → Refit, HttpClient
- Validação → FluentValidation, validators, InputModels
- Testes → xUnit, BDD, cobertura
- Stories → criação e desenvolvimento de stories técnicas

---

## Princípios-Chave (resumo)

**.NET:** PascalCase/camelCase/UPPERCASE; C# 13; construtores primários; System.Text.Json; async/await para I/O; DI só no bootstrap.

**Clean Architecture:** Api → Application → Domain; Domain sem deps externas; InputModel único (sem RequestModels); Controller minimalista; dados de rota/auth separados do body.
