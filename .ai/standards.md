# STANDARDS — Padrões de Código e Engenharia

**Este documento define as convenções, padrões e regras de engenharia para o projeto.**

---

## 1. CONVENÇÕES DE NOMENCLATURA C#

### 1.1 Classes e Tipos

```csharp
// PascalCase obrigatório
public class UserController { }
public class AuthService { }
public class IUserRepository { }
public enum ReservationStatus { }
public record CreateUserRequest { }

// Namespacing obrigatório (sem abreviações)
namespace AlugueldeCarros.Controllers { }
namespace AlugueldeCarros.Domain.Entities { }
namespace AlugueldeCarros.Services { }
namespace AlugueldeCarros.DTOs.Auth { }
```

### 1.2 Membros e Métodos

```csharp
// Propriedades: PascalCase
public string FullName { get; set; }
public DateTime CreatedAt { get; set; }
public Guid UserId { get; set; }

// Métodos: PascalCase + verbo
public async Task<UserDto> GetUserByIdAsync(Guid id) { }
public async Task<bool> CreateReservationAsync(CreateReservationRequest request) { }
public void ValidateAvailability() { }

// Parâmetros: camelCase
public Task<User> FindUserAsync(string email, int maxRetries = 3) { }

// Private/Internal fields: _camelCase
private readonly ILogger<UserService> _logger;
private const string JwtIssuer = "AlugueldeCarros";
```

### 1.3 Interfaces

```csharp
// Sempre começar com I
public interface IUserService { }
public interface IUserRepository { }
public interface IAuthHandler { }

// Nunca: UserService (sem I)
// Nunca: IUserServiceInterface (redundante)
```

### 1.4 Async Patterns

```csharp
// ALWAYS terminate async methods with Async suffix
public async Task<User> GetUserAsync(Guid id) { }
public async Task<IEnumerable<Vehicle>> SearchVehiclesAsync(SearchRequest request) { }

// Sync versions sem suffix
public User GetUser(Guid id) { }  // Nota: evitar se possível, preferir async

// Fire-and-forget deve ser documentado
#pragma warning disable CS4014
_ = BackgroundTaskAsync();
#pragma warning restore CS4014
```

---

## 2. ORGANIZAÇÃO DE PASTAS

### 2.1 Estrutura Obrigatória

