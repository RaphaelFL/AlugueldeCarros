# Aluguel de Carros — API REST

Plataforma de aluguel de veículos baseada em reservas por categoria, com autenticação JWT, gestão de frota distribuída por branches e processamento de pagamentos simplificado.

**Status**: Phase 1 (MVP com dados mockados) | **Versão API**: v1 | **Stack**: .NET 8.0 + ASP.NET Core

---

## 🎯 Visão Geral

**O que é?**
Sistema de gerenciamento de aluguel de veículos que permite clientes buscar, reservar e pagar pelo aluguel de carros, enquanto administradores gerenciam frota, categorias e preços.

**Por que?**
Validar o conceito de aluguel de carros como serviço via API, com suporte a múltiplos branches e categorias de veículos, antes de implementar persistência real em banco de dados.

**Para quem?**
- **Clientes:** Buscar e reservar carros pela web
- **Administradores:** Gerenciar veículos, categorias e preços através da API

**Quando?**
Phase 1 (MVP) — Dados em JSON, sem banco real. Phase 2+ → SQL + Entity Framework.

---

## Tecnologias

| Tecnologia | Versão |
|---|---|
| .NET SDK | 8.0 |
| ASP.NET Core Web API | 8.0 |
| JWT (Microsoft.AspNetCore.Authentication.JwtBearer) | 8.0.25 |
| System.IdentityModel.Tokens.Jwt | 8.17.0 |
| Swashbuckle.AspNetCore (Swagger) | 6.6.2 |

---

## Estrutura de Pastas

```
AlugueldeCarros/
├── Controllers/            # Endpoints da API (9 controllers)
│   ├── AuthController.cs           # /api/v1/auth/*
│   ├── UserController.cs           # /api/v1/users/*
│   ├── AdminUsersController.cs     # /api/v1/admin/users/*
│   ├── BranchesController.cs       # /api/v1/branches
│   ├── VehiclesController.cs       # /api/v1/vehicles/*
│   ├── AdminVehiclesController.cs  # /api/v1/admin/vehicles/*
│   ├── PricingController.cs        # /api/v1/pricing/rules/*
│   ├── ReservationsController.cs   # /api/v1/reservations/*
│   └── PaymentsController.cs       # /api/v1/payments/*
├── Services/               # Lógica de negócio (8 services)
│   ├── AuthService.cs
│   ├── UserService.cs
│   ├── BranchService.cs
│   ├── VehicleService.cs
│   ├── VehicleCategoryService.cs
│   ├── PricingService.cs
│   ├── ReservationService.cs
│   └── PaymentService.cs
├── Repositories/           # Acesso a dados em memória (8 repositórios)
│   ├── IUserRepository.cs          + InMemoryUserRepository
│   ├── IVehicleRepository.cs       + InMemoryVehicleRepository
│   ├── IVehicleCategoryRepository.cs + InMemoryVehicleCategoryRepository
│   ├── IBranchRepository.cs        + InMemoryBranchRepository
│   ├── IPricingRuleRepository.cs   + InMemoryPricingRuleRepository
│   ├── IReservationRepository.cs   + InMemoryReservationRepository
│   ├── IPaymentRepository.cs       + InMemoryPaymentRepository
│   └── IRoleRepository.cs          + InMemoryRoleRepository
├── Domain/
│   ├── Entities/           # Modelos de domínio
│   │   ├── User.cs, Role.cs, UserRole.cs, CustomerProfile.cs
│   │   ├── Vehicle.cs, VehicleCategory.cs, Branch.cs
│   │   ├── Reservation.cs, Payment.cs, PricingRule.cs
│   └── Enums/
│       ├── ReservationStatus.cs    # PENDING_PAYMENT, CONFIRMED, CANCELLED, EXPIRED
│       ├── PaymentStatus.cs        # PENDING, APPROVED, DECLINED, REFUNDED
│       ├── VehicleStatus.cs        # AVAILABLE, RESERVED, RENTED, MAINTENANCE, BLOCKED
│       └── UserRoleType.cs
├── DTOs/
│   ├── Auth/               # LoginRequest, RegisterRequest, AuthResponse
│   ├── Users/              # AddUserRolesRequest
│   ├── Vehicles/           # VehicleDto, CreateVehicleRequest, UpdateVehicleRequest
│   ├── Reservations/       # CreateReservationRequest, UpdateReservationRequest
│   └── Payments/           # PreauthRequest, CaptureRequest, RefundRequest
├── Security/               # JwtTokenService, PasswordHasher (SHA256)
├── Configurations/         # JwtSettings
├── Middleware/              # ExceptionHandlingMiddleware
├── Loaders/                # JsonDataLoader (carrega mocks na inicialização)
├── Resources/MockData/     # Arquivos JSON com dados de teste
│   ├── users.json, roles.json, user-roles.json
│   ├── branches.json, vehicles.json, vehicle-categories.json
│   ├── pricing-rules.json, reservations.json, payments.json
├── Program.cs              # Configuração DI, JWT, Swagger, pipeline
└── appsettings.json        # Configurações gerais e JWT
```

---

## Endpoints da API

### Autenticação e Usuários

| Método | Rota | Descrição | Auth |
|--------|------|-----------|------|
| POST | `/api/v1/auth/register` | Cadastro de novo usuário | Público |
| POST | `/api/v1/auth/login` | Login (retorna JWT) | Público |
| POST | `/api/v1/auth/refresh` | Renovar token JWT | Público |
| GET | `/api/v1/users/me` | Perfil do usuário autenticado | Bearer |
| GET | `/api/v1/users/me/reservations` | Reservas do usuário autenticado | Bearer |
| GET | `/api/v1/admin/users` | Listar todos os usuários | Admin |
| POST | `/api/v1/admin/users/{id}/roles` | Atribuir roles a um usuário | Admin |

