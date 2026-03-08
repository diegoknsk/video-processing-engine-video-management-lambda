# Subtask 01: Adapter de evento (detectar SQS vs JSON direto e extrair DTOs)

## Descrição
Criar o componente adapter de entrada que recebe o evento bruto da Lambda (no formato SQS da AWS ou JSON direto do DTO de update) e retorna uma sequência de `UpdateVideoLambdaEvent` para processamento. O adapter deve detectar o formato de forma confiável: se o evento contiver a propriedade `Records` como array com pelo menos um elemento que possua `body`, tratar como SQS (extrair e desserializar cada `body`); caso contrário, tratar o payload como um único DTO e desserializar diretamente para `UpdateVideoLambdaEvent`.

## Passos de Implementação
1. Definir interface do adapter (ex.: `IUpdateVideoEventAdapter`) com método que recebe o evento bruto (ex.: `JsonDocument` ou `Stream`/string) e retorna `IReadOnlyList<UpdateVideoLambdaEvent>` (ou resultado tipado com sucesso/erro por record).
2. Implementar a lógica de detecção: verificar se existe `Records` (array) e se o primeiro item tem propriedade `body`; se sim, iterar sobre `Records`, ler cada `body` (string), desserializar com `JsonSerializer.Deserialize<UpdateVideoLambdaEvent>` usando opções compatíveis com o contrato (camelCase, case-insensitive).
3. Se não for formato SQS, desserializar o JSON completo como um único `UpdateVideoLambdaEvent`.
4. Tratar erros de desserialização (ex.: body inválido em SQS, JSON direto malformado): retornar lista vazia ou resultado de erro conforme contrato definido (ex.: exceção ou resultado tipado com mensagem de erro); garantir que não quebre a Lambda com exceção não tratada.
5. Manter opções de serialização alinhadas ao contrato existente (JsonPropertyName, camelCase) já usadas em `UpdateVideoLambdaEvent` e testes de deserialização.

## Formas de Teste
1. Teste unitário: passar JSON no formato SQS com um `Record` e `body` contendo payload válido (videoId, userId, status) e garantir que o adapter retorna uma lista com um elemento com VideoId e UserId preenchidos.
2. Teste unitário: passar JSON direto (objeto com videoId, userId, status) e garantir que o adapter retorna uma lista com um único elemento com os mesmos valores.
3. Teste unitário: passar evento SQS com dois Records (dois bodies válidos) e garantir que a lista retornada tem dois DTOs.
4. Teste unitário: passar evento com `Records` vazio ou body inválido e validar comportamento (lista vazia ou erro explícito, sem exceção não tratada).

## Critérios de Aceite da Subtask
- [ ] Interface (ou contrato) do adapter definida no projeto LambdaUpdateVideo e implementação que recebe evento bruto (JsonDocument/Stream/string).
- [ ] Formato SQS detectado quando existir `Records` (array) com pelo menos um item contendo `body`; cada `body` desserializado para `UpdateVideoLambdaEvent`.
- [ ] Formato JSON direto detectado quando não for SQS; payload desserializado como um único `UpdateVideoLambdaEvent`.
- [ ] Desserialização usa opções consistentes com o contrato (camelCase, case-insensitive); a mensagem do Step Function já utiliza os mesmos nomes do DTO (`s3BucketFrames`, `framesPrefix`, `videoId`, `userId`, etc.).
- [ ] Casos de payload inválido ou malformado tratados sem lançar exceção não capturada (retorno ou exceção controlada documentada).
- [ ] Testes unitários cobrindo: SQS 1 record válido; JSON direto válido; SQS 2 records; Records vazio ou body inválido (mínimo 4 cenários).
