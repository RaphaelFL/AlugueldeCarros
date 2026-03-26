# ARCHITECTURE — Visão Arquitetural e Decisões Técnicas

**Este documento define as decisões, padrões e responsabilidades arquiteturais do projeto.**

---

## 1. VISÃO ARQUITETURAL

### 1.1 Diagrama de Camadas

```
┌─────────────────────────────────────────────────────────┐
│                    HTTP CLIENTS                          │
│          (Web Browser, Mobile App, API Client)           │
└─────────────────┬───────────────────────────────────────┘
                  │
┌─────────────────▼───────────────────────────────────────┐
│                  CONTROLLERS (HTTP)                      │
│  [AuthController] [UserController] [VehiclesController] │
│  [AdminUsersController] [ReservationsController] ...     │
│  ▲ Responsibility: Route & Authorize                     │
└─────────────────┬───────────────────────────────────────┘
                  │
┌─────────────────▼───────────────────────────────────────┐
│            MIDDLEWARE (Cross-cutting)                    │
│  [ExceptionHandlingMiddleware] [AuthMiddleware]          │
│  ▲ Responsibility: Intercept & Handle                    │
└─────────────────┬───────────────────────────────────────┘
                  │
┌─────────────────▼───────────────────────────────────────┐
│              SERVICES (Business Logic)                   │
│  [UserService] [ReservationService] [PaymentService]     │
│  [VehicleService] [PricingService] [AuthService] ...     │
│  ▲ Responsibility: Orchestrate & Validate                │
└─────────────────┬───────────────────────────────────────┘
                  │
┌─────────────────▼───────────────────────────────────────┐
│         REPOSITORIES (Data Abstraction)                  │
│  [IUserRepository] [IVehicleRepository]                  │
│  [IReservationRepository] [IPaymentRepository] ...       │
│  ▲ Responsibility: Persist & Query                       │
└─────────────────┬───────────────────────────────────────┘
                  │
┌─────────────────▼───────────────────────────────────────┐
│           DATA LAYER (JSON Mock - Phase 1)              │
│  resources/MockData/                                     │
│  [users.json] [vehicles.json] [reservations.json] ...   │
│  ▲ Implemented via List<T> in memory                     │
│                                                           │
│  └─→ Future: Database (SQL Server, PostgreSQL)          │
│      Implemented via Entity Framework                    │
└─────────────────────────────────────────────────────────┘
```

### 1.2 Fluxo de Requisição

```
1. Client Request
   ↓
2. HTTP Middleware Chain
   ├─ Authentication
   ├─ Authorization
   ├─ ExceptionHandling
   ↓
3. Controller (Route → Action)
   ├─ Validate Request (ModelState)
   ├─ Extract Claims (UserId, Roles)
   ├─ Call Service
   ↓
4. Service (Business Logic)
   ├─ Validate Business Rules
   ├─ Call Repository(ies)
   ├─ Compose Response
   ↓
5. Repository (Data Access)
   ├─ Query JSON/Database
   ├─ Transform to Domain Entity
   ↓
6. Response Assembly
   ├─ DTO Mapping
   ├─ Status Code Selection
   ↓
7. Response to Client
   └─ JSON + Headers
```

---

## 2. RESPONSABILIDADES POR CAMADA

### 2.1 Controllers

```
ENTRADA:
- Receber requisição HTTP
- Extrair claims do JWT
- Validar ModelState (DataAnnotations)

PROCESSAMENTO:
- Chamar serviço correspondente
- Mapear DTO request → request object
- Delegar lógica para service

SAÍDA:
- Transformar resultado em DTO response
- Selecionar status HTTP apropriado
- Retornar ProblemDetails ou dados

NÃO FAZER:
- Lógica de negócio complexa
- Acessar repositories diretamente
- Queries que envolvem múltiplas tabelas
```

### 2.2 Services

