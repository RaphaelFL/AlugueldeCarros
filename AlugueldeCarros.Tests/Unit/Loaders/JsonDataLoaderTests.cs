using System.Text.Json;
using System.Text.Json.Serialization;
using AlugueldeCarros.Domain.Entities;
using AlugueldeCarros.Loaders;
using AlugueldeCarros.Repositories;
using FluentAssertions;
using Moq;

namespace AlugueldeCarros.Tests.Unit.Loaders;

public class JsonDataLoaderTests
{
    [Fact]
    public async Task LoadAllDataAsync_WithValidMockFiles_LoadsRepositoriesAndAppliesUserRoles()
    {
        var users = await ReadFromMockDataAsync<List<User>>("users.json") ?? new List<User>();
        var roleSeed = await ReadFromMockDataAsync<List<Role>>("roles.json") ?? new List<Role>();
        var branchSeed = await ReadFromMockDataAsync<List<Branch>>("branches.json") ?? new List<Branch>();
        var categorySeed = await ReadFromMockDataAsync<List<VehicleCategory>>("vehicle-categories.json") ?? new List<VehicleCategory>();
        var pricingSeed = await ReadFromMockDataAsync<List<PricingRule>>("pricing-rules.json") ?? new List<PricingRule>();
        var vehicleSeed = await ReadFromMockDataAsync<List<Vehicle>>("vehicles.json") ?? new List<Vehicle>();
        var reservationSeed = await ReadFromMockDataAsync<List<Reservation>>("reservations.json") ?? new List<Reservation>();
        var paymentSeed = await ReadFromMockDataAsync<List<Payment>>("payments.json") ?? new List<Payment>();
        var userRoleSeed = await ReadFromMockDataAsync<List<UserRole>>("user-roles.json") ?? new List<UserRole>();

        var roleRepository = new Mock<IRoleRepository>();
        var branchRepository = new Mock<IBranchRepository>();
        var vehicleCategoryRepository = new Mock<IVehicleCategoryRepository>();
        var pricingRuleRepository = new Mock<IPricingRuleRepository>();
        var vehicleRepository = new Mock<IVehicleRepository>();
        var reservationRepository = new Mock<IReservationRepository>();
        var paymentRepository = new Mock<IPaymentRepository>();
        var userRepository = new Mock<IUserRepository>();

        var loadedRoles = new List<Role>();
        var loadedBranches = new List<Branch>();
        var loadedCategories = new List<VehicleCategory>();
        var loadedPricingRules = new List<PricingRule>();
        var loadedVehicles = new List<Vehicle>();
        var loadedReservations = new List<Reservation>();
        var loadedPayments = new List<Payment>();

        roleRepository
            .Setup(repository => repository.AddAsync(It.IsAny<Role>()))
            .Callback<Role>(role => loadedRoles.Add(role))
            .Returns(Task.CompletedTask);

        roleRepository
            .Setup(repository => repository.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => loadedRoles.FirstOrDefault(role => role.Id == id));

        branchRepository
            .Setup(repository => repository.AddAsync(It.IsAny<Branch>()))
            .Callback<Branch>(branch => loadedBranches.Add(branch))
            .Returns(Task.CompletedTask);

        vehicleCategoryRepository
            .Setup(repository => repository.AddAsync(It.IsAny<VehicleCategory>()))
            .Callback<VehicleCategory>(category => loadedCategories.Add(category))
            .Returns(Task.CompletedTask);

        pricingRuleRepository
            .Setup(repository => repository.AddAsync(It.IsAny<PricingRule>()))
            .Callback<PricingRule>(rule => loadedPricingRules.Add(rule))
            .Returns(Task.CompletedTask);

        vehicleRepository
            .Setup(repository => repository.AddAsync(It.IsAny<Vehicle>()))
            .Callback<Vehicle>(vehicle => loadedVehicles.Add(vehicle))
            .Returns(Task.CompletedTask);

        reservationRepository
            .Setup(repository => repository.AddAsync(It.IsAny<Reservation>()))
            .Callback<Reservation>(reservation => loadedReservations.Add(reservation))
            .Returns(Task.CompletedTask);

        paymentRepository
            .Setup(repository => repository.AddAsync(It.IsAny<Payment>()))
            .Callback<Payment>(payment => loadedPayments.Add(payment))
            .Returns(Task.CompletedTask);

        userRepository
            .Setup(repository => repository.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => users.FirstOrDefault(user => user.Id == id));

        var loader = new JsonDataLoader(
            userRepository.Object,
            vehicleRepository.Object,
            reservationRepository.Object,
            paymentRepository.Object,
            roleRepository.Object,
            branchRepository.Object,
            vehicleCategoryRepository.Object,
            pricingRuleRepository.Object);

        await loader.LoadAllDataAsync();

        loadedRoles.Should().HaveCount(roleSeed.Count);
        loadedBranches.Should().HaveCount(branchSeed.Count);
        loadedCategories.Should().HaveCount(categorySeed.Count);
        loadedPricingRules.Should().HaveCount(pricingSeed.Count);
        loadedVehicles.Should().HaveCount(vehicleSeed.Count);
        loadedReservations.Should().HaveCount(reservationSeed.Count);
        loadedPayments.Should().HaveCount(paymentSeed.Count);

        foreach (var mapping in userRoleSeed)
        {
            var user = users.Single(candidate => candidate.Id == mapping.UserId);
            var role = loadedRoles.Single(candidate => candidate.Id == mapping.RoleId);
            user.Roles.Should().Contain(role.Name);
        }
    }

    private static async Task<T?> ReadFromMockDataAsync<T>(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Resources", "MockData", fileName);
        File.Exists(path).Should().BeTrue($"seed file '{fileName}' should be available in the test output");

        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<T>(stream, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        });
    }
}