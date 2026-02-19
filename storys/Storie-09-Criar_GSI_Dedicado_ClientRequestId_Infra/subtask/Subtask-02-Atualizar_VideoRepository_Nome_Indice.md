# Subtask-02: Atualizar VideoRepository para Usar o Novo Nome do Índice

## Descrição
Após o GSI `GSI_ClientRequestId` estar ativo na tabela, atualizar `VideoRepository.cs` e `VideoEntity.cs` para trocar a referência de `"GSI1"` (workaround atual) para `"GSI_ClientRequestId"`.

## Passos de Implementação
1. Em `VideoRepository.cs`, no método `GetByClientRequestIdAsync`, alterar:
   ```csharp
   // De:
   IndexName = "GSI1",
   // Para:
   IndexName = "GSI_ClientRequestId",
   ```
2. Em `VideoEntity.cs`, atualizar os comentários para refletir o nome correto do índice.
3. Verificar se há outros usos de `"GSI1"` no repositório que também precisam de atualização.

## Formas de Teste
1. Executar `dotnet test` para garantir que todos os testes unitários passam.
2. Testar manualmente o endpoint `POST /videos` com `clientRequestId` duplicado e verificar retorno `409 Conflict`.
3. Verificar nos logs que a query DynamoDB usa `GSI_ClientRequestId` (não `GSI1`).

## Critérios de Aceite
- [ ] `VideoRepository.cs` referencia `"GSI_ClientRequestId"` no método `GetByClientRequestIdAsync`
- [ ] Comentários em `VideoEntity.cs` refletem o nome correto
- [ ] `dotnet test` passa sem erros
