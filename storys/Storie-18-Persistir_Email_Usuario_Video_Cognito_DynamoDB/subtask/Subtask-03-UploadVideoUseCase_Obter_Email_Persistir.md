# Subtask 03: UploadVideoUseCase — obter email na criação e persistir no vídeo

## Descrição
No fluxo de criação de vídeo (POST /videos), após obter o `userId` (sub do JWT), chamar o port de obtenção de email do usuário e associar o valor ao vídeo antes de persistir no DynamoDB.

## Passos de implementação
1. **Injetar o port** (ex.: `IGetUserEmailService`) no `UploadVideoUseCase` via construtor primário.
2. **No fluxo de criação** (caminho onde se cria novo `Video` e chama `repository.CreateAsync`): antes de persistir, chamar o serviço de email passando o `userId` (como string, ex.: `userId.ToString()`); obter o resultado (`string?`).
3. **Associar o email ao vídeo:** Como a entidade `Video` pode não expor setter público para `UserEmail`, definir no Domain uma forma de criar o vídeo já com UserEmail (ex.: overload do construtor, factory ou método estático que recebe email) ou adicionar método interno/ público que defina o UserEmail antes do primeiro persist. A opção mais limpa é incluir `userEmail` no caminho de construção do `Video` que leva ao `CreateAsync` (ex.: factory ou construtor que aceita email opcional).
4. **Persistir:** Garantir que o `Video` passado a `CreateAsync` (ou o DTO usado no repositório) contenha o `UserEmail`; o repositório já mapeia para `VideoEntity` (Subtask 02).
5. **Tratamento de falha:** Se a obtenção do email falhar (ex.: serviço Cognito indisponível ou sem permissão), decidir política: (a) falhar a criação do vídeo (401/503) ou (b) criar o vídeo com `UserEmail` null e logar o erro. Documentar a decisão; recomendação pragmática: (b) para não bloquear a criação, com log de warning.
6. **Idempotência:** No caminho idempotente (vídeo já existe para o mesmo clientRequestId), não sobrescrever o vídeo existente; o email já estará no item se foi criado com a nova versão do código.

## Formas de teste
1. **Teste unitário:** Mockar `IGetUserEmailService` para retornar um email; executar o UseCase de criação; verificar que o repositório recebeu um `Video` (ou entidade) com `UserEmail` preenchido.
2. **Teste unitário:** Mockar o serviço para retornar null; verificar que a criação ainda ocorre e que o vídeo é persistido com `UserEmail` null (ou que a política de falha está aplicada, conforme decisão).
3. **Teste manual:** POST /videos com token válido; inspecionar item no DynamoDB e confirmar presença do atributo de email.

## Critérios de aceite da subtask
- [ ] O `UploadVideoUseCase` utiliza o port de email; na criação de novo vídeo, o email é obtido a partir do `userId` e associado ao vídeo antes de `CreateAsync`.
- [ ] O vídeo persistido no DynamoDB contém o atributo de email do usuário quando a obtenção for bem-sucedida; quando falhar ou retornar null, o comportamento está de acordo com a política definida (criar com null e logar, ou falhar).
- [ ] Testes unitários do UseCase cobrindo cenário com email e sem email (ou falha) passando.
