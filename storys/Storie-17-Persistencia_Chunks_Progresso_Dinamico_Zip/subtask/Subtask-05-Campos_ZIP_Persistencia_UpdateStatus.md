# Subtask-05: Campos ZIP no Domínio/Contratos e Persistência via Lambda UpdateStatus

## Descrição
Adicionar os campos do arquivo ZIP final (`ZipBucket`, `ZipKey`, `ZipFileName`) ao domínio, contratos e mapper. Evoluir o `UpdateVideoUseCase` para que, a cada execução no Lambda `UpdateStatus`, persista também o chunk individual na tabela de chunks via `IVideoChunkRepository`. Dados do ZIP são persistidos no item principal do vídeo quando presentes.

## Passos de Implementação
1. Adicionar propriedades `ZipBucket?`, `ZipKey?`, `ZipFileName?` à entidade `Video` (Domain).
2. Adicionar os mesmos campos em `VideoUpdateValues` (Domain) para suportar o patch parcial.
3. Adicionar campos opcionais `ZipBucket?`, `ZipKey?`, `ZipFileName?` em `UpdateVideoInputModel` (Application).
4. Atualizar `VideoMapper` (Infra.Data):
   - Gravar os três atributos DynamoDB: `zipBucket`, `zipKey`, `zipFileName` (quando presentes).
   - Ler e mapear de volta ao `Video` no método de leitura.
5. Atualizar `UpdateVideoUseCase`:
   - Após a atualização bem-sucedida do vídeo principal via `IVideoRepository.UpdateAsync`, chamar `IVideoChunkRepository.UpsertAsync` com os dados do chunk presentes no `UpdateVideoInputModel` (campos de `ProcessingSummary` / identificador do chunk, start/end sec, status atual do payload).
   - A chamada ao repositório de chunks deve ser feita com `try/catch` isolado: falha na persistência do chunk **não** deve reverter a atualização do vídeo principal (falha tolerada com log de warning).
6. Verificar que `UpdateVideoInputModel` já possui os campos de chunk necessários para montar o `VideoChunk` (ChunkId, StartSec, EndSec, IntervalSec, etc.) — incluir se faltarem.

## Formas de Teste
1. Teste unitário: `UpdateVideoUseCase` recebe payload com `ZipBucket`, `ZipKey`, `ZipFileName` preenchidos → verificar que `IVideoRepository.UpdateAsync` é chamado com os valores mapeados.
2. Teste unitário: `UpdateVideoUseCase` com dados de chunk válidos → verificar que `IVideoChunkRepository.UpsertAsync` é chamado uma vez com o `VideoChunk` correto.
3. Teste unitário: `IVideoChunkRepository.UpsertAsync` lança exceção → verificar que o use case **não** propaga a exceção (falha tolerada) e retorna sucesso na atualização principal.
4. Verificar mapeamento no mapper: `VideoMapper` serializa e desserializa os campos ZIP corretamente.

## Critérios de Aceite
- [ ] `Video`, `VideoUpdateValues` e `UpdateVideoInputModel` possuem `ZipBucket?`, `ZipKey?`, `ZipFileName?`.
- [ ] `VideoMapper` persiste e lê os atributos `zipBucket`, `zipKey`, `zipFileName` do DynamoDB.
- [ ] `UpdateVideoUseCase` chama `IVideoChunkRepository.UpsertAsync` após cada atualização de status bem-sucedida.
- [ ] Falha no repositório de chunks não interrompe a atualização do vídeo principal.
- [ ] Testes unitários do use case cobrindo os novos fluxos passando.