### Catálogo / Frota

| Método | Rota | Descrição | Auth |
|--------|------|-----------|------|
| GET | `/api/v1/branches` | Listar filiais | Público |
| GET | `/api/v1/vehicles/categories` | Listar categorias de veículos | Público |
| GET | `/api/v1/vehicles/search` | Buscar veículos disponíveis | Público |
| GET | `/api/v1/vehicles/{id}` | Detalhes de um veículo | Público |
| POST | `/api/v1/admin/vehicles` | Cadastrar novo veículo | Admin |
| PATCH | `/api/v1/admin/vehicles/{id}` | Atualizar veículo | Admin |

**Parâmetros de busca** (`/vehicles/search`):
- `branchId` — filial
- `from` / `to` (ou `startDate` / `endDate`) — período
- `categoryId` — categoria
- `priceMin` / `priceMax` — faixa de preço

### Preços

| Método | Rota | Descrição | Auth |
|--------|------|-----------|------|
| GET | `/api/v1/pricing/rules` | Listar regras de preço | Público |
| GET | `/api/v1/pricing/rules/{id}` | Detalhe de uma regra | Público |
| POST | `/api/v1/pricing/rules` | Criar regra de preço | Admin |
| PATCH | `/api/v1/pricing/rules/{id}` | Atualizar regra de preço | Admin |

### Reservas

| Método | Rota | Descrição | Auth |
|--------|------|-----------|------|
| POST | `/api/v1/reservations` | Criar reserva | Bearer |
| GET | `/api/v1/reservations/{id}` | Detalhe da reserva | Bearer (dono ou Admin) |
| PATCH | `/api/v1/reservations/{id}` | Atualizar reserva | Bearer (dono ou Admin) |
| POST | `/api/v1/reservations/{id}/cancel` | Cancelar reserva | Bearer (dono ou Admin) |

### Pagamentos

| Método | Rota | Descrição | Auth |
|--------|------|-----------|------|
| POST | `/api/v1/payments/preauth` | Pré-autorizar pagamento | Bearer (dono ou Admin) |
| POST | `/api/v1/payments/capture` | Capturar pagamento | Bearer (dono ou Admin) |
| POST | `/api/v1/payments/refund` | Reembolsar pagamento | Bearer (dono ou Admin) |
| GET | `/api/v1/payments/{id}` | Detalhe do pagamento | Bearer (dono ou Admin) |

---

## Segurança

### Autenticação JWT
- Tokens gerados no login/registro com expiração de **60 minutos**.
- Hash de senha: **SHA256** (Base64).
- Configurações em `appsettings.json` → seção `JwtSettings`.

### Autorização (RBAC)

| Role | Permissões |
|------|------------|
| **Customer** | Operações pessoais: perfil, reservas próprias, pagamentos próprios |
| **Admin** | Tudo acima + gestão de usuários, veículos, categorias, preços |

- Endpoints administrativos protegidos por `[Authorize(Roles = "Admin")]`.
- Endpoints de usuário protegidos por `[Authorize]` com validação de ownership.
- Pagamentos validam que o usuário autenticado é dono da reserva associada (ou Admin).

---

## Dados Mockados

Dados de teste em `Resources/MockData/` carregados na inicialização via `JsonDataLoader`.

### Credenciais de Teste

| Email | Senha | Role |
|-------|-------|------|
| `customer@example.com` | `123456` | Customer |
| `admin@aluguel.com` | `admin123` | Admin |

### Arquivos Mock

| Arquivo | Conteúdo |
|---------|----------|
| `users.json` | 3 usuários |
| `roles.json` | Customer, Admin |
| `user-roles.json` | Mapeamento usuário→role |
| `branches.json` | 1 filial (Filial Centro) |
| `vehicle-categories.json` | 2 categorias (Econômico, SUV) |
| `vehicles.json` | 2 veículos (Fiat Uno, Toyota Corolla) |
| `pricing-rules.json` | Regras de preço por categoria |
| `reservations.json` | 3 reservas (IDs 1, 2, 10) |
| `payments.json` | 3 pagamentos (IDs 1, 2, 10) |

> Alterações feitas via API ficam apenas em memória e são perdidas ao reiniciar (exceto `users.json` que é persistido).

---

## Execução

### Pré-requisitos
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Comandos

```bash
cd AlugueldeCarros
dotnet restore
dotnet build
dotnet run
```

### URLs

| Recurso | URL |
|---------|-----|
| API (HTTPS) | `https://localhost:7110` |
| API (HTTP) | `http://localhost:5097` |
| Swagger UI | `https://localhost:7110/swagger` |

---

## Fluxo de Teste Completo

1. **Login** → `POST /api/v1/auth/login` com credenciais mock
2. **Copiar token** retornado no campo `token`
3. **Authorize no Swagger** → clicar em "Authorize" e colar: `Bearer SEU_TOKEN`
4. **Buscar veículos** → `GET /api/v1/vehicles/search?categoryId=1`
5. **Criar reserva** → `POST /api/v1/reservations`
6. **Pré-autorizar pagamento** → `POST /api/v1/payments/preauth`
7. **Capturar pagamento** → `POST /api/v1/payments/capture`

Para rotas **Admin**, faça login com `admin@aluguel.com` / `admin123`.

---

## Arquitetura

```
Request → Controller → Service → Repository (InMemory) → JSON MockData
                ↓
          DTOs (Request/Response)
                ↓
          Domain Entities + Enums
```

- **Controllers**: recebem HTTP, validam input, delegam para Services.
- **Services**: lógica de negócio, validações de domínio.
- **Repositories**: interface + implementação in-memory com dados carregados de JSON.
- **Middleware**: tratamento global de exceções (`ExceptionHandlingMiddleware`).
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