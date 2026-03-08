# Subtask 01: Domain — Video e VideoRehydrationData com FrameIntervalSec

## Descrição
Adicionar o atributo `FrameIntervalSec` (double?, segundos) na entidade `Video` do Domain e no record `VideoRehydrationData`, garantindo que o valor seja definido na criação ou via setter e que `FromMerge` e reidratação o preservem.

## Passos de implementação
1. Em `Video.cs`: adicionar propriedade `public double? FrameIntervalSec { get; private set; }` e método `public void SetFrameIntervalSec(double frameIntervalSec)` (validar > 0; atualizar `UpdatedAt`).
2. Em `VideoRehydrationData`: adicionar parâmetro `double? FrameIntervalSec` na posição adequada (ex.: após `DurationSec` ou próximo a outros opcionais numéricos).
3. No construtor interno que recebe `VideoRehydrationData`: atribuir `FrameIntervalSec = d.FrameIntervalSec`.
4. Em `Video.FromMerge`: incluir `FrameIntervalSec: existing.FrameIntervalSec` no `VideoRehydrationData` (frameIntervalSec não é alterável via PATCH; manter valor existente).
5. No construtor público `Video(userId, originalFileName, ...)`: não definir FrameIntervalSec (será setado pelo UseCase após criação, se o input tiver o valor).

## Formas de teste
- Build da solution; nenhum uso de Video sem o novo campo deve quebrar.
- Teste unitário opcional: criar Video, chamar `SetFrameIntervalSec(10)` e verificar que a propriedade retorna 10 e UpdatedAt foi setado.
- Verificar que `FromPersistence` e `FromMerge` compilam e preservam FrameIntervalSec.

## Critérios de aceite da subtask
- [ ] Propriedade `FrameIntervalSec` (double?) existe em `Video` com get private set.
- [ ] Método `SetFrameIntervalSec(double)` valida > 0 e seta `UpdatedAt`.
- [ ] `VideoRehydrationData` inclui `FrameIntervalSec`; reidratação e `FromMerge` o utilizam corretamente.
