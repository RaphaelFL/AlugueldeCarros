# Aluguel de Carros

Plataforma de aluguel de veículos com backend ASP.NET Core e front-end React oficial na pasta frontend.

Estado atual do projeto:

- API REST em .NET 8.
- SPA React em TypeScript com Vite.
- autenticação JWT.
- RBAC com Customer e Admin.
- dados mockados em JSON com operação principal em memória.

## Visão Geral

O produto cobre hoje:

- autenticação e renovação de sessão.
- catálogo público de veículos.
- criação de reserva por categoria.
- fluxo de pagamento com preauth, capture e refund.
- área autenticada do cliente.
- área administrativa para usuários, veículos e pricing rules.

Limites importantes:

- não existe banco relacional.
- não existe listagem completa de pagamentos por reserva ou por usuário.
- não existe módulo de check-in, check-out, promoções ou relatórios complexos.

## Estrutura do Repositório

```text
.
├── .ai/                       # contexto oficial do projeto
├── AlugueldeCarros/           # backend ASP.NET Core Web API
├── AlugueldeCarros.Tests/     # testes do backend
├── frontend/                  # aplicação React com Vite, src e configs
└── README.md
```

## Stack

### Backend

- .NET 8
- ASP.NET Core Web API
- Microsoft.AspNetCore.Authentication.JwtBearer 8.0.25
- Swashbuckle.AspNetCore 6.6.2
- System.IdentityModel.Tokens.Jwt 8.17.0
- BCrypt.Net-Next 4.1.0

### Front-end

- React 19
- TypeScript 5
- Vite 6
- React Router 7
- TanStack Query 5
- Zustand 5
- React Hook Form 7
- Zod 3
- Tailwind CSS 3
- Axios 1

## Endpoints Reais da API

### Auth

- POST /api/v1/auth/register
- POST /api/v1/auth/login
- POST /api/v1/auth/refresh

### Usuário

- GET /api/v1/users/me
- GET /api/v1/users/me/reservations
- GET /api/v1/admin/users
- POST /api/v1/admin/users/{id}/roles

### Catálogo e frota

- GET /api/v1/branches
- GET /api/v1/vehicles/categories
- GET /api/v1/vehicles/search
- GET /api/v1/vehicles/{id}
- POST /api/v1/admin/vehicles
- PATCH /api/v1/admin/vehicles/{id}

### Pricing

- GET /api/v1/pricing/rules
- GET /api/v1/pricing/rules/{id}
- POST /api/v1/pricing/rules
- PATCH /api/v1/pricing/rules/{id}

### Reservas

- POST /api/v1/reservations
- GET /api/v1/reservations/{id}
- PATCH /api/v1/reservations/{id}
- POST /api/v1/reservations/{id}/cancel

### Pagamentos

- POST /api/v1/payments/preauth
- POST /api/v1/payments/capture
- POST /api/v1/payments/refund
- GET /api/v1/payments/{id}

## Regras Técnicas e de Negócio Relevantes

- reserva é criada por categoria.
- o front usa o veículo encontrado como contexto visual, não como contrato de criação.
- reservas começam em PENDING_PAYMENT.
- capture aprovada pode confirmar a reserva e reservar o veículo.
- roles reais: Customer e Admin.
- autenticação é por JWT Bearer.
- senhas novas usam BCrypt; o login atual ainda aceita fallback em texto puro por compatibilidade com dados mockados existentes.

## Front-end Oficial

### Áreas disponíveis

Públicas:

- /
- /login
- /catalogo
- /catalogo/:vehicleId

Customer:

- /app/dashboard
- /app/profile
- /app/reservas
- /app/reservas/:reservationId
- /app/reservas/:reservationId/pagamento

Admin:

- /admin/dashboard
- /admin/users
- /admin/vehicles
- /admin/vehicles/new
- /admin/vehicles/:vehicleId
- /admin/pricing

### Estratégia de autenticação no front

- JWT persistido no cliente.
- refresh via /api/v1/auth/refresh quando o token se aproxima da expiração.
- logout automático ao receber 401 da API.
- menus e rotas protegidos por role.

### Estratégia de integração

- Axios centralizado em frontend/src/api/http.ts.
- TanStack Query para queries e mutations.
- proxy do Vite para /api em desenvolvimento, apontando por padrão para http://localhost:5097.

