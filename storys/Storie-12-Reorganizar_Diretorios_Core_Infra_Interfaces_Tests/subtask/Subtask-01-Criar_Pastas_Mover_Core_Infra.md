# Subtask-01: Criar pastas físicas e mover projetos Core e Infra

## Descrição
Criar as pastas `src/Core` e `src/Infra` e mover os projetos Domain e Application para Core, e Infra.Data e Infra.CrossCutting para Infra, mantendo os nomes dos projetos e o conteúdo dos .csproj; ajustar referências entre projetos se os paths relativos mudarem.

## Passos de Implementação
1. Criar diretório `src/Core/` e mover para dentro dele as pastas dos projetos:
   - `VideoProcessing.VideoManagement.Domain`
   - `VideoProcessing.VideoManagement.Application`
2. Criar diretório `src/Infra/` e mover para dentro dele as pastas dos projetos:
   - `VideoProcessing.VideoManagement.Infra.Data`
   - `VideoProcessing.VideoManagement.Infra.CrossCutting`
3. Verificar que cada .csproj mantém referências corretas (ProjectReference) — paths relativos podem precisar de ajuste (ex.: Application referenciando Domain com `../VideoProcessing.VideoManagement.Domain/...` dentro de Core).
4. Executar `dotnet build` na raiz da solution para validar que Core e Infra compilam.

## Formas de Teste
1. Listar `src/Core` e `src/Infra` e confirmar que os quatro projetos estão nas pastas corretas.
2. Abrir a solution na IDE e verificar que os projetos aparecem e que não há referências quebradas (avisos de projeto não encontrado).
3. Rodar `dotnet build` e garantir que não há erros de referência ou path.

## Critérios de Aceite
- [x] Pastas `src/Core/` e `src/Infra/` existem com os projetos Domain, Application, Infra.Data e Infra.CrossCutting nos locais corretos.
- [x] Nenhum arquivo de código foi alterado além da eventual correção de paths em .csproj (ProjectReference).
- [x] `dotnet build` conclui sem erros para os projetos movidos.
