# Storie-04.2: Revisão e Correções — Autenticação Cognito JWT + Extração do claim "sub"

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** —

## Descrição
Como time de desenvolvimento, queremos garantir que a API valide corretamente o JWT do Cognito via JwtBearer (defense-in-depth) e que o claim `sub` seja extraído de forma confiável, para que o endpoint `POST /videos` funcione com autenticação segura tanto localmente (Kestrel) quanto em produção (API Gateway + Lambda).

## Objetivo
Corrigir todos os problemas identificados na revisão técnica de autenticação: configuração incorreta/incompleta do JwtBearer (MapInboundClaims, ValidateAudience), ordem incorreta do middleware no pipeline, filtro OpenAPI que não detecta `[Authorize]` em nível de classe, ausência de `[AllowAnonymous]` explícito no health check, e extração do `sub` sem helper centralizado. Ao final, `User.FindFirst("sub")` deve funcionar de forma confiável local e na AWS.

## Escopo Técnico
- **Tecnologias:** .NET 10, C# 13, ASP.NET Core, Microsoft.AspNetCore.Authentication.JwtBearer 10.0.3
- **Arquivos afetados:**
  - `src/VideoProcessing.VideoManagement.Api/DependencyInjection/ServiceCollectionExtensions.cs`
  - `src/VideoProcessing.VideoManagement.Api/Program.cs`
  - `src/VideoProcessing.VideoManagement.Api/Filters/BearerAuthSecurityOperationFilter.cs`
  - `src/VideoProcessing.VideoManagement.Api/Controllers/HealthController.cs`
  - `src/VideoProcessing.VideoManagement.Api/Controllers/VideosController.cs`
  - `tests/VideoProcessing.VideoManagement.UnitTests/Application/` (testes de auth flow)
- **Componentes:** JwtBearerOptions, pipeline de middleware, BearerAuthSecurityOperationFilter, HealthController, VideosController
- **Pacotes/Dependências:**
  - Microsoft.AspNetCore.Authentication.JwtBearer (10.0.3) — já adicionado
  - xUnit (2.9.x) — já adicionado
  - Moq (4.20.x) — já adicionado

## Dependências e Riscos (para estimativa)
- **Dependências:**
  - Story 04 e 04.1 (implementação base do POST /videos com autenticação parcial)
- **Riscos:**
  - `ValidateAudience = false` abre validação de audience; compensar documentando que o token deve ser emitido pelo User Pool correto (Authority garante isso)
  - `MapInboundClaims = false` pode impactar código que usa `ClaimTypes.NameIdentifier`; verificar se há outros pontos no projeto que dependem do mapeamento padrão
  - Reordenar middleware pode alterar comportamento de outros middlewares; testar pipeline completo após mudança

## Subtasks
- [Subtask 01: Corrigir configuração JwtBearer — MapInboundClaims, ValidateAudience e TokenValidationParameters](./subtask/Subtask-01-Corrigir_JwtBearer_MapInboundClaims_Audience.md)
- [Subtask 02: Corrigir ordem do middleware no pipeline (Program.cs)](./subtask/Subtask-02-Corrigir_Ordem_Middleware_Pipeline.md)
- [Subtask 03: Corrigir BearerAuthSecurityOperationFilter — detectar [Authorize] em nível de classe](./subtask/Subtask-03-Corrigir_BearerAuthFilter_ClassLevel_Authorize.md)
- [Subtask 04: Adicionar [AllowAnonymous] no HealthController e refatorar extração do sub no VideosController](./subtask/Subtask-04-AllowAnonymous_Health_Sub_Helper.md)
- [Subtask 05: Testes unitários — cobertura dos cenários de autenticação e extração do sub](./subtask/Subtask-05-Testes_Autenticacao_Sub.md)

## Critérios de Aceite da História
- [ ] `options.MapInboundClaims = false` configurado explicitamente no `AddJwtBearer` — `User.FindFirst("sub")` retorna o UUID do usuário Cognito sem mapeamento de claim
- [ ] `options.TokenValidationParameters.ValidateAudience = false` configurado — access tokens do Cognito são aceitos (não apenas ID tokens)
- [ ] Validações explícitas no `TokenValidationParameters`: `ValidateIssuer = true`, `ValidateLifetime = true`, `ValidateIssuerSigningKey = true`, `NameClaimType = "sub"`
- [ ] Ordem do middleware correta no `Program.cs`: `GatewayPathBaseMiddleware` → `UseRouting()` → `UseAuthentication()` → `UseAuthorization()`
- [ ] `BearerAuthSecurityOperationFilter` detecta `[Authorize]` tanto em nível de método quanto em nível de classe — todos os endpoints de `VideosController` exibem o cadeado BearerAuth no Swagger/Scalar
- [ ] `HealthController` possui `[AllowAnonymous]` explícito — `GET /health` retorna 200 sem token
- [ ] Extração do `sub` no `VideosController` usa o claim diretamente como string antes da conversão para `Guid`, com comentário explicando a conversão — sem misturar verificação de nulidade e parse na mesma condição
- [ ] `POST /videos` sem token retorna 401; com token Cognito válido retorna 201 com `userId` correto no log
- [ ] Testes unitários criados cobrindo: validação com token sem claim `sub`, extração correta do `sub`, filtro OpenAPI com `[Authorize]` em classe; `dotnet test` passa sem erros
- [ ] `dotnet build` sem erros ou warnings novos após todas as correções

## Rastreamento (dev tracking)
- **Início:** dia 18/02/2026, às 21:54 (Brasília)
- **Fim:** —
- **Tempo total de desenvolvimento:** —
