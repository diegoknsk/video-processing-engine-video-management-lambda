# Subtask 02: Integrar adapter no ponto de entrada da Lambda (Function)

## Descrição
Alterar o ponto de entrada da Lambda (Function) para que o handler receba o **evento bruto** (formato que a AWS envia: SQS envelope ou JSON direto) em vez de `UpdateVideoLambdaEvent` diretamente. O Function deve usar o adapter criado na Subtask 01 para obter um ou mais `UpdateVideoLambdaEvent`, invocar `IUpdateVideoHandler.HandleAsync` para cada um e retornar a resposta de forma consistente (ex.: última resposta quando múltiplos records; ou falhar no primeiro erro para permitir retry do batch SQS).

## Passos de Implementação
1. Alterar a assinatura do método handler na `Function.cs`: o parâmetro de evento deve ser um tipo que represente o payload bruto (ex.: `Stream`, `JsonDocument`, ou tipo genérico que o runtime Lambda desserializa para SQS/direto). Consultar documentação AWS Lambda .NET para o tipo de entrada quando a origem é SQS (ex.: `Amazon.Lambda.SQSEvents.SQSEvent` ou entrada como `JsonDocument`/objeto dinâmico).
2. No início do handler: obter o adapter via DI (registrar o adapter no `Startup`/container); chamar o adapter passando o evento bruto e obter a lista de `UpdateVideoLambdaEvent`.
3. Se a lista estiver vazia (evento malformado ou sem records válidos): retornar resposta de erro adequada (ex.: 400 com mensagem indicando payload inválido) ou lançar exceção controlada conforme decisão da Subtask 01.
4. Para cada `UpdateVideoLambdaEvent` retornado pelo adapter: invocar `_handler.HandleAsync(evt, cancellationToken)` e coletar a resposta. Definir política: retornar a última resposta de sucesso; ou retornar a primeira resposta com erro e interromper; ou processar todos e retornar a última (documentar a escolha).
5. Registrar o adapter no container de DI (Startup) para ser injetado na Function (ou instanciar o adapter dentro da Function se for stateless e sem dependências).
6. Garantir que o serializer/entrada da Lambda permita receber tanto o envelope SQS quanto o JSON direto (o adapter fará a distinção em runtime).

## Formas de Teste
1. Teste de integração ou unitário do Function (com adapter mockado): evento SQS simulado → adapter retorna um DTO → handler é invocado uma vez com o DTO correto.
2. Teste com evento JSON direto simulado → adapter retorna um DTO → handler é invocado uma vez.
3. Invocação local (Lambda Test Tool) com payload SQS de exemplo e com payload JSON direto; validar que a resposta da Lambda é a mesma que antes (200/400/404/409) e que os logs mostram VideoId/UserId preenchidos quando via SQS.
4. Verificar que os testes existentes do `UpdateVideoHandler` continuam passando (o handler em si não muda; apenas a forma de invocação no Function).

## Critérios de Aceite da Subtask
- [ ] O método handler da `Function` recebe o evento bruto (tipo adequado para SQS + JSON direto) e não mais diretamente `UpdateVideoLambdaEvent`.
- [ ] O adapter é utilizado para converter o evento bruto em lista de `UpdateVideoLambdaEvent`; o handler existente é invocado para cada item.
- [ ] Com um único evento (SQS com 1 record ou JSON direto), o comportamento e a resposta da Lambda são equivalentes ao atual (sem regressão).
- [ ] Com múltiplos SQS Records, a política de processamento e de retorno está implementada e documentada (ex.: processar todos e retornar última resposta; ou falhar no primeiro erro).
- [ ] Caso o adapter retorne lista vazia, a Lambda retorna resposta de erro (ex.: 400) ou trata de forma consistente sem crash.
- [ ] Adapter registrado no DI (ou instanciado) e `dotnet build` passa; testes existentes do handler continuam verdes.
