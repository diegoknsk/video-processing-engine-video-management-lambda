# Subtask 05: Testes unitários (adapter, UseCase, mapeamento e resposta)

## Descrição
Garantir cobertura de testes unitários para o adapter de email (Cognito), para o uso no UploadVideoUseCase (persistência do email), para o mapeamento Domain/Entity/ResponseModel com e sem UserEmail, e para o comportamento do GET quando o item tem ou não o atributo.

## Passos de implementação
1. **Testes do adapter de email (Infra):** Criar ou estender testes do serviço que implementa o port (ex.: `CognitoUserEmailServiceTests`). Cenários: (a) AdminGetUser retorna usuário com atributo email → retorno do adapter é o email; (b) usuário sem email ou atributo ausente → retorno null; (c) exceção do cliente (ex.: UserNotFoundException) → retorno null ou exceção tratada conforme contrato.
2. **Testes do UploadVideoUseCase:** (a) Quando o serviço de email retorna um email, o repositório recebe uma chamada CreateAsync com entidade/Video que contém UserEmail preenchido; (b) quando o serviço retorna null, a criação ainda ocorre e o vídeo persistido tem UserEmail null (ou comportamento definido na Subtask 03).
3. **Testes do VideoMapper (Domain ↔ Entity):** (a) ToEntity com Video que tem UserEmail preenchido → VideoEntity com UserEmail; (b) ToDomain com VideoEntity sem o atributo (ou null) → Video com UserEmail null; (c) ToDomain com VideoEntity com UserEmail → Video com mesmo valor.
4. **Testes do VideoResponseModelMapper:** (a) Video com UserEmail → VideoResponseModel com UserEmail; (b) Video com UserEmail null → VideoResponseModel com UserEmail null.
5. **Testes do GetVideoByIdUseCase (opcional):** Garantir que quando o repositório retorna um Video com UserEmail, o resultado do UseCase (VideoResponseModel) inclui o email; quando retorna Video sem email, o response tem null. Isso pode já estar coberto pelos testes de mapper e do repositório.
6. **Executar** `dotnet test` e verificar que todos os testes passam; verificar cobertura (≥ 80% nos trechos alterados, quando aplicável) conforme política do projeto.

## Formas de teste
1. Executar a suíte de testes unitários; todos devem passar.
2. Gerar relatório de cobertura (se configurado) e revisar cobertura nos namespaces/classes alterados.
3. Teste manual de ponta a ponta: POST /videos → GET /videos/{id} e GET internal; validar presença do email na resposta.

## Critérios de aceite da subtask
- [ ] Testes unitários do adapter de email passando (sucesso com email, sem email/null, exceção tratada).
- [ ] Testes unitários do UploadVideoUseCase passando (email persistido quando serviço retorna email; criação com null quando serviço retorna null).
- [ ] Testes do VideoMapper e do VideoResponseModelMapper cobrindo UserEmail (preenchido e null); testes do repositório ou do GetVideoByIdUseCase quando aplicável.
- [ ] `dotnet test` passa sem erros; cobertura adequada nos trechos alterados (conforme critério do projeto, ex.: ≥ 80%).
