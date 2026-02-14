---
description: Clean Architecture — camadas Api/Application/Domain, InputModel único, fluxo Controller → UseCase → Presenter (max 250 chars)
---

# Clean Architecture Core — Estrutura Universal

- **Camadas:** Api → Application → Domain; Infra.* implementa Ports; Domain sem dependências externas.
- **Fluxo:** Controller recebe InputModel no body; chama UseCase com dados de rota/auth separados; UseCase chama Ports, Presenter e retorna ResponseModel. **Não** criar RequestModels; InputModel é contrato único.
- **Controllers:** apenas receber InputModel, extrair rota/auth, chamar UseCase, retornar resultado; filtro/middleware padronizam respostas e erros.

**Referência rápida:** princípios-chave e tabela de decisão na documentação do projeto.

**Por contexto:** Persistência → repositórios/EF Core; API externa → Refit; Validação → FluentValidation; Testes → xUnit/BDD; Stories → stories técnicas.
