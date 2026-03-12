# Subtask 01: Port e adapter para obter email do usuário (Cognito)

## Descrição
Criar o port na camada Application para obter o email do usuário a partir do identificador (ex.: `sub` do Cognito) e implementar o adapter em Infra que integra com o Amazon Cognito (Admin GetUser ou leitura de claim, conforme viabilidade).

## Passos de implementação
1. **Definir o port** na Application (ex.: `IGetUserEmailService` ou `IUserEmailProvider`) com método assíncrono que recebe o identificador do usuário (string, ex.: `sub`) e retorna `Task<string?>`, permitindo null quando o email não for encontrado.
2. **Decidir a estratégia de obtenção do email:** (a) se o access token já incluir o claim `email`, extrair no middleware/controller e opcionalmente usar um serviço que apenas lê do contexto; (b) caso contrário, implementar adapter que chama `AdminGetUser` do Cognito Identity Provider com o `sub`, lendo o atributo `email` da resposta.
3. **Implementar o adapter** em Infra (ex.: `CognitoUserEmailService`) que implementa o port: injetar `IAmazonCognitoIdentityProvider` (ou cliente equivalente), configurar User Pool ID via options/ambiente e implementar a chamada a `AdminGetUserAsync`; tratar exceções (usuário não encontrado, permissão negada) retornando null ou logando e relançando conforme política do projeto.
4. **Registrar o adapter** no DI da API (Program ou ServiceCollectionExtensions), usando configuração existente de Cognito (ex.: `COGNITO_USER_POOL_ID`) quando houver.
5. **Documentar** no código ou em comentário a necessidade de permissão IAM `cognito-idp:AdminGetUser` para o role da Lambda/API, caso se use Admin GetUser.

## Formas de teste
1. **Teste unitário:** Mockar o cliente Cognito (ou o port); quando o usuário existe e tem email, o adapter retorna o email; quando não existe ou não tem email, retorna null.
2. **Teste de integração (opcional):** Chamar o adapter com um `sub` real de um usuário de teste no User Pool e validar que o email é retornado.
3. **Teste manual:** Após deploy, criar um vídeo via POST /videos e verificar no DynamoDB que o atributo de email foi preenchido (validado na Subtask 03).

## Critérios de aceite da subtask
- [ ] O port está definido na Application (interface com método async que recebe identificador e retorna `string?`).
- [ ] O adapter em Infra implementa o port e obtém o email via Cognito (Admin GetUser ou claim), com tratamento de erro definido (retorno null ou exceção documentada).
- [ ] O adapter está registrado no container de DI e a configuração (User Pool ID, etc.) está ligada a variáveis de ambiente ou options existentes.
- [ ] Testes unitários do adapter passando (cenário sucesso com email, cenário sem email/null).
