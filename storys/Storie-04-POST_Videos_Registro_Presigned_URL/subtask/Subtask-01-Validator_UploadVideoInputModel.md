# Subtask 01: Criar validator de UploadVideoInputModel (FluentValidation)

## Descrição
Implementar `UploadVideoInputModelValidator` usando FluentValidation com regras: originalFileName obrigatório (max 255), contentType whitelist (video/*), sizeBytes positivo e <= limite, durationSec opcional positivo, clientRequestId obrigatório UUID, e registrar validator no DI.

## Passos de Implementação
1. Criar classe `UploadVideoInputModelValidator : AbstractValidator<UploadVideoInputModel>`
2. Regras: RuleFor(x => x.OriginalFileName).NotEmpty().MaximumLength(255)
3. RuleFor(x => x.ContentType).NotEmpty().Must(BeValidVideoContentType) com whitelist: video/mp4, video/quicktime, video/x-msvideo, video/mpeg
4. RuleFor(x => x.SizeBytes).GreaterThan(0).LessThanOrEqualTo(5_000_000_000) // 5 GB
5. RuleFor(x => x.DurationSec).GreaterThan(0).When(x => x.DurationSec.HasValue)
6. RuleFor(x => x.ClientRequestId).NotEmpty().Must(BeValidGuid)
7. Registrar no DI: `services.AddValidatorsFromAssemblyContaining<UploadVideoInputModelValidator>()`

## Formas de Teste
1. Valid input test: validar input válido retorna IsValid = true
2. Invalid tests: originalFileName vazio, contentType inválido (image/png), sizeBytes > limite, clientRequestId não-UUID; validar falhas

## Critérios de Aceite da Subtask
- [ ] UploadVideoInputModelValidator implementado com todas as regras
- [ ] Whitelist de contentType: video/mp4, video/quicktime, video/x-msvideo, video/mpeg
- [ ] SizeBytes limite 5 GB configurável (ler de options ou hardcoded documentado)
- [ ] ClientRequestId validado como UUID
- [ ] Validator registrado no DI
- [ ] Testes unitários validam cada regra (mínimo 6 testes)