```
AlugueldeCarros/
│
├── Program.cs                    (entry point, configurações)
├── appsettings.json
├── appsettings.Development.json
│
├── .ai/                          (contexto e regras)
├── .cursorrules                  (instruções para agentes)
├── README.md                     (documentação)
│
├── Configurations/               (config classes)
│   ├── JwtSettings.cs
│   └── ...
│
├── Controllers/                  (HTTP endpoints, [ApiController])
│   ├── AuthController.cs
│   ├── UserController.cs
│   ├── AdminUsersController.cs
│   ├── AdminVehiclesController.cs
│   ├── VehiclesController.cs
│   ├── ReservationsController.cs
│   ├── PaymentsController.cs
│   ├── BranchesController.cs
│   └── PricingController.cs
│
├── Domain/
│   ├── Entities/                 (models de domínio apenas)
│   │   ├── User.cs
│   │   ├── Role.cs
│   │   ├── UserRole.cs
│   │   ├── Vehicle.cs
│   │   ├── VehicleCategory.cs
│   │   ├── Branch.cs
│   │   ├── Reservation.cs
│   │   ├── Payment.cs
│   │   ├── PricingRule.cs
│   │   └── CustomerProfile.cs
│   │
│   └── Enums/                    (constantes de enum)
│       ├── ReservationStatus.cs
│       ├── VehicleStatus.cs
│       ├── PaymentStatus.cs
│       ├── UserRoleType.cs
│       └── ...
│
├── DTOs/                         (Data Transfer Objects)
│   ├── Auth/
│   │   ├── LoginRequest.cs
│   │   ├── RegisterRequest.cs
│   │   ├── AuthResponse.cs
│   │   └── RefreshTokenRequest.cs
│   │
│   ├── Users/
│   │   ├── UserDto.cs
│   │   ├── CreateUserRequest.cs
│   │   ├── AddUserRolesRequest.cs
│   │   └── ...
│   │
│   ├── Vehicles/
│   │   ├── VehicleDto.cs
│   │   ├── CreateVehicleRequest.cs
│   │   ├── UpdateVehicleRequest.cs
│   │   └── ...
│   │
│   ├── Reservations/
│   │   ├── CreateReservationRequest.cs
│   │   ├── ReservationDto.cs
│   │   ├── UpdateReservationRequest.cs
│   │   └── ...
│   │
│   ├── Payments/
│   │   ├── PreauthRequest.cs
│   │   ├── CaptureRequest.cs
│   │   ├── RefundRequest.cs
│   │   └── PaymentDto.cs
│   │
│   └── Pricing/
│       ├── PricingRuleDto.cs
│       └── CreatePricingRuleRequest.cs
│
├── Services/                     (lógica de negócio)
│   ├── AuthService.cs
│   ├── UserService.cs
│   ├── VehicleService.cs
│   ├── ReservationService.cs
│   ├── PaymentService.cs
│   ├── PricingService.cs
│   ├── BranchService.cs
│   └── VehicleCategoryService.cs
│
├── Repositories/                 (camada de dados abstrata)
│   ├── IUserRepository.cs
│   ├── IVehicleRepository.cs
│   ├── IReservationRepository.cs
│   ├── IPaymentRepository.cs
│   ├── IPricingRuleRepository.cs
│   ├── IBranchRepository.cs
│   ├── IRoleRepository.cs
│   ├── IVehicleCategoryRepository.cs
│   │
│   └── Implementations/          (opcional se houver repos reais)
│       ├── JsonUserRepository.cs
│       └── ...
│
├── Resources/                    (dados mockados)
│   ├── MockData/
│   │   ├── users.json
│   │   ├── roles.json
│   │   ├── branches.json
│   │   ├── vehicles.json
│   │   ├── categories.json
│   │   ├── pricing-rules.json
│   │   ├── reservations.json
│   │   └── payments.json
│   │
│   └── Loaders/
│       └── JsonDataLoader.cs     (carrega JSON em memória)
│
├── Security/                     (autenticação e segurança)
│   ├── JwtTokenService.cs
│   ├── PasswordHasher.cs
│   └── JwtClaimsPrincipal.cs
│
├── Middleware/                   (middleware ASP.NET Core)
│   └── ExceptionHandlingMiddleware.cs
│
├── Exceptions/                   (exceções customizadas)
│   ├── CustomException.cs
│   ├── NotFoundException.cs
│   ├── UnauthorizedException.cs
│   ├── BadRequestException.cs
│   └── ConflictException.cs
│
├── Validations/                  (validadores de request)
│   ├── CreateReservationValidator.cs  (FluentValidation ou DataAnnotations)
│   ├── CreateVehicleValidator.cs
│   └── ...
│
└── bin/, obj/, Properties/      (gerados)
```

### 2.2 Padrão de Nomenclatura por Pasta

| Pasta | Padrão | Exemplo |
|-------|--------|---------|
| Controllers | `{Domain}Controller.cs` | `UserController.cs`, `AdminVehiclesController.cs` |
| Services | `{Domain}Service.cs` | `UserService.cs`, `ReservationService.cs` |
| Repositories | `I{Domain}Repository.cs` | `IUserRepository.cs`, `IVehicleRepository.cs` |
| DTOs | `{Action}{Domain}Request.cs`, `{Domain}Dto.cs` | `CreateUserRequest.cs`, `UserDto.cs` |
| Entities | `{Domain}.cs` | `User.cs`, `Vehicle.cs` |
| Enums | `{Domain}Status.cs` | `ReservationStatus.cs`, `VehicleStatus.cs` |
| Exceptions | `{Type}Exception.cs` | `NotFoundException.cs`, `BadRequestException.cs` |

---

## 3. PADRÃO DE CONTROLLERS

