# TECH STACK — Stack Obrigatório e Restrições Técnicas

**Este documento define as tecnologias, versões e restrições obrigatórias para o projeto.**

---

## 1. STACK OFICIAL

### 1.1 Runtime e Framework

```
Language:                C# 12+
Runtime:                 .NET 8.0 LTS
Web Framework:           ASP.NET Core 8.0 Web API
Project Format:          .csproj (SDK style)
Target Framework:        net8.0
```

### 1.2 Banco e Persistência (Phase 1)

```
Data Source (Phase 1):   JSON mockado em arquivos
Memory Storage:          List<T> em RAM (Singleton)
Data Loading:            JsonDataLoader
Serialization:           System.Text.Json (padrão .NET 8)

❌ NÃO USAR:
- Entity Framework (Phase 2+)
- DbContext (Phase 2+)
- SQL Server / PostgreSQL (Phase 2+)
- Migrations (Phase 2+)
```

### 1.3 Autenticação e Segurança

```
Authentication:          JWT (JSON Web Tokens)
Handler:                 Microsoft.AspNetCore.Authentication.JwtBearer
Password Hashing:        Bcrypt via BCrypt.Net-Next
Token Signing:           HS256 (HMAC SHA-256)
Secret Management:       IConfiguration (appsettings.json)
Authorization:           RBAC (Role-Based Access Control) nativo do ASP.NET
```

### 1.4 HTTP e API

```
Framework API:           ASP.NET Core Web API
HTTP Routing:            Attribute Routing
Content Negotiation:     JSON
CORS:                    Microsoft.AspNetCore.Cors (se necessário)
Versioning:              URL Path (api/v1/, api/v2/)
```

### 1.5 Documentação API

```
Spec:                    OpenAPI 3.0
Generator:               Swashbuckle.AspNetCore
UI:                      Swagger UI
Anotações:               XML comments ou attributes
```

### 1.6 Logging

```
Framework:               ILogger<T> (built-in)
Sink:                    Console (stdout)
Nivel:                   Debug (dev), Information (prod)
Correlation:             Opcional TraceId (via middleware)
```

### 1.7 Validation

```
DataAnnotations:         System.ComponentModel.DataAnnotations
FluentValidation:        Opcional para validações complexas
Modelo:                  DTO validation em Controllers
Negócio:                 Service validation em Services
```

---

## 2. VERSÕES OBRIGATÓRIAS

```
.NET:                    8.0.0 ou superior
C#:                      12.0 ou superior
BCrypt.Net-Next:         4.0.3 ou superior
Swashbuckle.Core:        6.4.0 ou superior
System.Text.Json:        Built-in (não instalar separadamente)
Microsoft.AspNetCore.*:  8.0.0 (matching .NET 8.0)
```

---

## 3. DEPENDÊNCIAS PERMITIDAS

### 3.1 Permitidas (Obrigatórias ou Recomendadas)

```
✅ Microsoft.AspNetCore.Authentication.JwtBearer
✅ Microsoft.AspNetCore.Authorization
✅ Microsoft.AspNetCore.Cors
✅ BCrypt.Net-Next
✅ Swashbuckle.AspNetCore
✅ Swashbuckle.AspNetCore.Swagger
✅ Swashbuckle.AspNetCore.SwaggerUI
✅ System.ComponentModel.DataAnnotations
✅ System.Text.Json
✅ Microsoft.Extensions.Logging
✅ Microsoft.Extensions.Configuration
```

### 3.2 Permitidas (Caso Necessário)

```
✅ FluentValidation          → Se regras muito complexas
✅ CsvHelper                 → Se exportar dados CSV
✅ Newtonsoft.Json           → Alternativa ao System.Text.Json
✅ Serilog                   → Log avançado (se necessário)
✅ Polly                     → Retry/circuit breaker (futuro)
```

### 3.3 NÃO Permitidas (Phase 1)

```
❌ Entity Framework Core     → Fase 2+
❌ Dapper                    → Fase 2+
❌ NHibernate                → Não suportado
❌ Hangfire                  → Fase 3+
❌ MassTransit               → Fase 3+
❌ IdentityServer            → Fase 3+
❌ GraphQL                   → Futuro, avaliar
❌ gRPC                      → Não planejado
```

---

## 4. REGRAS DE JSON MOCKADO

### 4.1 Formato

```
Localização:             ./Resources/MockData/
Padrão:                 {entity}.json
Encoding:               UTF-8
Pretty Print:           Ativado (indentação)
```

### 4.2 Arquivos Obrigatórios

```
users.json              → [User]
roles.json              → [Role]
user-roles.json         → [UserRole]
branches.json           → [Branch]
vehicle-categories.json → [VehicleCategory]
vehicles.json           → [Vehicle]
pricing-rules.json      → [PricingRule]
reservations.json       → [Reservation]
payments.json           → [Payment]
customer-profiles.json  → [CustomerProfile]
```

### 4.3 Exemplo: users.json

