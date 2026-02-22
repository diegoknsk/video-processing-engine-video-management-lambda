# Subtask 03: Variáveis e secrets (LAMBDA_FUNCTION_UPDATE_STATUS_NAME e AWS Academy)

## Descrição
Documentar e utilizar no workflow a variável (ou secret) **LAMBDA_FUNCTION_UPDATE_STATUS_NAME** para o nome da função Lambda na AWS, e as credenciais padrão do AWS Academy: AWS_ACCESS_KEY_ID, AWS_SECRET_ACCESS_KEY e AWS_SESSION_TOKEN. Considerar limitações de permissão do AWS Academy/LabRole e registrar na documentação os riscos e as permissões IAM mínimas necessárias (lambda:UpdateFunctionCode, lambda:UpdateFunctionConfiguration, lambda:GetFunction); não presumir permissões além do necessário.

## Passos de Implementação
1. No workflow, usar para o nome da função: ${{ vars.LAMBDA_FUNCTION_UPDATE_STATUS_NAME }} ou ${{ secrets.LAMBDA_FUNCTION_UPDATE_STATUS_NAME }} conforme convenção do repositório (recomendação: Variable para nome da função, pois não é sensível)
2. Usar secrets para credenciais: AWS_ACCESS_KEY_ID, AWS_SECRET_ACCESS_KEY, AWS_SESSION_TOKEN (obrigatório para AWS Academy que usa credenciais temporárias)
3. Usar variável AWS_REGION (ex.: vars.AWS_REGION) com fallback para 'us-east-1' quando aplicável
4. Criar seção na documentação (docs ou na story) listando: Variáveis (LAMBDA_FUNCTION_UPDATE_STATUS_NAME, AWS_REGION) e Secrets (AWS_ACCESS_KEY_ID, AWS_SECRET_ACCESS_KEY, AWS_SESSION_TOKEN); onde configurar no GitHub (Settings → Secrets and variables → Actions)
5. Documentar risco AWS Academy: se a role/lab não permitir lambda:UpdateFunctionCode ou lambda:UpdateFunctionConfiguration, o deploy falhará; permissões mínimas necessárias: lambda:UpdateFunctionCode, lambda:UpdateFunctionConfiguration, lambda:GetFunction; a função Lambda deve já existir (criada via IaC ou manualmente)
6. Incluir na documentação que o uso de Session Token é necessário para credenciais temporárias do AWS Academy

## Formas de Teste
1. Executar workflow sem LAMBDA_FUNCTION_UPDATE_STATUS_NAME configurada e validar que o workflow falha com mensagem clara (ou que a documentação orienta a configurar)
2. Com todas as variáveis e secrets configurados, executar deploy e validar que não há erro de permissão (ou documentar o erro típico de Academy caso ocorra)
3. Revisar documentação: um usuário consegue seguir a lista de variáveis/secrets e configurar o repositório

## Critérios de Aceite da Subtask
- [ ] Workflow utiliza LAMBDA_FUNCTION_UPDATE_STATUS_NAME (Variable ou Secret) para o nome da função em todos os comandos AWS Lambda
- [ ] Credenciais AWS utilizadas: AWS_ACCESS_KEY_ID, AWS_SECRET_ACCESS_KEY, AWS_SESSION_TOKEN (e AWS_REGION quando aplicável)
- [ ] Documentação lista todas as variáveis e secrets necessários e onde configurá-los no GitHub
- [ ] Riscos/mitigações AWS Academy documentados: permissões IAM mínimas e premissa de que a função já existe
- [ ] Session Token documentado como necessário para AWS Academy (credenciais temporárias)
