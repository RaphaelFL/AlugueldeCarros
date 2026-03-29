# Aula 4 - Relatorio de Hardening e Preservacao de Comportamento

## Confirmacao

Sim. A Aula 4 pede um markdown de rastreabilidade tecnica da alteracao. Este arquivo cumpre esse papel para a rodada de hardening realizada em 2026-03-28.

## Problema identificado e impacto tecnico

### Problema principal

O backend e o frontend tinham fragilidades de contrato e previsibilidade em pontos sensiveis do fluxo de autenticacao e administracao:

- requests criticos sem validacao explicita suficiente no backend
- tratamento de erro inconsistente entre `400`, `401`, `404` e `500`
- possibilidade de exposicao de mensagem interna em erro `500`
- normalizacao incompleta de e-mail e dados de autenticacao
- frontend menos restritivo do que o backend para campos sensiveis
- parser de erros do frontend sem suporte adequado para detalhes estruturados de validacao
- `AlugueldeCarros.sln` apontando para a pasta `.ia`, mas ainda com referencias fisicas antigas em `.ai`

### Impacto tecnico

- maior chance de respostas ambiguas para o frontend
- risco de regressao silenciosa de contrato entre front e back
- aumento de superficie para brute force e abuso em endpoints de auth
- comportamento menos auditavel em cenarios de falha
- risco de confusao operacional na documentacao do repositorio e no Visual Studio

## Prompt usado

O trabalho foi guiado pelo protocolo enviado pelo usuario, com enfase nos pontos abaixo:

- planejar antes de alterar
- aplicar defesa em profundidade
- duplicar validacao no backend
- reduzir enumeracao e exposicao de detalhe interno
- usar terminal-first e validar build/testes
- registrar problema, tecnica, ganho e paridade funcional

Resumo do prompt operacional utilizado nesta entrega:

"Aplicar hardening em front e back, ler obrigatoriamente a pasta .ia, agir como engenheiro senior/pentester, validar por terminal, preservar comportamento existente, registrar problema tecnico, tecnica aplicada, testes usados e evidencia de paridade funcional."

## Estrategia de correcao utilizada

Foi adotado refactoring incremental e atomico, sem trocar a arquitetura real do projeto.

### Tecnicas aplicadas

- padronizacao de erro HTTP em JSON no backend
- validacao de request por Data Annotations nos DTOs sensiveis
- normalizacao de e-mail e dados textuais no servico de autenticacao
- resposta generica para erro interno
- uniformizacao do refresh token invalido
- rate limiting para endpoints de autenticacao
- endurecimento dos schemas do frontend
- leitura de `details` de validacao no frontend
- correcao das referencias `.ai` para `.ia` na solucao

## Antes vs. Depois

### Antes

#### Backend

- requests aceitavam entradas sem validacao explicita em varios DTOs criticos
- `ValidationException` e `KeyNotFoundException` nao eram mapeadas de forma consistente no middleware
- erros `500` podiam expor mensagem interna em vez de payload generico
- `AuthService` aceitava e-mail sem normalizacao defensiva
- refresh podia distinguir token invalido de usuario inexistente
- payloads administrativos semanticamente vazios podiam ter tratamento inconsistente

#### Frontend

- login, cadastro e administracao tinham limites menos rigidos do que os contratos esperados no backend
- erros estruturados de validacao nao eram interpretados corretamente
- campos criticos nao tinham todos os `maxLength` e normalizacoes coerentes com o backend

### Depois

#### Backend

- `Program.cs`
  - `ModelState` invalido responde JSON padronizado com `error` e `details`
  - rate limiter adicionado para auth com resposta `429` consistente
- `ExceptionHandlingMiddleware.cs`
  - `ValidationException` responde `400`
  - `KeyNotFoundException` responde `404`
  - `500` responde mensagem generica
- `AuthService.cs`
  - e-mail normalizado com `trim` e `lowercase`
  - nomes normalizados com `trim`
  - refresh responde `Invalid token` de forma uniforme em cenarios invalidos
- DTOs endurecidos em:
  - auth
  - reservations
  - payments
  - users roles
  - vehicles
- `AdminUsersController.cs`
  - roles semanticamente vazias respondem `400` consistente
