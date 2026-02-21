# Subtask 01: Atualizar UpdateVideoInputModel e criar validator

## Descrição
Adicionar campo `UserId` (Guid, obrigatório) ao `UpdateVideoInputModel`, pois a rota PATCH é interna (sem JWT) e o caller sempre conhece o userId. Criar `UpdateVideoInputModelValidator` com regras: `UserId` obrigatório e não-empty, pelo menos 1 campo de atualização presente, `ProgressPercent` entre 0–100 quando informado, `Status` deve ser enum válido quando informado.

## Passos de Implementação
1. Adicionar campo ao `UpdateVideoInputModel`:
   ```csharp
   /// <summary>Identificador do usuário dono do vídeo (obrigatório — caller interno conhece o userId).</summary>
   [Description("Identificador do usuário dono do vídeo")]
   public Guid UserId { get; init; }
   ```
2. Criar `UpdateVideoInputModelValidator`:
   ```csharp
   RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId é obrigatório.");
   RuleFor(x => x)
       .Must(x => x.Status != null || x.ProgressPercent != null || x.ErrorMessage != null
                  || x.ErrorCode != null || x.FramesPrefix != null || x.S3KeyZip != null
                  || x.S3BucketFrames != null || x.S3BucketZip != null || x.StepExecutionArn != null)
       .WithMessage("Pelo menos um campo de atualização deve ser informado.");
   RuleFor(x => x.ProgressPercent).InclusiveBetween(0, 100).When(x => x.ProgressPercent.HasValue);
   RuleFor(x => x.Status).IsInEnum().When(x => x.Status.HasValue);
   ```
3. Registrar `IValidator<UpdateVideoInputModel>, UpdateVideoInputModelValidator` no DI em `ServiceCollectionExtensions`

## Formas de Teste
1. `UserId == Guid.Empty`: deve falhar
2. Todos os campos de atualização `null` (só `UserId` preenchido): deve falhar
3. Apenas `Status` preenchido (além de `UserId`): deve passar
4. `ProgressPercent = 101`: deve falhar
5. `ProgressPercent = 50` com `UserId` válido: deve passar

## Critérios de Aceite da Subtask
- [ ] `UpdateVideoInputModel` possui campo `UserId` (Guid) sem valor default (Guid.Empty inválido)
- [ ] `UpdateVideoInputModelValidator` implementado com regra de `UserId` obrigatório
- [ ] Regra: pelo menos 1 campo de atualização presente além de `UserId`
- [ ] `ProgressPercent` 0–100 quando presente
- [ ] `Status` enum válido quando presente
- [ ] Registrado no DI
- [ ] Testes unitários (mínimo 5 testes)
