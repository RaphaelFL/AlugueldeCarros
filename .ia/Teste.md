# TESTS

## 1. Visão geral

Este documento descreve a estratégia de testes do projeto completo, separando claramente back-end e front-end.

Objetivo operacional:

- manter cobertura útil sobre regras críticas e fluxos principais.
- permitir que outra IA ou outro desenvolvedor repita a execução local sem depender de contexto implícito.
- usar cobertura para priorizar os próximos testes, em vez de inflar percentual com testes de baixo valor.

Estado atual do projeto:

- back-end em .NET 8 com testes unitários e de integração.
- front-end em React 19 + TypeScript + Vite.
- front-end agora possui estratégia de testes com Jest.

Separação adotada:

- back-end: valida regras de negócio, controllers, middleware, segurança, repositórios in-memory e endpoints HTTP reais.
- front-end: valida store de autenticação, hooks, utilitários, lógica de features, componentes estáveis e fluxos de UI prioritários.

## 2. Tecnologias de teste do back-end

Stack real usada hoje no back-end:

- xUnit
- FluentAssertions
- Moq
- Microsoft.AspNetCore.Mvc.Testing
- coverlet collector via `dotnet test --collect:"XPlat Code Coverage"`
- ReportGenerator para HTML

O back-end já possui:

- testes unitários em `AlugueldeCarros.Tests/Unit`
- testes de integração em `AlugueldeCarros.Tests/Integration`
- cobertura prática voltada para controllers, services, middleware, segurança e repositórios

Exemplos de áreas cobertas no back-end:

- autenticação e refresh
- ownership de reservas e pagamentos
- exceções e middleware
- repositórios in-memory
- carga inicial com `JsonDataLoader`

Baseline atual validado:

- `dotnet test AlugueldeCarros.sln`
- resultado consolidado atual: 134 testes passando

## 3. Tecnologias de teste do front-end

Stack real usada hoje no front-end para testes:

- Jest
- ts-jest
- jest-environment-jsdom
- Testing Library para React
- `@testing-library/jest-dom`
- `@testing-library/user-event`
- cobertura nativa do Jest com relatório HTML, lcov e resumo textual

Arquivos de configuração adicionados no front-end:

- `frontend/jest.config.cjs`
- `frontend/tsconfig.jest.json`
- `frontend/jest.setup.ts`

Scripts disponíveis em `frontend/package.json`:

- `npm test`
- `npm run test:watch`
- `npm run test:coverage`

Abordagem adotada no front-end:

- hooks e stores: validar estado derivado, autenticação, hidratação e regras de sessão
- utilitários: validar parsing de JWT, expiração e helpers puros
- features: validar schema, lógica crítica de login/cadastro e view models
- componentes estáveis: validar renderização, estados básicos e ações principais
- cobertura inicial sem reescrever a arquitetura do front

O front-end não usava suíte de testes antes desta implementação.

## 4. O que já existe de teste hoje

### 4.1 Back-end

O back-end já possuía cobertura consolidada em:

- services
- controllers
- middleware
- segurança
- repositórios in-memory
- integração HTTP da API

### 4.2 Front-end

Testes iniciais implementados no front-end:

- `frontend/src/store/auth-store.test.ts`
- `frontend/src/hooks/use-auth.test.tsx`
- `frontend/src/lib/jwt.test.ts`
- `frontend/src/features/not-found-page/not-found-page.test.tsx`
- `frontend/src/features/login-page/login-page.logic.test.tsx`

Esses testes cobrem, no mínimo:

- persistência e limpeza do estado de autenticação
- cálculo de `isAuthenticated` e `isAdmin`
- leitura de roles e userId a partir do JWT
- verificação de expiração com buffer
- schema e fluxo crítico de login/cadastro no front
- renderização de componente de página estável

Baseline atual validado do front-end:

- `npm test -- --runInBand`
- 5 suítes passando
- 13 testes passando

## 5. Como rodar os testes do back-end

Na raiz do repositório:

```bash
dotnet test AlugueldeCarros.sln
```

Se quiser rodar apenas o projeto de testes:

```bash
dotnet test AlugueldeCarros.Tests/AlugueldeCarros.Tests.csproj
```

## 6. Como coletar cobertura do back-end

Na raiz do repositório:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

Ou de forma explícita pela solução:

```bash
dotnet test AlugueldeCarros.sln --collect:"XPlat Code Coverage"
```

Instalação do gerador de relatório HTML:

```bash
dotnet tool install --global dotnet-reportgenerator-globaltool
```

Geração do relatório HTML:

```bash
reportgenerator -reports:"**/TestResults/**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
```

Abrir o relatório HTML no Windows:

```bash
start coveragereport\index.html
```

Artefato principal do back-end:

- `coveragereport/index.html`

## 7. Como rodar os testes do front-end

Dentro da pasta `frontend`:

