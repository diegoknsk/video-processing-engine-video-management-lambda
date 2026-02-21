# Subtask 05 — Testes unitários: cobertura dos cenários de autenticação e extração do sub

## Descrição
Criar testes unitários que cobrem os cenários críticos relacionados à autenticação Cognito e extração do claim `sub`, garantindo que as correções das Subtasks 01–04 sejam verificáveis de forma automatizada.

**Arquivos a criar/atualizar:**
- `tests/VideoProcessing.VideoManagement.UnitTests/Api/Controllers/VideosControllerAuthTests.cs` (novo)
- `tests/VideoProcessing.VideoManagement.UnitTests/Api/Filters/BearerAuthSecurityOperationFilterTests.cs` (novo)

**Cenários a cobrir:**
- Controller retorna 401 quando claim `sub` está ausente
- Controller retorna 401 quando claim `sub` não é UUID válido
- Controller executa o UseCase quando `sub` é UUID válido
- Filtro OpenAPI adiciona BearerAuth para endpoint com `[Authorize]` em nível de classe
- Filtro OpenAPI não adiciona BearerAuth para endpoint com `[AllowAnonymous]`
- Filtro OpenAPI não adiciona BearerAuth para endpoint sem `[Authorize]`

## Passos de Implementação

1. Criar o arquivo `VideosControllerAuthTests.cs` com os seguintes testes:

   - `UploadVideo_WhenSubClaimMissing_ReturnsUnauthorized`
     - Arrange: `ClaimsPrincipal` sem claim `sub`; mock do `IUploadVideoUseCase`
     - Act: chamar `UploadVideo(input, CancellationToken.None)`
     - Assert: resultado é `UnauthorizedResult`; UseCase **não** é chamado

   - `UploadVideo_WhenSubClaimIsNotGuid_ReturnsUnauthorized`
     - Arrange: `ClaimsPrincipal` com claim `sub` = `"not-a-guid"`
     - Assert: resultado é `UnauthorizedResult`

   - `UploadVideo_WhenSubClaimIsValidGuid_CallsUseCaseWithCorrectUserId`
     - Arrange: `ClaimsPrincipal` com claim `sub` = UUID válido; UseCase mock retornando `UploadVideoResponseModel`
     - Assert: resultado é `CreatedAtActionResult`; UseCase chamado com o `Guid` correspondente ao sub

2. Criar o arquivo `BearerAuthSecurityOperationFilterTests.cs` com os seguintes testes:

   - `Apply_WhenMethodHasClassLevelAuthorize_AddsBearerSecurity`
     - Arrange: `MethodInfo` de um método em classe com `[Authorize]`; sem `[AllowAnonymous]`
     - Assert: `operation.Security` contém o requisito `BearerAuth`

   - `Apply_WhenMethodHasAllowAnonymous_DoesNotAddSecurity`
     - Arrange: `MethodInfo` de método com `[AllowAnonymous]`
     - Assert: `operation.Security` está vazio

   - `Apply_WhenMethodHasNoAuthorize_DoesNotAddSecurity`
     - Arrange: `MethodInfo` de método sem `[Authorize]` e em classe sem `[Authorize]`
     - Assert: `operation.Security` está vazio

3. Configurar o `HttpContext` no controller via `ControllerContext` para injetar o `ClaimsPrincipal` nos testes do controller.

4. Executar `dotnet test` e confirmar que todos os novos testes passam.

5. Verificar cobertura mínima de 80% para `VideosController` e `BearerAuthSecurityOperationFilter`.

## Formas de Teste

1. **Automático:** `dotnet test` deve passar com todos os novos testes sem erros.
2. **Cobertura:** `dotnet test --collect:"XPlat Code Coverage"` — verificar relatório para os dois arquivos-alvo.
3. **Revisão manual:** confirmar que os testes cobrem os três cenários de sub (ausente, não-UUID, UUID válido) e os três cenários do filtro (classe com Authorize, método com AllowAnonymous, sem Authorize).

## Critérios de Aceite

- [ ] `VideosControllerAuthTests.cs` criado com mínimo 3 testes (sub ausente, sub não-UUID, sub UUID válido)
- [ ] `BearerAuthSecurityOperationFilterTests.cs` criado com mínimo 3 testes (classe-level Authorize, AllowAnonymous, sem Authorize)
- [ ] Todos os testes passam com `dotnet test` sem erros
- [ ] Cobertura mínima de 80% para `VideosController` (fluxo de autenticação) e `BearerAuthSecurityOperationFilter`
- [ ] Testes não dependem de rede (mock do UseCase; sem chamadas reais ao Cognito)
- [ ] Nenhum teste existente quebra após as adições
