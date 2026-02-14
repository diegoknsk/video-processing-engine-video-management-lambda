---
name: performance-optimization
description: Guia para otimização de performance — Span<T>, Memory<T>, ArrayPool, ValueTask, compiled queries e técnicas avançadas. Use quando a tarefa envolver performance, otimização, alocações, memory pooling ou hot paths.
---

# Performance Optimization — .NET

## Quando Usar

- **Otimização de performance**, redução de alocações
- **Span<T>**, **Memory<T>**, **ArrayPool<T>**, **ValueTask<T>**
- **Hot paths**, queries compiladas, zero-allocation
- Palavras-chave: "performance", "otimizar", "alocações", "Span", "ArrayPool", "rápido", "hot path"

⚠️ **Importante:** Otimize apenas com **profiling** (medição real). Código legível > performance prematura.

## Princípios Essenciais

### ✅ Fazer

- Usar **Span<T>** para manipulação de arrays/strings sem alocações (parsing, slicing)
- Usar **ArrayPool<T>** para arrays temporários (reutilização, menos GC pressure)
- Usar **ValueTask<T>** quando operação frequentemente completa de forma síncrona
- **Compiled queries** (EF Core) para queries repetitivas
- **AsNoTracking()** em EF Core para queries read-only
- **Profiling primeiro:** medir antes de otimizar (BenchmarkDotNet)

### ❌ Não Fazer

- **Nunca** otimizar sem medir (profiling)
- **Nunca** sacrificar legibilidade por micro-otimizações sem impacto
- **Nunca** usar `Span<T>` em métodos async (usar `Memory<T>`)
- **Nunca** esquecer de devolver arrays ao `ArrayPool` (usar `try/finally`)
- **Nunca** assumir que "mais rápido" = "melhor" (trade-offs)

**Regra de ouro:** Profile → Otimize → Meça novamente. Span<T> + ArrayPool<T> cobrem 80% dos casos.

## Checklist Rápido

1. **Profile primeiro:** BenchmarkDotNet ou dotTrace para identificar hot paths
2. **Span<T>** para parsing, slicing, manipulação de strings sem alocações
3. **ArrayPool<T>** para arrays temporários (Rent → usar → Return no `finally`)
4. **ValueTask<T>** para operações que frequentemente completam síncronamente
5. **AsNoTracking()** em queries EF Core read-only
6. **Compiled queries** para queries EF Core repetitivas
7. **Measure again:** validar que otimização teve efeito

## Exemplo Mínimo

**Cenário:** Parsing de CSV com Span<T> e ArrayPool<T> (zero alocações)

### Span<T> — Parsing sem Alocações

```csharp
// ❌ Alocações desnecessárias
public static string[] ParseCsvLine(string line)
{
    return line.Split(','); // Aloca array de strings
}

// ✅ Zero alocações com Span
public static void ParseCsvLine(ReadOnlySpan<char> line, Span<Range> ranges, out int count)
{
    count = 0;
    int start = 0;
    
    for (int i = 0; i <= line.Length; i++)
    {
        if (i == line.Length || line[i] == ',')
        {
            ranges[count++] = new Range(start, i);
            start = i + 1;
        }
    }
}

// Uso
var line = "John,Doe,30".AsSpan();
Span<Range> ranges = stackalloc Range[10]; // No stack, zero alocação
ParseCsvLine(line, ranges, out int count);

for (int i = 0; i < count; i++)
{
    var field = line[ranges[i]]; // ReadOnlySpan<char>, zero alocação
    Console.WriteLine(field.ToString());
}
```

### ArrayPool<T> — Reutilização de Arrays

```csharp
using System.Buffers;

// ❌ Alocação a cada chamada
public byte[] ProcessData(int size)
{
    var buffer = new byte[size]; // GC pressure
    // ... processa
    return buffer;
}

// ✅ Reutilização com ArrayPool
public void ProcessData(int size, Span<byte> destination)
{
    var buffer = ArrayPool<byte>.Shared.Rent(size); // Reutiliza array
    
    try
    {
        var span = buffer.AsSpan(0, size);
        // ... processa span
        span.CopyTo(destination);
    }
    finally
    {
        ArrayPool<byte>.Shared.Return(buffer); // Devolve ao pool
    }
}
```

