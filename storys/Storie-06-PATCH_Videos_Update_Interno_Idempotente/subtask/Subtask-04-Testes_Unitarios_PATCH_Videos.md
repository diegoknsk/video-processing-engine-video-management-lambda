# Subtask 04: Testes unitários (use case e validator)

## Descrição
Criar testes unitários para `UpdateVideoUseCase` e `UpdateVideoInputModelValidator`, cobrindo updates de campos individuais, múltiplos campos, vídeo não encontrado, conflito de concorrência e regras de validação, garantindo cobertura >= 80%.

> **Nota:** Testes do controller (`UpdateVideo` action) são opcionais nesta subtask. Focar a cobertura no UseCase e no Validator.

## Passos de Implementação
1. `UpdateVideoUseCaseTests`:
   - Mock `IVideoRepository` com `GetByIdAsync` e `UpdateAsync`
   - **Update de status:** input com `Status = Processing`, `UserId = userId`; validar `VideoResponseModel.Status == Processing`
   - **Update de progressPercent:** input com `ProgressPercent = 50`; validar campo atualizado
   - **Múltiplos campos:** input com status + progressPercent + errorMessage; validar que todos aplicados e campos não informados mantêm valor original do video
   - **Vídeo não encontrado:** mock `GetByIdAsync` retorna `null`; validar que UseCase retorna `null`
   - **Conflito:** mock `UpdateAsync` lança `VideoUpdateConflictException`; validar que exceção propaga sem ser capturada

2. `UpdateVideoInputModelValidatorTests`:
   - **UserId vazio:** `UserId = Guid.Empty`; deve falhar com mensagem de UserId
   - **Todos campos de atualização nulos:** apenas `UserId` preenchido; deve falhar com "pelo menos um campo"
   - **Apenas Status presente:** `UserId` + `Status = Processing`; deve passar
   - **ProgressPercent = 101:** deve falhar
   - **ProgressPercent = 50:** deve passar

3. Verificar `dotnet test` passa sem erros

## Formas de Teste
```
dotnet test --filter "UpdateVideo"
```

## Critérios de Aceite da Subtask
- [ ] `UpdateVideoUseCaseTests` com mínimo 5 testes (status, progressPercent, múltiplos campos, não encontrado, conflito)
- [ ] Validar que campos não informados no input mantêm o valor existente do vídeo (merge correto)
- [ ] `VideoUpdateConflictException` propaga sem captura no UseCase
- [ ] `UpdateVideoInputModelValidatorTests` com mínimo 5 testes
- [ ] `GetByIdAsync` chamado com `userId` e `videoId` como strings
- [ ] Todos os testes passam
- [ ] Cobertura >= 80% para `UpdateVideoUseCase`
