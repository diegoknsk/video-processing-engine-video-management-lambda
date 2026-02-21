# Subtask 02: Corrigir validator — validação UUID em ClientRequestId

## Descrição
Adicionar regra no `UploadVideoInputModelValidator` para validar que `ClientRequestId`, quando preenchido, seja um UUID válido (Guid.TryParse). O campo permanece opcional no contrato, mas se fornecido deve ter formato UUID para garantir a idempotência correta no UseCase.

## Passos de Implementação
1. Abrir `src/VideoProcessing.VideoManagement.Application/Validators/UploadVideoInputModelValidator.cs`
2. Na regra de `ClientRequestId`, encadear após `MaximumLength(100)` a regra:
   `.Must(id => Guid.TryParse(id, out _)).WithMessage("ClientRequestId must be a valid UUID (GUID format).")`
3. A condição `When(x => !string.IsNullOrEmpty(x.ClientRequestId))` deve envolver ambas as regras (MaximumLength e Must)
4. Verificar que o teste de validator já existente (quando for criado na subtask 05) cobre este cenário

## Formas de Teste
1. Teste unitário: ClientRequestId = "not-a-guid" → validação deve falhar com mensagem clara
2. Teste unitário: ClientRequestId = Guid.NewGuid().ToString() → validação deve passar
3. Teste unitário: ClientRequestId = null → validação deve passar (campo opcional)
4. Compilar: `dotnet build` sem erros

## Critérios de Aceite da Subtask
- [ ] `ClientRequestId` com valor não-UUID retorna erro de validação com mensagem "ClientRequestId must be a valid UUID (GUID format)."
- [ ] `ClientRequestId` null ou vazio não aciona as regras de formato (campo opcional)
- [ ] `ClientRequestId` com UUID válido (ex.: "550e8400-e29b-41d4-a716-446655440000") passa a validação
- [ ] `dotnet build` sem erros
