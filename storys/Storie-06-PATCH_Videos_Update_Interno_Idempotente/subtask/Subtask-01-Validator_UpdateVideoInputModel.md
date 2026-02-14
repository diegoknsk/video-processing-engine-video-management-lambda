# Subtask 01: Criar validator de UpdateVideoInputModel (pelo menos 1 campo presente)

## Descrição
Implementar UpdateVideoInputModelValidator com regras: pelo menos 1 campo deve estar presente (status, progressPercent, errorMessage, errorCode, framesPrefix, s3KeyZip, s3BucketFrames, s3BucketZip, stepExecutionArn), progressPercent se presente deve estar entre 0-100, status se presente deve ser enum válido.

## Passos de Implementação
1. Criar classe UpdateVideoInputModelValidator
2. RuleFor: verificar que pelo menos 1 propriedade != null (custom validator: `Must(x => x.Status != null || x.ProgressPercent != null || ...)`)
3. RuleFor(x => x.ProgressPercent).InclusiveBetween(0, 100).When(x => x.ProgressPercent.HasValue)
4. RuleFor(x => x.Status).IsInEnum().When(x => x.Status.HasValue)
5. Registrar no DI

## Formas de Teste
1. All null test: validar que falha (pelo menos 1 campo obrigatório)
2. Valid single field: status presente, demais null; validar sucesso
3. ProgressPercent > 100: validar falha

## Critérios de Aceite da Subtask
- [ ] UpdateVideoInputModelValidator implementado
- [ ] Pelo menos 1 campo obrigatório (custom rule)
- [ ] ProgressPercent 0-100 quando presente
- [ ] Status enum válido quando presente
- [ ] Registrado no DI
- [ ] Testes unitários (mínimo 4 testes)
