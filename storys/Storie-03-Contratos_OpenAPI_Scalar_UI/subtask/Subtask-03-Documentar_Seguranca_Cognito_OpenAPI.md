# Subtask 03: Documentar esquemas de segurança (Cognito/JWT) no OpenAPI

## Descrição
Adicionar securitySchemes ao OpenAPI documentando autenticação via JWT (Bearer token do Cognito), aplicar security requirement nas rotas protegidas (POST /videos, GET /videos, GET /videos/{id}, PATCH /videos/{id}), deixar GET /health como público, e garantir que Swagger UI exibe botão "Authorize" para input de token.

## Passos de Implementação
1. Configurar AddSwaggerGen para incluir securityScheme: tipo "http", scheme "bearer", bearerFormat "JWT", description "JWT do Amazon Cognito (claim sub = userId)"
2. Aplicar security requirement global ou por operação usando OperationFilter
3. Criar OperationFilter customizado que aplica security apenas em rotas protegidas (excluir /health)
4. Validar que botão "Authorize" aparece no Swagger UI
5. Documentar no OpenAPI info/description que autenticação é via Cognito User Pool

## Formas de Teste
1. Swagger UI authorize button: abrir /swagger, validar que botão "Authorize" está presente
2. Security scheme test: validar que /swagger/v1/swagger.json contém securitySchemes.BearerAuth
3. Protected routes test: validar que operations de POST /videos, GET /videos têm security: [{ BearerAuth: [] }]

## Critérios de Aceite da Subtask
- [ ] securitySchemes.BearerAuth configurado no OpenAPI (tipo http, scheme bearer, bearerFormat JWT)
- [ ] Security requirement aplicada em rotas protegidas (POST /videos, GET /videos, GET /videos/{id}, PATCH /videos/{id})
- [ ] GET /health não tem security requirement (pública)
- [ ] Swagger UI exibe botão "Authorize" para input de token JWT
- [ ] Descrição de autenticação documentada no OpenAPI info ou description