```bash
npm test
```

Execução direta com Jest:

```bash
npx jest
```

Execução em modo watch:

```bash
npm run test:watch
```

Também é válido rodar a partir da raiz com prefixo:

```bash
npm --prefix frontend test
```

## 8. Como coletar cobertura do front-end

Dentro da pasta `frontend`:

```bash
npm run test:coverage
```

Ou diretamente com Jest:

```bash
npx jest --coverage
```

Artefatos gerados no front-end:

- `frontend/coverage/index.html`
- `frontend/coverage/lcov-report/index.html`
- `frontend/coverage/lcov.info`

Abrir o relatório HTML do front-end no Windows:

```bash
start coverage\index.html
```

## 9. Como interpretar a cobertura

Regra prática comum para back-end e front-end:

1. localizar arquivos de comportamento crítico com cobertura baixa
2. diferenciar código executável relevante de tipos, DTOs e wrappers triviais
3. priorizar branches, falhas, autorização, loading, erro e fluxo feliz
4. evitar criar teste só para subir percentual sem aumentar confiança

### 9.1 Interpretação no back-end

Priorize:

- auth e autorização
- rules de reservation e payment
- endpoints administrativos
- middleware e tratamento de erro
- repositories com branch de persistência e filtros

### 9.2 Interpretação no front-end

Priorize:

- autenticação e bootstrap de sessão
- proteção de rotas e RBAC visual
- hooks de tela com side effect
- stores persistidas
- services de API
- estados de loading, erro e sucesso
- componentes que tomam decisão de navegação ou autorização

## 10. O que deve ser testado no front-end

Cobertura prioritária do front-end daqui para frente:

- componentes com comportamento e decisão visual relevante
- páginas com fluxo crítico de negócio
- hooks customizados
- serviços de API
- autenticação e sessão
- estados de loading, erro e sucesso
- proteção de rotas
- RBAC visual, se o componente esconder ou liberar ação conforme role

Áreas concretas do projeto que merecem expansão após a base inicial:

- `frontend/src/app/providers.tsx`
- `frontend/src/api/services.ts`
- `frontend/src/api/http.ts`
- `frontend/src/routes/router.tsx`
- `frontend/src/hooks/use-reservation-payment.ts`
- features de catálogo, pagamento, reserva e áreas administrativas

## 11. Como usar cobertura para descobrir lacunas

### 11.1 Sinais de lacuna no back-end

- endpoints sem cenário de sucesso
- endpoints sem cenário de falha
- ramos de autorização sem teste
- services com exceções ou transições sem validação
- repositories com filtros, fallback ou persistência sem cenário de borda

### 11.2 Sinais de lacuna no front-end

- arquivos em `api` com 0%
- `providers.tsx` sem testes de bootstrap
- `router.tsx` sem validação de guards
- componentes com loading, erro e ação sem teste
- hooks e lógica de feature com 0%
- fluxos com navegação, persistência e refresh sem caracterização

### 11.3 Leitura prática da cobertura atual do front

A primeira rodada de cobertura do Jest mostrou:

- boa cobertura inicial em `auth-store.ts`, `use-auth.ts`, `jwt.ts`, `login-page.logic.ts` e not-found page
- cobertura ainda zerada em `api/http.ts`, `api/services.ts`, `app/providers.tsx`, `routes/router.tsx` e grande parte das features

Conclusão prática:

- a infraestrutura do Jest está funcional
- a cobertura do front foi contemplada
- a próxima expansão deve atacar bootstrap de sessão, serviços HTTP e guards de rota

## 12. Regras para criação de novos testes

Regras obrigatórias para novos testes neste projeto:

- seguir o padrão real do repositório
- não duplicar testes equivalentes
- priorizar fluxos críticos e branches reais
- usar cobertura como insumo, não como meta cega
- manter escopo pequeno e objetivo
- preferir testes unitários e de integração leve antes de montar cenários artificiais grandes
- não recriar a arquitetura do front só para testar

No front-end, a ordem recomendada é:

1. store e hooks críticos
2. lógica de feature
3. providers e guards
4. serviços de API
5. componentes de alto impacto visual e funcional

## 13. Coerência com o projeto real

Decisões preservadas nesta implementação:

- o back-end continua com sua stack original de testes
- o front-end permanece em React + TypeScript + Vite
- a solução não foi recriada
- o front-end não foi recriado
- Jest foi adicionado como stack de teste do front sem substituir a arquitetura existente
- o build do front continua funcionando
- a suíte de testes do front e a cobertura do Jest estão funcionando

Resumo operacional final:

- back-end: xUnit + FluentAssertions + integração HTTP + cobertura por XPlat Code Coverage
- front-end: Jest + ts-jest + jsdom + Testing Library + cobertura por Jest
- documentação unificada: este arquivo `.ia/Teste.md`