```
ENTRADA:
- Receber objetos de domínio e DTOs
- Extrair informações contextuais (userId do JWT)

PROCESSAMENTO:
- Validar regras de negócio (conforme business-rules.md)
- Orquestrar múltiplos repositories
- Calcular valores (preço, disponibilidade)
- Aplicar transformações de domínio
- Logging de operações importantes

SAÍDA:
- Retornar entidades de domínio
- Lançar exceções customizadas
- Aceitar objetos de dados

NÃO FAZER:
- Acessar HTTP diretamente
- Retornar JSON strings
- Criar connections a banco
- Fazer I/O de rede (simular com mock)
```

### 2.3 Repositories

```
ENTRADA:
- Métodos de query simples (GetById, GetAll)
- Métodos de mutação (Create, Update, Delete)

PROCESSAMENTO:
- Carregar dados JSON / executar queries
- Transformar raw data → domain entities
- Aplicar paginação/filtering se necessário

SAÍDA:
- Retornar entidades de domínio
- Nunca retornar null em lista (retornar vazio)
- Lançar NotFoundException em GET por ID se não existe

NÃO FAZER:
- Lógica de negócio
- Validações de regras
- Múltiplas operações atômicas (usar serviço)
- Acessar HTTP ou externos
```

### 2.4 Segurança (JWT, RBAC)

```
PONTO DE ENTRADA:
- Program.cs: AddAuthentication().AddJwtBearer()

VALIDAÇÃO:
- Controller: [Authorize] ou [Authorize(Roles = "...")]
- Middleware: ExceptionHandlingMiddleware captura erros

EXTRAÇÃO:
- User.FindFirst("sub")?.Value  → userId
- User.FindAll(ClaimTypes.Role)  → roles

PROTEÇÃO:
- Todo endpoint protegido usa [Authorize]
- Todo endpoint admin usa [Authorize(Roles = "ADMIN")]
- Públicos explícitos com [AllowAnonymous]
```

---

## 3. DECISÕES DE ALTO NÍVEL

### 3.1 Por Que JSON Mockado Nesta Fase?

```
RAZÃO 1: MVP Rápido
- Evita overhead de DB setup
- Permite iterar rapidamente no API design

RAZÃO 2: Validação de Conceito
- Testa fluxos e regras sem dependência de DBA
- Permite ajustes sem migrations

RAZÃO 3: Testabilidade
- Dados em memória = testes determinísticos
- Sem flakiness de conexão DB

RAZÃO 4: Escalabilidade de Desenvolvimento
- Cada dev roda projeto localmente
- Sem compartilhar DB dev environment
```

### 3.2 Estratégia Futura de Troca para Persistência Real

```
PASSO 1: Preparação Atual (Phase 1 - Agora)
- Repositories são interfaces agnósticas
- Implementação via List<T> em memória
- JsonDataLoader carrega dados iniciais

PASSO 2: Entity Framework Setup (Phase 2 - Futuro)
- Adicionar DbContext
- Criar Entities ≈ Domain Classes (pode herdar ou mapear)
- Criar Migrations
- Implementar EF repositories

PASSO 3: Migração sem Breaking Changes
- Trocar JsonRepository por EFRepository em DI
- Controllers e Services NUNCA mudam
- DTOs permanecem iguais
- API versions disponíveis em paralelo se necessário

CÓDIGO DE HOJE → COMPATÍVEL COM FASE 2:
{
  Services: Implementados com interface IRepository
  Repositories: Interface agnóstica, implementação em memória
  Controllers: Sem conhecimento de implementação
  DTOs: Independente de persistência
}

LOGO:
{
  Repositories: Trocar implementação (memória → EF)
  Tudo mais: Sem mudança
}
```

### 3.3 Esta é uma Camada Agnóstica por Projeto

