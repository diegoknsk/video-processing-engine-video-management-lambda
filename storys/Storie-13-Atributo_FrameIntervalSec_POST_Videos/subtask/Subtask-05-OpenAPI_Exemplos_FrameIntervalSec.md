# Subtask 05: API/OpenAPI e documentação

## Descrição
Atualizar a documentação OpenAPI (exemplos e descrições) para o POST /videos e GET /videos de forma que `frameIntervalSec` apareça nos exemplos e na descrição do contrato.

## Passos de implementação
1. Em `OpenApiExamplesAndErrorsFilter.cs`, no método que adiciona exemplos do POST /videos (ex.: AddUploadVideoExamples): no `OpenApiObject` do request body, adicionar `["frameIntervalSec"] = new OpenApiDouble(30)` (ou valor coerente com durationSec do exemplo, ex.: durationSec 120.5 → frameIntervalSec 60).
2. Opcional: na descrição do endpoint POST (AddUploadVideoDocumentation), mencionar que `frameIntervalSec` é opcional e indica o intervalo em segundos para captura de frames por outro microserviço, não podendo exceder 50% da duração quando durationSec for informado.
3. No exemplo de resposta do GET /videos (AddGetVideoResponseExample), adicionar `["frameIntervalSec"] = new OpenApiDouble(30)` no exemplo de resposta para consistência.

## Formas de teste
- Abrir Scalar UI (ou Swagger); verificar que o exemplo de POST /videos inclui `frameIntervalSec` e que o schema do GET exibe o campo no exemplo de resposta.

## Critérios de aceite da subtask
- [ ] Exemplo de request POST /videos no OpenAPI inclui `frameIntervalSec`.
- [ ] Exemplo de response GET /videos no OpenAPI inclui `frameIntervalSec` quando aplicável.
- [ ] Documentação do POST menciona o campo e a regra dos 50% (opcional mas recomendado).
