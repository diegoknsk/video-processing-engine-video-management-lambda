# Subtask 02: InputModel e Validator — frameIntervalSec e regra 50%

## Descrição
Adicionar a propriedade `FrameIntervalSec` no `UploadVideoInputModel` e implementar no `UploadVideoInputModelValidator` as regras: valor opcional; quando presente deve ser > 0; quando ambos `DurationSec` e `FrameIntervalSec` estiverem presentes, `FrameIntervalSec` não pode ser superior a 50% da duração (ex.: vídeo 60s → máx. 30s).

## Passos de implementação
1. Em `UploadVideoInputModel.cs`: adicionar `public double? FrameIntervalSec { get; init; }` com comentário/Description indicando que é o intervalo em segundos para outro microserviço tirar prints do vídeo.
2. Em `UploadVideoInputModelValidator.cs`:
   - `RuleFor(x => x.FrameIntervalSec).GreaterThan(0).When(x => x.FrameIntervalSec.HasValue).WithMessage("...")`.
   - Regra condicional: quando `DurationSec` e `FrameIntervalSec` têm valor, `FrameIntervalSec <= Math.Floor(DurationSec.Value * 0.5)`. Usar `Must(...)` ou `Custom(...)` com mensagem clara (ex.: "FrameIntervalSec cannot exceed 50% of the video duration.").
3. Garantir que a ordem das regras não conflite (primeiro GreaterThan(0), depois a regra dos 50%).

## Formas de teste
- Teste unitário: FrameIntervalSec = 31 com DurationSec = 60 → deve falhar.
- FrameIntervalSec = 30 com DurationSec = 60 → deve passar.
- FrameIntervalSec = 10 sem DurationSec → deve passar (apenas > 0).
- FrameIntervalSec = 0 ou negativo → deve falhar.

## Critérios de aceite da subtask
- [ ] `UploadVideoInputModel` possui `FrameIntervalSec` (double?, opcional).
- [ ] Validator rejeita FrameIntervalSec <= 0 quando informado.
- [ ] Validator rejeita FrameIntervalSec > Floor(DurationSec * 0.5) quando ambos estão presentes; mensagem de erro clara.