```
Em cada domínio, a implementação do Repository é agnóstica:

HOJE (JSON):
public class JsonUserRepository : IUserRepository
{
    private readonly List<User> _users;
    public Task<User?> GetByIdAsync(Guid id) { ... }
}

FUTURO (EF):
public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    public Task<User?> GetByIdAsync(Guid id) 
        => _context.Users.FindAsync(id);
}

A LÓGICA TODA PERMANECE:
- Controllers chamam Services
- Services chamam IUserRepository
- Implementation troca, contrato permanece
```

---

## 4. CONTEXTO DE AUTENTICAÇÃO JWT

### 4.1 Fluxo

```
1. User: POST /api/v1/auth/login
   Body: { email, password }
   ↓
2. AuthService.AuthenticateAsync()
   ├─ Busca user por email
   ├─ Valida password (PasswordHasher.Verify)
   ├─ Gera JWT token
   ↓
3. Response: { token: "eyJhbGc..." }
   ↓
4. Client: Próximas requisições
   Header: Authorization: Bearer eyJhbGc...
   ↓
5. JWT Middleware
   ├─ Extract token do header
   ├─ Valida assinatura
   ├─ Lê claims
   ├─ Set HttpContext.User
   ↓
6. Controller: [Authorize]
   ├─ User.FindFirst("sub") → userId
   ├─ User.IsInRole("ADMIN") → false/true
```

### 4.2 Token Payload

```json
{
  "sub": "{{userId}}",
  "email": "user@example.com",
  "roles": ["USER"],
  "iat": 1711353600,
  "exp": 1711357200,
  "iss": "AlugueldeCarros"
}
```

### 4.3 Refresh Token (Futuro)

```
HOJE: Apenas access token, válido por 1 hora

FUTURO:
POST /api/v1/auth/refresh
Body: { refreshToken }
Response: { accessToken, refreshToken }

Implementação: Não incluir Phase 1
```

---

## 5. CONTEXTO DE AUTORIZAÇÃO COM RBAC

### 5.1 Fluxo

```
Token JWT contém: roles: ["USER", "ADMIN"]
           ↓
Controller: [Authorize(Roles = "ADMIN")]
           ↓
ASP.NET Core verifica
User.IsInRole("ADMIN")
           ↓
✅ true  → prossegue para ação
❌ false → retorna 403 Forbidden
```

### 5.2 Papéis Suportados

```
ADMIN:
- Acesso a /admin/**
- Gerenciar frota completa
- Gerenciar usuários e roles
- Ver todas as reservas

USER:
- Acesso a /api/v1/** (exceto /admin)
- Próprias reservas
- Próprio perfil
- Buscar veículos públicos

GUEST: (Futuro)
- Apenas leitura pública
```

### 5.3 Middleware de Autorização

```csharp
// Validação automática via [Authorize(Roles = "ADMIN")]
[HttpPost("users/{id}/roles")]
[Authorize(Roles = "ADMIN")]
public async Task<IActionResult> AddRole(Guid id, [FromBody] AddUserRolesRequest request)
{
    // Se chegar aqui, User.IsInRole("ADMIN") = true
    await _userService.AddRoleAsync(id, request.RoleId);
    return NoContent();
}

// Sem [Authorize]: público
[HttpGet("categories")]
public async Task<ActionResult<IEnumerable<VehicleCategoryDto>>> GetCategories()
{
    // Pode ser acessado sem token
}
```

---

## 6. CONTEXTO POR MÓDULO

### 6.1 AUTH Module

```
Controllers:
  - AuthController
    POST /api/v1/auth/register
    POST /api/v1/auth/login
    POST /api/v1/auth/refresh  (futuro)

Services:
  - AuthService
    public async Task<AuthResponse> RegisterAsync(RegisterRequest)
    public async Task<AuthResponse> LoginAsync(LoginRequest)

Security:
  - JwtTokenService (gera token)
  - PasswordHasher (hash/verify)

Repositories:
  - IUserRepository

DTOs:
  - RegisterRequest
  - LoginRequest
  - AuthResponse
  - RefreshTokenRequest

Responsibilidades:
  ✓ Criar user com senha hasheada
  ✓ Autenticar validando password
  ✓ Gerar JWT token
  ✓ Validar formato de entrada
```

