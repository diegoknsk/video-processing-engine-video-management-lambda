# Subtask-03: Validar e Atualizar Testes Unitários do Repositório

## Descrição
Garantir que os testes unitários do `VideoRepository` reflitam o nome correto do índice `GSI_ClientRequestId` e que a cobertura dos cenários de idempotência esteja completa.

## Passos de Implementação
1. Em `VideoRepositoryTests.cs`, verificar os testes de `GetByClientRequestIdAsync` e atualizar eventuais assertions que verifiquem o `IndexName` usado na query DynamoDB.
2. Confirmar que os cenários cobertos incluem: item encontrado, item não encontrado, `clientRequestId` vazio/nulo.
3. Executar `dotnet test --collect:"XPlat Code Coverage"` e verificar cobertura mínima.

## Formas de Teste
1. `dotnet test` — todos os testes passando (exit code 0).
2. Verificar que nenhum teste usa o nome `"GSI1"` hardcoded nos assertions.
3. Revisar cobertura dos métodos `GetByClientRequestIdAsync` e `CreateAsync`.

## Critérios de Aceite
- [ ] Todos os testes unitários do repositório passando após a mudança do nome do índice
- [ ] Nenhuma referência a `"GSI1"` hardcoded nos arquivos de teste
- [ ] Cobertura dos cenários: item encontrado, não encontrado e clientRequestId vazio