### 3.1 Estrutura Básica

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace AlugueldeCarros.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpGet("me")]
    [Authorize]  // Exigir autenticação
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var userId = User.FindFirst("sub")?.Value;
        var user = await _userService.GetUserByIdAsync(Guid.Parse(userId!));
        
        if (user == null)
            return NotFound();
        
        return Ok(user);
    }

    [HttpPost]
    [AllowAnonymous]  // Públicos
    public async Task<ActionResult<UserDto>> Register([FromBody] RegisterRequest request)
    {
        var user = await _userService.CreateUserAsync(request);
        return CreatedAtAction(nameof(GetCurrentUser), user);
    }

    [HttpPost("{id}/roles")]
    [Authorize(Roles = "ADMIN")]  // Apenas ADMIN
    public async Task<IActionResult> AddRole(Guid id, [FromBody] AddUserRolesRequest request)
    {
        await _userService.AddRoleAsync(id, request.RoleId);
        return NoContent();
    }
}
```

### 3.2 Requerimentos

```
✅ [ApiController] obrigatório
✅ [Route("api/v1/[controller]")] obrigatório
✅ ControllerBase (não Controller)
✅ Injeção de dependência pelo constructor
✅ [Authorize] para endpoints protegidos
✅ [Authorize(Roles = "ADMIN")] para endpoints admin
✅ [AllowAnonymous] explícito para públicos
✅ Logging com ILogger
✅ Validação de entrada
✅ Tratamento de exceções delegado ao middleware
✅ Responses: Ok(), NotFound(), BadRequest(), CreatedAtAction()
```

### 3.3 HTTP Verbs e Status Codes

```csharp
[HttpGet]            → 200 Ok, 404 NotFound
[HttpPost]           → 201 Created, 400 Bad Request, 409 Conflict
[HttpPut]            → 200 Ok, 204 NoContent, 400 Bad Request
[HttpPatch]          → 200 Ok, 204 NoContent, 400 Bad Request
[HttpDelete]         → 204 NoContent, 404 NotFound

// Convenção: DELETE retorna 204 (sem body)
// PATCH é preferido sobre PUT para atualizações parciais
```

---

## 4. PADRÃO DE SERVICES

### 4.1 Estrutura Básica

```csharp
using AlugueldeCarros.Domain.Entities;
using AlugueldeCarros.DTOs;
using AlugueldeCarros.Repositories;

namespace AlugueldeCarros.Services;

public interface IUserService
{
    Task<UserDto> GetUserByIdAsync(Guid id);
    Task<UserDto> CreateUserAsync(RegisterRequest request);
    Task AddRoleAsync(Guid userId, Guid roleId);
}

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<UserDto> GetUserByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        
        if (user == null)
            throw new NotFoundException($"Usuário {id} não encontrado");
        
        return MapToDto(user);
    }

    public async Task<UserDto> CreateUserAsync(RegisterRequest request)
    {
        ValidateRequest(request);
        
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
            throw new ConflictException("Email já cadastrado");

        var hashedPassword = _passwordHasher.Hash(request.Password);
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FullName = request.FullName,
            PasswordHash = hashedPassword,
            CreatedAt = DateTime.UtcNow,
            Status = "ACTIVE"
        };

        await _userRepository.CreateAsync(user);
        _logger.LogInformation("Usuário criado: {UserId}", user.Id);

        return MapToDto(user);
    }

    private static void ValidateRequest(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new BadRequestException("Email é obrigatório");
        
        if (request.Password.Length < 8)
            throw new BadRequestException("Senha deve ter no mínimo 8 caracteres");
    }

    private static UserDto MapToDto(User user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        FullName = user.FullName,
        CreatedAt = user.CreatedAt
    };
}
```

### 4.2 Responsabilidades

```
✅ Lógica de negócio (validações, cálculos)
✅ Orquestração de repositories
✅ Transformações entre domain e DTOs
✅ Logging de operações importantes
✅ Lançar exceções customizadas

❌ Retornar dados com password
❌ Acessar HTTP direto
❌ Acessar DbContext (usar repository)
❌ Criar múltiplas instâncias de dependência
```

---

## 5. PADRÃO DE REPOSITORIES

### 5.1 Interface

```csharp
namespace AlugueldeCarros.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetAllAsync();
    Task CreateAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(Guid id);
}
```

### 5.2 Implementação com JSON Mock

```csharp
namespace AlugueldeCarros.Repositories.Implementations;

public class JsonUserRepository : IUserRepository
{
    private readonly List<User> _users;
    private readonly IJsonDataLoader _dataLoader;

    public JsonUserRepository(IJsonDataLoader dataLoader)
    {
        _dataLoader = dataLoader;
        _users = dataLoader.LoadUsers();  // Carregado uma vez
    }

    public Task<User?> GetByIdAsync(Guid id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        return Task.FromResult(user);
    }

    public Task<User?> GetByEmailAsync(string email)
    {
        var user = _users.FirstOrDefault(u => u.Email == email);
        return Task.FromResult(user);
    }

    public Task<IEnumerable<User>> GetAllAsync()
    {
        return Task.FromResult(_users.AsEnumerable());
    }