### 6.2 USERS Module

```
Controllers:
  - UserController
    GET /api/v1/users/me
  - AdminUsersController
    GET /api/v1/admin/users
    POST /api/v1/admin/users/{id}/roles

Services:
  - UserService
    public async Task<UserDto> GetUserByIdAsync(Guid)
    public async Task<IEnumerable<UserDto>> GetAllAsync()
    public async Task AddRoleAsync(Guid userId, Guid roleId)

Repositories:
  - IUserRepository
  - IRoleRepository
  - IUserRoleRepository

DTOs:
  - UserDto
  - CreateUserRequest
  - AddUserRolesRequest

Responsibilidades:
  ✓ Gerenciar dados de usuário
  ✓ Associar papéis
  ✓ Validar email único
  ✓ Proteção de dados sensíveis (sem password em response)
```

### 6.3 FLEET Module (Vehicles)

```
Controllers:
  - VehiclesController
    GET /api/v1/vehicles/categories
    GET /api/v1/vehicles/search?...
    GET /api/v1/vehicles/{id}
  - AdminVehiclesController
    POST /api/v1/admin/vehicles
    PATCH /api/v1/admin/vehicles/{id}

Services:
  - VehicleService
  - VehicleCategoryService

Repositories:
  - IVehicleRepository
  - IVehicleCategoryRepository
  - IBranchRepository

DTOs:
  - VehicleDto
  - CreateVehicleRequest
  - UpdateVehicleRequest
  - VehicleCategoryDto

Responsibilidades:
  ✓ Gerenciar catálogo de veículos
  ✓ Disponibilidade de busca
  ✓ Consistência de status
  ✓ Relacionar vehicle ↔ category ↔ branch
```

### 6.4 RESERVATIONS Module

```
Controllers:
  - ReservationsController
    POST /api/v1/reservations
    GET /api/v1/reservations/{id}
    PATCH /api/v1/reservations/{id}
    POST /api/v1/reservations/{id}/cancel
    GET /api/v1/users/me/reservations

Services:
  - ReservationService
    public async Task<ReservationDto> CreateAsync(...)
    public async Task<ReservationDto> GetByIdAsync(...)
    public async Task<ReservationDto> CancelAsync(...)
    public async Task<IEnumerable<ReservationDto>> GetUserReservationsAsync(...)

Repositories:
  - IReservationRepository
  - IVehicleRepository

DTOs:
  - ReservationDto
  - CreateReservationRequest
  - UpdateReservationRequest
  - CancelReservationRequest

Responsibilidades:
  ✓ Criar reserva com check de disponibilidade
  ✓ Validar datas e regras
  ✓ Atualizar vehicle status
  ✓ Orquestrar com PaymentService
  ✓ Cancelar com refund logic
```

### 6.5 PAYMENTS Module

```
Controllers:
  - PaymentsController
    POST /api/v1/payments/preauth
    POST /api/v1/payments/capture
    POST /api/v1/payments/refund
    GET /api/v1/payments/{id}

Services:
  - PaymentService
    public async Task<PaymentDto> PreAuthorizeAsync(...)
    public async Task<PaymentDto> CaptureAsync(...)
    public async Task<PaymentDto> RefundAsync(...)

Repositories:
  - IPaymentRepository
  - IReservationRepository

DTOs:
  - PaymentDto
  - PreauthRequest
  - CaptureRequest
  - RefundRequest

Responsibilidades:
  ✓ Processar pré-autorização
  ✓ Calcular refund value
  ✓ Atualizar status de pagamento
  ✓ Simular gateway externo (mock)
  ✓ Validar elegibilidade de refund
```

### 6.6 PRICING Module

