# TECH STACK

Este documento lista a stack efetivamente usada hoje no projeto. Use-o como referência para reproduzir a aplicação sem trocar tecnologia ou introduzir componentes que não existem.

## 1. Runtime e Framework

- Linguagem: C#
- Target framework: net8.0
- Tipo de projeto: ASP.NET Core Web API
- Formato: SDK-style csproj

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

### 11.1 Estratégia de Build

- desenvolvimento com Vite.
- build de produção com tsc -b && vite build.
- code splitting por lazy loading nas rotas principais.

### 11.2 Estratégia de Autenticação no Front

- armazenamento de JWT no cliente para suportar a SPA.
- refresh usando o endpoint existente /api/v1/auth/refresh.
- leitura de claims do token para montar contexto de sessão.
- fallback de logout em qualquer 401 retornado pelo backend.

### 11.3 Estratégia de Consumo HTTP

- Axios centralizado em frontend/src/api/http.ts.
- baseURL configurável por variável de ambiente.
- proxy do Vite para /api no ambiente local.
- TanStack Query para cache, refetch e invalidação.

### 11.4 Bibliotecas Permitidas no Front

UI e experiência:

- Tailwind CSS
- Lucide React
- @fontsource-variable/manrope
- @fontsource/space-grotesk

Estado e dados:

- TanStack Query
- Zustand
- Axios

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
