# Subtask 02 — Corrigir ordem do middleware no pipeline (Program.cs)

## Descrição
Corrigir a ordem dos middlewares em `Program.cs` para seguir a ordem recomendada pelo ASP.NET Core:
`GatewayPathBaseMiddleware` → `UseRouting()` → `UseAuthentication()` → `UseAuthorization()`.

**Arquivo:** `src/VideoProcessing.VideoManagement.Api/Program.cs`

**Problema (P6 — BAIXO/MÉDIO):** Atualmente:
```
UseAuthentication()           ← antes de UseRouting() — subótimo
GatewayPathBaseMiddleware     ← entre auth e routing — semanticamente incorreto
UseRouting()
UseAuthorization()
```
O `GatewayPathBaseMiddleware` altera `Request.Path` removendo o stage prefix do API Gateway. Ele deve rodar **antes** do `UseRouting()` para que o router veja o path já normalizado. A autenticação deve vir depois do routing para que a authorization middleware tenha acesso aos metadados de endpoint (ex.: `[AllowAnonymous]`, `[Authorize]`).

Embora a auth funcione na ordem atual (JWT não depende do path), a ordem incorreta é um risco para features futuras baseadas em metadata de endpoint.

## Passos de Implementação

1. Localizar o bloco de pipeline no `Program.cs` (linhas 44–66 aproximadamente).

2. Reordenar para:
   ```csharp
   app.UseSerilogRequestLogging(options => { ... });
   app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
   app.UseMiddleware<GatewayPathBaseMiddleware>();  // ← ANTES do UseRouting
   app.UseRouting();                                 // ← ANTES de UseAuthentication
   app.UseAuthentication();
   app.UseAuthorization();
   app.UseOpenApiDocumentation();
   app.MapScalarApiReference(...);
   app.MapControllers();
   app.MapGet("/", ...);
   app.MapGet("/openapi/v1.json", ...);
   ```

3. Remover a chamada `app.UseRouting()` da posição atual (após `GatewayPathBaseMiddleware`) — ela deve existir apenas uma vez, antes de `UseAuthentication()`.

4. Verificar que `UseSerilogRequestLogging` continua sendo o primeiro middleware do pipeline (para logar todas as requests, incluindo as não autenticadas).

5. Executar `dotnet build` para confirmar sem erros.

## Formas de Teste

1. **Manual local — health check:** `GET /health` sem token retorna 200.
2. **Manual local — rota protegida sem token:** `POST /videos` sem `Authorization` header retorna 401.
3. **Manual local — rota protegida com token válido:** `POST /videos` com token Cognito retorna 201.
4. **Manual local com GATEWAY_PATH_PREFIX:** setar env var `GATEWAY_PATH_PREFIX=api` e fazer `POST /api/videos` — path deve ser normalizado antes do routing, rota deve resolver corretamente.

## Critérios de Aceite

- [ ] Ordem no `Program.cs`: `GatewayPathBaseMiddleware` vem antes de `UseRouting()`
- [ ] `UseRouting()` vem antes de `UseAuthentication()`
- [ ] `UseAuthentication()` vem antes de `UseAuthorization()`
- [ ] `UseRouting()` aparece apenas uma vez no pipeline
- [ ] `dotnet build` sem erros após a reordenação
- [ ] `GET /health` retorna 200 sem token após reordenação
- [ ] `POST /videos` retorna 401 sem token e 201 com token válido após reordenação
