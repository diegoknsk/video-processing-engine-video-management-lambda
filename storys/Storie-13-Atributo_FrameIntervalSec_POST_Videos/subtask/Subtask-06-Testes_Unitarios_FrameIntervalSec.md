# Subtask 06: Testes unitários (validator e use case)

## Descrição
Incluir testes unitários para as novas regras do validator (FrameIntervalSec > 0, regra dos 50% com DurationSec) e para o UseCase (propagação de FrameIntervalSec para a entidade e persistência).

## Passos de implementação
1. Em `UploadVideoInputModelValidatorTests.cs` (ou equivalente):
   - Cenário: FrameIntervalSec válido com DurationSec (ex.: 30 para vídeo 60s) → sucesso.
   - Cenário: FrameIntervalSec > 50% de DurationSec (ex.: 31 para 60s) → falha com mensagem esperada.
   - Cenário: FrameIntervalSec presente e <= 0 → falha.
   - Cenário: FrameIntervalSec presente sem DurationSec (apenas > 0) → sucesso.
2. Em `UploadVideoUseCaseTests.cs`:
   - Cenário: input com FrameIntervalSec preenchido; verificar que a entidade Video passada ao repositório (CreateAsync) possui FrameIntervalSec igual ao input (via mock/captor ou assert no objeto criado).
   - Opcional: idempotência com vídeo já existente que tem FrameIntervalSec → resposta não altera o valor persistido.

## Formas de teste
- Executar `dotnet test` nos projetos de teste da Application; todos os testes devem passar.
- Cobertura: manter ou melhorar cobertura do validator e do UseCase.

## Critérios de aceite da subtask
- [ ] Testes do validator cobrem: valor válido (≤ 50% de durationSec), valor inválido (> 50%), valor ≤ 0, e FrameIntervalSec sem durationSec (> 0).
- [ ] Teste do UseCase verifica que FrameIntervalSec do input é persistido na entidade enviada ao repositório.
