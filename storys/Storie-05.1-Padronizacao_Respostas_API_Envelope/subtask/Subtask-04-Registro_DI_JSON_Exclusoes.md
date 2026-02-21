# Subtask 04: Registro no DI, JSON camelCase e exclusões (health)

## Descrição
Registrar o filtro de resposta nos controllers, garantir opções de JSON com camelCase (e WhenWritingNull se desejado) e definir exclusão do endpoint `/health` (ou equivalente) do envelope para não quebrar ferramentas que esperam corpo específico de health.

## Passos de Implementação
1. Em `ServiceCollectionExtensions` (ou onde os controllers são configurados), registrar o filtro global:
   - `AddControllers(options => options.Filters.Add<ApiResponseFilter>())`.
2. Garantir que `AddControllers()` use `AddJsonOptions` com:
   - `PropertyNamingPolicy = JsonNamingPolicy.CamelCase`
   - Opcional: `DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull`
3. No `ApiResponseFilter`, implementar exclusão para não encapsular respostas de health:
   - Por exemplo: se `context.ActionDescriptor.RouteValues["action"]` ou nome do controller indicar Health, não aplicar o envelope (deixar o resultado original).
   - Alternativa: atributo customizado no HealthController/action para opt-out do filtro (se a infraestrutura permitir).
4. Em `Program.cs`, garantir que o middleware de exceção global esteja registrado (feito na Subtask 03) e que a ordem do pipeline esteja correta (middleware de exceção antes de UseRouting).

## Formas de Teste
1. Chamar GET /health e verificar que a resposta não está encapsulada em `{ "success", "data", "timestamp" }` (ou que mantém formato esperado pelo load balancer/ferramenta).
2. Chamar qualquer outro endpoint de sucesso e verificar que a resposta está encapsulada e em camelCase.
3. Verificar que propriedades nulas (se aplicável) são omitidas quando UsingWhenWritingNull.

## Critérios de Aceite da Subtask
- [ ] `ApiResponseFilter` registrado globalmente em `AddControllers(options => options.Filters.Add<ApiResponseFilter>())`.
- [ ] JSON da API usa `PropertyNamingPolicy.CamelCase`; contrato exposto como success, data, error, timestamp, code, message.
- [ ] Endpoint de health excluído do envelope (resposta sem encapsulamento ou conforme necessidade do projeto).
- [ ] Pipeline mantém middleware de exceção na ordem correta; `dotnet run` e chamadas manuais passam.
