# Subtask 04: UseCase e Response — UploadVideoUseCase e VideoResponseModel

## Descrição
Propagar `FrameIntervalSec` do input para a entidade `Video` no `UploadVideoUseCase` (ao criar novo vídeo). Incluir `FrameIntervalSec` no `VideoResponseModel` e no `VideoResponseModelMapper` para que GET /videos e listagem retornem o valor.

## Passos de implementação
1. Em `UploadVideoUseCase.cs`: após criar o `Video` e chamar `SetDuration` quando aplicável, se `input.FrameIntervalSec.HasValue`, chamar `video.SetFrameIntervalSec(input.FrameIntervalSec.Value)` antes de `SetS3Source` e `CreateAsync`.
2. Em cenário de idempotência (retorno de vídeo existente): não alterar FrameIntervalSec; o valor já está persistido.
3. Em `VideoResponseModel.cs`: adicionar `public double? FrameIntervalSec { get; init; }` com Description.
4. Em `VideoResponseModelMapper.ToResponseModel(Video)`: mapear `FrameIntervalSec = video.FrameIntervalSec`.

## Formas de teste
- Teste unitário do UseCase: input com FrameIntervalSec = 10; mock de repository.CreateAsync; verificar que a entidade passada ao repositório tem FrameIntervalSec = 10 (via captor ou comportamento do mock).
- GET /videos/{id} com vídeo que possui FrameIntervalSec persistido deve retornar o campo no JSON.

## Critérios de aceite da subtask
- [ ] Ao criar novo vídeo, `UploadVideoUseCase` chama `SetFrameIntervalSec` quando o input contém `FrameIntervalSec`.
- [ ] `VideoResponseModel` expõe `FrameIntervalSec`; `VideoResponseModelMapper` preenche o campo a partir da entidade.