```
Controllers:
  - PricingController
    GET /api/v1/pricing/rules
    POST /api/v1/pricing/rules
    PATCH /api/v1/pricing/rules/{id}

Services:
  - PricingService
    public async Task<decimal> CalculateTotalAsync(...)
    public async Task<PricingRuleDto> CreateRuleAsync(...)
    public async Task<PricingRuleDto> UpdateRuleAsync(...)

Repositories:
  - IPricingRuleRepository

DTOs:
  - PricingRuleDto
  - CreatePricingRuleRequest
  - UpdatePricingRuleRequest

Responsibilidades:
  ✓ Gerenciar regras de preço por categoria/branch
  ✓ Calcular preço total (datas + categoria + branch)
  ✓ Aplicar tax/fees
```

### 6.7 BRANCHES Module

```
Controllers:
  - BranchesController
    GET /api/v1/branches
    GET /api/v1/branches/{id}

Services:
  - BranchService

Repositories:
  - IBranchRepository

DTOs:
  - BranchDto

Responsibilidades:
  ✓ Listar branches
  ✓ Fornecer informações de operação (horários, endereço)
```

---

## 7. LIMITES ARQUITETURAIS

### 7.1 O Que Pode Ser Criado

```
✅ Novos Controllers para novos domínios
✅ Novos Services para lógica nova
✅ Novos Repositories para abstração de dados
✅ Novos DTOs para novos endpoints
✅ Novas Enums para novos status
✅ Novas Entities no Domain
✅ Novos Middlewares (cross-cutting concerns)
✅ Novos Extension Methods
```

### 7.2 O Que NÃO Pode Ser Criado

```
❌ Novos Frameworks fora do tech-stack.md
❌ Novos padrões que divergem de standards.md
❌ Endpoints fora dos definidos em business-rules.md
❌ Camadas paralelas ao padrão (ex: "Handlers" ao lado de Services)
❌ Integrações externas sem aprovação (ex: SMS, Email, API terceira)
❌ DbContext para persistência real (Phase 1 é mock)
❌ Controllers que acessam repositories diretamente
❌ Business logic em Controllers
```

---

## 8. INTEGRAÇÕES SIMULADAS E REAIS

### 8.1 Simuladas (Phase 1)

```
Payment Gateway:
- Mock com 90% APPROVED, 10% DECLINED
- Usar seed aleatório ou ID para determinismo

Email Notifications:
- Não implementar. Reservado para futuro
- Adicionar placeholders nos logs

SMS/Push:
- Não implementar. Fora do escopo

Analytics:
- Não implementar. Futuro
```

### 8.2 Reais (Obrigatórias Phase 1)

```
JWT Authentication:
- Usar Microsoft.AspNetCore.Authentication.JwtBearer
- Validar assinatura e expiração
- Extrair claims

Password Hashing:
- Usar BCrypt (BCrypt.Net-Next)
- Nunca armazenar plain text

Swagger/OpenAPI:
- Gerar automaticamente a partir do código
- Descrever todos os endpoints
- Incluir exemplos de request/response
```

---

## 9. TRATAMENTO DE ERROS ARQUITETURAL

### 9.1 Fluxo

```
Controller
  ↓
Service (valida, lança CustomException)
  ↓ (exception não capturada)
Middleware.ExceptionHandlingMiddleware
  ↓
Mapeia para ProblemDetails
  ↓
Response: { errorCode, message, timestamp }
```

### 9.2 Status Codes Padronizados

```
200 OK              → Get, Create (com body)
201 Created         → Post novo recurso
204 NoContent       → Delete, Patch sem body
400 BadRequest      → Validação falhou
401 Unauthorized    → Token inválido/expirado
403 Forbidden       → Papel insuficiente
404 NotFound        → Recurso não existe
409 Conflict        → Overbooking, email duplicate
422 Unprocessable   → Regra de negócio violada
500 InternalError   → Exception não esperada
```

---

## 10. ESCALABILIDADE E FUTUROS PASSOS

