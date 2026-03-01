# Subtask-02: Mover projetos InterfacesExternas (Api e Lambda)

## Descrição
Criar a pasta `src/InterfacesExternas/` e mover para dentro dela os projetos Api e LambdaUpdateVideo, garantindo que as referências a Application, Domain e Infra permaneçam válidas (paths nos .csproj).

## Passos de Implementação
1. Criar diretório `src/InterfacesExternas/`.
2. Mover para `src/InterfacesExternas/` as pastas dos projetos:
   - `VideoProcessing.VideoManagement.Api`
   - `VideoProcessing.VideoManagement.LambdaUpdateVideo`
3. Ajustar ProjectReference nos .csproj da Api e da Lambda para os novos paths relativos (ex.: referência a Application em `../Core/VideoProcessing.VideoManagement.Application/...`, Infra em `../Infra/...`).
4. Executar `dotnet build` para validar que Api e Lambda compilam com as novas localizações.

## Formas de Teste
1. Confirmar que `src/InterfacesExternas/` contém apenas Api e LambdaUpdateVideo.
2. Verificar na IDE que não há referências quebradas aos projetos Core e Infra.
3. Rodar `dotnet build` na raiz da solution e garantir sucesso.

## Critérios de Aceite
- [x] Pasta `src/InterfacesExternas/` existe com os projetos Api e LambdaUpdateVideo.
- [x] ProjectReference nos .csproj da Api e da Lambda apontam corretamente para Core e Infra (paths relativos atualizados).
- [x] `dotnet build` conclui sem erros para Api e LambdaUpdateVideo.
