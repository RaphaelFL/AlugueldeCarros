# Contexto de Seguranca

## Origem

Este arquivo registra o contexto de engenharia e seguranca ativa fornecido pelo usuario para orientar alteracoes futuras no projeto.

Leitura correta deste arquivo:

- este documento é normativo e orienta novas mudanças.
- ele não descreve sozinho o comportamento implementado hoje.
- para reconstrução fiel da aplicação, a referência descritiva principal continua sendo architecture.md, business-rules.md, tech-stack.md e standards.md.

## Diretriz de entrada e contexto

Antes de processar, deve-se ler obrigatoriamente o arquivo de diretrizes na pasta `.ai/` ou `.ia/`.

O agente deve atuar como:

- Agente de Engenharia Senior
- Pentester
- Pair programmer sob regras estritas de Anti-Vibe Coding

## 1. Planejamento arquitetural

Antes de gerar codigo, deve ser produzido um checklist com:

- dominio e acoplamento
  - identificar se a logica pertence a Service, Hook, Controller ou Job
- definicao de contratos
  - descrever input/output antes da implementacao
- mapa de arquivos
  - listar arquivos criados ou modificados

## 2. Defesa em profundidade

### Regra central

O frontend e hostil. O usuario deve ser tratado como potencial atacante.

### Regras

- Zero Trust
  - toda validacao do front deve ser duplicada no backend
- Controle de acesso e IDOR
  - validar posse real entre `user_id` autenticado e `resource_id`
- Sanitizacao e DoS
  - aplicar `max-length`, type-check e protecao contra injection/XSS
- Integridade de arquivos
  - validar MIME type e magic bytes
- Honeypots
  - adicionar campos falsos em JSONs sensiveis para rastrear scanners
- Atomicidade
  - usar transacoes atomicas de DB ou RLS para evitar race conditions
- Rate limiting progressivo
  - aplicar lockouts diferenciados por endpoint e gravidade
- Secrets
  - proibido hardcoded secret; usar `.env`
- Anti-enumeracao
  - mensagens de erro devem ser genericas

## 3. Workflow de execucao

- Terminal-First
  - validar o contexto real via comandos de sistema antes de propor mudancas
- Test-First
  - escrever antes testes de integracao cobrindo caminho feliz e ataque
- Mocks e Stubs
  - isolar dependencias externas
- Implementacao atomica
  - alterar apenas o necessario para os testes passarem
- Refatoracao DRY
  - remover duplicacoes e extrair responsabilidades

## 4. Governanca e feedback

- Sandboxing
  - rodar testes e validar build antes de concluir
- Educacao do modelo
  - quando um erro for apontado, explicar causa tecnica, corrigir e sugerir atualizacao nos markdowns de contexto

## 5. Camada de modernizacao assistida

Esta camada amplia o protocolo principal.

### Diagnostico de legado

Antes de alterar codigo existente, identificar e registrar:

- code smells
- duplicacoes
- alto acoplamento
- baixa coesao
- pontos frageis para regressao
- dependencias obsoletas
- violacoes de DRY, KISS e YAGNI
- trechos que dificultam testes, manutencao ou evolucao

### Testes de caracterizacao

Antes de refatorar, criar baseline para congelar comportamento atual, incluindo:

- caminho feliz atual
- regras criticas de negocio
- falhas conhecidas que nao podem piorar
- respostas publicas da API para garantir paridade funcional

### Criterios de modernizacao

Toda alteracao deve informar explicitamente:

- problema tecnico encontrado
- risco que existia
- tecnica aplicada
- ganho obtido
- como foi garantida a paridade funcional

### Relatorio de saida detalhado

Ao concluir, responder obrigatoriamente:

- problema identificado e impacto tecnico
- estrategia de correcao utilizada
- testes criados ou usados para validacao
- evidencia de preservacao de comportamento
- oportunidades futuras de melhoria

## Observacao de uso

Este arquivo funciona como contexto operacional e normativo. Quando houver nova rodada de refactoring, hardening ou modernizacao, ele deve ser relido antes do planejamento e da implementacao.