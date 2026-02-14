---
name: database-persistence
description: Guia para persistência com EF Core, repositórios, DbContext, migrations e queries eficientes. Use quando a tarefa envolver banco de dados, repositórios, EF Core, DbContext, migrations, queries ou persistência.
---

# Database Persistence — EF Core e Repositórios

## Quando Usar

- Criar ou modificar **repositórios**
- Trabalhar com **EF Core**, **DbContext**, **migrations**
- Queries, persistência de dados
- Palavras-chave: "banco", "database", "repositório", "EF Core", "migration", "DbContext", "query"

## Princípios Essenciais

### ✅ Fazer

- Usar **Repository Pattern** (interface na Application/Ports, implementação na Infra.Persistence)
- **AsNoTracking()** para queries read-only (melhor performance)
- **Include()** para eager loading (evitar N+1 problem)
- **Projeções** (Select) para buscar apenas dados necessários
- **Paginação** para listagens grandes (Skip/Take)
- **CancellationToken** em todas as operações assíncronas
- **Fluent API** (IEntityTypeConfiguration) para configuração de entidades

### ❌ Não Fazer

- **Nunca** expor DbContext para Application ou API (usar repositórios)
- **Nunca** queries síncronas (ToList(), First() — sempre async)
- **Nunca** lógica de negócio no repositório (apenas persistência)
- **Nunca** esquecer AsNoTracking() em queries read-only
- **Nunca** N+1 problem (sempre Include() relações)

**Regra de ouro:** Repositórios fazem ponte entre Application e DB; DbContext fica escondido.

## Checklist Rápido

1. Criar interface na Application/Ports: `IUserRepository`
2. Implementar repositório na Infra.Persistence: `UserRepository(AppDbContext context)`
3. Criar Entity Configuration: `UserConfiguration : IEntityTypeConfiguration<User>`
4. Aplicar configurações no DbContext: `modelBuilder.ApplyConfigurationsFromAssembly()`
5. Registrar no DI: `AddDbContext<AppDbContext>()` + `AddScoped<IUserRepository, UserRepository>()`
6. Criar migration: `dotnet ef migrations add InitialCreate`
7. Aplicar migration: `dotnet ef database update` ou `context.Database.MigrateAsync()`

## Exemplo Mínimo

**Cenário:** Repositório de usuário com CRUD básico

### Interface (Application/Ports)

```csharp
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<User>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
    Task<User> CreateAsync(User user, CancellationToken ct = default);
    Task UpdateAsync(User user, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
```

### Implementação (Infra.Persistence)

```csharp
public class UserRepository(AppDbContext context) : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await context.Users
            .AsNoTracking() // Read-only
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<IEnumerable<User>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
    {
        return await context.Users
            .AsNoTracking()
            .OrderBy(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<User> CreateAsync(User user, CancellationToken ct = default)
    {
        context.Users.Add(user);
        await context.SaveChangesAsync(ct);
        return user;
    }

    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        context.Users.Update(user);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await context.Users.Where(u => u.Id == id).ExecuteDeleteAsync(ct); // EF Core 7+
    }
}
```

### DbContext

```csharp
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
```

### Entity Configuration

```csharp
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);
        
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.HasIndex(u => u.Email).IsUnique();
    }
}
```

### Configuração (Program.cs)

```csharp
// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString); // ou UseNpgsql, UseSqlite
    
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Repositórios
builder.Services.AddScoped<IUserRepository, UserRepository>();
```

**Pontos-chave:**
- **AsNoTracking()** em queries read-only (não rastreia mudanças, mais rápido)
- **Paginação** com Skip/Take para listas grandes
- **Fluent API** para configurações (melhor que Data Annotations)
- **ExecuteDeleteAsync()** para delete direto (EF Core 7+, sem carregar entidade)

## Migrations

```bash
# Criar migration
dotnet ef migrations add InitialCreate --project <Projeto>.Infra.Persistence --startup-project <Projeto>.Api

# Aplicar migration (dev)
dotnet ef database update --project <Projeto>.Infra.Persistence --startup-project <Projeto>.Api

# Aplicar migration (produção, via código)
if (app.Environment.IsProduction())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();
}
```

## Queries Eficientes

```csharp
// ✅ Eager Loading (evitar N+1)
var orders = await context.Orders.Include(o => o.User).ToListAsync(ct);

// ✅ Projeção (apenas campos necessários)
var users = await context.Users.AsNoTracking().Select(u => new { u.Id, u.Name }).ToListAsync(ct);

// ✅ Compiled Query (performance)
private static readonly Func<AppDbContext, Guid, Task<User?>> GetUserByIdQuery =
    EF.CompileAsyncQuery((AppDbContext ctx, Guid id) => ctx.Users.AsNoTracking().FirstOrDefault(u => u.Id == id));

// ✅ Batch operations (EF Core 7+)
await context.Users.Where(u => !u.IsActive).ExecuteDeleteAsync(ct);
```

## Estrutura de Pastas

```
<Projeto>.Infra.Persistence/
  Context/
    AppDbContext.cs
  Repositories/
    <Contexto>/
      UserRepository.cs
  Configurations/
    UserConfiguration.cs
  Migrations/              (geradas automaticamente)
```

## Testes

```csharp
// In-Memory Database
var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
await using var context = new AppDbContext(options);
var repository = new UserRepository(context);

// Arrange, Act, Assert
var user = new User { Id = Guid.NewGuid(), Email = "test@test.com" };
await repository.CreateAsync(user);
var result = await repository.GetByIdAsync(user.Id);
result.Should().NotBeNull();
```

## Connection Strings

```json
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": ""
  }
}
```

```bash
# User Secrets (dev)
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=mydb;..."

# Variável de ambiente (prod)
export ConnectionStrings__DefaultConnection="Server=prod;Database=mydb;..."
```

## Referências

- [EF Core Documentation](https://learn.microsoft.com/en-us/ef/core/)
- [Query Performance](https://learn.microsoft.com/en-us/ef/core/performance/efficient-querying)
- [Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
