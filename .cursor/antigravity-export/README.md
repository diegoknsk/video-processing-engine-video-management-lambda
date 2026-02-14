# Export das Rules para Antigravity (Gemini)

Este diretório contém as **rules globais** do Cursor convertidas para formato válido no Antigravity (Gemini).

## Formato Antigravity

- **Markdown** (.md), até ~12.000 caracteres por arquivo
- **Regras globais:** `~/.gemini/GEMINI.md` (aplicam em todos os projetos)
- **Regras de workspace:** pasta `.agent/rules` na raiz do repositório

## Como usar

### Opção 1 — Regras globais (recomendado)

1. Copie o conteúdo de **`GEMINI.md`**.
2. No Antigravity: ícone (•••) → **Customizations** → **+ Global**.
3. Cole o conteúdo (ou aponte para o arquivo, se o Antigravity permitir).

Se usar arquivo no disco:

- Crie/edite `~/.gemini/GEMINI.md` (Linux/macOS) ou `%USERPROFILE%\.gemini\GEMINI.md` (Windows).
- Cole o conteúdo de `GEMINI.md`.

### Opção 2 — Regras de workspace

1. Crie a pasta `.agent/rules` na raiz do seu projeto (se não existir).
2. Copie para lá os arquivos da pasta **`rules/`** (um por regra).
3. As rules passam a valer apenas para esse workspace.

## Arquivos exportados

| Arquivo | Uso |
|--------|-----|
| **GEMINI.md** | Tudo em um só — ideal para `~/.gemini/GEMINI.md` ou colar em Customizations → Global |
| **rules/core_dotnet.md** | Só convenções .NET (para `.agent/rules`) |
| **rules/core_clean_architecture.md** | Só Clean Architecture (para `.agent/rules`) |

## Observação

Referências a `.cursor/documents/quick-reference.md` e `.cursor/skills/` foram adaptadas para texto genérico, pois são caminhos do Cursor. No Antigravity você pode manter docs equivalentes no projeto e referenciá-los (ex.: `@docs/quick-reference.md` se existir).
