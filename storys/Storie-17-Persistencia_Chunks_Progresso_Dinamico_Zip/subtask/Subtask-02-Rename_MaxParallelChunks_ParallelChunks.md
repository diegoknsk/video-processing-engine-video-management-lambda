# Subtask-02: Renomear maxParallelChunks para parallelChunks

## Descrição
Renomear o campo `maxParallelChunks` / `MaxParallelChunks` para `parallelChunks` / `ParallelChunks` em toda a base de código: entidade de domínio, value object de update, input model, response model, mapper DynamoDB e referências nos workflows. Garantir retrocompatibilidade de leitura no mapper para registros DynamoDB já existentes com o atributo `maxParallelChunks`.

## Passos de Implementação
1. Renomear propriedade `MaxParallelChunks` → `ParallelChunks` na entidade `Video` (Domain).
2. Renomear propriedade `MaxParallelChunks` → `ParallelChunks` em `VideoUpdateValues` (Domain).
3. Renomear campo `MaxParallelChunks` → `ParallelChunks` em `UpdateVideoInputModel` (Application).
4. Renomear campo `MaxParallelChunks` → `ParallelChunks` em `VideoResponseModel` (Application).
5. Atualizar `VideoMapper` (Infra.Data): ao gravar, usar atributo DynamoDB `parallelChunks`; ao ler, tentar `parallelChunks` primeiro e fazer fallback para `maxParallelChunks` (retrocompatibilidade silenciosa).
6. Ajustar qualquer referência ao campo nos testes unitários existentes.
7. Se o campo for referenciado em `VideoRepository.UpdateAsync` (expression string ou condição), atualizar o nome do atributo DynamoDB correspondente.

## Formas de Teste
1. Compilar o projeto e verificar que não há referências quebradas ao nome antigo (`maxParallelChunks` / `MaxParallelChunks`) fora do fallback de leitura.
2. Teste unitário: criar um `VideoMapper` que lê um `Dictionary<string, AttributeValue>` com `maxParallelChunks = "4"` (nome antigo) e verificar que `parallelChunks` é preenchido com `4`.
3. Teste unitário: criar um `VideoMapper` que lê com `parallelChunks = "4"` (nome novo) e verificar que o valor é preenchido corretamente.
4. Teste de build: `dotnet build` passa sem warnings de obsolescência relacionados ao campo renomeado.

## Critérios de Aceite
- [ ] Nenhuma referência ao nome `maxParallelChunks` / `MaxParallelChunks` no código de produção, exceto no fallback de leitura do mapper.
- [ ] Mapper grava com atributo `parallelChunks` e lê com fallback para `maxParallelChunks`.
- [ ] Testes unitários do mapper para ambos os cenários (nome antigo e novo) passando.
- [ ] `dotnet build` e `dotnet test` passam sem erros após o rename.
