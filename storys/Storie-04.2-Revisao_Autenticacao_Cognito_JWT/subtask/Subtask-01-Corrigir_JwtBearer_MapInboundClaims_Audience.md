# Subtask 01 — Corrigir configuração JwtBearer: MapInboundClaims, ValidateAudience e TokenValidationParameters

## Descrição
Corrigir a configuração do `AddJwtBearer` em `ServiceCollectionExtensions.cs` para garantir que:
1. O claim `sub` chegue com o nome `"sub"` (sem remapeamento para `ClaimTypes.NameIdentifier`)
2. Access tokens do Cognito sejam aceitos (Cognito access tokens não têm `aud` = ClientId)
3. As validações críticas estejam explicitamente configuradas (issuer, lifetime, signature)

**Arquivo:** `src/VideoProcessing.VideoManagement.Api/DependencyInjection/ServiceCollectionExtensions.cs`

**Problema (P1 — CRÍTICO):** `MapInboundClaims` não está explicitamente configurado. Dependendo da versão do runtime, o handler pode remapear `sub` → `ClaimTypes.NameIdentifier`, fazendo `User.FindFirst("sub")` retornar `null` → 401 mesmo com token válido.

**Problema (P2 — ALTO):** `options.Audience = cognitoClientId` faz a validação de audience falhar para access tokens do Cognito, pois eles não carregam `aud` = ClientId. O resultado é 401 para tokens válidos.

## Passos de Implementação

1. Localizar o bloco `services.AddAuthentication(...).AddJwtBearer(options => { ... })` em `ServiceCollectionExtensions.cs`.

2. Adicionar `options.MapInboundClaims = false;` logo após a atribuição de `Authority`:
   ```csharp
   options.MapInboundClaims = false; // garante que "sub" chegue como "sub"
   ```

3. Substituir a atribuição direta de `options.Audience` por `TokenValidationParameters` explícito:
   ```csharp
   options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
   {
       ValidateIssuer = true,
       ValidateAudience = false,         // Cognito access tokens não têm aud = clientId
       ValidateLifetime = true,
       ValidateIssuerSigningKey = true,
       NameClaimType = "sub"             // User.Identity.Name retorna o sub
   };
   ```
   Manter o campo `options.Audience = cognitoClientId;` apenas como referência (ou remover, já que ValidateAudience = false).

4. Manter `options.Authority` inalterado — ele é usado para buscar o OIDC discovery endpoint e validar o issuer e a chave de assinatura.

5. Executar `dotnet build` para confirmar que não há erros de compilação.

## Formas de Teste

1. **Manual local:** iniciar a API, obter um access token do Cognito com `aws cognito-idp initiate-auth` e fazer `POST /videos` com `Authorization: Bearer <access_token>`. Esperar 201 (não 401).
2. **Manual local (ID token):** repetir o teste com o `IdToken` do mesmo response. Ambos devem retornar 201.
3. **Log de debug:** adicionar temporariamente um log `_logger.LogDebug("sub claim: {Sub}", User.FindFirst("sub")?.Value)` no controller e confirmar que o valor é o UUID do usuário Cognito.
4. **Teste unitário (Subtask 05):** mock de `ClaimsPrincipal` com claim `"sub"` = UUID; garantir que a extração funciona.

## Critérios de Aceite

- [ ] `options.MapInboundClaims = false` presente na configuração do JwtBearer
- [ ] `options.TokenValidationParameters.ValidateAudience = false` configurado
- [ ] `options.TokenValidationParameters.ValidateIssuer = true`, `ValidateLifetime = true`, `ValidateIssuerSigningKey = true` configurados explicitamente
- [ ] `options.TokenValidationParameters.NameClaimType = "sub"` configurado
- [ ] `dotnet build` sem erros após a alteração
- [ ] `User.FindFirst("sub")` retorna o UUID Cognito com token válido (verificado em teste manual ou unitário)
