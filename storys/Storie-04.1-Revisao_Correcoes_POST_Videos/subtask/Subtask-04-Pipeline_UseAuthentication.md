# Subtask 04: Adicionar UseAuthentication no pipeline (Program.cs)

## Descrição
Adicionar `app.UseAuthentication()` no pipeline do `Program.cs` antes de `app.UseRouting()`, de modo que o middleware de autenticação popule `HttpContext.User` com os claims do JWT (incluindo "sub") antes que o endpoint seja processado.

## Passos de Implementação
1. Abrir `src/VideoProcessing.VideoManagement.Api/Program.cs`
2. Verificar se `AddAuthentication` / `AddJwtBearer` (Cognito) já está configurado no `ServiceCollectionExtensions` ou em alguma extension. Se não estiver, registrar no DI a autenticação JWT com as configurações do Cognito (CognitoOptions: Authority, Audience).
3. Adicionar `app.UseAuthentication()` no pipeline, antes de `app.UseRouting()` (e antes de `app.UseAuthorization()` se existir)
4. A linha deve ficar na sequência: `app.UseMiddleware<GlobalExceptionHandlerMiddleware>()` → `app.UseAuthentication()` → `app.UseMiddleware<GatewayPathBaseMiddleware>()` → `app.UseRouting()`
5. Compilar e verificar que o pipeline está correto

## Formas de Teste
1. `dotnet build` sem erros
2. Teste manual no Scalar UI: POST /videos com JWT válido (claim "sub" = UUID) → 201 Created
3. Teste manual: POST /videos sem Authorization header → 401 Unauthorized
4. Teste manual: POST /videos com JWT sem claim "sub" → 401 Unauthorized

## Critérios de Aceite da Subtask
- [ ] `app.UseAuthentication()` presente no pipeline antes de `app.UseRouting()`
- [ ] `AddAuthentication` / JWT Bearer configurado no DI (se ainda não estava)
- [ ] POST /videos com token JWT válido popula `httpContext.User` e extrai claim "sub" corretamente
- [ ] `dotnet build` sem erros
