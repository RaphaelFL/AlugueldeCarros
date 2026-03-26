# TECH STACK

Este documento lista a stack efetivamente usada hoje no projeto. Use-o como referência para reproduzir a aplicação sem trocar tecnologia ou introduzir componentes que não existem.

## 1. Runtime e Framework

- Linguagem: C#
- Target framework: net8.0
- Tipo de projeto: ASP.NET Core Web API
- Formato: SDK-style csproj

Objetivo deste arquivo:

- registrar a stack efetivamente instalada e usada.
- evitar substituição automática por tecnologias equivalentes.
- servir como referência operacional de ambiente para reconstrução.

## 2. Pacotes do Projeto Principal

Pacotes confirmados no csproj atual:

- Microsoft.AspNetCore.Authentication.JwtBearer 8.0.25
- Swashbuckle.AspNetCore 6.6.2
- System.IdentityModel.Tokens.Jwt 8.17.0
- BCrypt.Net-Next 4.1.0

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
- JsonDataLoader para carregar dados no startup.
- repositórios singleton em memória usando List<T>.

Consequência prática:

- alterações feitas durante a execução não são persistidas de volta nos arquivos.

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
- JwtTokenService: singleton
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
- controllers injetam classes concretas de service.

## 10. Restrições para Reconstrução

Para manter equivalência com o projeto atual, não troque automaticamente por:

- Clean Architecture completa.
- EF Core.
- banco relacional.
- filas.
- serviços externos.
- autenticação com Identity.

## 11. Stack Oficial do Front-end

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

Comandos operacionais de referência:

- backend restore: dotnet restore.
- backend build: dotnet build.
- backend run: dotnet run.
- backend tests: dotnet test.
- frontend install: npm install.
- frontend dev: npm run dev.
- frontend build: npm run build.
