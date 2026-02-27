# Subtask 04: Validação de payload e testes unitários (contrato e handler)

## Descrição
Garantir validação do payload de entrada da Lambda (UserId obrigatório, pelo menos um campo de atualização, ProgressPercent 0–100, Status enum válido) e cobertura de testes unitários para o contrato e o handler: validação de payload, handler com repositório mockado (sucesso, 404, 409), e deserialização do event shape com exemplos mínimo e completo.

## Passos de Implementação
1. Implementar ou reutilizar validação (FluentValidation ou regras manuais) no projeto Lambda: UserId não vazio, pelo menos um campo de atualização presente, ProgressPercent entre 0 e 100 quando informado, Status valor válido do enum quando informado
2. Criar testes unitários para a validação: payload sem UserId (inválido), payload só com UserId sem outros campos (inválido), payload com UserId e Status (válido), ProgressPercent fora de 0–100 (inválido)
3. Criar testes unitários para o handler: evento válido → retorno de sucesso e chamada a UpdateAsync; GetByIdAsync retorna null → retorno 404; UpdateAsync lança VideoUpdateConflictException → retorno 409
4. Incluir teste de deserialização: JSON do exemplo mínimo e do exemplo completo (docs) deserializam corretamente para o tipo de entrada da Lambda
5. Garantir cobertura ≥ 80% para o handler/camada de aplicação da Lambda (excluindo bootstrap e infra se necessário)

## Formas de Teste
1. Executar `dotnet test` no projeto LambdaUpdateVideo e na solução; todos os testes passam
2. Verificar cobertura (dotnet test /p:CollectCoverage=true ou ferramenta do repositório) ≥ 80% para o handler
3. Alterar um valor do JSON de exemplo (ex.: ProgressPercent = 101) e validar que o teste de validação falha conforme esperado

## Critérios de Aceite da Subtask
- [ ] Validação de payload implementada e alinhada às regras do UpdateVideoInputModelValidator (UserId, pelo menos um campo, ProgressPercent 0–100, Status válido)
- [ ] Testes unitários para validação: mínimo 4 cenários (inválido sem UserId, inválido sem campos de update, válido, ProgressPercent inválido)
- [ ] Testes unitários para handler: sucesso, 404, 409 (mínimo 3 cenários)
- [ ] Teste de deserialização dos exemplos JSON (mínimo e completo) documentados
- [ ] Cobertura ≥ 80% para o handler/use case da Lambda; `dotnet test` passa