## Como rodar

### Pré-requisitos

- .NET 8 SDK
- Node.js 20+
- npm 10+

### Backend

```bash
cd AlugueldeCarros
dotnet restore
dotnet build
dotnet run
```

Backend local:

- http://localhost:5097
- https://localhost:7110
- Swagger: https://localhost:7110/swagger

### Front-end

Na pasta do front:

```bash
cd frontend
npm install
npm run dev
```

Front local:

- http://localhost:5173

### Build do front

```bash
cd frontend
npm run build
```

## Variáveis de Ambiente do Front

Arquivo de referência:

- frontend/.env.example

Variáveis:

- VITE_API_BASE_URL: base absoluta da API quando necessário.
- VITE_PROXY_TARGET: alvo do proxy local do Vite. Padrão: http://localhost:5097.

## Credenciais de Teste

- customer@example.com / 123456
- admin@aluguel.com / admin123

## Dados Mockados

Arquivos em AlugueldeCarros/Resources/MockData:

- users.json
- roles.json
- user-roles.json
- branches.json
- vehicle-categories.json
- vehicles.json
- pricing-rules.json
- reservations.json
- payments.json

Observação:

- a maior parte das mudanças permanece em memória durante a execução.
- users.json é persistido pelo repositório de usuários.

## UX por Perfil

Customer:

- consulta catálogo.
- cria reserva.
- acompanha status.
- executa pagamento.
- remarca ou cancela quando permitido.

Admin:

- acompanha visão operacional.
- atribui roles.
- cria e edita veículos.
- cria e edita pricing rules.

## Validação Atual

Validações já executadas no projeto:

- suíte do backend aprovada anteriormente.
- build do front executado com sucesso via npm run build.

## Documentação de Contexto

Consulte a pasta .ai para reconstrução fiel do sistema:

- .ai/architecture.md
- .ai/business-rules.md
- .ai/standards.md
- .ai/tech-stack.md
- **DI**: todos os repositórios registrados como `Singleton`, services como `Scoped`.

---

## Troubleshooting

| Problema | Solução |
|----------|---------|
| Erro 401 no Swagger | Faça login primeiro e use "Authorize" com `Bearer TOKEN` |
| Build falha | Verifique se .NET 8 SDK está instalado (`dotnet --version`) |
| Dados não carregam | Confirme que `Resources/MockData/*.json` existem e têm conteúdo válido |
| Token expirado | Faça login novamente ou use `POST /api/v1/auth/refresh` |
| 403 Forbidden | Verifique se o usuário tem a role necessária (Customer vs Admin) |

---

## Testes

### Estrutura

```
tests/AlugueldeCarros.Tests/
├── Unit/
│   ├── Services/          (8 classes de teste)
│   └── Security/          (JWT + RBAC)
├── Integration/
│   ├── Controllers/       (9 controllers)
│   └── Endpoints/
└── Fixtures/
    ├── TestDataBuilder.cs
    ├── JwtTokenFixture.cs
    └── WebApplicationFactoryFixture.cs
```

### Stack

- **xUnit** — Framework de testes
- **FluentAssertions** — Assertions legíveis
- **Moq** — Mocking de dependências
- **WebApplicationFactory** — Testes HTTP

### Executar Testes

```bash
# Todos os testes
dotnet test tests/AlugueldeCarros.Tests.csproj

# Teste específico
dotnet test --filter "ClassName=UserServiceTests"

# Watch mode
dotnet test --watch
```

### Cobertura

- **Services**: 80%+ (lógica crítica)
- **Controllers**: 70%+ (HTTP + autenticação)
- **Security**: 90%+ (JWT, RBAC)
- **Repositories**: 60%+ (mock data)
- **DTOs/Enums**: 0% (não testam estrutura)

### Padrão AAA

```csharp
[Fact]
public async Task LoginUser_ValidCredentials_ReturnsJwtToken()
{
    // Arrange
    var request = new LoginRequest { Email = "user@test.com", Password = "Test@123" };

    // Act
    var result = await _authService.LoginAsync(request);

    // Assert
    result.Should().NotBeNull();
    result.Token.Should().NotBeEmpty();
}
```

### Sem CI/CD

Phase 1 = Testes locais apenas (`dotnet test`)  
Phase 2+ = GitHub Actions (futuro)