# Subtask 03: Implementar handler e persistência DynamoDB (update condicional)

## Descrição
Implementar no projeto LambdaUpdateVideo a lógica do handler: receber o evento (videoId + body), validar entrada, buscar o vídeo existente no DynamoDB (GetByIdAsync por userId e videoId), aplicar o merge dos campos presentes (Video.FromMerge / VideoUpdateValues), executar update condicional no DynamoDB (mesmas regras do UpdateVideoUseCase: ownership, progressPercent monotônico, transições de status) e retornar sucesso com vídeo atualizado ou erro (404, 409, 400) com código e mensagem. Utilizar a mesma tabela e modelagem do VideoManagement (IVideoRepository ou cliente DynamoDB equivalente).

## Passos de Implementação
1. Implementar no handler: deserializar evento para o tipo de entrada (videoId + campos do body), validar UserId e pelo menos um campo de atualização (ou reutilizar regras do UpdateVideoInputModelValidator)
2. Obter repositório ou cliente DynamoDB (via DI no bootstrap do Lambda ou instanciação): GetByIdAsync(userId, videoId) para carregar o vídeo existente; retornar 404 se não encontrado
3. Construir VideoUpdateValues a partir dos campos presentes no evento e chamar Video.FromMerge(video, patch); chamar UpdateAsync (ou equivalente) com condições DynamoDB (condição de versão/progressão conforme VideoRepository atual)
4. Tratar VideoUpdateConflictException (ou equivalente) e retornar resposta de erro 409 com mensagem; retornar 200 com representação do vídeo atualizado (ex.: mesmo shape do VideoResponseModel) em caso de sucesso
5. Configurar variáveis de ambiente da Lambda para tabela DynamoDB e região (DynamoDB__TableName, AWS__Region ou equivalente), sem hardcode

## Formas de Teste
1. Teste unitário: handler com repositório mockado — vídeo existente, patch aplicado, UpdateAsync chamado uma vez com entidade merged
2. Teste unitário: vídeo não encontrado (GetByIdAsync retorna null) → handler retorna 404
3. Teste unitário: UpdateAsync lança VideoUpdateConflictException → handler retorna 409 com mensagem
4. Invocação local com evento JSON de exemplo (mínimo e completo) e tabela DynamoDB de dev (se disponível) para validar persistência

## Critérios de Aceite da Subtask
- [ ] Handler implementado: deserialização do evento, validação, busca do vídeo, merge e update condicional
- [ ] Mesmas regras de negócio do UpdateVideoUseCase (idempotência, progressão monotônica, transições de status) aplicadas
- [ ] Resposta de sucesso com dados do vídeo atualizado; erros 404 (não encontrado), 409 (conflito), 400 (validação)
- [ ] Configuração de tabela DynamoDB e região via variáveis de ambiente
- [ ] Testes unitários cobrindo cenários: sucesso, 404, 409 (mínimo 3 cenários)
