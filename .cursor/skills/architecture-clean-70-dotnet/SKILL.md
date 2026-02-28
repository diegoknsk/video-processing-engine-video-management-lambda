---
name: architecture-clean-70-dotnet
description: Estrutura padrão de projetos .NET com Clean Architecture pragmática (~70%), multi-entrypoint (API + Lambda), diretórios físicos espelhando Solution Folders. Use quando criar novo projeto, definir macro-estrutura, organizar pastas, multi-entrypoint ou dependências entre Core/Infra/Interfaces.
---

# Clean Architecture 70% — Estrutura e Multi-Entrypoint

**Quando usar:** novo projeto .NET; estrutura de pastas; Solution Folders; API + Lambda no mesmo repo; regras de dependência; fluxo Controller/Handler → UseCase → Ports.

**Observação:** abordagem pragmática (~70% Clean), não purista. Domain puro; Application com Ports; Infra implementa; entradas (API, Lambda) orquestram.

---

## Estrutura padrão (físico = Solution Folders)

```
/src
  /Core
    - *.Domain          (entidades, value objects, exceções de domínio)
    - *.Application     (use cases, ports, DTOs, validators)
  /Infra
    - *.Infra.Data      (repositórios, EF/DynamoDB, implementação de ports)
    - *.Infra.CrossCutting (logging, config, serviços transversais)
  /InterfacesExternas
    - *.Api             (controllers, filters, entrada HTTP)
    - *.LambdaXxx       (handler SQS/Event, entrada Lambda)
/tests
  - *.UnitTests
```

**Solution (.sln ou .slnx):** Solution Folders exatamente `Core` | `Infra` | `InterfacesExternas` | `Tests`. Cada projeto dentro da pasta lógica correspondente. Diretório físico deve espelhar Solution Folders (não usar apenas virtual). Funciona tanto com arquivo .sln clássico quanto com .slnx (solution filter / formato moderno).

---

## Regras de dependência

| Camada | Pode depender de | Não pode depender de |
|--------|-------------------|------------------------|
| **Domain** | Nada (zero deps externas) | Application, Infra, Api, Lambda |
| **Application** | Domain | Infra, Api, Lambda |
| **Infra.*** | Application, Domain | Api, Lambda (entre si: só se necessário e documentado) |
| **Api / Lambda** | Application, Domain, Infra | Um do outro |

**Ports:** interfaces em Application; implementações em Infra. Domain não conhece Ports.

---

## Multi-entrypoint (API + Lambda)

- **API:** Controller recebe InputModel (body), extrai rota/auth, chama UseCase, retorna ResponseModel. Filtro/middleware padronizam resposta.
- **Lambda (ex.: SQS):** Handler deserializa evento, chama UseCase (mesma Application), opcionalmente publica resultado. Não expor lógica de negócio no handler.
- **Uma Application, duas entradas:** ambos referenciam Core; cada entrada registra apenas os serviços de Infra que usa. Entradas **não** referenciam uma à outra.

---

## Fluxo padrão

`Controller/Handler` → `UseCase` → `Ports (interfaces)` → `Infra implementa` → `Presenter` → `Response`

- Controller/Handler: fino; recebe input, chama use case, devolve resultado.
- UseCase: orquestra Ports (repositórios, serviços), aplica regras de aplicação.
- Ports: definidos na Application; implementados em Infra.
- Domain: entidades e regras de domínio puras.

---

## Checklist rápido (novo projeto)

1. Criar pastas físicas: `src/Core`, `src/Infra`, `src/InterfacesExternas`, `tests`.
2. Criar projetos: Domain e Application em Core; Infra.Data e Infra.CrossCutting em Infra; Api e Lambda em InterfacesExternas.
3. Solution (.sln ou .slnx): Solution Folders Core, Infra, InterfacesExternas, Tests; projetos nas pastas corretas.
4. Referências: Application → Domain; Infra → Application (+ Domain); Api/Lambda → Application, Domain, Infra (nunca Api ↔ Lambda).
5. Ports na Application; implementações na Infra.
6. Controllers/Handlers finos; UseCases com orquestração.
7. InputModel = contrato único (body); rota/auth separados nos parâmetros do UseCase.
8. Validar build e que Domain não referencia outros projetos.

---

## Exemplo mínimo

**Cenário:** API GET que chama use case e retorna DTO.

```csharp
// Application: port
public interface IGetItemRepository { Task<Item?> GetByIdAsync(Guid id, CancellationToken ct); }

// Application: use case
public class GetItemUseCase(IGetItemRepository repo)
{
    public async Task<ItemResponse?> ExecuteAsync(Guid id, CancellationToken ct) =>
        (await repo.GetByIdAsync(id, ct))?.ToResponse();
}

// Api: controller fino
[HttpGet("{id:guid}")]
public async Task<IActionResult> Get(Guid id, CancellationToken ct)
{
    var result = await _getItemUseCase.ExecuteAsync(id, ct);
    return result is null ? NotFound() : Ok(result);
}
```

**Pontos-chave:** Port em Application; UseCase recebe id (rota) e ct; Controller só chama UseCase e mapeia HTTP.

---

## Anti-patterns

- **Domain** com referência a EF, ASP.NET ou qualquer projeto externo.
- **Application** referenciando Infra ou entradas (Api/Lambda).
- **Entrada referenciar outra entrada** (ex.: Api referenciar projeto Lambda).
- **Lógica de negócio em Controller/Handler** (deve estar em UseCase).
- **Solution Folders** desalinhados da estrutura física (pastas virtuais sem espelho em disco).
- **RequestModels** separados do body (usar InputModel como contrato único).

---

## Iniciar novo projeto

1. Clonar ou criar repo; criar estrutura de pastas acima.
2. Criar solution (.sln ou .slnx) e projetos; organizar em Solution Folders.
3. Configurar referências conforme tabela de dependências.
4. Definir Ports na Application; implementar na Infra; registrar DI na entrada (Api ou Lambda).
5. Seguir quick-reference e skills por contexto (persistência, validação, etc.).

**Migração de projeto existente:** mover projetos para as pastas Core/Infra/InterfacesExternas; ajustar referências e .csproj paths; atualizar solution (.sln ou .slnx) e Solution Folders; rodar build e testes.
