# Storie-18: Persistir e expor email do usuário no vídeo (Cognito + DynamoDB)

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** —

## Descrição
Como orquestrador do processamento de vídeo, quero que o VideoManagement recupere e persista o email do usuário autenticado no momento da criação do vídeo e o exponha em todos os GETs, para que eu possa enriquecer o processamento sem acoplamento direto ao Cognito.

## Objetivo
Recuperar o email do usuário autenticado (via Cognito) no momento da criação do vídeo, persistir esse valor na tabela principal de vídeos do DynamoDB e retorná-lo em todos os GETs (endpoint público e interno), mantendo a responsabilidade de integração com Cognito no VideoManagement.

## Contexto
- O orquestrador precisa do email do usuário para enriquecer o processamento.
- Para evitar acoplamento direto do orquestrador com Cognito, essa responsabilidade fica no VideoManagement.
- O usuário já está autenticado via Cognito (JWT); no token existem pelo menos `sub`/`username`, mas o email pode não vir no payload usado hoje.
- O VideoManagement deve recuperar o email do usuário autenticado e salvá-lo junto ao item principal do vídeo na criação.
- O email deve ser retornado em todos os GETs (API pública e rota interna) para consumo pelo orquestrador e pelo cliente.

## Escopo

### Dentro do escopo
- Port/Adapter para obter email do usuário a partir do identificador (ex.: `sub`) — integração com Cognito (Admin GetUser ou atributo no token, conforme viabilidade).
- Inclusão do campo `UserEmail` (ou equivalente) no modelo de domínio `Video`, em `VideoRehydrationData`, em `VideoEntity` (DynamoDB) e no mapeamento.
- No fluxo de criação (POST /videos): obter email do usuário autenticado e persistir no registro do vídeo.
- Inclusão do email em `VideoResponseModel` e no mapeamento para resposta; retorno do email no GET público (GET /videos/{id}) e no GET interno (GET internal/videos/{userId}/{id}).
- Testes unitários cobrindo o novo port/adapter, uso no UseCase de criação e mapeamento Domain/Response.
- Seguir arquitetura, rules e skills do projeto (Clean Architecture, skills de persistência e segurança quando aplicável).

### Fora do escopo
- Alterar o orquestrador ou outros serviços externos (apenas consumirão o email já exposto no GET).
- Incluir email em listagem (GET /videos) — apenas em GET por id, salvo decisão explícita de incluir na listagem.
- Refatoração desnecessária de código existente.
- Migração/backfill de vídeos já existentes no DynamoDB (itens antigos podem ter `UserEmail` nulo; comportamento definido nos critérios de aceite).

## Escopo Técnico
- **Tecnologias:** .NET 10, C# 13, Amazon Cognito (Admin API ou claims), DynamoDB (tabela principal de vídeos já existente).
- **Arquivos afetados/criados:**
  - Application: port `IGetUserEmailService` (ou equivalente), `UploadVideoUseCase`, `VideoResponseModel`, `VideoResponseModelMapper`, `VideoRehydrationData` (Domain) e entidade `Video` (Domain).
  - Domain: `Video.cs`, `VideoRehydrationData.cs` (novo campo opcional).
  - Infra.Data: `VideoEntity.cs`, `VideoMapper.cs`, `VideoRepository.cs` (persistência do novo atributo); adapter Cognito (ex.: `CognitoUserEmailService` ou uso de Admin GetUser).
  - Api: contratos de resposta (OpenAPI) para GET /videos/{id} e GET internal; sem alteração de rota.
- **Pacotes/Dependências:** AWSSDK.CognitoIdentityProvider (se uso de Admin GetUser) — versão alinhada ao restante do projeto; demais já existentes.

