# Subtask 05: Testes unitários (filtro e middleware)

## Descrição
Garantir cobertura de testes unitários para o filtro de resposta (ApiResponseFilter) e para o mapeamento de exceções do middleware (MapException ou equivalente), de forma que alterações futuras não quebrem o contrato de padronização.

## Passos de Implementação
1. Criar ou usar projeto de testes unitários da API (ex.: `tests/VideoProcessing.VideoManagement.UnitTests` ou projeto específico para Api).
2. Testes do **ApiResponseFilter**:
   - Cenário: `Result` é `OkObjectResult` com um objeto → após OnActionExecuted, Result deve ser ObjectResult com valor do tipo ApiResponse com Success=true, Data igual ao objeto original, Timestamp preenchido.
   - Cenário: `Result` é ObjectResult com status 201 e um valor → encapsulado em ApiResponse com status 201.
   - Cenário: `Result` é NotFoundResult ou BadRequestResult → não alterado.
3. Testes do **mapeamento de exceções** (se o mapeamento estiver em classe estática ou injetável):
   - UnauthorizedAccessException → (401, "Unauthorized", mensagem esperada).
   - ArgumentException → (400, "BadRequest", mensagem esperada).
   - KeyNotFoundException (ou tipo "not found" do projeto) → (404, "NotFound", mensagem esperada).
   - Exception genérica → (500, "InternalServerError", mensagem esperada).
4. Executar `dotnet test` e garantir que todos os testes passam; cobertura mínima sugerida para as classes do envelope: ≥ 80%.

## Formas de Teste
1. Executar `dotnet test` no projeto de testes.
2. Verificar que cada cenário do filtro e do mapeamento está coberto por pelo menos um teste.
3. Opcional: verificar relatório de cobertura para ApiResponseFilter e lógica de MapException.

## Critérios de Aceite da Subtask
- [ ] Existem testes para ApiResponseFilter: OkObjectResult e ObjectResult 200/201 encapsulados; outros resultados não alterados.
- [ ] Existem testes para o mapeamento de exceção: pelo menos 401, 400, 404 e 500.
- [ ] `dotnet test` passa sem erros.
- [ ] Cobertura ≥ 80% para as classes de padronização (filtro e mapeamento), quando mensurável.
