# Subtask 04: Testes unitários (use case, validator, conflitos)

## Descrição
Criar testes unitários para UpdateVideoUseCase, UpdateVideoInputModelValidator e endpoint PATCH /videos/{id}, cobrindo updates de campos individuais, múltiplos campos, conflitos (409), video não encontrado (404), garantindo cobertura >= 80%.

## Passos de Implementação
1. UpdateVideoUseCaseTests: mock repository, testar update de status, progressPercent, múltiplos campos, conflict, not found
2. UpdateVideoInputModelValidatorTests: testar all null (falha), single field (sucesso), progressPercent > 100 (falha)
3. Endpoint tests (opcional): mock useCase, validar status codes

## Formas de Teste
1. All tests pass
2. Coverage >= 80%

## Critérios de Aceite da Subtask
- [ ] UpdateVideoUseCaseTests: mínimo 4 testes (update sucesso, conflict, not found, múltiplos campos)
- [ ] UpdateVideoInputModelValidatorTests: mínimo 4 testes
- [ ] Todos os testes passam
- [ ] Cobertura >= 80%
