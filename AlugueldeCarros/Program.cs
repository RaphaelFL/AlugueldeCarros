using AlugueldeCarros.Configurations;
using AlugueldeCarros.Loaders;
using AlugueldeCarros.Middleware;
using AlugueldeCarros.Repositories;
using AlugueldeCarros.Security;
using AlugueldeCarros.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Aluguel de Carros API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
});

// JWT Configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
                 ?? throw new InvalidOperationException("JwtSettings n�o foi configurado.");

builder.Services.AddSingleton(jwtSettings);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
        };
    });

builder.Services.AddAuthorization();

// Dependency Injection
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();
builder.Services.AddSingleton<IVehicleRepository, InMemoryVehicleRepository>();
builder.Services.AddSingleton<IReservationRepository, InMemoryReservationRepository>();
builder.Services.AddSingleton<IPaymentRepository, InMemoryPaymentRepository>();
builder.Services.AddSingleton<IBranchRepository, InMemoryBranchRepository>();
builder.Services.AddSingleton<IVehicleCategoryRepository, InMemoryVehicleCategoryRepository>();
builder.Services.AddSingleton<IPricingRuleRepository, InMemoryPricingRuleRepository>();
builder.Services.AddSingleton<IRoleRepository, InMemoryRoleRepository>();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<VehicleService>();
builder.Services.AddScoped<ReservationService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<PricingService>();
builder.Services.AddScoped<BranchService>();
builder.Services.AddScoped<VehicleCategoryService>();

builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddSingleton<JsonDataLoader>();

var app = builder.Build();

// Load mock data
var dataLoader = app.Services.GetRequiredService<JsonDataLoader>();
await dataLoader.LoadAllDataAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();