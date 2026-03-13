# Subtask-05: Branch Protection Rule e Badges no README

## Descrição
Configurar manualmente a Branch Protection Rule no GitHub para tornar o Quality Gate do SonarCloud obrigatório antes de qualquer merge na `main`, e adicionar os badges de Quality Gate e Coverage ao `README.md` do repositório.

## Arquivos afetados
- `README.md` (raiz do repositório)
- Configuração manual no GitHub (Settings → Branches)

## Passos de implementação

### Branch Protection Rule (manual no GitHub)

1. Acessar **Settings → Branches → Branch protection rules** no repositório.
2. Clicar em **Add rule** (ou editar a regra existente para `main`).
3. Em **Branch name pattern**, inserir `main`.
4. Habilitar **Require status checks to pass before merging**.
5. Na caixa de busca de status checks, digitar `SonarCloud Analysis` (nome exato do campo `name:` do job no workflow).
6. Adicionar `SonarCloud Analysis` como check obrigatório.
7. Opcionalmente habilitar **Require branches to be up to date before merging** para garantir que o Sonar analise código atualizado.
8. Salvar a regra.

> **Atenção:** o check `SonarCloud Analysis` só aparece na lista de busca após o job ter executado pelo menos uma vez no repositório.

### Badges no README

9. Obter as URLs dos badges no SonarCloud:
   - Acessar o projeto no SonarCloud → aba **Badges** (ou construir as URLs manualmente):
     ```
     Quality Gate:
     https://sonarcloud.io/api/project_badges/measure?project=SONAR_PROJECT_KEY&metric=alert_status

     Coverage:
     https://sonarcloud.io/api/project_badges/measure?project=SONAR_PROJECT_KEY&metric=coverage
     ```
   - Substituir `SONAR_PROJECT_KEY` pelo valor real do projeto.

10. Adicionar os badges ao `README.md`, preferencialmente no topo, após o título:
    ```markdown
    [![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=SONAR_PROJECT_KEY&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=SONAR_PROJECT_KEY)
    [![Coverage](https://sonarcloud.io/api/project_badges/measure?project=SONAR_PROJECT_KEY&metric=coverage)](https://sonarcloud.io/summary/new_code?id=SONAR_PROJECT_KEY)
    ```

## Formas de teste
1. Tentar fazer merge de um PR para `main` sem a aprovação do Quality Gate e verificar que o GitHub bloqueia o merge.
2. Abrir o `README.md` renderizado no GitHub e confirmar que os badges aparecem corretamente com o status atual (verde/vermelho).
3. Clicar nos badges e confirmar que redirecionam para a página do projeto no SonarCloud.

## Critérios de aceite
- [ ] Branch Protection Rule para `main` configurada com check `SonarCloud Analysis` obrigatório.
- [ ] Merge para `main` é bloqueado quando o Quality Gate falha.
- [ ] Badge de Quality Gate visível e funcional no `README.md`.
- [ ] Badge de Coverage visível e funcional no `README.md`.
- [ ] Os badges refletem o status atual do projeto (não ficam com loading infinito após o primeiro push para `main`).
