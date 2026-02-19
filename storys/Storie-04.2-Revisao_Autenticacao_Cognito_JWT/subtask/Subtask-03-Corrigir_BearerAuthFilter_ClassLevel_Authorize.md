# Subtask 03 — Corrigir BearerAuthSecurityOperationFilter: detectar [Authorize] em nível de classe

## Descrição
Corrigir o filtro `BearerAuthSecurityOperationFilter` para detectar o atributo `[Authorize]` quando ele está aplicado na **classe** do controller (e não apenas no método individual), e analogamente para `[AllowAnonymous]`.

**Arquivo:** `src/VideoProcessing.VideoManagement.Api/Filters/BearerAuthSecurityOperationFilter.cs`

**Problema (P3 — MÉDIO):** `context.MethodInfo.GetCustomAttributes(true)` busca atributos no método e em sobrescritas herdadas, mas **não inclui atributos da classe declarante**. O `[Authorize]` está em `VideosController` (nível de classe), então nenhum endpoint exibe o ícone de cadeado BearerAuth no Swagger/Scalar.

**Comportamento atual:** nenhum endpoint de `VideosController` mostra `BearerAuth` no Swagger/Scalar (o filtro retorna sem adicionar o requisito de segurança).

**Comportamento esperado:** todos os endpoints de `VideosController` exibem `BearerAuth`; `GET /health` não exibe (tem `[AllowAnonymous]` após a Subtask 04).

## Passos de Implementação

1. Localizar o método `Apply` em `BearerAuthSecurityOperationFilter.cs`.

2. Substituir a verificação de `hasAllowAnonymous` para incluir atributos da classe declarante:
   ```csharp
   var hasAllowAnonymous =
       context.MethodInfo.GetCustomAttributes(true)
           .OfType<AllowAnonymousAttribute>().Any()
       || (context.MethodInfo.DeclaringType?.GetCustomAttributes(true)
           .OfType<AllowAnonymousAttribute>().Any() ?? false);
   ```

3. Substituir a verificação de `hasAuthorize` analogamente:
   ```csharp
   var hasAuthorize =
       context.MethodInfo.GetCustomAttributes(true)
           .OfType<AuthorizeAttribute>().Any()
       || (context.MethodInfo.DeclaringType?.GetCustomAttributes(true)
           .OfType<AuthorizeAttribute>().Any() ?? false);
   ```

4. Manter o restante do método inalterado (adição do `OpenApiSecurityRequirement`).

5. Executar `dotnet build` para confirmar sem erros.

## Formas de Teste

1. **Swagger UI:** navegar para `GET /swagger/v1/swagger.json` e verificar que `POST /videos`, `GET /videos`, `GET /videos/{id}` e `PATCH /videos/{id}` possuem a seção `security: [{ BearerAuth: [] }]` no JSON.
2. **Scalar:** abrir `/scalar` e confirmar que os endpoints de vídeos exibem o cadeado de autenticação.
3. **GET /health via Swagger:** confirmar que `GET /health` **não** exibe `BearerAuth` (após `[AllowAnonymous]` adicionado na Subtask 04).
4. **Teste unitário (Subtask 05):** instanciar o filtro e verificar comportamento com `[Authorize]` em nível de classe.

## Critérios de Aceite

- [ ] A verificação de `AllowAnonymous` inclui atributos da classe declarante (`DeclaringType`)
- [ ] A verificação de `Authorize` inclui atributos da classe declarante (`DeclaringType`)
- [ ] `POST /videos` exibe `BearerAuth` no Swagger/Scalar (cadeado visível)
- [ ] `GET /videos` exibe `BearerAuth` no Swagger/Scalar
- [ ] `GET /health` **não** exibe `BearerAuth` no Swagger/Scalar
- [ ] `dotnet build` sem erros após a alteração