```json
[
  {
    "id": "00000000-0000-0000-0000-000000000001",
    "email": "admin@aluguel.com",
    "passwordHash": "$2a$11$...",
    "fullName": "Admin User",
    "cpf": "00000000000",
    "phone": "+55 11 99999-9999",
    "createdAt": "2026-01-01T10:00:00Z",
    "status": "ACTIVE"
  },
  {
    "id": "00000000-0000-0000-0000-000000000002",
    "email": "user@example.com",
    "passwordHash": "$2a$11$...",
    "fullName": "Cliente Padrão",
    "cpf": "12345678900",
    "phone": "+55 11 98888-8888",
    "createdAt": "2026-01-20T14:30:00Z",
    "status": "ACTIVE"
  }
]
```

### 4.4 Carregamento

```csharp
// JsonDataLoader.cs
public class JsonDataLoader : IJsonDataLoader
{
    private readonly IWebHostEnvironment _environment;

    public JsonDataLoader(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public List<User> LoadUsers()
    {
        var path = Path.Combine(_environment.ContentRootPath, 
            "Resources", "MockData", "users.json");
        
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<List<User>>(json) ?? [];
    }

    public List<Vehicle> LoadVehicles() { /* ... */ }
    public List<Reservation> LoadReservations() { /* ... */ }
    // ... demais
}

// Program.cs
services.AddSingleton<IJsonDataLoader, JsonDataLoader>();

// UserRepository.cs
public class JsonUserRepository : IUserRepository
{
    private readonly List<User> _users;

    public JsonUserRepository(IJsonDataLoader loader)
    {
        _users = loader.LoadUsers();
    }

    public Task<User?> GetByIdAsync(Guid id) 
        => Task.FromResult(_users.FirstOrDefault(u => u.Id == id));
}
```

---

## 5. REGRAS DE SERIALIZAÇÃO

### 5.1 JSON Naming Policy

```csharp
// Program.cs
services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = 
            JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = 
            JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(JsonKnownNamingPolicy.CamelCase));
    });
```

### 5.2 Request/Response

```
C# Property:    CreatedAt
JSON Field:     createdAt
C# Property:    ReservationStatus
JSON Field:     reservationStatus
C# Enum:        ReservationStatus.PendingPayment
JSON Field:     "pendingPayment"
```

### 5.3 Null Handling

```
WhenWritingNull:  Omitir null fields do JSON
Resultado:        JSON mais compacto
{
  "id": "123",
  "name": "Car",
  "description": null  ← OMITIDO
}
```

---

## 6. REGRAS DE JWT

### 6.1 Configuração

```csharp
// Configurations/JwtSettings.cs
public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 60;
    public string Issuer { get; set; } = "AlugueldeCarros";
    public string Audience { get; set; } = "AlugueldeCarrosApi";
}

// appsettings.json
{
  "JwtSettings": {
    "Secret": "your-secret-key-min-32-chars-long!!!",
    "ExpirationMinutes": 60,
    "Issuer": "AlugueldeCarros",
    "Audience": "AlugueldeCarrosApi"
  }
}
```

### 6.2 Token Generation

```csharp
public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _jwtSettings;

    public string GenerateToken(User user, IEnumerable<string> roles)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim("sub", user.Id.ToString()),
            new Claim("email", user.Email)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

### 6.3 Validação

```csharp
// Program.cs
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.Secret)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });
```

---

## 7. REGRAS DE SWAGGER

### 7.1 Configuração

```csharp
// Program.cs
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Aluguel de Carros API",
        Version = "v1",
        Description = "API REST para sistema de aluguel de veículos"
    });

    // Adicionar autenticação JWT
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

// Em Program.cs (app configuration)
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.RoutePrefix = "api/docs";
});
```

### 7.2 XML Comments

```csharp
/// <summary>
/// Cria uma nova reserva de veículo
/// </summary>
/// <param name="request">Dados da reserva</param>
/// <returns>Reserva criada com status PENDING_PAYMENT</returns>
/// <response code="201">Reserva criada com sucesso</response>
/// <response code="400">Validação falhou</response>
/// <response code="409">Veículo não disponível</response>
[HttpPost]
public async Task<ActionResult<ReservationDto>> Create(
    [FromBody] CreateReservationRequest request)
{
    // ...
}
```

---

## 8. REGRAS DE PASSWORD HASHING

### 8.1 BCrypt

```csharp
public class PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        // BCrypt.Net-Next automaticamente gera salt
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool Verify(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}