### ValueTask<T> — Operações Frequentemente Síncronas

```csharp
// ✅ ValueTask quando operação pode ser síncrona (ex.: cache hit)
public class CachedUserRepository(IUserRepository repository, IMemoryCache cache)
{
    public async ValueTask<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        // Cache hit: retorna de forma síncrona (sem alocação de Task)
        if (cache.TryGetValue(id, out User? cached))
            return cached;

        // Cache miss: chama repositório (assíncrono)
        var user = await repository.GetByIdAsync(id, ct);
        if (user != null)
            cache.Set(id, user, TimeSpan.FromMinutes(5));
        
        return user;
    }
}

// ❌ Task<T> sempre aloca mesmo quando síncrono
public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
{
    if (cache.TryGetValue(id, out User? cached))
        return cached; // Ainda aloca Task<User>
    // ...
}
```

**Pontos-chave:**
- **Span<T>:** parsing, slicing, manipulação sem alocações (só código síncrono)
- **ArrayPool<T>:** arrays temporários, reduz GC pressure (sempre Return no `finally`)
- **ValueTask<T>:** quando operação frequentemente completa de forma síncrona

## Memory<T> — Span para Async

`Span<T>` não pode ser usado em métodos async (vive no stack). Use `Memory<T>`:

```csharp
public async Task<int> ProcessAsync(Memory<byte> buffer, CancellationToken ct)
{
    await ReadDataAsync(buffer, ct); // Memory pode ser passado para async
    
    Span<byte> span = buffer.Span; // Converter para Span quando necessário
    return ProcessBytes(span);
}

private int ProcessBytes(Span<byte> data)
{
    int sum = 0;
    foreach (var b in data) sum += b;
    return sum;
}
```

## Compiled Queries (EF Core)

Para queries repetitivas, compile uma vez:

```csharp
private static readonly Func<AppDbContext, Guid, Task<User?>> GetUserByIdQuery =
    EF.CompileAsyncQuery((AppDbContext ctx, Guid id) =>
        ctx.Users.AsNoTracking().FirstOrDefault(u => u.Id == id));

public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
{
    return await GetUserByIdQuery(context, id);
}
```

## Profiling com BenchmarkDotNet

```bash
dotnet add package BenchmarkDotNet
```

```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

[MemoryDiagnoser]
public class ParsingBenchmark
{
    private const string Input = "10,20,30,40,50";

    [Benchmark(Baseline = true)]
    public int[] ParseWithSplit()
    {
        var parts = Input.Split(',');
        var numbers = new int[parts.Length];
        for (int i = 0; i < parts.Length; i++)
            numbers[i] = int.Parse(parts[i]);
        return numbers;
    }

    [Benchmark]
    public int[] ParseWithSpan()
    {
        var span = Input.AsSpan();
        Span<int> numbers = stackalloc int[5];
        // ... parsing com span
        return numbers.ToArray();
    }
}

// Program.cs
BenchmarkRunner.Run<ParsingBenchmark>();
```

## Técnicas por Cenário

| Cenário | Técnica | Ganho |
|---------|---------|-------|
| Parsing de strings/CSV | `Span<T>` | Zero alocações |
| Arrays temporários (loops) | `ArrayPool<T>` | -70% GC pressure |
| Cache/operações síncronas | `ValueTask<T>` | -50% alocações |
| Queries EF Core repetitivas | Compiled queries | +30% throughput |
| Queries EF Core read-only | `AsNoTracking()` | +20% performance |

## Referências

- [Span<T> Documentation](https://learn.microsoft.com/en-us/dotnet/api/system.span-1)
- [ArrayPool<T> Documentation](https://learn.microsoft.com/en-us/dotnet/api/system.buffers.arraypool-1)
- [BenchmarkDotNet](https://benchmarkdotnet.org/)
- [Performance Best Practices](https://learn.microsoft.com/en-us/dotnet/core/extensions/high-performance-logging)
