# Subtask 01: Revisar artefatos no repositório (dry run, build txt, .gitignore)

## Descrição
Revisar e documentar os artefatos presentes no repositório que não deveriam estar versionados (publish_dry_run/, build_log.txt, build_output.txt, test_output.txt, test_output_ascii.txt) e propor ajustes no .gitignore.

## Passos de implementação
1. Identificar no repositório todas as pastas/arquivos que são saída de build ou de comandos (publish, logs).
2. Documentar o que é "publish_dry_run" (pasta de teste de `dotnet publish` com outro output) e os arquivos .txt de build/test.
3. Verificar o .gitignore atual e listar entradas faltantes (publish/, publish_dry_run/, logs de build/test).
4. Registrar na Storie-01.4 (Resultado da Revisão) as recomendações: adicionar ao .gitignore e não commitar esses artefatos.

## Formas de teste
1. Conferir que a seção "Artefatos no repositório" da revisão está preenchida.
2. Verificar que .gitignore foi revisado e que as recomendações estão documentadas.
3. Validar que não foi feita alteração no .gitignore nesta story (apenas revisão); correção será em follow-up.

## Critérios de aceite da subtask
- [x] Documentado o que é publish_dry_run e os arquivos build_log.txt, build_output.txt, test_output.txt, test_output_ascii.txt
- [x] Listadas no story.md as recomendações para .gitignore (publish/, publish_dry_run/, logs)
- [x] Revisão não altera .gitignore nem remove arquivos — apenas documenta