## Dependências e Riscos (para estimativa)
- **Dependências:** Stories 02 (IVideoRepository, Video entity), 04/04.2 (POST /videos, autenticação JWT, extração de `sub`), 05 (GET por id).
- **Riscos/Pré-condições:**
  - Email pode não estar no access token; solução pragmática: usar Admin GetUser com `sub` (requer permissão IAM na Lambda/role para cognito-idp:AdminGetUser) ou configurar User Pool para incluir `email` em access token (custom scope/claim). Decisão arquitetural: preferir Admin GetUser para não depender de reconfiguração do token.
  - Itens já existentes na tabela não terão `UserEmail`; GET deve retornar `null` ou omitir campo quando ausente.

## Subtasks
- [Subtask 01: Port e adapter para obter email do usuário (Cognito)](./subtask/Subtask-01-Port_Adapter_Email_Usuario_Cognito.md)
- [Subtask 02: Domain e persistência — UserEmail em Video, VideoRehydrationData, VideoEntity e mapeamento](./subtask/Subtask-02-Domain_Persistencia_UserEmail.md)
- [Subtask 03: UploadVideoUseCase — obter email na criação e persistir no vídeo](./subtask/Subtask-03-UploadVideoUseCase_Obter_Email_Persistir.md)
- [Subtask 04: VideoResponseModel e GETs — expor UserEmail em GET por id (público e interno)](./subtask/Subtask-04-ResponseModel_GETs_Expor_UserEmail.md)
- [Subtask 05: Testes unitários (adapter, UseCase, mapeamento e resposta)](./subtask/Subtask-05-Testes_Unitarios_UserEmail.md)

## Critérios de Aceite da História
- [ ] Existe um port na Application para obter o email do usuário a partir do identificador (ex.: `sub`); a implementação em Infra usa Cognito de forma adequada (Admin GetUser ou claim, conforme decisão técnica).
- [ ] O campo de email do usuário (ex.: `UserEmail`) está presente na entidade de domínio `Video`, em `VideoRehydrationData`, em `VideoEntity` (DynamoDB) e nos mapeamentos Domain ↔ Entity e Domain → VideoResponseModel; itens antigos sem o atributo são tratados com valor nulo/omitido.
- [ ] No fluxo de criação (POST /videos), o email do usuário autenticado é obtido e persistido no item do vídeo na tabela principal do DynamoDB.
- [ ] O GET público (GET /videos/{id}) e o GET interno (GET internal/videos/{userId}/{id}) retornam o email do usuário no corpo da resposta (ex.: `VideoResponseModel.UserEmail`), quando disponível.
- [ ] Testes unitários cobrem: adapter de email (com mock do Cognito), uso no UploadVideoUseCase (email persistido), mapeamento Video → VideoResponseModel com e sem UserEmail; `dotnet test` passa e cobertura permanece adequada (≥ 80% nos trechos alterados, quando aplicável).
- [ ] Documentação OpenAPI/Scalar atualizada para incluir o novo campo na resposta dos GETs de vídeo por id.
- [ ] Nenhuma refatoração desnecessária; alterações mínimas e defensáveis arquiteturalmente.

## Observações Técnicas
- **Cognito:** Se o access token já incluir `email` (por escopo ou atributo customizado), usar o claim para evitar chamada extra. Caso contrário, usar `AdminGetUser` com o `sub` do token; a role da Lambda (ou do runtime da API) deve ter permissão `cognito-idp:AdminGetUser` no User Pool.
- **DynamoDB:** Novo atributo opcional na tabela existente; não é necessária migração de schema — apenas incluir o campo no PutItem na criação e no mapeamento de leitura (retornar null se ausente).
- **Compatibilidade:** Vídeos criados antes da mudança não terão `UserEmail`; a API deve retornar `null` ou omitir o campo no JSON para não quebrar clientes.
- **Segurança:** O email é dado do próprio usuário autenticado (criação) ou do dono do recurso (GET); não expor email de outros usuários. O GET interno já é protegido por política (ScopeAnalyzeRun) e por ownership (userId no path).

## Rastreamento (dev tracking)
- **Início:** 12/03/2026, às 16:26 (Brasília)
- **Fim:** —
- **Tempo total de desenvolvimento:** —
