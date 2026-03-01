# Subtask-06: Validar build e testes

## Descrição
Executar build completo da solution, rodar todos os testes unitários e validar que a reorganização não quebrou funcionalidade, dependências ou a padronização de respostas da API (envelope da Storie-05.1).

## Passos de Implementação
1. Na raiz do repositório, executar `dotnet restore` e `dotnet build` e garantir que não há erros nem warnings de referência.
2. Executar `dotnet test` e garantir que todos os testes passam.
3. Se houver testes de integração ou smoke que dependam de paths (ex.: carregar assembly), executá-los e corrigir se necessário.
4. Opcional: rodar a API localmente e validar que um endpoint (ex.: GET /health ou GET /videos) retorna resposta no formato padronizado (success, data/error, timestamp) conforme api-response-standardization.

## Formas de Teste
1. Build: `dotnet build` na raiz — saída sem erros.
2. Testes: `dotnet test --no-build` (ou `dotnet test`) — todos os testes verdes.
3. Execução da API: `dotnet run --project src/InterfacesExternas/VideoProcessing.VideoManagement.Api` (ou path equivalente) e chamada a um endpoint para verificar resposta envelope.
4. Verificar que Domain não referencia outros projetos (regra de dependência); pode usar análise de referências do IDE ou script.

## Critérios de Aceite
- [x] `dotnet build` conclui com sucesso na raiz da solution.
- [x] `dotnet test` conclui com sucesso; nenhum teste ignorado ou falhando por causa da reorganização.
- [x] Regras de dependência respeitadas: Domain sem referências a Application/Infra/Api/Lambda; Application só Domain; Infra Application+Domain; Api/Lambda referenciam Application, Domain, Infra (não entre si).
- [x] Comportamento da API inalterado: respostas continuam no formato padronizado (Storie-05.1 / api-response-standardization) quando aplicável.
