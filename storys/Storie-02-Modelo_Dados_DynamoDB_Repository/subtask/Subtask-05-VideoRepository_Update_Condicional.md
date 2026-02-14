# Subtask 05: Implementar Update condicional (ownership, monotonia, status transitions)

## Descrição
Implementar método `UpdateAsync` em VideoRepository usando UpdateItem do DynamoDB com ConditionExpression para garantir: (1) ownership (pk do userId corresponde ao video.UserId), (2) progressPercent monotônico (novo valor >= anterior), (3) status transitions válidas (não voltar de Completed/Failed para Processing), (4) idempotência (múltiplos writers podem chamar com mesmos valores sem erro). Tratar ConditionalCheckFailedException e retornar exceção de negócio clara.

## Passos de Implementação
1. **Definir regras de status transition**: criar mapa de transições válidas (ex.: Pending → Uploading → Processing → Completed; Processing → Failed; qualquer → Cancelled). Documentar no Domain (comentário ou enum helper)
2. **Implementar UpdateAsync no VideoRepository**:
   - Criar UpdateItemRequest com Key (pk = USER#{video.UserId}, sk = VIDEO#{video.VideoId})
   - UpdateExpression: SET campos a atualizar (progressPercent, status, updatedAt, errorMessage, errorCode, framesPrefix, s3KeyZip, etc.)
   - ConditionExpression: combinar condições:
     - `attribute_exists(pk) AND pk = :expectedPk` (ownership; evitar update em video de outro usuário)
     - `progressPercent <= :newProgress` (monotonia; permitir igual para idempotência)
     - `#status IN (:validFromStatus1, :validFromStatus2, ...)` (status transitions; calcular dinamicamente com base no novo status)
   - ExpressionAttributeNames e ExpressionAttributeValues
   - ReturnValues = ALL_NEW (retornar item atualizado)
3. **Executar UpdateItemAsync**: capturar Attributes retornado, converter para Video usando VideoMapper, retornar
4. **Tratar ConditionalCheckFailedException**: logar detalhes, lançar exceção customizada `VideoUpdateConflictException` (criar em Domain/Exceptions) com mensagem clara (ex.: "Update failed: ownership mismatch, progress regression, or invalid status transition")
5. **Criar testes de conflito**: mockar ConditionalCheckFailedException, validar que VideoUpdateConflictException é lançada
6. **Validar idempotência**: testar chamada de UpdateAsync com mesmos valores 2x, garantir que segunda chamada não falha (progressPercent igual permitido)

## Formas de Teste
1. **Update sucesso (mock)**: mockar UpdateItemAsync retornando Attributes, validar que Video atualizado é retornado
2. **Ownership conflict (mock)**: mockar ConditionalCheckFailedException, validar que VideoUpdateConflictException é lançada
3. **Progress regression (integration/manual)**: criar cenário onde progressPercent novo < anterior, validar que update falha
4. **Invalid status transition (integration/manual)**: tentar atualizar de Completed para Processing, validar que falha

## Critérios de Aceite da Subtask
- [ ] UpdateAsync implementado usando UpdateItem com ConditionExpression
- [ ] ConditionExpression valida ownership (pk = USER#{userId})
- [ ] ConditionExpression valida progressPercent monotônico (novo >= anterior)
- [ ] ConditionExpression valida status transitions (documentadas no Domain)
- [ ] ConditionalCheckFailedException capturada e convertida em VideoUpdateConflictException
- [ ] VideoUpdateConflictException criada em Domain/Exceptions com mensagem clara
- [ ] UpdateAsync retorna Video atualizado (usando ReturnValues ALL_NEW)
- [ ] Idempotência garantida (mesmo update 2x não falha se valores iguais)
- [ ] Testes unitários com mocks validam sucesso, ownership conflict, progress regression (cobertura >= 80%)