- `AlugueldeCarros.sln`
  - referencias da pasta `.ia` corrigidas

#### Frontend

- `frontend/src/pages/public-pages.tsx`
  - schema de auth com limites e normalizacao coerentes com backend
  - inputs com `autocomplete` e `maxLength`
- `frontend/src/pages/admin-pages.tsx`
  - schema de veiculo com limites mais estritos e placa normalizada
- `frontend/src/lib/utils.ts`
  - extracao de mensagens vindas de `details` do backend

## O que foi mudado

### Backend

- `AlugueldeCarros/Program.cs`
- `AlugueldeCarros/Middleware/ExceptionHandlingMiddleware.cs`
- `AlugueldeCarros/Services/AuthService.cs`
- `AlugueldeCarros/DTOs/Auth/LoginRequest.cs`
- `AlugueldeCarros/DTOs/Reservations/CreateReservationRequest.cs`
- `AlugueldeCarros/DTOs/Payments/PreauthRequest.cs`
- `AlugueldeCarros/DTOs/Payments/CaptureRequest.cs`
- `AlugueldeCarros/DTOs/Payments/RefundRequest.cs`
- `AlugueldeCarros/DTOs/Users/AddUserRolesRequest.cs`
- `AlugueldeCarros/DTOs/Vehicles/CreateVehicleRequest.cs`
- `AlugueldeCarros/DTOs/Vehicles/UpdateVehicleRequest.cs`
- `AlugueldeCarros/Controllers/AuthController.cs`
- `AlugueldeCarros/Controllers/AdminUsersController.cs`
- `AlugueldeCarros.sln`

### Frontend

- `frontend/src/lib/utils.ts`
- `frontend/src/pages/public-pages.tsx`
- `frontend/src/pages/admin-pages.tsx`

### Testes

- `AlugueldeCarros.Tests/Unit/Services/AuthServiceTests.cs`
- `AlugueldeCarros.Tests/Integration/Controllers/AuthControllerTests.cs`
- `AlugueldeCarros.Tests/Integration/Controllers/AdminEndpointsTests.cs`

## Testes criados ou usados para validacao

### Testes adicionados/ajustados

- normalizacao de e-mail no `AuthService`
- normalizacao de nome/sobrenome no `AuthService`
- refresh com token invalido retornando falha uniforme
- login com payload invalido retornando `400` com `details`
- roles semanticamente vazias retornando `400`

### Validacao executada

- `dotnet test AlugueldeCarros.sln`
  - resultado da rodada atual consolidada: `134/134` testes passando
- `npm run build` em `frontend/`
  - resultado: build concluido com sucesso

## Evidencia de preservacao do comportamento

A paridade funcional foi preservada pelos seguintes criterios:

- fluxo principal de login continua retornando token para credenciais validas
- fluxo de cadastro continua autenticando o usuario apos criacao
- refresh continua emitindo novo token para token valido
- frontend continua consumindo os mesmos endpoints centrais
- nao foram introduzidos banco, cache, fila, refresh token persistido ou servicos externos inexistentes no projeto
- o endurecimento atuou no contrato e no tratamento de falha, nao na regra de negocio principal

## Ganho obtido

- mais previsibilidade entre backend e frontend
- menor exposicao de detalhe interno em cenarios de erro
- contrato de validacao mais claro e defensivo
- melhor testabilidade dos cenarios de auth e validacao
- menor risco de regressao silenciosa em payloads invalidos

## Oportunidades futuras de melhoria

- tratar warnings de nulabilidade do backend e dos testes
- introduzir DTO dedicado para pricing em vez de receber entidade de dominio direto no controller
- expandir rate limiting para politicas diferenciadas por endpoint e severidade
- adicionar trilha de auditoria persistente para acoes administrativas
- endurecer mais cenarios de caracterizacao e abuso em endpoints de reservas e pagamentos

## Decisao de engenharia

As alteracoes foram limitadas ao que traz ganho real imediato de seguranca, consistencia e testabilidade sem quebrar a arquitetura atual em memoria baseada em JSON.

## Referencia cruzada

O contexto normativo de seguranca que orientou esta entrega esta registrado em `.ia/Security.md`.