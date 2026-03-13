# Subtask 04: VideoResponseModel e GETs — expor UserEmail em GET por id (público e interno)

## Descrição
Incluir o campo de email do usuário (ex.: `UserEmail`) no `VideoResponseModel` e no mapeamento Domain → ResponseModel; garantir que o GET público (GET /videos/{id}) e o GET interno (GET internal/videos/{userId}/{id}) retornem esse campo no corpo da resposta.

## Passos de implementação
1. **VideoResponseModel:** Adicionar propriedade opcional `string? UserEmail` (ou nome alinhado ao contrato da API) no record `VideoResponseModel`.
2. **VideoResponseModelMapper:** No método que mapeia `Video` → `VideoResponseModel`, incluir o mapeamento de `UserEmail` (entidade.Video.UserEmail → response.UserEmail); quando for null, o JSON pode serializar como `null` ou omitir o campo conforme configuração do System.Text.Json.
3. **GET público:** O endpoint GET /videos/{id} já utiliza `GetVideoByIdUseCase` e retorna `VideoResponseModel`; como o mapper passa a incluir `UserEmail`, a resposta do GET público passará a expor o campo automaticamente.
4. **GET interno:** O endpoint GET internal/videos/{userId}/{id} também retorna `VideoResponseModel` via `GetVideoByIdUseCase`; nenhuma alteração adicional além do mapper é necessária para expor o email.
5. **OpenAPI/Scalar:** Atualizar a documentação (exemplo de resposta, schema) para incluir o campo `userEmail` (ou `UserEmail`) na resposta dos GETs de vídeo por id; marcar como opcional (nullable) na especificação.

## Formas de teste
1. **Teste unitário:** Dado um `Video` com `UserEmail` preenchido, mapear para `VideoResponseModel` e verificar que a propriedade está preenchida; com `UserEmail` null, verificar que o modelo tem null.
2. **Teste de integração ou manual:** Criar um vídeo (POST) com usuário que tenha email; chamar GET /videos/{id} e GET internal/videos/{userId}/{id} e validar que o corpo da resposta contém o campo de email.
3. **Verificação OpenAPI:** Abrir Scalar/Swagger e inspecionar o schema de resposta do GET por id; confirmar que o novo campo aparece e está documentado como opcional.

## Critérios de aceite da subtask
- [ ] `VideoResponseModel` possui a propriedade opcional de email; `VideoResponseModelMapper` mapeia `Video.UserEmail` para o modelo de resposta.
- [ ] O GET público (GET /videos/{id}) e o GET interno (GET internal/videos/{userId}/{id}) retornam o email do usuário no corpo da resposta quando o dado estiver disponível no item do DynamoDB; quando ausente, o campo é null ou omitido.
- [ ] A documentação OpenAPI/Scalar foi atualizada para incluir o campo na resposta dos GETs de vídeo por id.
