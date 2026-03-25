using System.Text.Json;
using System.Text.Json.Serialization;
using AlugueldeCarros.Domain.Entities;
using AlugueldeCarros.Repositories;

namespace AlugueldeCarros.Loaders;

public class JsonDataLoader
{
    private readonly IConfiguration _configuration;
    private readonly IUserRepository _userRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IVehicleCategoryRepository _categoryRepository;
    private readonly IPricingRuleRepository _pricingRuleRepository;

    public JsonDataLoader(
        IConfiguration configuration,
        IUserRepository userRepository,
        IVehicleRepository vehicleRepository,
        IReservationRepository reservationRepository,
        IPaymentRepository paymentRepository,
        IRoleRepository roleRepository,
        IBranchRepository branchRepository,
        IVehicleCategoryRepository categoryRepository,
        IPricingRuleRepository pricingRuleRepository)
    {
        _configuration = configuration;
        _userRepository = userRepository;
        _vehicleRepository = vehicleRepository;
        _reservationRepository = reservationRepository;
        _paymentRepository = paymentRepository;
        _roleRepository = roleRepository;
        _branchRepository = branchRepository;
        _categoryRepository = categoryRepository;
        _pricingRuleRepository = pricingRuleRepository;
    }

    public async Task LoadAllDataAsync()
    {
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        jsonOptions.Converters.Add(new JsonStringEnumConverter());

        var baseFolder = Path.Combine(AppContext.BaseDirectory, "Resources", "MockData");

        // Roles
        var rolesPath = Path.Combine(baseFolder, "roles.json");
        if (File.Exists(rolesPath))
        {
            var json = await File.ReadAllTextAsync(rolesPath);
            var roles = JsonSerializer.Deserialize<List<Role>>(json, jsonOptions) ?? new List<Role>();
            foreach (var role in roles)
                await _roleRepository.AddAsync(role);
        }

        // Load users from users.json (already handled by repository initialization)
        // Load user roles mapping from separate file and apply to each user.
        var userRolesPath = Path.Combine(baseFolder, "user-roles.json");
        if (File.Exists(userRolesPath))
        {
            var json = await File.ReadAllTextAsync(userRolesPath);
            var userRoleMappings = JsonSerializer.Deserialize<List<UserRole>>(json, jsonOptions) ?? new List<UserRole>();

            foreach (var mapping in userRoleMappings)
            {
                var user = await _userRepository.GetByIdAsync(mapping.UserId);
                var role = await _roleRepository.GetByIdAsync(mapping.RoleId);
                if (user != null && role != null)
                {
                    user.Roles ??= new List<string>();
                    if (!user.Roles.Contains(role.Name, StringComparer.OrdinalIgnoreCase))
                        user.Roles.Add(role.Name);
                }
            }
        }

        // Branches
        var branchesPath = Path.Combine(baseFolder, "branches.json");
        if (File.Exists(branchesPath))
        {
            var json = await File.ReadAllTextAsync(branchesPath);
            var branches = JsonSerializer.Deserialize<List<Branch>>(json, jsonOptions) ?? new List<Branch>();
            foreach (var branch in branches)
                await _branchRepository.AddAsync(branch);
        }

        // Vehicle categories
        var categoriesPath = Path.Combine(baseFolder, "vehicle-categories.json");
        if (File.Exists(categoriesPath))
        {
            var json = await File.ReadAllTextAsync(categoriesPath);
            var categories = JsonSerializer.Deserialize<List<VehicleCategory>>(json, jsonOptions) ?? new List<VehicleCategory>();
            foreach (var category in categories)
                await _categoryRepository.AddAsync(category);
        }

        // Pricing rules
        var pricingPath = Path.Combine(baseFolder, "pricing-rules.json");
        if (File.Exists(pricingPath))
        {
            var json = await File.ReadAllTextAsync(pricingPath);
            var pricingRules = JsonSerializer.Deserialize<List<PricingRule>>(json, jsonOptions) ?? new List<PricingRule>();
            foreach (var rule in pricingRules)
                await _pricingRuleRepository.AddAsync(rule);
        }

        // Vehicles
        var vehiclesPath = Path.Combine(baseFolder, "vehicles.json");
        if (File.Exists(vehiclesPath))
        {
            var json = await File.ReadAllTextAsync(vehiclesPath);
            var vehicles = JsonSerializer.Deserialize<List<Vehicle>>(json, jsonOptions) ?? new List<Vehicle>();
            foreach (var vehicle in vehicles)
                await _vehicleRepository.AddAsync(vehicle);
        }

        // Reservations
        var reservationsPath = Path.Combine(baseFolder, "reservations.json");
        if (File.Exists(reservationsPath))
        {
            var json = await File.ReadAllTextAsync(reservationsPath);
            var reservations = JsonSerializer.Deserialize<List<Reservation>>(json, jsonOptions) ?? new List<Reservation>();
            foreach (var reservation in reservations)
                await _reservationRepository.AddAsync(reservation);
        }

        // Payments
        var paymentsPath = Path.Combine(baseFolder, "payments.json");
        if (File.Exists(paymentsPath))
        {
            var json = await File.ReadAllTextAsync(paymentsPath);
            var payments = JsonSerializer.Deserialize<List<Payment>>(json, jsonOptions) ?? new List<Payment>();
            foreach (var payment in payments)
                await _paymentRepository.AddAsync(payment);
        }
    }
}