# Storie-01.3: Test Deploy Controller (GitHub)

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** —

## Descrição
Como desenvolvedor/DevOps, quero executar o deploy da Lambda Video Management via GitHub Actions (usando o workflow da Storie-07), configurando os secrets e variables necessários no repositório GitHub, para validar minimamente que o pipeline de deploy e o endpoint de health funcionam após o deploy.

## Objetivo
Configurar no repositório GitHub os secrets de AWS (Access Key, Secret Key e, se aplicável, Session Token), as variables necessárias ao workflow de deploy, executar o deploy via workflow e validar minimamente o sucesso (workflow concluído e smoke test GET /health na URL do API Gateway).

## Escopo Técnico
- **Tecnologias:** GitHub Actions, GitHub Secrets/Variables, AWS CLI (via workflow), curl ou equivalente para smoke test
- **Arquivos criados/modificados:**
  - Documentação em `docs/` (opcional): checklist de configuração de secrets/variables para deploy
  - Nenhuma alteração obrigatória no código; uso do workflow existente da Storie-07
- **Componentes:**
  - GitHub Secrets: AWS_ACCESS_KEY_ID, AWS_SECRET_ACCESS_KEY, AWS_SESSION_TOKEN (quando uso de credenciais temporárias)
  - GitHub Variables: AWS_REGION, LAMBDA_FUNCTION_NAME e demais variáveis exigidas pelo workflow da Storie-07
  - Execução do workflow de deploy (manual ou por push)
  - Validação mínima: conclusão do workflow e GET /health retornando 200
- **Pacotes/Dependências:** Nenhum novo (apenas configuração no GitHub e execução do pipeline existente)

## Dependências e Riscos (para estimativa)
- **Dependências:**
  - Storie-07 (workflow de deploy) com workflow criado em `.github/workflows/deploy-lambda-video-management.yml`
  - Storie-01.2 concluída (endpoint GET /health e gateway configurados)
  - Lambda e API Gateway provisionados na AWS (via IaC)
  - Credenciais AWS disponíveis (Access Key, Secret Key; Session Token se temporárias)
- **Riscos:**
  - Secrets/variables incorretos ou faltando causam falha no step "Configure AWS" ou no deploy
  - Session Token expirado (credenciais temporárias) exige renovação nos secrets

## Subtasks
- [Subtask 01: Configurar GitHub Secrets (AWS_ACCESS_KEY_ID, AWS_SECRET_ACCESS_KEY, AWS_SESSION_TOKEN)](./subtask/Subtask-01-GitHub_Secrets_AWS.md)
- [Subtask 02: Configurar GitHub Variables do workflow de deploy](./subtask/Subtask-02-GitHub_Variables_Deploy.md)
- [Subtask 03: Executar workflow de deploy e validar conclusão](./subtask/Subtask-03-Executar_Workflow_Deploy.md)
- [Subtask 04: Validar smoke test GET /health pós-deploy](./subtask/Subtask-04-Smoke_Test_Health_Pos_Deploy.md)

## Critérios de Aceite da História
- [ ] GitHub Secrets configurados: AWS_ACCESS_KEY_ID, AWS_SECRET_ACCESS_KEY e, quando aplicável, AWS_SESSION_TOKEN (para credenciais temporárias)
- [ ] GitHub Variables configuradas conforme exigido pelo workflow da Storie-07 (ex.: AWS_REGION, LAMBDA_FUNCTION_NAME, GATEWAY_PATH_PREFIX, GATEWAY_STAGE e demais env vars da Lambda)
- [ ] Workflow de deploy executado manualmente (workflow_dispatch) ou por push na branch configurada, com todos os steps concluídos com sucesso
- [ ] Smoke test mínimo: GET na URL do API Gateway (ex.: `https://.../default/videos/health`) retorna 200 OK e JSON com "status": "healthy"
- [ ] Documentação ou checklist (em docs ou README) descreve quais secrets e variables são obrigatórios para o deploy via GitHub

## Rastreamento (dev tracking)
- **Início:** —
- **Fim:** —
- **Tempo total de desenvolvimento:** —
