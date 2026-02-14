# Subtask 03: Implementar health check e middleware de erros

## Descrição
Criar endpoint GET /health que retorna status da aplicação (200 OK com JSON contendo status e timestamp), implementar middleware global de exception handling que captura exceções não tratadas e retorna respostas padronizadas (500 Internal Server Error com estrutura JSON clara), e garantir que erros não exponham detalhes sensíveis em produção.

## Passos de Implementação
1. **Criar endpoint de health check**:
   - No Program.cs, adicionar rota: `app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))`
   - Garantir que rota `/health` está fora de qualquer grupo que exija autenticação (pública)
   - Testar localmente: `curl http://localhost:5000/health` deve retornar 200 com JSON

2. **Criar modelo de resposta de erro padronizado**:
   - Em Application/Models/, criar record `ErrorResponse(string Type, string Title, int Status, string? Detail, string? TraceId)`
   - Seguir RFC 7807 (Problem Details): Type (tipo do erro), Title (mensagem legível), Status (código HTTP), Detail (detalhes opcionais), TraceId (correlação de log)

3. **Implementar middleware de exception handler**:
   - Criar classe `GlobalExceptionHandlerMiddleware` em Infra.CrossCutting/Middleware
   - Middleware intercepta exceções, loga erro com contexto (usando ILogger), e retorna ErrorResponse padronizada
   - Em produção (`ASPNETCORE_ENVIRONMENT=Production`): ocultar detalhes da exceção (Detail = "An error occurred"); em desenvolvimento: incluir message e stack trace
   - Adicionar TraceId usando `Activity.Current?.Id ?? HttpContext.TraceIdentifier`

4. **Registrar middleware no pipeline**:
   - No Program.cs, adicionar antes de qualquer rota: `app.UseMiddleware<GlobalExceptionHandlerMiddleware>()`
   - Alternativamente: usar `app.UseExceptionHandler()` com endpoint customizado `/error` que retorna ErrorResponse
   - Decisão recomendada: usar middleware customizado para controle total sobre logging e formato de resposta

5. **Criar testes para health check**:
   - Teste de integração (opcional para Story 01; pode ser adicionado em story futura): chamar GET /health e validar status code 200 e estrutura do JSON
   - Teste unitário (mock): validar que endpoint retorna objeto esperado

6. **Criar testes para exception handler**:
   - Teste unitário: simular exceção em middleware, validar que resposta é 500 com ErrorResponse
   - Teste com ambiente "Production": validar que detalhes sensíveis não são expostos
   - Teste com ambiente "Development": validar que stack trace é incluído no Detail

7. **Validar comportamento em erro real**:
   - Criar rota temporária de teste no Program.cs: `app.MapGet("/test-error", () => { throw new Exception("Test exception"); })`
   - Executar `dotnet run`, chamar GET /test-error, validar que retorna 500 com ErrorResponse padronizada
   - Remover rota de teste após validação

## Formas de Teste
1. **Health check test**: executar `curl http://localhost:5000/health`; validar status 200, JSON com campos "status" (valor "healthy") e "timestamp" (ISO 8601)
2. **Exception handler test (development)**: com ASPNETCORE_ENVIRONMENT=Development, criar rota que lança exceção, chamar rota, validar resposta 500 com Detail contendo mensagem da exceção
3. **Exception handler test (production)**: com ASPNETCORE_ENVIRONMENT=Production, repetir teste acima, validar que Detail não contém stack trace (mensagem genérica)
4. **Logs test**: validar que exceções são logadas com nível Error e incluem TraceId para correlação

## Critérios de Aceite da Subtask
- [ ] Endpoint GET /health implementado, retorna 200 OK com JSON `{ "status": "healthy", "timestamp": "<UTC ISO 8601>" }`
- [ ] Health check é público (não requer autenticação)
- [ ] Record `ErrorResponse` criado seguindo RFC 7807 (Type, Title, Status, Detail, TraceId)
- [ ] Middleware `GlobalExceptionHandlerMiddleware` implementado e registrado no pipeline
- [ ] Exceções não tratadas retornam 500 com ErrorResponse padronizada
- [ ] Em produção: Detail não expõe stack trace ou mensagens sensíveis (mensagem genérica)
- [ ] Em desenvolvimento: Detail inclui mensagem da exceção para debugging
- [ ] TraceId é incluído em todas as ErrorResponse e correlaciona com logs
- [ ] Exceções são logadas com nível Error, incluindo stack trace e TraceId
- [ ] Testes unitários validam comportamento do middleware (production vs development)
- [ ] `curl http://localhost:5000/health` retorna resposta esperada sem erros
