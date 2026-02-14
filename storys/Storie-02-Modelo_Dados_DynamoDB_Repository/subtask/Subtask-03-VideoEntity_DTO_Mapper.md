# Subtask 03: Implementar VideoEntity (DTO DynamoDB) e VideoMapper

## Descrição
Criar classe `VideoEntity` como DTO para DynamoDB com atributos [DynamoDBHashKey("pk")], [DynamoDBRangeKey("sk")] e campos correspondentes aos da entidade Video do Domain, definir padrão de keys (pk = USER#{userId}, sk = VIDEO#{videoId}), implementar `VideoMapper` com métodos estáticos ToEntity (Domain → DTO) e ToDomain (DTO → Domain), garantir mapeamento correto de todos os campos incluindo opcionais e enums.

## Passos de Implementação
1. **Criar VideoEntity class** em `Infra.Data/Entities/VideoEntity.cs` com propriedades públicas mutable (para AWSSDK.DynamoDBv2.DataModel)
2. **Adicionar atributos DynamoDB**: `[DynamoDBHashKey("pk")]` em Pk, `[DynamoDBRangeKey("sk")]` em Sk, `[DynamoDBProperty]` ou `[DynamoDBIgnore]` conforme necessário
3. **Definir propriedades**: Pk (string), Sk (string), VideoId, UserId, OriginalFileName, ContentType, SizeBytes, DurationSec, CreatedAt (string ISO 8601), UpdatedAt, Status (string), ProgressPercent, S3BucketVideo, S3KeyVideo, ... (todos os campos de Video)
4. **Campos de idempotência**: adicionar ClientRequestId (string?) e ClientRequestIdCreatedAt (string?) para deduplicação; criar GSI opcional (gsi1pk = USER#{userId}, gsi1sk = CLIENT_REQUEST#{clientRequestId})
5. **Criar VideoMapper** em `Infra.Data/Mappers/VideoMapper.cs` com métodos `public static VideoEntity ToEntity(Video video, string clientRequestId)` e `public static Video ToDomain(VideoEntity entity)`
6. **ToEntity**: preencher Pk = $"USER#{video.UserId}", Sk = $"VIDEO#{video.VideoId}", mapear campos 1:1, converter enums para string
7. **ToDomain**: parsear enums de string, converter timestamps de string para DateTime, criar instância de Video
8. **Tratar opcionais**: usar null-conditional operator (?.) para campos opcionais; enums opcionais usar Enum.TryParse

## Formas de Teste
1. **Mapeamento Domain → DTO**: criar Video, chamar ToEntity, validar que Pk/Sk corretos e todos os campos mapeados
2. **Mapeamento DTO → Domain**: criar VideoEntity, chamar ToDomain, validar que Video resultante tem mesmos valores
3. **Round-trip test**: Video → ToEntity → ToDomain, validar que Video final é equivalente ao original

## Critérios de Aceite da Subtask
- [ ] VideoEntity criado com atributos [DynamoDBHashKey], [DynamoDBRangeKey]
- [ ] Pk/Sk seguem padrão USER#{userId} / VIDEO#{videoId}
- [ ] Todos os campos de Video presentes em VideoEntity (incluindo ClientRequestId para idempotência)
- [ ] VideoMapper implementado com ToEntity e ToDomain
- [ ] Enums convertidos corretamente (string no DynamoDB, enum no Domain)
- [ ] Timestamps em formato ISO 8601 (string no DynamoDB, DateTime no Domain)
- [ ] Testes unitários validam mapeamento 1:1 e round-trip (cobertura >= 80%)
