# Subtask-04: Ajustar referências, scripts e documentação

## Descrição
Identificar e atualizar referências aos caminhos antigos dos projetos em scripts de build/deploy, documentação (docs/), arquivos de configuração (ex.: .cursor) e qualquer outro artefato que use paths como `src/VideoProcessing.VideoManagement.Api` ou `src/VideoProcessing.VideoManagement.Application`. **Nota:** os workflows do GitHub Actions são tratados na [Subtask 05](./Subtask-05-Ajustar_Validar_GitHub_Actions.md).

## Passos de Implementação
1. Buscar no repositório por referências aos caminhos antigos (ex.: `src/VideoProcessing.VideoManagement.Api`, `src/VideoProcessing.VideoManagement.Application`, etc.) em arquivos .yml, .yaml, .md, .json, .ps1, Dockerfile e similares.
2. Atualizar cada referência para o novo path (ex.: `src/InterfacesExternas/VideoProcessing.VideoManagement.Api`, `src/Core/VideoProcessing.VideoManagement.Application`).
3. Verificar projetos de teste (UnitTests): path do .csproj e ProjectReference para Application/Infra devem estar corretos para a nova estrutura (ex.: tests/ permanece; referências a `../../src/Core/...` ou `../../src/Infra/...` conforme localização).
4. Documentar no story ou em docs/ a estrutura final (opcional: adicionar uma linha em README ou docs existente apontando para a skill de arquitetura).

## Formas de Teste
1. Executar scripts de build/CI (ex.: GitHub Actions ou comando local de build) e garantir que não falham por path inexistente.
2. Buscar novamente por ocorrências dos paths antigos e confirmar que não restam referências quebradas.
3. Rodar `dotnet test` e validar que os testes encontram os projetos referenciados.

## Critérios de Aceite
- [x] Scripts de build, deploy e CI (ex.: GitHub Actions) usam os novos caminhos dos projetos.
- [x] Documentação (docs/, README) que cita caminhos de projetos foi atualizada quando aplicável.
- [x] Projeto de testes (UnitTests) compila e referencia corretamente Core/Infra; `dotnet test` passa.
- [x] Nenhuma referência óbvia aos paths antigos permanece em arquivos que possam quebrar build ou deploy.
