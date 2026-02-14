# 📌 Video Processing Engine

## Documento de Contexto Arquitetural (ND)

---

## 🧭 Contexto Geral

Este projeto faz parte do **Hackathon FIAP Pós Tech em Arquitetura de Software** e tem como objetivo evoluir um sistema simples de processamento de vídeos para uma **arquitetura moderna, escalável, resiliente e orientada a eventos**, aplicando os principais conceitos estudados ao longo da pós-graduação.

O sistema permite que usuários enviem vídeos, processem esses vídeos de forma assíncrona e paralela (extraindo imagens/frames), e façam o download de um arquivo `.zip` contendo o resultado final do processamento.

Além do funcionamento técnico, o projeto foi pensado para **demonstrar boas práticas reais de mercado**, incluindo:

* Separação clara de responsabilidades
* Arquitetura serverless
* Mensageria e comunicação assíncrona
* Orquestração de fluxos complexos
* Infraestrutura como código (IaC)
* CI/CD e qualidade de software
* Clareza arquitetural para comunicação, avaliação e evolução

---

## 🎯 Objetivos Arquiteturais

Os objetivos que guiaram todas as decisões deste projeto são:

* **Escalabilidade horizontal**: suportar múltiplos vídeos sendo processados simultaneamente.
* **Resiliência**: evitar perda de requisições mesmo em cenários de erro ou pico.
* **Desacoplamento**: minimizar dependências diretas entre componentes.
* **Observabilidade**: permitir rastreabilidade do fluxo ponta a ponta.
* **Governança**: separar infraestrutura, aplicação e entrega de código.
* **Evolução incremental**: permitir iniciar simples e evoluir sem reescrita.

---

## 🧠 Visão Geral da Arquitetura

A arquitetura adota um **modelo serverless orientado a eventos**, utilizando serviços gerenciados da AWS.

Princípios-chave:

* Infraestrutura centralizada em um **repositório único de IaC**
* Aplicações desacopladas em múltiplas **AWS Lambdas**
* Comunicação assíncrona via **SNS, SQS e Step Functions**
* Persistência de estado via **DynamoDB**
* Armazenamento de arquivos via **Amazon S3**
* Autenticação e autorização via **Cognito + API Gateway**

---

## 🧩 Organização dos Repositórios

### 1️⃣ Repositório de Infraestrutura

**`video-processing-engine-infra`**

Responsável por:

* Provisionar toda a infraestrutura AWS via Terraform
* Criar os recursos base:

  * API Gateway
  * Cognito
  * DynamoDB
  * S3 (vídeos, imagens, zip)
  * SNS
  * SQS + DLQ
  * Step Functions
  * CloudWatch
  * Lambdas (somente a "casca")
* Executar apply/destroy via GitHub Actions

> Regra fundamental:
> **Este repositório cria recursos de infraestrutura, mas não realiza deploy de código de aplicação.**

---

### 2️⃣ Repositórios de Aplicação (1 por Lambda)

Cada Lambda possui seu próprio repositório, responsável exclusivamente por:

* Código-fonte
* Testes automatizados
* Quality gates (ex.: cobertura e análise estática)
* Deploy da versão da Lambda

Repositórios:

* `video-processing-engine-auth-lambda`
* `video-processing-engine-video-management-lambda`
* `video-processing-engine-video-orchestrator-lambda`
* `video-processing-engine-video-processor-lambda`
* `video-processing-engine-video-finalizer-lambda`

Este modelo garante pipelines simples, responsabilidades bem definidas e facilidade de evolução.

---

## 🔐 Autenticação e Entrada no Sistema

### Fluxo de autenticação

1. O usuário acessa o sistema via **API Gateway**
2. A requisição é autenticada pela **Lambda de autenticação**
3. A Lambda integra com o **Amazon Cognito**
4. Um token JWT é emitido
5. As demais rotas da API podem ser protegidas por **Cognito Authorizer** no API Gateway

Esse modelo centraliza segurança e evita lógica de autenticação distribuída entre serviços.

---

## 🎥 Fluxo Funcional Ponta a Ponta

### 1️⃣ Cadastro e Upload do Vídeo

* O usuário chama a API de **gerenciamento de vídeos**
* A Lambda:

  * Cria o registro do vídeo no DynamoDB
  * Gera uma URL pré-assinada para upload no S3
  * Retorna essa URL ao cliente

O upload ocorre **diretamente no S3**, sem passar pela API.

---

### 2️⃣ Evento de Upload Concluído

* Após o upload, o S3 emite um evento
* Esse evento publica uma mensagem em um **SNS de vídeo enviado**
* O SNS encaminha a mensagem para uma **SQS de processamento**

Esse padrão desacopla o upload do processamento.

---

### 3️⃣ Orquestração do Processamento

* A Lambda **Video Orchestrator** consome a fila
* Inicia uma execução do **AWS Step Functions**
* Inicialmente, o fluxo pode ser simples (1 Lambda)
* A arquitetura já está preparada para evoluir para:

  * Map State
  * Processamento paralelo

---

### 4️⃣ Processamento do Vídeo

* A Lambda **Video Processor**:

  * Processa o vídeo
  * Extrai imagens/frames
  * Armazena os resultados no S3 (bucket de imagens)
* Durante o processamento, o status pode ser atualizado via fila específica

---

### 5️⃣ Finalização

* Ao concluir o processamento:

  * Uma SQS de finalização é acionada
* A Lambda **Video Finalizer**:

  * Consolida as imagens
  * Gera o arquivo `.zip`
  * Armazena no S3 (bucket de zip)
  * Publica um evento SNS de vídeo finalizado

---

### 6️⃣ Notificação

* O SNS de vídeo finalizado pode:

  * Disparar e-mail
  * Notificar outro sistema
  * Atualizar status final no banco

---

## 💾 Persistência e Estado

* **DynamoDB** é utilizado para:

  * Metadados do vídeo
  * Status de processamento
  * Consulta por usuário e por vídeo
* O banco é tratado como **fonte única de verdade** do estado do vídeo
* Nenhuma Lambda acessa dados de outra diretamente

---

## 📬 Mensageria e Resiliência

* Todas as filas possuem **DLQ**
* Falhas não causam perda de mensagens
* Processos pesados não bloqueiam requisições síncronas
* O sistema suporta picos de carga com degradação controlada

---

## 🔍 Observabilidade

* Logs centralizados no **CloudWatch Logs**
* Step Functions com logs habilitados
* Observabilidade pensada para debugging, entendimento do fluxo e apresentação

(Evoluções futuras podem incluir X-Ray ou Prometheus/Grafana.)

---

## 🧪 Qualidade e CI/CD

* Todos os repositórios possuem:

  * CI com build e testes
  * Gates de qualidade
  * Deploy automatizado
* Infraestrutura versionada e reproduzível
* Branch `main` protegida

---

## 📐 Estratégia de Evolução

A arquitetura foi desenhada para evoluir sem reescrita:

* Iniciar com fluxos simples
* Evoluir Step Functions
* Adicionar paralelismo
* Incluir novos tipos de processamento
* Expandir notificações e segurança

---

## 🧠 Uso deste Documento

Este documento deve ser utilizado como:

* Contexto base para escrita de **stories, tasks e subtasks**
* Fonte única de verdade arquitetural
* Material de apoio para decisões técnicas
* Guia de onboarding
* Base narrativa para apresentações

> **Nenhuma story deve ser escrita sem considerar este contexto.**
