# Subtask 02: Revisar Program.cs e AddAWSLambdaHosting

## Descrição
Revisar o Program.cs da API (Lambda Hosting, ordem do pipeline) e documentar se o estado atual está correto para deploy na AWS Lambda e conforme as regras/skills do projeto.

## Passos de implementação
1. Abrir Program.cs e verificar se AddAWSLambdaHosting está presente ou removido; anotar o comentário existente ("removed temporarily for diagnosis/compatibility on .NET 10").
2. Confirmar no skill lambda-api-hosting que o Handler e o uso de AddAWSLambdaHosting(LambdaEventSource.HttpApi) são obrigatórios para Lambda com HTTP API.
3. Verificar a ordem do pipeline: GatewayPathBaseMiddleware antes de UseRouting()/MapControllers(); documentar se UseRouting() explícito é necessário.
4. Registrar no Resultado da Revisão (story.md) os findings e a recomendação de reintroduzir AddAWSLambdaHosting (ou justificar exceção).

## Formas de teste
1. Ler a seção "Program.cs e Lambda Hosting" da revisão e confirmar que está completa.
2. Validar que a recomendação está clara para o time corrigir em follow-up.

## Critérios de aceite da subtask
- [x] Revisão documenta que AddAWSLambdaHosting foi removido e o impacto (deploy Lambda)
- [x] Recomendação registrada: reintroduzir AddAWSLambdaHosting ou documentar exceção
- [x] Ordem do pipeline (middleware antes de roteamento) verificada e documentada
- [x] Nenhuma alteração em Program.cs nesta story — apenas revisão
