# Subtask 05: Adaptar VideoManagement (remover ou encaminhar PATCH)

## Descrição
Adaptar o projeto VideoManagement (Lambda API com AddAWSLambdaHosting) de forma a deixar de expor/implementar o PATCH de update internamente **ou** manter o endpoint PATCH como proxy que encaminha a requisição para a nova Lambda (VideoProcessing.VideoManagement.LambdaUpdateVideo). Documentar claramente o comportamento esperado e os impactos para clientes (evitar breaking change se optar por proxy).

## Passos de Implementação
1. Decidir e documentar a estratégia: (A) Remover o action PATCH do VideosController e documentar que clientes (orchestrator, processor, finalizer) devem passar a invocar a nova Lambda (ou rota/API futura que a exponha); ou (B) Manter PATCH /videos/{id} no VideoManagement e implementar o action como proxy: receber body, chamar a Lambda Update Video (AWS Lambda Invoke) com o mesmo payload + videoId, e retornar a resposta da Lambda ao cliente (mesmo contrato HTTP 200/400/404/409)
2. Se (A): remover action UpdateVideo, remover registro de IUpdateVideoUseCase e validator do DI, remover ou ajustar testes do controller que dependem do PATCH; atualizar OpenAPI (remover operação PATCH); documentar em docs ou README a migração e a nova forma de invocação (Lambda direta ou rota futura)
3. Se (B): no action PATCH, em vez de chamar UpdateVideoUseCase, invocar a Lambda (AWSSDK.Lambda InvokeAsync ou Invoke) com payload serializado (videoId + body); mapear resposta da Lambda para 200/404/409/400 e corpo no padrão da Storie-05.1; manter [AllowAnonymous] e contratos iguais para o cliente
4. Atualizar critérios de aceite e documentação da Storie-06 (ou criar nota de migração) indicando que a implementação do update foi movida para a Lambda Update Video
5. Garantir que `dotnet build` e `dotnet test` passam no VideoManagement após as alterações (incluindo testes do controller que permanecem)

## Formas de Teste
1. Se proxy: chamar PATCH /videos/{id} com body válido e validar que a resposta é 200 com dados do vídeo (comportamento idêntico ao atual do ponto de vista do cliente)
2. Se remoção: chamar PATCH /videos/{id} e validar que retorna 404 ou que a rota não existe mais; testes unitários do controller atualizados ou removidos
3. Executar suite de testes do VideoManagement; nenhum teste quebrado por causa da mudança

## Critérios de Aceite da Subtask
- [ ] Estratégia (remover PATCH ou proxy para Lambda) documentada com comportamento esperado e impacto para clientes
- [ ] Se remoção: action PATCH removido, DI e OpenAPI atualizados; documentação de migração criada
- [ ] Se proxy: PATCH mantém mesmo contrato (body, respostas 200/400/404/409); chamada à Lambda Update Video implementada; respostas mapeadas no padrão da Storie-05.1
- [ ] Build e testes do VideoManagement passando após a alteração
- [ ] Sem breaking change para clientes no caso de proxy; no caso de remoção, documentação clara para migração