### 10.1 Próxima Phase (Phase 2)

```
Entity Framework + SQL Server
├─ Substituir List<T> em memória por EF DbContext
├─ Criar Entities ↔ Domain mapping se necessário
├─ Migrations iniciais
└─ Repositories com LINQ to SQL

Caching Layer
├─ IDistributedCache para session/tokens
├─ Redis connection string em appsettings
└─ Cache de pricing rules e categorias

Database Design
├─ Schema SQL Server
├─ Índices em queries frequentes
├─ Constraints de integridade
```

### 10.2 Próxima Phase (Phase 3)

```
Autenticação Avançada
├─ Refresh tokens persistidos
├─ Token revocation
├─ Social login (opcional)

Auditoria Completa
├─ Audit table para user actions
├─ Log de mudanças (soft delete, change tracking)
├─ Timestamps (created, updated, deleted)

Notificações
├─ RabbitMQ para async events
├─ Email via SendGrid
├─ SMS via Twilio

Busca Avançada
├─ Elasticsearch para busca de veículos
├─ Agregações e filtros complexos
```

---

## 11. DECISÕES JUSTIFICADAS

| Decisão | Razão |
|---------|-------|
| Clean Architecture (CAL) | Separação de concerns, testabilidade, substituição de implementações |
| JWT + RBAC | Stateless, escalável, suporta múltiplos serviços futuros |
| JSON Mock Phase 1 | MVP rápido, validação de conceito, independência de DBA |
| Interfaces para Repositories | Agnóstico a implementação (em-memory vs EF vs API) |
| Global Exception Middleware | Uniformidade de erros, não poluir controllers |
| Dependency Injection | Inversão de controle, testabilidadade |
| DTOs Separados | Validação de entrada, proteção de dados sensíveis |
| Enums para Status | Type safety, evitar strings mágicas |

---

## 12. TESTES E QUALIDADE

### 12.1 Pirâmide de Testes

```
    △ E2E (5%)
   △△ Integration (25%)
  △△△ Unit (70%)
```

- **Unit (70%)**: Testes isolados de services, security
- **Integration (25%)**: Testes de controllers via WebApplicationFactory
- **E2E (5%)**: Manual ou ferramentas específicas (Phase 2+)

### 12.2 Stack de Testes

```
xUnit              → Framework nativo .NET
FluentAssertions   → Asserções legíveis
Moq                → Mocking minimal
WebApplicationFactory → Testes HTTP (ASP.NET Core built-in)
```

### 12.3 Estrutura de Diretórios

```
tests/AlugueldeCarros.Tests/
├── Unit/Services/        (6+ test files)
├── Unit/Security/        (JWT + RBAC tests)
├── Integration/          (Controller tests)
└── Fixtures/             (Builders, test data)
```

### 12.4 Cobertura Mínima

| Módulo | Target | Crítico |
|--------|--------|----------|
| Services | 80%+ | Sim |
| Controllers | 70%+ | Sim |
| Security | 90%+ | Sim |
| Repositories | 60%+ | Não |

### 12.5 Execução Local

```bash
# Todos os testes
dotnet test tests/AlugueldeCarros.Tests.csproj

# Classe específica
dotnet test --filter "ClassName=UserServiceTests"

# Watch mode
dotnet test --watch
```

### 12.6 Ciclo de Desenvolvimento

```
1. Escrever teste (RED)
2. Escrever código mínimo para passar (GREEN)
3. Refatorar mantendo testes verdes (REFACTOR)
4. Repetir para próximo teste
```

### 12.7 Sem CI/CD em Phase 1

```
Phase 1:  Testes locais apenas (dotnet test)
Phase 2+: GitHub Actions + codecov (futuro)
```

---

## 13. CHANGELOG

| Data | Evento | Descrição |
|------|--------|-----------|
| 2026-03-25 | Criação | Arquitetura inicial com Clean Architecture |

