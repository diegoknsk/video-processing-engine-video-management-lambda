# Geração de Client C# com Kiota

## Visão Geral

O **Kiota** é uma ferramenta da Microsoft que gera clientes tipados e type-safe a partir de especificações OpenAPI. Este documento descreve como gerar um cliente C# para consumir a Video Processing Auth API.

## Pré-requisitos

### Instalação do Kiota CLI

Instale o Kiota CLI globalmente usando o .NET CLI:

```bash
dotnet tool install --global Microsoft.OpenApi.Kiota
```

**Verificar instalação:**
```bash
kiota --version
```

**Atualizar Kiota (se já instalado):**
```bash
dotnet tool update --global Microsoft.OpenApi.Kiota
```

## Gerar Client C#

### Comando Básico

Execute o comando abaixo para gerar o cliente C# a partir da especificação OpenAPI:

```bash
kiota generate \
  --openapi https://api.example.com/openapi/v1.json \
  --language CSharp \
  --class-name VideoProcessingAuthClient \
  --namespace VideoProcessing.Clients.Auth \
  --output ./clients/VideoProcessing.Clients.Auth
```

### Parâmetros Explicados

- `--openapi`: URL ou caminho local para o arquivo OpenAPI JSON
  - **Produção**: `https://api.example.com/prod/openapi/v1.json`
  - **Desenvolvimento**: `https://api.example.com/dev/openapi/v1.json`
  - **Local**: `http://localhost:5000/swagger/v1/swagger.json` (ou salvar JSON localmente)
- `--language`: Linguagem do cliente (`CSharp`, `TypeScript`, `Python`, `Java`, `Go`, etc.)
- `--class-name`: Nome da classe principal do cliente gerado
- `--namespace`: Namespace C# do cliente gerado
- `--output`: Diretório onde o cliente será gerado

### Exemplo com Arquivo Local

Se preferir usar um arquivo OpenAPI salvo localmente:

```bash
# 1. Baixar OpenAPI JSON
curl http://localhost:5000/swagger/v1/swagger.json -o openapi.json

# 2. Gerar cliente a partir do arquivo local
kiota generate \
  --openapi ./openapi.json \
  --language CSharp \
  --class-name VideoProcessingAuthClient \
  --namespace VideoProcessing.Clients.Auth \
  --output ./clients/VideoProcessing.Clients.Auth
```

## Onde Salvar o Client

### Opção 1: Repositório Separado (Recomendado para múltiplos serviços)

Crie um repositório dedicado para clients:

```
video-processing-clients/
  ├── VideoProcessing.Clients.Auth/
  │   └── [arquivos gerados pelo Kiota]
  └── VideoProcessing.Clients.VideoManagement/
      └── [outros clients]
```

### Opção 2: Pasta `clients/` na Raiz do Repositório

Se o client for específico deste serviço:

```
video-processing-engine-auth-lambda/
  ├── clients/
  │   └── VideoProcessing.Clients.Auth/
  │       └── [arquivos gerados pelo Kiota]
  ├── src/
  └── tests/
```

### Opção 3: Projeto Separado na Mesma Solução

Adicione um projeto de client na solução:

```
VideoProcessing.Auth.sln
  ├── src/
  │   ├── VideoProcessing.Auth.Api/
  │   └── VideoProcessing.Clients.Auth/  (novo projeto)
  └── tests/
```

## Uso Básico do Client Gerado

### Exemplo: Login

```csharp
using VideoProcessing.Clients.Auth;
using Microsoft.Kiota.Http.HttpClientLibrary;

// Criar cliente HTTP
var httpClient = new HttpClient 
{ 
    BaseAddress = new Uri("https://api.example.com/prod") 
};

// Criar cliente Kiota
var client = new VideoProcessingAuthClient(httpClient);

// Executar login
var loginRequest = new LoginRequest
{
    Username = "usuario123",
    Password = "senhaSegura123!"
};

try
{
    var loginResponse = await client.Auth.Login.PostAsync(loginRequest);
    Console.WriteLine($"Access Token: {loginResponse.AccessToken}");
    Console.WriteLine($"Id Token: {loginResponse.IdToken}");
    Console.WriteLine($"Refresh Token: {loginResponse.RefreshToken}");
    Console.WriteLine($"Expires In: {loginResponse.ExpiresIn} segundos");
}
catch (ApiException ex)
{
    Console.WriteLine($"Erro ao fazer login: {ex.Message}");
}
```

