# TECH STACK

Este documento lista a stack efetivamente usada hoje no projeto. Use-o como referência para reproduzir a aplicação sem trocar tecnologia ou introduzir componentes que não existem.

## 1. Runtime e Framework

- Linguagem: C#
- Target framework: net8.0
- Tipo de projeto: solução .NET com 1 Web API e 3 class libraries
- Formato: SDK-style csproj

Objetivo deste arquivo:

- registrar a stack efetivamente instalada e usada.
- evitar substituição automática por tecnologias equivalentes.
- servir como referência operacional de ambiente para reconstrução.

## 2. Pacotes do Projeto Principal

Pacotes confirmados no backend atual:

- API: Microsoft.AspNetCore.Authentication.JwtBearer 8.0.25
- API: Swashbuckle.AspNetCore 6.6.2
- API: System.IdentityModel.Tokens.Jwt 8.17.0
- Application: BCrypt.Net-Next 4.1.0
- Infrastructure: Microsoft.Extensions.Configuration.Abstractions 8.0.0

Recursos da base class library usados diretamente:

- System.ComponentModel.DataAnnotations
- System.Text.Json
- Microsoft.IdentityModel.Tokens

## 3. API e Middleware

- Controllers com atributo ApiController.
- Rotas por atributo, todas sob api/v1.
- Swagger e Swagger UI em ambiente Development.
- HTTPS redirection habilitado.
- ExceptionHandlingMiddleware no pipeline.

Ordem relevante do pipeline:

1. UseHttpsRedirection
2. UseMiddleware<ExceptionHandlingMiddleware>
3. UseAuthentication
4. UseAuthorization
5. MapControllers

## 4. Autenticação e Autorização

- esquema: JWT Bearer.
- assinatura: HmacSha256.
- segredo, issuer, audience e expiry vindos de JwtSettings no appsettings.
- autorização nativa do ASP.NET Core com Authorize e Authorize(Roles = "Admin").

Claims emitidos:

- ClaimTypes.NameIdentifier
- ClaimTypes.Email
- ClaimTypes.Name
- ClaimTypes.Role

## 5. Persistência

Persistência atual:

- arquivos em AlugueldeCarros/Resources/MockData.
- JsonDataLoader em AlugueldeCarros.Infrastructure para carregar dados no startup.
- repositórios singleton em memória usando List<T>.

Consequência prática:

- quase todas as alterações feitas durante a execução ficam apenas em memória.
- exceção importante: users.json é persistido de volta em add, update e delete do InMemoryUserRepository.

Tecnologias que não fazem parte da implementação atual:

- Entity Framework Core
- Dapper
- SQL Server
- PostgreSQL
- Redis

## 6. Injeção de Dependência

Lifetime atual:

- repositories: singleton
- services: scoped
- ITokenService/JwtTokenService: singleton
- JsonDataLoader: singleton

Observação importante:

- InMemoryVehicleRepository depende de IReservationRepository, por isso a ordem de registro em Program.cs foi tratada explicitamente.

## 7. Segurança de Senha

- senhas novas são geradas com BCrypt.Net-Next.
- existe utilitário PasswordHasher.
- AuthService ainda aceita fallback de comparação em texto puro para compatibilidade com dados já carregados.

## 8. Testes

O projeto já usa uma stack de testes .NET convencional:

- xUnit
- FluentAssertions
- Moq
- Microsoft.AspNetCore.Mvc.Testing
- coverlet.collector

Uso atual:

- testes unitários de services e middleware.
- testes de integração com WebApplicationFactory.

Versões confirmadas no projeto de testes:

- Microsoft.NET.Test.Sdk 17.8.0
- xUnit 2.5.3
- xunit.runner.visualstudio 2.5.3
- FluentAssertions 6.12.0
- Moq 4.20.0
- Microsoft.AspNetCore.Mvc.Testing 8.0.0
- coverlet.collector 6.0.0

## 9. Convenções Técnicas Importantes

- IDs atuais são inteiros.
- enums são usados diretamente no domínio e nos endpoints.
- alguns endpoints retornam entidades diretamente, sem DTO de saída dedicado.
- controllers injetam interfaces de service da Application.

## 10. Estrutura dos Projetos Backend

- AlugueldeCarros.Domain: entidades e enums.
- AlugueldeCarros.Application: regras de negócio, contratos de repository, contratos de service e abstrações de segurança.
- AlugueldeCarros.Infrastructure: repositórios in-memory e JsonDataLoader.
- AlugueldeCarros: host HTTP, controllers, DTOs, middleware, autenticação JWT concreta e configuração.

## 11. Restrições para Reconstrução

Para manter equivalência com o projeto atual, não troque automaticamente por:

- Clean Architecture completa.
- EF Core.
- banco relacional.
- filas.
- serviços externos.
- autenticação com Identity.

## 12. Stack Oficial do Front-end

O front oficial do projeto é React e fica na pasta frontend.

Stack adotada:

- React 19
- TypeScript 5
- Vite 6
- React Router 7
- TanStack Query 5
- Zustand 5
- React Hook Form 7
- Zod 3
- Axios 1
- Tailwind CSS 3
- Lucide React para ícones
- jwt-decode 4
- clsx 2
- @fontsource-variable/manrope
- @fontsource/space-grotesk

### 11.1 Estratégia de Build

