# Subtask 01: Configurar Swashbuckle e gerar OpenAPI JSON básico

## Descrição
Instalar Swashbuckle.AspNetCore, configurar AddSwaggerGen no Program.cs com informações básicas (título, versão, descrição), habilitar XML documentation comments, registrar middleware UseSwagger e UseSwaggerUI, validar que GET /swagger/v1/swagger.json retorna JSON válido do OpenAPI e que interface Swagger UI é acessível localmente.

## Passos de Implementação
1. Instalar package: `dotnet add src/VideoProcessing.VideoManagement.Api package Swashbuckle.AspNetCore`
2. Habilitar XML docs no .csproj da Api: `<GenerateDocumentationFile>true</GenerateDocumentationFile>`; suprimir warnings: `<NoWarn>$(NoWarn);1591</NoWarn>`
3. Configurar AddSwaggerGen: info (título, versão, descrição, contact), IncludeXmlComments (caminho para XML gerado)
4. Registrar UseSwagger e UseSwaggerUI no pipeline
5. Validar: `dotnet run`, acessar http://localhost:5000/swagger, validar UI carrega; acessar /swagger/v1/swagger.json, validar JSON válido

## Formas de Teste
1. Swagger JSON test: `curl http://localhost:5000/swagger/v1/swagger.json`; validar JSON válido
2. Swagger UI test: abrir navegador em http://localhost:5000/swagger; validar que UI carrega e exibe rotas
3. Validation test: copiar JSON do OpenAPI e validar em validator.swagger.io

## Critérios de Aceite da Subtask
- [ ] Swashbuckle.AspNetCore instalado
- [ ] XML documentation habilitado no .csproj
- [ ] AddSwaggerGen configurado com info (título, versão, descrição)
- [ ] UseSwagger e UseSwaggerUI registrados no pipeline
- [ ] GET /swagger/v1/swagger.json retorna JSON válido do OpenAPI
- [ ] Swagger UI acessível em /swagger e exibe rotas base (GET /health, GET /)