    public Task CreateAsync(User user)
    {
        _users.Add(user);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(User user)
    {
        var existing = _users.FirstOrDefault(u => u.Id == user.Id);
        if (existing != null)
        {
            var index = _users.IndexOf(existing);
            _users[index] = user;
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        if (user != null)
            _users.Remove(user);
        return Task.CompletedTask;
    }
}
```

### 5.3 Princípios

```
✅ Interface pública, implementação privada (Dependency Inversion)
✅ Sempre async (Task, Task<T>)
✅ Retornar null em vez de lançar em GET
✅ Mock com List<T> em memória
✅ Métodos agnósticos a qual persistência (preparado para EF6 depois)
```

---

## 6. PADRÃO DE DTOs

### 6.1 Tipos de DTOs

```csharp
// Request: entrada de dados
public record CreateUserRequest
{
    public required string Email { get; init; }
    public required string FullName { get; init; }
    public required string Password { get; init; }
}

// Response: saída de dados (nunca password)
public record UserDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

// Validation request: adicionar validações
public class CreateReservationRequest
{
    [Required]
    public Guid CategoryId { get; set; }

    [Required]
    public Guid BranchId { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime CheckIn { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime CheckOut { get; set; }
}
```

### 6.2 Regras

```
✅ Usar record quando possível (imutável)
✅ Usar class quando necessária mutação
✅ Não expor domains inteiras, apenas DTOs
✅ Separar Request e Response DTOs
✅ Validações com DataAnnotations ou FluentValidation
✅ Nunca retornar password em DTOs
✅ Nunca incluir dados sensíveis desnecessários
```

---

## 7. PADRÃO DE ENUMS

### 7.1 Definição

```csharp
namespace AlugueldeCarros.Domain.Enums;

public enum ReservationStatus
{
    PendingPayment = 0,
    Confirmed = 1,
    Cancelled = 2,
    Expired = 3
}

public enum VehicleStatus
{
    Available = 0,
    Reserved = 1,
    Rented = 2,
    Maintenance = 3,
    Blocked = 4
}

public enum PaymentStatus
{
    PreAuthPending = 0,
    PreAuthApproved = 1,
    PreAuthDeclined = 2,
    Captured = 3,
    Refunded = 4
}

public enum UserRoleType
{
    Admin = 0,
    User = 1,
    Guest = 2
}
```

### 7.2 Serialização

```csharp
// Em appsettings.json:
"JsonSerializerOptions": {
  "PropertyNamingPolicy": "CamelCase",
  "DefaultIgnoreCondition": "WhenWritingNull"
}

// Controllers:
services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(namingPolicy: JsonKnownNamingPolicy.CamelCase)
        );
    });

// Resultado JSON:
{ "status": "pendingPayment" }  // camelCase
```

---

## 8. INJEÇÃO DE DEPENDÊNCIA

### 8.1 Configuração em Program.cs

```csharp
var builder = WebApplicationBuilder.CreateBuilder(args);

// Serviços
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

// Repositories
builder.Services.AddScoped<IUserRepository, JsonUserRepository>();
builder.Services.AddScoped<IVehicleRepository, JsonVehicleRepository>();
builder.Services.AddScoped<IReservationRepository, JsonReservationRepository>();

// Data Loader (Singleton)
builder.Services.AddSingleton<IJsonDataLoader, JsonDataLoader>();

// Security
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddSwaggerGen();

// Authorization
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { /* config */ });

var app = builder.Build();

// Middleware
app.UseExceptionHandlingMiddleware();
app.UseSwagger();
app.UseSwaggerUI();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### 8.2 Princípios

```
✅ Addons Services: AddScoped, AddTransient, AddSingleton
✅ Ambos necessários. AddScoped (padrão para services/repos)
✅ AddSingleton apenas para stateless (logger, config, data loader)
✅ Osenumerator injeta-se a si mesmo implicitamente
✅ Evitar service locator (não fazer new Service())
```

---

## 9. VALIDAÇÃO

### 9.1 DataAnnotations (Simples)

```csharp
public class CreateUserRequest
{
    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(255, MinimumLength = 8, 
        ErrorMessage = "Senha deve ter entre 8 e 255 caracteres")]
    public string Password { get; set; } = string.Empty;
}

// No controller:
[HttpPost]
public async Task<ActionResult> Register([FromBody] CreateUserRequest request)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);

    // ...
}
```

### 9.2 Validção Customizada (Service)