// Uso:
var hashedPassword = _passwordHasher.Hash(plainPassword);
// armazenar: $2a$11$...
if (_passwordHasher.Verify(providedPassword, storedHash))
{
    // Autenticado
}
```

### 8.2 Regras

```
✅ Sempre hash com BCrypt
✅ Nunca armazenar plain text
✅ Verificar com Verify() de BCrypt
✅ Usar cost factor ≥ 11
❌ Nunca retornar hash em DTOs
❌ Nunca retornar password em nenhuma resposta
```

---

## 9. STACK DE TESTES

### 9.1 Frameworks Obrigatórios

| Pacote | Versão | Uso |
|--------|--------|-----|
| xUnit | 2.6+ | Framework de testes |
| FluentAssertions | 6.12+ | Assertions legíveis |
| Moq | 4.20+ | Mocking library |
| Microsoft.AspNetCore.Mvc.Testing | 8.0+ | WebApplicationFactory built-in |

### 9.2 Não Usar

```
❌ NUnit (verbose)
❌ Should (deprecado)
❌ AutoMocker (overkill)
❌ TestServer + IIS (use WebApplicationFactory)
❌ Entity Framework InMemory (mockar repositories)
❌ GitHub Actions (Phase 2+)
```

### 9.3 Estrutura de Projeto

```
tests/AlugueldeCarros.Tests/
├── AlugueldeCarros.Tests.csproj
├── Unit/Services/
├── Unit/Security/
├── Integration/Controllers/
├── Integration/Endpoints/
└── Fixtures/
```

### 9.4 Execução CLI

```bash
# Rodar todos os testes
dotnet test tests/AlugueldeCarros.Tests.csproj

# Teste específico
dotnet test --filter "ClassName=UserServiceTests"

# Watch mode
dotnet test --watch

# Verbose
dotnet test -v d
```

### 9.5 Cobertura

```
Services:     80%+
Controllers:  70%+
Security:     90%+
Repository:   60%+
DTOs/Enums:   0%
```

### 9.6 Sem CI/CD

Phase 1 = Testes locais apenas
GitHub Actions adicionado em Phase 2+

---

## 10. REGRAS PARA LOGGING

### 10.1 Configuração

```csharp
// Program.cs
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(
    app.Environment.IsDevelopment() 
        ? LogLevel.Debug 
        : LogLevel.Information);
```

### 10.2 Padrão de Uso

```csharp
public class UserService
{
    private readonly ILogger<UserService> _logger;

    public UserService(ILogger<UserService> logger)
    {
        _logger = logger;
    }

    public async Task<UserDto> CreateUserAsync(RegisterRequest request)
    {
        _logger.LogInformation("Tentativa de criar usuário com email: {Email}", 
            request.Email);

        try
        {
            // lógica
            _logger.LogInformation("Usuário criado com sucesso: {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar usuário: {Email}", request.Email);
            throw;
        }
    }
}
```

### 10.3 Níveis

```
Critical:      Erro fatal do sistema
Error:         Erro em operação (falha de negócio)
Warning:       Situação inesperada mas recuperável
Information:   Marcos importantes de operação
Debug:         Detalhes de execução (dev only)
Trace:         Mais verboso que Debug
```

---

## 11. REGRAS PARA NÃO SUGERIR TECNOLOGIAS FORA DO STACK

### 11.1 Bloqueadores de Sugestão

Se um proxy/agente/assistente sugerir:

```
❌ Node.js / Express               →  C#/.NET apenas
❌ Python / Django                 →  C#/.NET apenas
❌ Java / Spring                   →  C#/.NET apenas
❌ Entity Framework                →  Phase 2+ apenas
❌ Hangfire / RabbitMQ             →  Phase 3+ apenas
❌ Redis                           →  Phase 2+ apenas
❌ GraphQL                         →  Futuro, não Phase 1
❌ gRPC                            →  Não planejado
❌ Docker / Kubernetes             →  Futuro
❌ Authentication externo (Auth0)  →  JWT próprio apenas

Bloquear com mensagem:
"[STACK VIOLATION] A sugestão de {{technology}} viola 
o tech-stack.md. Alternativa para .NET 8: {{alternative}}"
```

---

## 12. STRUCTURE DE SOLUTION

### 12.1 .csproj

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" 
      Version="8.0.0" />
    <PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.4.0" />
  </ItemGroup>
</Project>
```

### 12.2 appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "JwtSettings": {
    "Secret": "your-secret-key-min-32-chars-long-for-hmac-sha256!!!",
    "ExpirationMinutes": 60,
    "Issuer": "AlugueldeCarros",
    "Audience": "AlugueldeCarrosApi"
  }
}
```

---

## 13. CHECKLIST DE CONFORMIDADE

```
✅ .NET 8.0
✅ C# 12+
✅ ASP.NET Core Web API
✅ JWT com BCrypt
✅ JSON mockado (memória)
✅ Clean Architecture (Controllers → Services → Repositories)
✅ RBAC via [Authorize(Roles = "...")]
✅ Swagger/OpenAPI documentado
✅ System.Text.Json com camelCase
✅ Global exception handling middleware
✅ ILogger para logging
✅ Sem Entity Framework (Phase 1)
✅ Sem banco real (Phase 1)
✅ Sem migrations (Phase 1)
```

---

## 14. CHANGELOG

| Data | Evento | Descrição |
|------|--------|-----------|
| 2026-03-25 | Criação | Tech stack oficial para Phase 1 |

