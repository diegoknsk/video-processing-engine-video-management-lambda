# Storie-12: Reorganizar diretórios do projeto (Core, Infra, InterfacesExternas, Tests)

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** [DD/MM/AAAA]

## Descrição
Como desenvolvedor do projeto, quero que a estrutura de diretórios e a solution sigam o padrão da Clean Architecture (~70%) definido na skill de arquitetura e na rule core-clean-architecture, separando fisicamente Core, Infra, InterfacesExternas e Tests, para facilitar navegação, onboarding e alinhamento com as regras de dependência entre camadas.

## Objetivo
Reorganizar o repositório para que os diretórios físicos e as Solution Folders espelhem a estrutura: `src/Core` (Domain, Application), `src/Infra` (Infra.Data, Infra.CrossCutting), `src/InterfacesExternas` (Api, LambdaUpdateVideo) e `tests/` (UnitTests). A padronização de respostas da API (Storie-05.1 / api-response-standardization) permanece inalterada — apenas a localização dos projetos muda.

## Escopo Técnico
- **Tecnologias:** .NET 10, estrutura de pastas, Solution (.slnx)
- **Arquivos afetados:**
  - `VideoProcessing.VideoManagement.slnx` (Solution Folders e paths dos projetos)
  - Projetos movidos para novas pastas (paths nos .csproj podem permanecer relativos ao novo local; referências entre projetos atualizadas conforme paths da solution)
  - Scripts de build/CI, documentação e referências a caminhos antigos (ex.: `docs/`, `.cursor/`, `README`)
- **Estrutura alvo (física):**
  - `src/Core/` → VideoProcessing.VideoManagement.Domain, VideoProcessing.VideoManagement.Application
  - `src/Infra/` → VideoProcessing.VideoManagement.Infra.Data, VideoProcessing.VideoManagement.Infra.CrossCutting
  - `src/InterfacesExternas/` → VideoProcessing.VideoManagement.Api, VideoProcessing.VideoManagement.LambdaUpdateVideo
  - `tests/` → VideoProcessing.VideoManagement.UnitTests (já existe; manter)
- **Pacotes/Dependências:** Nenhum novo; apenas reorganização de arquivos e solution.

## Dependências e Riscos (para estimativa)
- **Dependências:** Nenhuma outra story bloqueante. A API e o envelope de resposta (api-response-standardization) não mudam de contrato — apenas o caminho do projeto Api.
- **Riscos:** Referências quebradas em .csproj, .slnx, scripts (build, deploy), **GitHub Actions** (steps de publish usam paths dos projetos — podem quebrar até serem ajustados), documentação e ferramentas (.cursor, IDE). É essencial validar build, testes e workflows após cada movimentação.

## Subtasks
- [x] [Subtask 01: Criar pastas físicas e mover projetos Core e Infra](./subtask/Subtask-01-Criar_Pastas_Mover_Core_Infra.md)
- [x] [Subtask 02: Mover projetos InterfacesExternas (Api e Lambda)](./subtask/Subtask-02-Mover_InterfacesExternas_Api_Lambda.md)
- [x] [Subtask 03: Atualizar Solution (.slnx) com Solution Folders e paths](./subtask/Subtask-03-Atualizar_Solution_Folders_Paths.md)
- [x] [Subtask 04: Ajustar referências, scripts e documentação](./subtask/Subtask-04-Ajustar_Referencias_Scripts_Docs.md)
- [x] [Subtask 05: Ajustar e validar GitHub Actions](./subtask/Subtask-05-Ajustar_Validar_GitHub_Actions.md)
- [x] [Subtask 06: Validar build e testes](./subtask/Subtask-06-Validar_Build_Testes.md)

## Critérios de Aceite da História
- [x] Estrutura física existe: `src/Core/`, `src/Infra/`, `src/InterfacesExternas/`, `tests/` com projetos nas pastas corretas.
- [x] Solution (.slnx) possui Solution Folders Core, Infra, InterfacesExternas e Tests, com cada projeto na pasta lógica correspondente e paths corretos.
- [x] `dotnet build` e `dotnet test` executam sem erros; nenhuma referência quebrada entre projetos.
- [x] Scripts de build/deploy, **GitHub Actions** (paths nos steps de publish) e documentação que citam caminhos de projetos foram atualizados para os novos caminhos; workflows executam sem falha.
- [x] Regras de dependência da arquitetura continuam válidas: Domain sem deps externas; Application só Domain; Infra Application+Domain; Api/Lambda Application, Domain, Infra (não Api↔Lambda).
- [x] Padronização de respostas da API (envelope success/data/error/timestamp) permanece funcional; nenhuma alteração de contrato ou comportamento.

## Rastreamento (dev tracking)
- **Início:** 28/02/2026, às 20:43 (Brasília)
- **Fim:** —
- **Tempo total de desenvolvimento:** —
