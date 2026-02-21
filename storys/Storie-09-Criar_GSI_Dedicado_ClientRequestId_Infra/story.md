# Storie-09: Criar GSI Dedicado para ClientRequestId na Tabela DynamoDB (Infra)

## Status
- **Estado:** ⏸️ Pausada
- **Data de Conclusão:** [DD/MM/AAAA]

## Descrição
Como engenheiro de infra, quero criar um GSI dedicado para a chave de idempotência `clientRequestId` na tabela DynamoDB do serviço de vídeos, para que as queries de deduplicação sejam performáticas e semanticamente claras, sem depender do índice genérico `GSI1`.

## Objetivo
Criar o índice `GSI_ClientRequestId` (ou equivalente com nome semântico) na tabela DynamoDB `video-processing-engine-dev-videos` via projeto de infra (IaC/CDK/CloudFormation/Terraform), e atualizar o `VideoRepository` para usar o nome correto do novo índice.

## Contexto / Motivação
Atualmente o repositório usa o índice genérico `GSI1` (com chaves `gsi1pk`/`gsi1sk`) como solução temporária para as queries de idempotência por `clientRequestId`. O índice `GSI1` existe na tabela mas foi criado sem nome semântico. A criação do índice dedicado `GSI_ClientRequestId` é responsabilidade do projeto de infra, não do serviço de video-management.

**Workaround ativo:** `VideoRepository.GetByClientRequestIdAsync` usa `IndexName = "GSI1"` até este índice ser criado.

## Escopo Técnico
- Tecnologias: AWS DynamoDB, IaC (CDK/CloudFormation/Terraform — conforme padrão do projeto de infra)
- Arquivos afetados (infra): tabela `video-processing-engine-{env}-videos`
- Arquivos afetados (video-management): `src/VideoProcessing.VideoManagement.Infra.Data/Repositories/VideoRepository.cs`
- Componentes: GSI DynamoDB, VideoRepository
- Pacotes/Dependências: N/A (alteração de infra)

## Dependências e Riscos (para estimativa)
- Dependências: projeto de infra (IaC separado); requer deploy de infra antes de atualizar o código. Endpoints da API que eventualmente usem este repositório assumem o **padrão de resposta da Storie-05.1** (envelope success/data/error/timestamp).
- Riscos: adição de GSI em tabela existente pode levar minutos (backfill); monitorar status `ACTIVE` antes de deploy do serviço.

## Subtasks
- [Subtask 01: Criar GSI no projeto de infra (IaC)](./subtask/Subtask-01-Criar_GSI_No_Projeto_Infra.md)
- [Subtask 02: Atualizar VideoRepository para usar o novo nome do índice](./subtask/Subtask-02-Atualizar_VideoRepository_Nome_Indice.md)
- [Subtask 03: Validar e atualizar testes unitários do repositório](./subtask/Subtask-03-Validar_Testes_Unitarios_Repositorio.md)

## Critérios de Aceite da História
- [ ] GSI com nome `GSI_ClientRequestId` (ou nome acordado) criado e com status `ACTIVE` na tabela DynamoDB
- [ ] GSI usa `gsi1pk` (partition key) e `gsi1sk` (sort key) conforme padrão já definido no `VideoMapper`
- [ ] `VideoRepository.GetByClientRequestIdAsync` usa o novo `IndexName` correto e não mais `"GSI1"`
- [ ] Query de idempotência (`GetByClientRequestIdAsync`) retorna resultado correto em ambiente de dev/staging
- [ ] Testes unitários do `VideoRepository` passando com o novo nome de índice; `dotnet test` sem falhas
- [ ] Nenhum impacto em outros fluxos (criação, listagem, atualização de vídeos)

## Rastreamento (dev tracking)
- **Início:** —
- **Fim:** —
- **Tempo total de desenvolvimento:** —
