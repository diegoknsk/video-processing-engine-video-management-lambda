---
name: security
description: Guia para segurança — autenticação, autorização, secrets management, rate limiting, CORS, security headers e proteção de APIs. Use quando a tarefa envolver segurança, autenticação, JWT, secrets, proteção ou vulnerabilidades.
---

# Security — Autenticação, Secrets e Proteção

## Quando Usar

- **Autenticação**, **autorização**, **JWT**
- **Secrets management**, variáveis de ambiente
- **Rate limiting**, **CORS**, proteção de APIs
- Palavras-chave: "segurança", "autenticação", "JWT", "secrets", "proteção", "vulnerabilidade"

## Princípios Essenciais

### ✅ Fazer

- Usar **JWT** para autenticação stateless (APIs REST)
- **User Secrets** (dev), **variáveis de ambiente** (prod) para dados sensíveis
- **Rate limiting** para proteger contra abuso (ASP.NET Core 7+)
- **HTTPS** obrigatório em produção
- **[Authorize]** em controllers/endpoints que precisam autenticação
- Validar **Issuer, Audience, Lifetime, SigningKey** no JWT

### ❌ Não Fazer

- **Nunca** commitar secrets (senhas, tokens, API keys) no código/appsettings
- **Nunca** usar secrets no client-side (JavaScript, HTML)
- **Nunca** logar dados sensíveis (senhas, tokens, CPF, cartão)
- **Nunca** confiar em dados do client sem validação
- **Nunca** expor stack traces ou mensagens de erro detalhadas em produção

**Regra de ouro:** JWT + Secrets Management + Rate Limiting = 80% da segurança de API.

## Checklist Rápido

1. Instalar `Microsoft.AspNetCore.Authentication.JwtBearer`
2. Configurar JWT no `Program.cs`: `AddAuthentication().AddJwtBearer()`
3. Criar `TokenService` para gerar/validar tokens
4. Secrets em `User Secrets` (dev) e variáveis de ambiente (prod)
5. `[Authorize]` em controllers que precisam autenticação
6. Rate limiting: `AddRateLimiter()` (ASP.NET Core 7+)
7. HTTPS obrigatório: `UseHttpsRedirection()`

## Exemplo Mínimo

**Cenário:** JWT para autenticação de API

### Configuração JWT (Program.cs)

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] 
            ?? throw new InvalidOperationException("JWT SecretKey not configured");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
```

### Secrets (appsettings.json - Placeholders)

```json
{
  "JwtSettings": {
    "SecretKey": "",
    "Issuer": "",
    "Audience": "",
    "ExpirationMinutes": 60
  }
}
```

### User Secrets (Dev)

```bash
dotnet user-secrets init
dotnet user-secrets set "JwtSettings:SecretKey" "your-256-bit-secret-key-min-32-chars"
dotnet user-secrets set "JwtSettings:Issuer" "https://your-domain.com"
dotnet user-secrets set "JwtSettings:Audience" "https://your-api.com"
```

### Variáveis de Ambiente (Prod)

```bash
# Linux/Mac
export JwtSettings__SecretKey="your-secret-key"
export JwtSettings__Issuer="https://your-domain.com"

# Windows
setx JwtSettings__SecretKey "your-secret-key"
```

### TokenService (Gerar Token)

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

public class TokenService(IOptions<JwtSettings> jwtSettings) : ITokenService
{
    private readonly JwtSettings _settings = jwtSettings.Value;

    public string GenerateToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public record JwtSettings
{
    public required string SecretKey { get; init; }
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public int ExpirationMinutes { get; init; } = 60;
}
```

### Controller com [Authorize]

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize] // Requer JWT válido
public class UsersController(IGetUserUseCase getUserUseCase) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserAsync(Guid id)
    {
        // User.Identity.Name contém o "sub" claim do JWT
        var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        var result = await getUserUseCase.ExecuteAsync(id);
        return Ok(result);
    }

    [HttpGet("public")]
    [AllowAnonymous] // Endpoint público (não requer JWT)
    public IActionResult GetPublicData()
    {
        return Ok(new { Message = "Public data" });
    }
}
```

**Pontos-chave:**
- **Secrets** nunca no código (User Secrets dev, variáveis de ambiente prod)
- **JWT** valida Issuer, Audience, Lifetime, SigningKey automaticamente
- **[Authorize]** protege endpoints; **[AllowAnonymous]** permite acesso público

## Rate Limiting (ASP.NET Core 7+)

```csharp
using System.Threading.RateLimiting;

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));

    options.OnRejected = async (context, ct) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsync("Rate limit exceeded. Try again later.", ct);
    };
});

var app = builder.Build();
app.UseRateLimiter();
```

## CORS (APIs Consumidas por Frontend)

```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://your-frontend.com")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();
app.UseCors();
```

## Autorização por Role

```csharp
[HttpDelete("{id}")]
[Authorize(Roles = "Admin")] // Apenas usuários com role "Admin"
public async Task<IActionResult> DeleteUserAsync(Guid id)
{
    // ...
}

// Ou policies customizadas
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("MinimumAge", policy => policy.Requirements.Add(new MinimumAgeRequirement(18)));
});

[Authorize(Policy = "RequireAdmin")]
public async Task<IActionResult> AdminOnlyAsync() { }
```

## Dados Sensíveis (Nunca Expor)

- ❌ Senhas em plain text (usar hash: BCrypt, Argon2)
- ❌ Tokens, API keys no código/logs
- ❌ Dados pessoais (CPF, RG, cartão) em logs
- ❌ Stack traces em produção
- ✅ Hash senhas com `BCrypt.Net`
- ✅ Secrets em User Secrets (dev) ou variáveis de ambiente (prod)
- ✅ Logar apenas IDs não-sensíveis (UserId, OrderId)

```csharp
// Hash de senha com BCrypt
using BCrypt.Net;

public string HashPassword(string password)
{
    return BCrypt.HashPassword(password, BCrypt.GenerateSalt(12));
}

public bool VerifyPassword(string password, string hash)
{
    return BCrypt.Verify(password, hash);
}
```

## HTTPS Obrigatório

```csharp
// Program.cs
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
    app.UseHsts(); // HTTP Strict Transport Security
}
```

## Referências

- [ASP.NET Core Security](https://learn.microsoft.com/en-us/aspnet/core/security/)
- [JWT Authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn)
- [Rate Limiting](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit)
- [Safe Storage of Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)