```csharp
public class ReservationService : IReservationService
{
    public async Task<ReservationDto> CreateReservationAsync(
        Guid userId, 
        CreateReservationRequest request)
    {
        // Validações de negócio
        if (request.CheckIn >= request.CheckOut)
            throw new BadRequestException("CheckIn deve ser antes de CheckOut");

        if (request.CheckIn <= DateTime.UtcNow.AddHours(2))
            throw new BadRequestException("CheckIn deve ser no mínimo 2 horas no futuro");

        var availability = await _availabilityService
            .CheckAvailabilityAsync(request.CategoryId, request.BranchId, 
                request.CheckIn, request.CheckOut);

        if (!availability.IsAvailable)
            throw new ConflictException("Categoria não disponível para o período");

        // ...
    }
}
```

---

## 10. TRATAMENTO GLOBAL DE EXCEÇÕES

### 10.1 Exceções Customizadas

```csharp
namespace AlugueldeCarros.Exceptions;

public class CustomException : Exception
{
    public int StatusCode { get; set; }
    public string ErrorCode { get; set; }

    public CustomException(
        string message, 
        int statusCode = 500, 
        string errorCode = "INTERNAL_ERROR") 
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
}

public class NotFoundException : CustomException
{
    public NotFoundException(string message) 
        : base(message, 404, "NOT_FOUND") { }
}

public class BadRequestException : CustomException
{
    public BadRequestException(string message) 
        : base(message, 400, "BAD_REQUEST") { }
}

public class UnauthorizedException : CustomException
{
    public UnauthorizedException(string message) 
        : base(message, 401, "UNAUTHORIZED") { }
}

public class ConflictException : CustomException
{
    public ConflictException(string message) 
        : base(message, 409, "CONFLICT") { }
}
```

### 10.2 Middleware Global

```csharp
namespace AlugueldeCarros.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, 
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, errorCode) = exception switch
        {
            NotFoundException ex => ((int)HttpStatusCode.NotFound, "NOT_FOUND"),
            BadRequestException ex => ((int)HttpStatusCode.BadRequest, "BAD_REQUEST"),
            UnauthorizedException ex => ((int)HttpStatusCode.Unauthorized, "UNAUTHORIZED"),
            ConflictException ex => ((int)HttpStatusCode.Conflict, "CONFLICT"),
            _ => ((int)HttpStatusCode.InternalServerError, "INTERNAL_ERROR")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = new
        {
            errorCode,
            message = exception.Message,
            timestamp = DateTime.UtcNow
        };

        return context.Response.WriteAsJsonAsync(response);
    }
}
```

---

## 11. PADRÃO DE RESPONSES

### 11.1 Sucesso

```json
{
  "data": {
    "id": "uuid",
    "email": "user@example.com",
    "fullName": "John Doe"
  },
  "success": true,
  "timestamp": "2026-03-25T10:00:00Z"
}
```

### 11.2 Erro

```json
{
  "errorCode": "NOT_FOUND",
  "message": "Usuário não encontrado",
  "timestamp": "2026-03-25T10:00:00Z"
}
```

### 11.3 Validação

```json
{
  "errors": {
    "email": ["Email é obrigatório"],
    "password": ["Senha deve ter no mínimo 8 caracteres"]
  },
  "errorCode": "VALIDATION_ERROR",
  "timestamp": "2026-03-25T10:00:00Z"
}
```

---

## 12. EVITAR DUPLICAÇÃO

### 12.1 Princípio DRY

```
❌ Não copiar serviços entre domínios
❌ Não duplicar validações
❌ Não criar mappers redundantes
❌ Não copiar middlewares

✅ Extrair em BaseService se reuso
✅ Usar extension methods para operações comuns
✅ Centralizar validações em classes Validator
✅ Reutilizar middlewares e configuração
```

### 12.2 Extension Methods Para Reutilização

```csharp
namespace AlugueldeCarros.Extensions;

public static class ValidationExtensions
{
    public static void ValidateDateRange(this DateTime checkIn, DateTime checkOut)
    {
        if (checkIn >= checkOut)
            throw new BadRequestException("CheckIn deve ser antes de CheckOut");
        
        if (checkIn <= DateTime.UtcNow.AddHours(2))
            throw new BadRequestException("CheckIn deve ser no mínimo 2 horas no futuro");
    }
}

// Uso em qualquer service:
request.CheckIn.ValidateDateRange(request.CheckOut);
```

---

