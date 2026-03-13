# Configuração SonarCloud — GitHub Actions

Este documento descreve os passos manuais para integrar o SonarCloud ao repositório e fazer o job `sonar-analysis` do workflow funcionar.

## Pré-requisitos

- Conta no [SonarCloud](https://sonarcloud.io) (login via GitHub).
- Repositório conectado à organização no SonarCloud.

## 1. SonarCloud — Criar projeto e desativar Automatic Analysis

1. Acesse [sonarcloud.io](https://sonarcloud.io) e faça login com GitHub.
2. **+** → **Analyze new project** → selecione o repositório `video-processing-engine-video-management-lambda`.
3. Anote o **Project Key** (ex.: `minha-org_video-processing-engine-video-management-lambda`) e o **Organization slug** (ex.: `minha-org`).
4. **OBRIGATÓRIO:** no projeto criado, vá em **Administration → Analysis Method** e **desative** o toggle **Automatic Analysis**.  
   Sem isso, o CI falha com: `You are running CI analysis while Automatic Analysis is enabled`.

## 2. SonarCloud — Token de autenticação

1. Acesse [sonarcloud.io/account/security](https://sonarcloud.io/account/security).
2. Crie um token do tipo **Project Analysis Token** ou **Global Analysis Token**.
3. Copie o valor (exibido apenas uma vez).

## 3. GitHub — Secrets e Variables

Em **Settings → Secrets and variables → Actions** do repositório:

| Tipo     | Nome                 | Valor                                                                 |
|----------|----------------------|-----------------------------------------------------------------------|
| Secret   | `SONAR_TOKEN`        | Token gerado no passo 2 (SonarCloud).                                 |
| Variable | `SONAR_PROJECT_KEY`  | Project Key anotado no passo 1 (ex.: `minha-org_meu-repo`).            |
| Variable | `SONAR_ORGANIZATION` | Organization slug anotado no passo 1 (ex.: `minha-org`).              |

## 4. SonarCloud — Quality Gate e webhook (opcional)

- **Project Settings → Quality Gate:** configure ou use o padrão (ex.: cobertura em novo código ≥ 70%).
- **Project Settings → GitHub:** ative o webhook para que o status do Quality Gate apareça como check no PR.

## 5. Branch Protection (manual)

1. **Settings → Branches → Branch protection rules** → regra para `main`.
2. Habilite **Require status checks to pass before merging**.
3. Adicione o check **SonarCloud Analysis** (nome exato do job no workflow).  
   O check só aparece na lista após o job ter rodado pelo menos uma vez.

## 6. Badges no README

Substitua `SONAR_PROJECT_KEY` no `README.md` pelo Project Key real do seu projeto (nas URLs dos badges Quality Gate e Coverage).

## Referência

- Skill: `.cursor/skills/sonarcloud-dotnet/SKILL.md`
- Story: `storys/Storie-21-Integracao_SonarCloud_Cobertura_GitHub_Actions/`
