---
name: technical-stories
description: Creates technical stories (story.md and subtasks) inside the storys/ folder and applies dev tracking (start/end time, Brasília). Ensures storys/ exists at project root; each story lives in storys/Storie-XX-Name/. Use when the user asks to create a story, write a story, create a technical story, or when developing a story (record start on begin; on conclusion mark story and subtasks done and record end time and total duration).
---

# Histórias Técnicas — Criação e Rastreamento

## Regra obrigatória: pasta storys/

**Todas as stories devem ficar dentro da pasta `storys/`.**

1. **Antes de criar qualquer story:** verificar se existe a pasta **`storys/`** na **raiz do projeto**. Se não existir, **criar a pasta `storys/`**.
2. **Cada story** é criada **dentro** de `storys/`, no formato: `storys/Storie-XX-Descricao_Breve/`.

Estrutura esperada:

```
<raiz do projeto>/
└── storys/
    ├── Storie-01-Implementar_Autenticacao/
    │   ├── story.md
    │   └── subtask/
    │       ├── Subtask-01-Nome.md
    │       └── ...
    └── Storie-02-Outra_Historia/
        ├── story.md
        └── subtask/
```

Nunca criar stories fora de `storys/`. Nunca criar a pasta da story diretamente na raiz do projeto.

---

## Parte 1 — Criar uma story

**Quando o usuário pedir para criar uma story:** apenas criar (story.md + subtasks). Não desenvolver nem executar subtasks. Aguardar pedido explícito para implementar.

### Passos

1. Garantir que existe `storys/`; criar se faltar.
2. Definir número da story (XX com 2 dígitos) e nome da pasta: `Storie-XX-Descricao_Com_Underscore`.
3. Criar `storys/Storie-XX-Descricao/` com:
   - `story.md` na raiz da pasta da story (estrutura completa; ver [reference.md](reference.md)).
   - Pasta `subtask/` com um arquivo por subtask: `Subtask-01-Nome.md`, etc.
4. Mínimo 3 subtasks, máximo 8; mínimo 5 critérios de aceite no story.md; links relativos `./subtask/Subtask-XX-Nome.md`.
5. Se a story envolve código: incluir subtask(s) e critérios de aceite para testes unitários; registrar pacotes/dependências com nome e versão no Escopo Técnico.

### Checklist rápido (criação)

- [ ] Pasta `storys/` existe (criada se necessário)
- [ ] Story em `storys/Storie-XX-Descricao/`
- [ ] `story.md` + pasta `subtask/` com todos os arquivos de subtask
- [ ] Descrição "Como... quero... para..."; mínimo 5 critérios de aceite; links para subtasks corretos

---

## Parte 2 — Desenvolver uma story (dev tracking)

Aplica-se quando estivermos **desenvolvendo** uma story (arquivos em `storys/` ou referência a story/subtask).

**Fuso:** sempre **horário de Brasília**. 

**Regra de horário:**
- **SEMPRE obter o horário atual do sistema** usando comando `powershell -Command "Get-Date -Format 'dd/MM/yyyy HH:mm'"` ou equivalente quando iniciar ou finalizar o desenvolvimento.
- **Nunca inventar horários.**
- Se o usuário informar explicitamente um horário específico (ex.: "o horário agora é 20:56"), usar esse horário informado pelo usuário em vez de obter do sistema.
- Em dúvida, perguntar ao usuário.

### Ao iniciar o desenvolvimento

1. **OBTER o horário atual do sistema** usando comando apropriado (ex.: `powershell -Command "Get-Date -Format 'dd/MM/yyyy HH:mm'"`).
2. No **início** da sessão de dev, registrar no `story.md` a seção **Rastreamento (dev tracking)** (criar se não existir):
   - **Início:** dia DD/MM/AAAA, às HH:MM (Brasília) — usar o horário obtido do sistema
   - **Fim:** —
   - **Tempo total de desenvolvimento:** —

### Ao usuário avisar que está concluído ("pronto", "finalizado", etc.)

1. **OBTER o horário atual do sistema** usando comando apropriado (ex.: `powershell -Command "Get-Date -Format 'dd/MM/yyyy HH:mm'"`).
2. Marcar a story como **✅ Concluída** no `story.md` (Status e Data de Conclusão DD/MM/AAAA).
3. Marcar **todas as subtasks** como prontas (`[x]`) no `story.md` e, se aplicável, nos arquivos em `subtask/*.md`.
4. Na seção **Rastreamento (dev tracking)** do `story.md`:
   - **Fim:** dia DD/MM/AAAA, às HH:MM (Brasília) — usar o horário obtido do sistema
   - **Tempo total de desenvolvimento:** calcular corretamente a diferença entre Início e Fim:
     - **Método de cálculo:** converter ambos os horários para minutos desde meia-noite, calcular a diferença, e converter de volta para horas/minutos
     - Se a diferença for menor que 1 hora: usar formato "Xmin" (ex.: "15min", "45min", "1min")
     - Se a diferença for 1 hora ou mais: usar formato "Xh Ymin" (ex.: "1h 30min", "2h 15min")
     - Se a diferença for exatamente X horas sem minutos: usar formato "Xh" (ex.: "2h")
     - **Exemplos de cálculo correto:**
       - Início: 20:55, Fim: 20:56 → diferença = 1min → "1min"
       - Início: 14:00, Fim: 16:00 → diferença = 120min = 2h → "2h"
       - Início: 14:30, Fim: 16:15 → diferença = 105min = 1h 45min → "1h 45min"
       - Início: 09:10, Fim: 10:05 → diferença = 55min → "55min"

---

## Regra de conclusão (commit)

Quando o usuário solicitar o **commit** das mudanças de uma história: marcar a story como **✅ Concluída** com **data de conclusão** preenchida (DD/MM/AAAA) e aplicar o dev tracking de conclusão acima se ainda não aplicado.

---

## Referência completa

- Estrutura do `story.md`, formato das subtasks, nomenclatura, exemplos e checklist detalhado: [reference.md](reference.md)
