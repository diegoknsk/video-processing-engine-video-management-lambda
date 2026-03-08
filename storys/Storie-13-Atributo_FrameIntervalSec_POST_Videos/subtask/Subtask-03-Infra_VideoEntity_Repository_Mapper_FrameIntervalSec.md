# Subtask 03: Infra — VideoEntity, VideoRepository e VideoMapper

## Descrição
Incluir o atributo `frameIntervalSec` no DTO de persistência (`VideoEntity`), na leitura/escrita do `VideoRepository` (DynamoDB) e no `VideoMapper` (Domain ↔ Entity).

## Passos de implementação
1. Em `VideoEntity.cs`: adicionar `public double? FrameIntervalSec { get; set; }`.
2. Em `VideoRepository.cs` (método que monta o item DynamoDB para PutItem/Update): se `entity.FrameIntervalSec.HasValue`, adicionar `item["frameIntervalSec"] = new AttributeValue { N = entity.FrameIntervalSec.Value.ToString(CultureInfo.InvariantCulture) }`.
3. No método que mapeia atributos DynamoDB → VideoEntity (ex.: AttributeMapToEntity): ler `frameIntervalSec` com o mesmo padrão de outros números (GetD ou TryGetValue) e atribuir a `FrameIntervalSec`.
4. Em `VideoMapper.ToEntity(Video)`: mapear `video.FrameIntervalSec` para `entity.FrameIntervalSec`.
5. Em `VideoMapper.ToDomain(VideoEntity)`: passar `entity.FrameIntervalSec` para o construtor de `VideoRehydrationData` (ajustar a chamada ao record incluindo o novo parâmetro na ordem correta).

## Formas de teste
- Build; testes existentes do repositório devem continuar passando.
- Teste de integração ou unitário do repositório: criar entidade com FrameIntervalSec = 15, persistir, buscar e verificar que o valor é 15.

## Critérios de aceite da subtask
- [ ] `VideoEntity` possui `FrameIntervalSec` (double?).
- [ ] Persistência no DynamoDB grava e lê o atributo `frameIntervalSec`.
- [ ] `VideoMapper` mapeia `FrameIntervalSec` em ambas as direções (ToEntity e ToDomain).
