# Subtask-04: Atualizar .gitignore com entradas do Sonar

## Descrição
Adicionar ao `.gitignore` da raiz do repositório as entradas obrigatórias para evitar que artefatos gerados pelo SonarScanner e pelo coverlet sejam versionados acidentalmente.

## Arquivo alvo
`.gitignore` (raiz do repositório)

## Passos de implementação
1. Abrir o `.gitignore` existente na raiz do projeto.
2. Verificar se as entradas do Sonar já existem (buscar por `.sonarqube`).
3. Se não existirem, adicionar um bloco ao final do arquivo:
   ```gitignore
   # SonarCloud / SonarQube
   .sonarqube/
   **/.sonarqube/
   **/out/.sonar/
   .scannerwork/
   **/.scannerwork/
   coverage.opencover.xml
   ```
4. Confirmar que nenhuma dessas entradas duplica o que já existe no arquivo.

## Formas de teste
1. Após executar o SonarScanner localmente (ou via CI), verificar que `git status` não lista `.sonarqube/` nem `coverage.opencover.xml` como arquivos não rastreados.
2. Executar `git check-ignore -v .sonarqube/` e confirmar que o arquivo retorna a regra correspondente no `.gitignore`.
3. Verificar que `git status` continua limpo para os arquivos de produção do projeto (nenhum arquivo legítimo foi acidentalmente ignorado).

## Critérios de aceite
- [ ] `.gitignore` contém as entradas `.sonarqube/`, `**/.sonarqube/`, `.scannerwork/`, `**/.scannerwork/` e `coverage.opencover.xml`.
- [ ] Nenhuma entrada duplicada no `.gitignore`.
- [ ] `git status` não exibe artefatos do Sonar como untracked após execução do scanner.