### Exemplo: Criar Usuário

```csharp
var createUserRequest = new CreateUserRequest
{
    Username = "novousuario",
    Password = "senhaSegura123!",
    Email = "usuario@example.com"
};

try
{
    var createUserResponse = await client.Auth.Users.Create.PostAsync(createUserRequest);
    Console.WriteLine($"Usuário criado: {createUserResponse.UserId}");
    Console.WriteLine($"Confirmação necessária: {createUserResponse.ConfirmationRequired}");
}
catch (ApiException ex)
{
    if (ex.StatusCode == 409)
    {
        Console.WriteLine("Usuário já existe");
    }
    else
    {
        Console.WriteLine($"Erro ao criar usuário: {ex.Message}");
    }
}
```

### Exemplo: Health Check

```csharp
var healthResponse = await client.Health.GetAsync();
Console.WriteLine($"Status: {healthResponse.Status}");
Console.WriteLine($"Timestamp: {healthResponse.Timestamp}");
```

## Atualizar Client

Sempre que a especificação OpenAPI mudar, atualize o client:

```bash
# Usar comando update (se suportado) ou generate novamente
kiota generate \
  --openapi https://api.example.com/prod/openapi/v1.json \
  --language CSharp \
  --class-name VideoProcessingAuthClient \
  --namespace VideoProcessing.Clients.Auth \
  --output ./clients/VideoProcessing.Clients.Auth \
  --clear-output
```

O parâmetro `--clear-output` limpa o diretório antes de gerar novos arquivos.

## Versionamento do Client

Recomendações:

1. **Tags no Git**: Crie tags alinhadas às versões da API (ex.: `v1.0.0`, `v1.1.0`)
2. **NuGet Package**: Considere publicar o client como pacote NuGet para facilitar consumo
3. **Changelog**: Mantenha um changelog documentando mudanças entre versões

## Outras Linguagens Suportadas

O Kiota suporta múltiplas linguagens. Exemplos:

### TypeScript
```bash
kiota generate \
  --openapi https://api.example.com/openapi/v1.json \
  --language TypeScript \
  --class-name VideoProcessingAuthClient \
  --namespace VideoProcessing.Clients.Auth \
  --output ./clients/typescript
```

### Python
```bash
kiota generate \
  --openapi https://api.example.com/openapi/v1.json \
  --language Python \
  --class-name VideoProcessingAuthClient \
  --namespace video_processing.clients.auth \
  --output ./clients/python
```

### Java
```bash
kiota generate \
  --openapi https://api.example.com/openapi/v1.json \
  --language Java \
  --class-name VideoProcessingAuthClient \
  --namespace com.videoprocessing.clients.auth \
  --output ./clients/java
```

## Notas Importantes

- **Type-Safe**: O client gerado é totalmente type-safe, evitando erros de compilação
- **IntelliSense**: Aproveite o IntelliSense completo do IDE para descobrir métodos e propriedades
- **Validação**: O client valida automaticamente os dados antes de enviar requisições
- **Documentação**: Consulte a [documentação oficial do Kiota](https://github.com/microsoft/kiota) para recursos avançados

## Troubleshooting

### Erro: "OpenAPI specification is invalid"

- Valide o JSON OpenAPI em https://editor.swagger.io
- Verifique se a URL está acessível e retorna JSON válido

### Erro: "Namespace conflict"

- Altere o `--namespace` para evitar conflitos com namespaces existentes

### Client não compila

- Verifique se todas as dependências NuGet necessárias estão instaladas
- O Kiota pode gerar referências a pacotes que precisam ser instalados manualmente

## Referências

- [Kiota GitHub](https://github.com/microsoft/kiota)
- [Kiota Documentation](https://learn.microsoft.com/en-us/openapi/kiota/)
- [OpenAPI Specification](https://swagger.io/specification/)
