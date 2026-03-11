# Storie-14: Adapter de entrada SQS e JSON direto na Lambda Update Video

## Status
- **Estado:** 🔄 Em desenvolvimento
- **Data de Conclusão:** —

## Descrição
Como desenvolvedor do Video Processing Engine, quero que a Lambda de atualização de vídeo (VideoManagement.LambdaUpdateVideo) aceite tanto invocação via **SQS** (evento no formato padrão AWS com `Records[].Body`) quanto **JSON direto** (payload igual ao DTO de update), para que o Step Function possa acionar a Lambda via fila SQS em produção e os testes locais/manuais continuem usando o mesmo contrato de payload direto, sem campos vazios (ex.: VideoId = 00000000-0000-0000-0000-000000000000).

## Objetivo
Implementar um **adapter de entrada** na Lambda Update Video que normalize o evento bruto em um ou mais `UpdateVideoLambdaEvent` (DTO de update), de forma que:
- **Invocação via SQS (produção):** o evento chega como `{ "Records": [ { "body": "{ ... json do update ... }" } ] }`; o adapter lê `Records`, extrai cada `Body` e desserializa para `UpdateVideoLambdaEvent`.
- **Invocação direta (testes locais / execução manual):** o evento é o próprio JSON do update (`videoId`, `userId`, `status`, etc.); o adapter trata o payload como DTO e retorna um único `UpdateVideoLambdaEvent`.

O handler existente (`IUpdateVideoHandler` / `UpdateVideoLambdaEvent` → `UpdateVideoLambdaResponse`) permanece inalterado em contrato; apenas o **ponto de entrada** da Function passa a receber o evento bruto e usar o adapter antes de chamar o handler.

## Escopo Técnico
- **Tecnologias:** .NET 10, AWS Lambda, System.Text.Json. Contrato de saída do adapter: `UpdateVideoLambdaEvent` (ou lista quando SQS com múltiplos Records).
- **Arquivos/projetos:**
  - Projeto: `src/InterfacesExternas/VideoProcessing.VideoManagement.LambdaUpdateVideo/`
  - Novo: componente de adapter (ex.: `IUpdateVideoEventAdapter` + implementação que recebe evento bruto e retorna um ou mais `UpdateVideoLambdaEvent`)
  - Alteração: `Function.cs` — assinatura do handler passa a receber o evento bruto (ex.: `JsonDocument`, `Stream` ou tipo que represente SQS + JSON direto); usar adapter e invocar `IUpdateVideoHandler` para cada evento extraído
  - Documentação: `docs/lambda-update-video-contract.md` — descrever os dois formatos de invocação (SQS e JSON direto) e exemplos
- **Componentes:** Adapter de evento de entrada; ajuste no ponto de entrada da Lambda (Function); testes unitários do adapter; documentação atualizada.
- **Pacotes/Dependências:** Nenhum pacote novo obrigatório; uso de System.Text.Json já presente no projeto.

## Dependências e Riscos (para estimativa)
- **Dependências:** Storie-10 (Lambda Update Video existente com handler e contrato `UpdateVideoLambdaEvent`).
- **Riscos:**
  - Detecção segura do formato: distinguir SQS (presença de `Records` array com pelo menos um item com `body`) de JSON direto (objeto com `videoId`/`userId`); evitar falsos positivos.
  - SQS batch: múltiplos `Records` — definir se processamos todos e como agregar respostas (ex.: processar cada record e retornar a última resposta; ou falhar no primeiro erro para retry do batch).

## Subtasks
- [Subtask 01: Adapter de evento (detectar SQS vs JSON direto e extrair DTOs)](./subtask/Subtask-01-Adapter_Evento_SQS_JSON_DTO.md)
- [Subtask 02: Integrar adapter no ponto de entrada da Lambda (Function)](./subtask/Subtask-02-Integrar_Adapter_Function_Entrada.md)
- [Subtask 03: Testes unitários do adapter e atualização da documentação](./subtask/Subtask-03-Testes_Adapter_Documentacao.md)

## Critérios de Aceite da História
- [ ] Com evento no formato SQS (`Records[].Body` com JSON do update), a Lambda extrai o body, desserializa para `UpdateVideoLambdaEvent` e processa com o handler existente; VideoId e UserId (e demais campos) preenchidos corretamente.
- [ ] Com evento em JSON direto (payload igual ao DTO com `videoId`, `userId`, etc.), a Lambda trata como um único `UpdateVideoLambdaEvent` e processa normalmente (comportamento atual de testes manuais preservado).
- [ ] Adapter detecta de forma confiável os dois formatos (SQS vs direto) sem ambiguidade (ex.: presença de `Records` como array com itens que tenham `body` = SQS).
- [ ] Quando houver múltiplos SQS Records, a Lambda processa cada record (invocando o handler para cada um); critério de retorno (ex.: última resposta ou primeira com erro) documentado e implementado de forma consistente.
- [ ] Testes unitários cobrindo: evento SQS com um body válido; evento JSON direto válido; evento SQS com body inválido ou vazio; evento malformado; cobertura do adapter ≥ 80%.
- [ ] Documentação (`docs/lambda-update-video-contract.md`) atualizada com os dois modos de invocação (SQS e JSON direto) e exemplos de payload para cada um.
- [ ] `dotnet build` e `dotnet test` passam na solução; nenhuma regressão nos testes existentes do handler.

## Rastreamento (dev tracking)
- **Início:** —
- **Fim:** —
- **Tempo total de desenvolvimento:** —
