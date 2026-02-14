# Referência — Estrutura de story e subtasks

## Nomenclatura

- **Pasta da story:** `Storie-XX-Descricao_Breve` (XX com 2 dígitos; underscore entre palavras; PascalCase por palavra). Ex.: `Storie-01-Implementar_Autenticacao_Usuario`.
- **Arquivos de subtask:** `Subtask-01-Nome.md`, `Subtask-02-Nome.md`, etc., dentro de `subtask/`.

---

## Estrutura obrigatória do story.md

```markdown
# Storie-XX: Título da História

## Status
- **Estado:** 🔄 Em desenvolvimento | ✅ Concluída | ⏸️ Pausada
- **Data de Conclusão:** [DD/MM/AAAA] (preencher quando concluída)

## Descrição
Como [papel], quero [ação], para [benefício].

## Objetivo
[O que será entregue — resultado final]

## Escopo Técnico
- Tecnologias: [tecnologias e versões]
- Arquivos afetados: [caminhos]
- Componentes/Recursos: [módulos criados/modificados]
- Pacotes/Dependências: [nome e versão de cada pacote externo]

## Dependências e Riscos (para estimativa)
- Dependências: [outras stories, APIs, serviços]
- Riscos/Pré-condições: [riscos e pré-condições]

## Subtasks
- [Subtask 01: Nome](./subtask/Subtask-01-Nome.md)
- [Subtask 02: Nome](./subtask/Subtask-02-Nome.md)
- [Subtask 03: Nome](./subtask/Subtask-03-Nome.md)

## Critérios de Aceite da História
- [ ] Critério 1 (específico e mensurável)
- [ ] Critério 2 (específico e mensurável)
- [ ] ... (mínimo 5; se envolve código, incluir testes unitários)

## Rastreamento (dev tracking)
- **Início:** dia DD/MM/AAAA, às HH:MM (Brasília)
- **Fim:** —
- **Tempo total de desenvolvimento:** —
```

---

## Regras do story.md

- **Status:** sempre preenchido; ao concluir: "✅ Concluída" + data DD/MM/AAAA.
- **Descrição:** formato "Como [papel], quero [ação], para [benefício]".
- **Objetivo:** O QUE será entregue, não COMO.
- **Escopo Técnico:** listar todos os pacotes externos com **nome e versão**.
- **Subtasks:** mínimo 3, máximo 8; links relativos `./subtask/Subtask-XX-Nome.md`.
- **Critérios de Aceite:** mínimo 5; específicos e mensuráveis; se há código, incluir testes unitários (ex.: "Testes unitários passando; cobertura ≥ 80%").

---

## Formato de cada arquivo de subtask

Cada `Subtask-XX-Nome.md` deve ter:

- **Descrição** clara do que será feito.
- **Passos de implementação** (mínimo 3): ordem lógica e verificável.
- **Formas de teste** (mínimo 3): como validar (unitário, manual, integração).
- **Critérios de aceite da subtask** (mínimo 3): específicos e mensuráveis.
- Se envolve código: incluir criação/atualização de testes unitários nos passos ou critérios.

Subtasks devem ser implementáveis em 1–2 horas (ideal), testáveis de forma independente.

---

## Exemplo resumido de story.md

```markdown
# Storie-05: Implementar Autenticação de Usuário

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** [DD/MM/AAAA]

## Descrição
Como usuário do sistema, quero fazer login com email e senha, para acessar funcionalidades restritas da aplicação.

## Objetivo
Implementar fluxo completo de autenticação: endpoint de login, validação de credenciais, emissão de token e proteção de rotas que exigem autenticação.

## Escopo Técnico
- Tecnologias: .NET 8, ASP.NET Core, JWT
- Arquivos afetados: `src/Api/Controllers/AuthController.cs`, `src/Application/UseCases/Auth/`, `src/Infra/Services/TokenService.cs`
- Componentes: AuthController, LoginUseCase, ITokenService, RequireAuth filter
- Pacotes/Dependências:
  - Microsoft.AspNetCore.Authentication.JwtBearer (8.0.0)
  - System.IdentityModel.Tokens.Jwt (7.0.0)

## Dependências e Riscos (para estimativa)
- Dependências: Nenhuma outra story; depende do contrato de login (request/response).
- Riscos: Nenhum crítico; pré-condição: contrato de API definido.

## Subtasks
- [Subtask 01: Criar InputModel e validator de login](./subtask/Subtask-01-InputModel_Validator_Login.md)
- [Subtask 02: Implementar UseCase e integração com serviço de token](./subtask/Subtask-02-UseCase_Login.md)
- [Subtask 03: Criar endpoint e filter de autenticação](./subtask/Subtask-03-Endpoint_Filter_Auth.md)
- [Subtask 04: Testes unitários](./subtask/Subtask-04-Testes_Unitarios_Auth.md)

## Critérios de Aceite da História
- [ ] Endpoint POST /auth/login aceita email e senha e retorna token JWT
- [ ] Validação de input (FluentValidation) retorna 400 com mensagens claras quando inválido
- [ ] Token JWT contém claims esperados (sub, exp, etc.)
- [ ] Rotas protegidas retornam 401 sem token válido
- [ ] Testes unitários cobrindo UseCase, Validator e Controller; cobertura mínima 80%
- [ ] Documentação Swagger atualizada para o endpoint de login
- [ ] Tratamento de erro para credenciais inválidas (401) e servidor indisponível (503)

## Rastreamento (dev tracking)
- **Início:** —
- **Fim:** —
- **Tempo total de desenvolvimento:** —
```

---

## Checklist completo (criação)

### Estrutura
- [ ] Pasta `storys/` existe na raiz; story em `storys/Storie-XX-Descricao/`
- [ ] Arquivo `story.md` na raiz da pasta da story
- [ ] Pasta `subtask/` criada; todas as subtasks em arquivos `Subtask-XX-Nome.md`

### Conteúdo do story.md
- [ ] Status preenchido (estado; data de conclusão quando aplicável)
- [ ] Título no formato `# Storie-XX: Título`
- [ ] Descrição "Como... quero... para..."
- [ ] Objetivo claro; escopo técnico com pacotes/dependências (nome e versão)
- [ ] Dependências e riscos descritos
- [ ] Mínimo 3 subtasks listadas; links `./subtask/Subtask-XX-Nome.md` funcionando
- [ ] Mínimo 5 critérios de aceite; específicos e mensuráveis
- [ ] Se envolve código: critérios de aceite incluem testes unitários
- [ ] Seção Rastreamento (dev tracking) presente (pode estar com campos em branco)

### Conteúdo das subtasks
- [ ] Descrição clara; mínimo 3 passos de implementação
- [ ] Mínimo 3 formas de teste; mínimo 3 critérios de aceite por subtask
- [ ] Ordem lógica; se envolve código: incluir testes unitários

### Qualidade
- [ ] Nomenclatura consistente (Storie-XX, Subtask-XX, underscore na descrição)
- [ ] Linguagem clara; termos técnicos corretos

---

## Erros comuns a evitar

1. Criar story **fora** de `storys/` — sempre dentro de `storys/Storie-XX-Descricao/`.
2. Número com 1 dígito (ex.: Storie-1) — usar 2 dígitos (Storie-01).
3. Hífen na descrição da pasta (ex.: Storie-01-backend-api) — usar underscore.
4. Subtasks muito grandes — quebrar em várias (ideal 1–2 h cada).
5. Critérios vagos ("código funciona") — usar critérios executáveis (ex.: "dotnet test passa; cobertura ≥ 80%").
6. Links quebrados — sempre `./subtask/Subtask-XX-Nome.md`.
