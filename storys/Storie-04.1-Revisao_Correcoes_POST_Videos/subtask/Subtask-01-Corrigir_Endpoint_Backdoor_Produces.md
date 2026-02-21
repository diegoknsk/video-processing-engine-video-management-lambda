# Subtask 01: Corrigir endpoint — remover backdoor e corrigir .Produces duplicado

## Descrição
Remover o fallback inseguro de `x-user-id` header do endpoint POST /videos e corrigir o `.Produces(401)` duplicado que deveria documentar o status 500.

## Passos de Implementação
1. Abrir `src/VideoProcessing.VideoManagement.Api/Endpoints/VideosEndpoints.cs`
2. Remover o bloco `if (httpContext.Request.Headers.TryGetValue("x-user-id", ...)` — manter apenas o retorno `Results.Unauthorized()` quando claim "sub" estiver ausente ou inválido
3. Substituir o segundo `.Produces(StatusCodes.Status401Unauthorized)` (linha 70) por `.Produces<ErrorResponse>(StatusCodes.Status500InternalServerError)`
4. Remover o `using Microsoft.Extensions.DependencyInjection; // For FromServices` se não for mais necessário após limpeza
5. Garantir que o bloco de extração de userId fique simples: parse do claim → falhou → 401

## Formas de Teste
1. Compilar o projeto: `dotnet build` sem warnings de código morto
2. Testar manualmente no Scalar UI: POST /videos sem Authorization header → deve retornar 401
3. Testar com header `x-user-id` após remoção: deve retornar 401 (não aceita mais o bypass)
4. Verificar documentação OpenAPI: endpoint deve listar respostas 201, 400, 401 e 500 corretamente

## Critérios de Aceite da Subtask
- [ ] Bloco `x-user-id` completamente removido do endpoint
- [ ] Sem token JWT, endpoint retorna 401 (não aceita mais bypass por header)
- [ ] Documentação do endpoint lista: 201 (UploadVideoResponseModel), 400 (ErrorResponse), 401, 500 (ErrorResponse)
- [ ] `dotnet build` sem erros nem warnings relacionados ao endpoint
