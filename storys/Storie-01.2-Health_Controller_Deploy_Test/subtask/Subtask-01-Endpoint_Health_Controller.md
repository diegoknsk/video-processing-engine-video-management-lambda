# Subtask 01: Garantir endpoint GET /health (controller ou minimal API)

## Descrição
Garantir que a API expõe GET /health retornando 200 OK com JSON `{ "status": "healthy", "timestamp": "<UTC ISO 8601>" }`, de forma pública (sem autenticação), para uso no smoke test de deploy. Se a Story 01 já tiver implementado via minimal API (MapGet), consolidar ou migrar para controller se for padrão do projeto; caso contrário, manter endpoint mínimo.

## Passos de Implementação
1. Verificar implementação existente da Story 01: se já existe GET /health em Program.cs (MapGet), manter e garantir contrato de resposta.
2. Se o padrão do projeto for controllers: criar `HealthController` com ação GET retornando ResponseModel ou objeto anônimo com status e timestamp (UTC).
3. Garantir que a rota é pública (não exige autorização) e que o conteúdo da resposta é JSON com campos "status" e "timestamp".
4. Testar localmente: `curl http://localhost:PORT/health` retorna 200 e JSON esperado.

## Formas de Teste
1. Requisição local: `curl -i http://localhost:5000/health` (ou porta configurada); validar 200 e JSON.
2. Teste unitário (opcional): request GET /health em WebApplicationFactory ou mock; validar status 200 e corpo.
3. Validação de contrato: corpo contém "status":"healthy" e "timestamp" em formato ISO 8601.

## Critérios de Aceite da Subtask
- [ ] GET /health implementado e retorna 200 OK
- [ ] Resposta JSON com "status": "healthy" e "timestamp" (UTC, ISO 8601)
- [ ] Rota é pública (sem autorização)
- [ ] `curl http://localhost:PORT/health` retorna resposta esperada
