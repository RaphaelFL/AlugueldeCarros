using AlugueldeCarros;
using AlugueldeCarros.Data;
using AlugueldeCarros.Domain.Entities;
using AlugueldeCarros.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AlugueldeCarros.Tests.Integration.Controllers;

public class CustomWebApplicationFactory : WebApplicationFactory<global::Program>
{
    private readonly string _dbName = $"IntegrationTestDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Mark that we're in test environment to skip DbInitializer
            services.AddSingleton<ITestEnvironmentMarker, TestEnvironmentMarker>();
            System.Diagnostics.Debug.WriteLine("[CustomWebApplicationFactory] Registered ITestEnvironmentMarker");

            // Remove ALL DbContext registrations to prevent conflicts
            var allDbContextDescriptors = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                d.ServiceType == typeof(DbContextOptions) ||
                d.ServiceType == typeof(AppDbContext) ||
                (d.ServiceType.IsGenericType && d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>))
            ).ToList();

            foreach (var descriptor in allDbContextDescriptors)
            {
                services.Remove(descriptor);
                Console.WriteLine($"[Factory] Removed service: {descriptor.ServiceType.Name}");
            }

            // Remove InMemory JSON-based repositories and replace with DbContext-based
            var repositoryTypes = new[] { typeof(IUserRepository), typeof(IReservationRepository), typeof(IVehicleRepository), 
                                         typeof(IPaymentRepository), typeof(IBranchRepository), typeof(IVehicleCategoryRepository),
                                         typeof(IPricingRuleRepository), typeof(IRoleRepository) };
            
            foreach (var repoType in repositoryTypes)
            {
                var descriptor = services.FirstOrDefault(d => d.ServiceType == repoType);
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                    Console.WriteLine($"[Factory] Removed repository: {repoType.Name}");
                }
            }

            // Add in-memory database for testing with unique name per factory instance
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(_dbName);
                Console.WriteLine($"[Factory] Registered AppDbContext with in-memory database: {_dbName}");
            }, ServiceLifetime.Scoped);

            // Register DbContext-based repositories
            services.AddScoped<IUserRepository, DbContextUserRepository>();
            services.AddScoped<IReservationRepository, DbContextReservationRepository>();
            services.AddScoped<IVehicleRepository, DbContextVehicleRepository>();
            services.AddScoped<IPaymentRepository, DbContextPaymentRepository>();
            services.AddScoped<IBranchRepository, DbContextBranchRepository>();
            services.AddScoped<IVehicleCategoryRepository, DbContextVehicleCategoryRepository>();
            services.AddScoped<IPricingRuleRepository, DbContextPricingRuleRepository>();
            services.AddScoped<IRoleRepository, DbContextRoleRepository>();

            // Build the service provider
            var sp = services.BuildServiceProvider();

            // Create a scope to get the DbContext and seed data
            using var scope = sp.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            context.Database.EnsureCreated();

            // Seed test data if not already present
            SeedTestData(context);
        });

        // DO NOT call base.ConfigureWebHost - it would re-add SqlLite configuration
        // base.ConfigureWebHost(builder);
    }

    private void SeedTestData(AppDbContext context)
    {
        // Check if data already exists
        if (context.Users.Any())
        {
            var existingUser = context.Users.FirstOrDefault(u => u.Email == "customer@example.com");
            if (existingUser != null)
            {
                System.Diagnostics.Debug.WriteLine($"✓ Seed data already exists. Customer user found: {existingUser.Email}, Hash length: {existingUser.PasswordHash.Length}");
            }
            return;
        }

        System.Diagnostics.Debug.WriteLine("✓ Starting seed data...");

        // Add test roles
        var adminRole = new Role { Name = "Admin" };
        var customerRole = new Role { Name = "Customer" };

        context.Roles.AddRange(adminRole, customerRole);
        context.SaveChanges();

        // Add test admin user
        var adminHash = PasswordHasher.HashPassword("admin123");
        System.Diagnostics.Debug.WriteLine($"✓ Admin hash created: {adminHash}");
        
        var adminUser = new User
        {
            Email = "admin@aluguel.com",
            FirstName = "Admin",
            LastName = "Sistema",
            PasswordHash = adminHash,
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(adminUser);
        context.SaveChanges();

        // Add admin role to admin user
        var adminUserRole = new UserRole
        {
            UserId = adminUser.Id,
            RoleId = adminRole.Id
        };

        context.UserRoles.Add(adminUserRole);
        context.SaveChanges();

        // Add test customer user
        var customerHash = PasswordHasher.HashPassword("123456");
        Console.WriteLine($"[Factory] Customer hash created: {customerHash}");
        
        var customerUser = new User
        {
            Email = "customer@example.com",
            FirstName = "João",
            LastName = "Cliente",
            PasswordHash = customerHash,
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(customerUser);
        context.SaveChanges();
        
        // Verify what was actually saved
        var savedUser = context.Users.FirstOrDefault(u => u.Email == "customer@example.com");
        if (savedUser != null)
        {
            Console.WriteLine($"[Factory] Customer user saved - PasswordHash: {savedUser.PasswordHash}");
            Console.WriteLine($"[Factory] Created={customerHash} Saved={savedUser.PasswordHash} Match={savedUser.PasswordHash == customerHash}");
        }

        // Add customer role to customer user
        var customerUserRole = new UserRole
        {
            UserId = customerUser.Id,
            RoleId = customerRole.Id
        };

        context.UserRoles.Add(customerUserRole);
        context.SaveChanges();
        
        System.Diagnostics.Debug.WriteLine($"✓ Seed data completed successfully");
    }
}

public class TestEnvironmentMarker : ITestEnvironmentMarker
{
}

