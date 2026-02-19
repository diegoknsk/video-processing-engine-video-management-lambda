# Subtask-01: Criar GSI no Projeto de Infra (IaC)

## Descrição
Adicionar o Global Secondary Index `GSI_ClientRequestId` na definição IaC da tabela DynamoDB `video-processing-engine-{env}-videos`, no projeto de infraestrutura separado.

## Passos de Implementação
1. Localizar no projeto de infra a definição da tabela `video-processing-engine-{env}-videos`.
2. Adicionar o GSI com as configurações:
   - **IndexName:** `GSI_ClientRequestId`
   - **Partition key:** `gsi1pk` (String)
   - **Sort key:** `gsi1sk` (String)
   - **Projection:** `ALL`
   - **Billing:** On-demand (conforme padrão da tabela)
3. Fazer deploy do projeto de infra no ambiente de dev e aguardar o status do GSI mudar para `ACTIVE` no console da AWS.

## Formas de Teste
1. No console AWS DynamoDB → aba **Indexes** → verificar que `GSI_ClientRequestId` aparece com status `Active`.
2. Via AWS CLI: `aws dynamodb describe-table --table-name video-processing-engine-dev-videos` e verificar o GSI na lista `GlobalSecondaryIndexes`.
3. Realizar uma query manual usando o novo índice via console AWS para confirmar que funciona.

## Critérios de Aceite
- [ ] GSI `GSI_ClientRequestId` com status `ACTIVE` na tabela em dev
- [ ] GSI configurado com `gsi1pk` (PK) e `gsi1sk` (SK), projeção `ALL`
- [ ] Deploy de infra concluído sem erros
