# Subtask 03: Testes unitários do adapter e atualização da documentação

## Descrição
Completar a cobertura de testes unitários do adapter de entrada (cenários não cobertos na Subtask 01, se houver) e atualizar a documentação do contrato da Lambda (`docs/lambda-update-video-contract.md`) para descrever explicitamente os dois modos de invocação: via SQS (produção, evento com `Records[].Body`) e via JSON direto (testes locais e execução manual), com exemplos de payload para cada um.

## Passos de Implementação
1. Revisar os testes do adapter: garantir cenários para evento SQS válido (1 e N records), JSON direto válido, `Records` vazio, `body` nulo ou string não-JSON, e evento com estrutura híbrida ou inesperada (ex.: objeto com `Records` mas também `videoId` — definir qual formato prevalece e testar).
2. Atualizar `docs/lambda-update-video-contract.md`: adicionar seção "Formatos de invocação" (ou equivalente) descrevendo (a) **Invocação via SQS:** estrutura do evento (`Records`, `body`), exemplo de mensagem enviada pelo Step Function e exemplo do envelope completo que a Lambda recebe; (b) **Invocação direta (JSON):** uso do mesmo JSON do DTO (videoId, userId, status, etc.) como corpo do evento para testes no Lambda Test Tool ou invocação manual.
3. Incluir na documentação um exemplo de evento SQS completo (com um record e body no mesmo formato do DTO: `videoId`, `userId`, `s3BucketFrames`, `framesPrefix`, etc.) e reforçar que em produção a Lambda é acionada pela fila SQS com esse formato.
4. Verificar cobertura de testes do adapter (meta ≥ 80%) e que `dotnet test` passa para o projeto LambdaUpdateVideo e testes da solução.

## Formas de Teste
1. Executar `dotnet test` no projeto de testes e no projeto da Lambda; todos os testes devem passar.
2. Rodar cobertura (ex.: `dotnet test --collect:"XPlat Code Coverage"`) e verificar que o adapter está coberto conforme meta.
3. Revisão da documentação: um desenvolvedor consegue invocar a Lambda manualmente com JSON direto e via simulação SQS seguindo apenas o doc.

## Critérios de Aceite da Subtask
- [ ] Testes unitários do adapter cobrem eventos SQS (1 e N records), JSON direto, e casos de erro/malformado; cobertura do adapter ≥ 80%.
- [ ] Documentação `docs/lambda-update-video-contract.md` contém seção clara sobre os dois formatos de invocação (SQS e JSON direto).
- [ ] Exemplo de evento SQS completo (envelope com `Records[].body`) documentado; exemplo de JSON direto já existente mantido ou referenciado.
- [ ] `dotnet build` e `dotnet test` passam na solução; nenhuma regressão nos testes do handler ou da Lambda.
- [ ] Payload do body (SQS) e do JSON direto seguem o mesmo contrato do DTO (`videoId`, `userId`, `s3BucketFrames`, `framesPrefix`, etc.); não há mapeamento de nomes adicional.