- desenvolvimento com Vite.
- build de produção com tsc -b && vite build.
- code splitting por lazy loading nas rotas principais.

Ambiente local observado hoje:

- Vite roda na porta 5173.
- proxy local de /api aponta por padrão para https://localhost:7110.
- esse alvo pode ser alterado por VITE_PROXY_TARGET.
- o cliente HTTP usa VITE_API_BASE_URL quando configurado; se vazio, usa chamadas relativas.

Gerenciamento de dependências e ambiente:

- existe package-lock.json no frontend e ele deve ser tratado como referência primária das versões instaladas.
- o fluxo padrão atual usa npm.
- Node.js 20+ é compatível com a execução atual documentada no projeto.

### 11.2 Estratégia de Autenticação no Front

- armazenamento de JWT no cliente para suportar a SPA.
- refresh usando o endpoint existente /api/v1/auth/refresh.
- leitura de claims do token para montar contexto de sessão.
- fallback de logout em qualquer 401 retornado pelo backend.
- após login, cadastro ou refresh, a chamada a /api/v1/users/me deve usar explicitamente o token mais recente quando necessário, em vez de depender apenas da persistência assíncrona do store.

### 11.3 Estratégia de Consumo HTTP

- Axios centralizado em frontend/src/api/http.ts.
- baseURL configurável por variável de ambiente.
- proxy do Vite para /api no ambiente local.
- TanStack Query para cache, refetch e invalidação.

Variáveis de ambiente relevantes:

- VITE_API_BASE_URL
- VITE_PROXY_TARGET

Valor padrão de referência:

- VITE_PROXY_TARGET=https://localhost:7110

### 11.4 Bibliotecas Permitidas no Front

UI e experiência:

- Tailwind CSS
- Lucide React
- @fontsource-variable/manrope
- @fontsource/space-grotesk
- clsx

Estado e dados:

- TanStack Query
- Zustand
- Axios
- jwt-decode

Formulários e validação:

- React Hook Form
- Zod
- @hookform/resolvers

### 11.5 Restrições para o Front

Para manter coerência com o projeto atual, evitar:

- Next.js ou SSR por padrão.
- Redux ou camadas de estado mais pesadas sem necessidade.
- bibliotecas de UI grandes que descaracterizem o produto.
- consumo de endpoints inexistentes.
- mock paralelo de API no front como fonte de verdade.

Dependências de ambiente que devem ser mantidas explícitas na reconstrução:

- Node.js em versão compatível com Vite 6 e TypeScript 5.
- gerenciador de pacote compatível com o lockfile presente no projeto.
- backend .NET acessível no endereço configurado para proxy ou baseURL.

Estado atual de testes do front:

- não há stack de testes de front documentada como parte obrigatória do produto atual.
- a reprodução fiel do sistema não depende de introduzir Vitest, Jest ou Cypress se isso não existir no código reconstruído.

## 12. Separação Técnica Obrigatória no Front-end

Como o front oficial usa React + TypeScript, a separação entre estilo, lógica e estrutura visual deve ser aplicada respeitando a stack existente, sem substituí-la.

### 12.1 Papel de cada tipo de arquivo

- `.ts`
	- lógica pura
	- handlers reutilizáveis
	- estado derivado
	- hooks
	- services
	- validação e transformação de dados
- `.tsx`
	- camada de view/template
	- estrutura visual
	- composição de componentes
	- binding entre props e renderização
- `.css`
	- camada de estilo dedicada
	- regras visuais desacopladas da lógica
	- estilos específicos de componente, feature ou view

### 12.2 Regra oficial para React no projeto

Em React, `.tsx` é aceito como camada de apresentação. Isso não autoriza usar o mesmo arquivo para concentrar indiscriminadamente:

- regra de domínio
- handlers extensos
- integração de dados
- validação extensa
- estilo acoplado

Quando a complexidade crescer, a organização correta é separar:

- view em `.tsx`
- lógica em `.ts`
- estilo em `.css`

### 12.3 Padrões aceitos para feature ou componente

Exemplo de organização recomendada:

- `ComponentName.view.tsx`
- `ComponentName.logic.ts`
- `ComponentName.css`

Exemplo equivalente por feature:

- `reservation-details.view.tsx`
- `reservation-details.logic.ts`
- `reservation-details.css`

### 12.4 Relação com Tailwind CSS

O projeto usa Tailwind CSS 3. Isso continua válido.

Mesmo assim, a documentação oficial passa a exigir que a arquitetura do front trate estilo como responsabilidade separada. Portanto:

- Tailwind não deve justificar mistura de regra de negócio com markup
- quando houver estilo específico, ele deve ser organizado em arquivo de estilo dedicado ou camada de estilo explicitamente isolada
- componentes visuais não devem acumular lógica de domínio só porque usam classes utilitárias inline

### 12.5 Resumo operacional para futuras IAs

Para reconstruir ou refatorar o front oficial deste projeto:

- manter React + TypeScript + Vite
- usar `.tsx` como camada visual
- usar `.ts` como camada de lógica
- usar `.css` como camada de estilo dedicada
- evitar concentrar estilo, regra e markup de forma desorganizada em um único arquivo

Comandos operacionais de referência:

- backend restore: dotnet restore.
- backend build: dotnet build.
- backend run: dotnet run.
- backend tests: dotnet test.
- frontend install: npm install.
- frontend dev: npm run dev.
- frontend build: npm run build.
