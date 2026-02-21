# Subtask 04 — Adicionar [AllowAnonymous] no HealthController e refatorar extração do sub no VideosController

## Descrição
Duas correções pontuais no nível de controller:

1. **`HealthController`:** adicionar `[AllowAnonymous]` explícito para proteger o health check de futuros `[Authorize]` globais e deixar a intenção documentada no código.

2. **`VideosController`:** refatorar a extração do claim `sub` para separar a leitura do claim (string) da conversão para `Guid`, adicionando comentário que explica a conversão. Isso torna o código mais claro e seguro para manutenção futura.

**Arquivos:**
- `src/VideoProcessing.VideoManagement.Api/Controllers/HealthController.cs`
- `src/VideoProcessing.VideoManagement.Api/Controllers/VideosController.cs`

**Problema (P4 — MÉDIO):** A linha atual combina verificação de nulidade e conversão de tipo numa única condição, dificultando diagnóstico quando `sub` é null vs quando o formato não é UUID:
```csharp
// atual — mistura nulidade + parse + validação de formato
if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
    return Unauthorized();
```

**Problema (P7 — BAIXO):** `HealthController` não tem `[AllowAnonymous]` explícito. Funciona hoje, mas é frágil se um `[Authorize]` for adicionado globalmente no futuro.

## Passos de Implementação

### HealthController

1. Adicionar `using Microsoft.AspNetCore.Authorization;` se não existir.
2. Adicionar `[AllowAnonymous]` no `HealthController`:
   ```csharp
   [ApiController]
   [Route("[controller]")]
   [AllowAnonymous]
   public class HealthController : ControllerBase { ... }
   ```

### VideosController

3. Localizar o bloco de extração do `sub` no método `UploadVideo` (linhas 33–35):
   ```csharp
   var userIdClaim = User.FindFirst("sub")?.Value;
   if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
       return Unauthorized();
   ```

4. Refatorar separando a leitura do parse:
   ```csharp
   var sub = User.FindFirst("sub")?.Value;
   if (string.IsNullOrEmpty(sub))
       return Unauthorized();

   // Cognito sub é UUID-format — conversão para Guid segura neste contexto (Domain exige Guid)
   if (!Guid.TryParse(sub, out var userId))
       return Unauthorized();
   ```

5. Executar `dotnet build` para confirmar sem erros.

## Formas de Teste

1. **Manual — health check sem token:** `GET /health` retorna 200 após adição de `[AllowAnonymous]`.
2. **Manual — health check com token:** `GET /health` com token válido também retorna 200 (não deve exigir auth).
3. **Manual — upload com token válido:** `POST /videos` com token Cognito válido retorna 201.
4. **Manual — upload sem token:** `POST /videos` sem header `Authorization` retorna 401.
5. **Teste unitário (Subtask 05):** testar o método com `ClaimsPrincipal` sem claim `sub` (esperar Unauthorized) e com `sub` = UUID válido (esperar fluxo normal).

## Critérios de Aceite

- [ ] `[AllowAnonymous]` presente no `HealthController`
- [ ] `GET /health` retorna 200 sem token
- [ ] No `VideosController.UploadVideo`, leitura de `sub` (string) e conversão para `Guid` estão em linhas separadas
- [ ] Comentário explicando a conversão Cognito sub → Guid presente no código
- [ ] `dotnet build` sem erros após as alterações
- [ ] Nenhum outro comportamento de endpoint alterado além do que está descrito nesta subtask