## 13. MUDANÇAS PEQUENAS E SEGURAS

### 13.1 Princípio

```
Toda mudança DEVE:
1. Ser pequena o suficiente para ser revista em menos de 5 min
2. Compilar sem erros
3. Respeitar testes existentes
4. Não quebrar compatibilidade com código existente
5. Ser rastreável em git commit
```

### 13.2 Exemplo de Mudança Segura

```
❌ Wrong:
- Refatorar 5 services de uma vez
- Renomear 10 endpoints
- Reescrever camada de persistência

✅ Correct:
- Adicionar uma nova interface em um repository
- Estender um service com um novo método
- Criar um novo DTO para novo endpoint
- Ajustar um validator
- Corrigir um bug
```

---

## 14. MANUTENÇÃO DE COMPATIBILIDADE

### 14.1 Regras

```
✅ Preservar assinaturas públicas existentes
✅ Adicionar novos métodos sem quebrar antigos
✅ Versionar DTOs se endpoints mudam
✅ Anunciar breaking changes
✅ Manter compilação limpa

❌ Renomear métodos públicos
❌ Remover parâmetros
❌ Mudar tipos de retorno
❌ Quebrar endpoints sem aviso
```

### 14.2 Versionamento de API

```csharp
// Atual
[Route("api/v1/[controller]")]

// Se breaking change necessário:
[Route("api/v2/[controller]")]

// Original permanece suportada:
[Route("api/v1/[controller]")]

// API pode suportar ambas:
[Route("api/v{version}/[controller]")]
```

---

## 15. PADRÕES DE TESTES

### 15.1 Padrão AAA (Arrange-Act-Assert)

```csharp
[Fact]
public async Task CreateReservation_ValidRequest_ReturnsReservationWithPendingPaymentStatus()
{
    // Arrange
    var userId = Guid.NewGuid();
    var request = new CreateReservationRequest
    {
        UserId = userId,
        VehicleCategoryId = Guid.NewGuid(),
        StartDate = DateTime.UtcNow.AddDays(1),
        EndDate = DateTime.UtcNow.AddDays(3)
    };

    // Act
    var result = await _reservationService.CreateAsync(request);

    // Assert
    result.Should().NotBeNull();
    result.Status.Should().Be(ReservationStatus.PENDING_PAYMENT);
    result.UserId.Should().Be(userId);
}
```

### 15.2 Nomenclatura de Testes

**Padrão**: `{MethodName}_{Scenario}_{ExpectedResult}`

```csharp
public class UserServiceTests
{
    [Fact]
    public async Task Login_ValidCredentials_ReturnsJwtToken() { }

    [Fact]
    public async Task Login_InvalidPassword_ThrowsUnauthorizedException() { }

    [Theory]
    [InlineData("invalid@")]
    [InlineData("")]
    public async Task Register_InvalidEmail_ThrowsValidationException(string email) { }
}
```

### 15.3 TestDataBuilder

```csharp
public class TestDataBuilder
{
    private User _user;

    public TestDataBuilder WithUser(string email = "test@email.com")
    {
        _user = new User { Id = Guid.NewGuid(), Email = email };
        return this;
    }

    public User BuildUser() => _user;
}

// Uso:
var user = new TestDataBuilder().WithUser("admin@test.com").BuildUser();
```

### 15.4 Regras de Testes

```
✅ Testar Happy Path + Unhappy Paths
✅ Testar Autenticação e Autorização
✅ Testar Limites de negócio (ex: 5 reservas)
✅ Nomear descritivamente
✅ Mockar dependências externas
✅ Isolar testes (sem estado compartilhado)

❌ NUNCA testar DTOs, Entities, Enums
❌ NUNCA testar framework code (JWT, DI)
❌ NUNCA usar bancos reais (mock obrigatório)
❌ NUNCA deixar código comentado
```

### 15.5 Mocking com Moq

```csharp
[Fact]
public async Task CreateUser_CallsRepository_OnSuccess()
{
    var mockRepo = new Mock<IUserRepository>();
    var service = new UserService(mockRepo.Object);

    mockRepo
        .Setup(r => r.AddAsync(It.IsAny<User>()))
        .ReturnsAsync(true);

    var result = await service.CreateAsync(request);

    mockRepo.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
}
```

---

## 16. CHANGELOG

| Data | Evento | Descrição |
|------|--------|-----------|
| 2026-03-25 | Criação | Padrões iniciais de código e engenharia |

