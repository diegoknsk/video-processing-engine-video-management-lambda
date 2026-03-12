# Subtask 02: Domain e persistência — UserEmail em Video, VideoRehydrationData, VideoEntity e mapeamento

## Descrição
Incluir o campo de email do usuário (ex.: `UserEmail`) na entidade de domínio `Video`, no DTO de reidratação `VideoRehydrationData`, na entidade de persistência `VideoEntity` e nos mapeamentos (VideoMapper, leitura/escrita no VideoRepository), garantindo compatibilidade com itens existentes sem o atributo.

## Passos de implementação
1. **Domain — Video:** Adicionar propriedade opcional `UserEmail` (ex.: `string? UserEmail { get; private set; }`) na entidade `Video`; incluir no construtor interno que usa `VideoRehydrationData` e em `FromMerge` (preservar valor existente quando o patch não alterar email). No construtor público de criação de vídeo, não definir UserEmail (será definido pelo UseCase antes de persistir).
2. **Domain — VideoRehydrationData:** Adicionar parâmetro opcional `string? UserEmail` no record `VideoRehydrationData` e repassar em todas as chamadas que constroem o record (FromPersistence, FromMerge).
3. **Infra — VideoEntity:** Adicionar propriedade `string? UserEmail` na classe `VideoEntity` (DynamoDB); garantir que o nome do atributo no DynamoDB seja consistente (ex.: `userEmail` ou `UserEmail` conforme convenção do projeto).
4. **Infra — VideoMapper:** Em `ToEntity(Video, string?)`, mapear `UserEmail` do Video para a entidade; em `ToDomain(VideoEntity)`, mapear o atributo da entidade para o Video (ou VideoRehydrationData), tratando ausência do atributo no item como null.
5. **Infra — VideoRepository:** Garantir que no `PutItem`/`Create` o atributo `UserEmail` seja incluído quando presente; na leitura (GetItem, query), o mapeamento já deve preencher a partir do item (VideoMapper.ToDomain). Verificar se o repositório usa mapa de atributos genérico ou modelo anotado e ajustar para incluir o novo campo.

## Formas de teste
1. **Teste unitário — VideoMapper:** Mapear `VideoEntity` com `UserEmail` preenchido para Domain e verificar que o `Video` resultante tem o mesmo valor; mapear entidade sem `UserEmail` (null ou atributo ausente) e verificar que o Domain tem `UserEmail` null.
2. **Teste unitário — VideoRepository (se aplicável):** Criar um vídeo com UserEmail e ler de volta; verificar que o atributo está persistido e recuperado.
3. **Teste de integração (opcional):** Inserir item manualmente no DynamoDB sem o atributo e chamar GetById; validar que não há exceção e que `UserEmail` é null.

## Critérios de aceite da subtask
- [ ] A entidade `Video` (Domain) possui a propriedade opcional `UserEmail`; `VideoRehydrationData` inclui o parâmetro e todos os fluxos de construção (FromPersistence, FromMerge) estão atualizados.
- [ ] `VideoEntity` possui a propriedade `UserEmail`; o `VideoRepository` persiste e lê o atributo; itens sem o atributo resultam em `UserEmail` null no domínio.
- [ ] `VideoMapper.ToEntity` e `ToDomain` mapeiam `UserEmail` corretamente; testes unitários do mapper e do repositório (quando cabível) passando.
