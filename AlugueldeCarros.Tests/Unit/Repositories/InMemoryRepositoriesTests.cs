using AlugueldeCarros.Domain.Entities;
using AlugueldeCarros.Domain.Enums;
using AlugueldeCarros.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;

namespace AlugueldeCarros.Tests.Unit.Repositories;

[Collection("CurrentDirectoryTests")]
public class InMemoryRepositoriesTests
{
    [Fact]
    public async Task BranchRepository_ShouldAddUpdateAndDelete()
    {
        var repository = new InMemoryBranchRepository();

        await repository.AddAsync(new Branch { Name = "Centro", Address = "Rua A", Phone = "1111" });
        var branch = await repository.GetByIdAsync(1);
        branch!.Name.Should().Be("Centro");

        branch.Name = "Centro Novo";
        await repository.UpdateAsync(branch);
        (await repository.GetByIdAsync(1))!.Name.Should().Be("Centro Novo");

        await repository.DeleteAsync(1);
        (await repository.GetAllAsync()).Should().BeEmpty();
    }

    [Fact]
    public async Task PaymentRepository_ShouldAddAndUpdate()
    {
        var repository = new InMemoryPaymentRepository();

        await repository.AddAsync(new Payment { ReservationId = 1, Amount = 100m, Status = PaymentStatus.PENDING, CreatedAt = DateTime.UtcNow });
        var payment = await repository.GetByIdAsync(1);

        payment!.Status = PaymentStatus.APPROVED;
        await repository.UpdateAsync(payment);

        (await repository.GetByIdAsync(1))!.Status.Should().Be(PaymentStatus.APPROVED);
    }

    [Fact]
    public async Task PricingRuleRepository_ShouldPerformCrud()
    {
        var repository = new InMemoryPricingRuleRepository();

        await repository.AddAsync(new PricingRule { CategoryId = 1, BaseDailyRate = 100m, WeekendMultiplier = 1.3m, PeakSeasonMultiplier = 1.6m });
        (await repository.GetAllAsync()).Should().HaveCount(1);

        var rule = await repository.GetByIdAsync(1);
        rule!.WeekendMultiplier = 1.6m;
        await repository.UpdateAsync(rule);

        (await repository.GetByIdAsync(1))!.WeekendMultiplier.Should().Be(1.6m);
        await repository.DeleteAsync(1);
        (await repository.GetAllAsync()).Should().BeEmpty();
    }

    [Fact]
    public async Task ReservationRepository_ShouldFilterByUserId()
    {
        var repository = new InMemoryReservationRepository();

        await repository.AddAsync(new Reservation { UserId = 1, CategoryId = 1, Status = ReservationStatus.PENDING_PAYMENT, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(2) });
        await repository.AddAsync(new Reservation { UserId = 2, CategoryId = 1, Status = ReservationStatus.CONFIRMED, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(2) });

        var reservations = await repository.GetByUserIdAsync(1);

        reservations.Should().ContainSingle(r => r.UserId == 1);
    }

    [Fact]
    public async Task ReservationRepository_ShouldUpdateDeleteAndListAll()
    {
        var repository = new InMemoryReservationRepository();

        await repository.AddAsync(new Reservation { UserId = 1, CategoryId = 1, Status = ReservationStatus.PENDING_PAYMENT, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(1) });
        await repository.AddAsync(new Reservation { UserId = 1, CategoryId = 2, Status = ReservationStatus.CONFIRMED, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(2) });

        (await repository.GetAllAsync()).Should().HaveCount(2);

        var reservation = await repository.GetByIdAsync(1);
        reservation!.Status = ReservationStatus.CANCELLED;
        await repository.UpdateAsync(reservation);
        (await repository.GetByIdAsync(1))!.Status.Should().Be(ReservationStatus.CANCELLED);

        await repository.DeleteAsync(2);
        (await repository.GetAllAsync()).Should().HaveCount(1);
    }

    [Fact]
    public async Task RoleRepository_ShouldGetByNameIgnoringCase()
    {
        var repository = new InMemoryRoleRepository();

        await repository.AddAsync(new Role { Name = "Admin" });

        var role = await repository.GetByNameAsync("admin");

        role.Should().NotBeNull();
        role!.Name.Should().Be("Admin");
    }

    [Fact]
    public async Task RoleRepository_ShouldGetByIdAndListAll()
    {
        var repository = new InMemoryRoleRepository();

        await repository.AddAsync(new Role { Name = "Customer" });
        await repository.AddAsync(new Role { Name = "Admin" });

        (await repository.GetAllAsync()).Should().HaveCount(2);
        (await repository.GetByIdAsync(2))!.Name.Should().Be("Admin");
    }

    [Fact]
    public async Task VehicleCategoryRepository_ShouldPerformCrud()
    {
        var repository = new InMemoryVehicleCategoryRepository();

        await repository.AddAsync(new VehicleCategory { Name = "SUV", Description = "Esportivos utilitarios" });
        var category = await repository.GetByIdAsync(1);
        category!.Description.Should().Be("Esportivos utilitarios");

        category.Description = "SUV atualizado";
        await repository.UpdateAsync(category);
        (await repository.GetByIdAsync(1))!.Description.Should().Be("SUV atualizado");

        await repository.DeleteAsync(1);
        (await repository.GetAllAsync()).Should().BeEmpty();
    }

    [Fact]
    public async Task VehicleRepository_SearchShouldReturnOnlyAvailableVehiclesWithoutOverlap()
    {
        var reservationRepository = new Mock<IReservationRepository>();
        reservationRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new[]
        {
            new Reservation
            {
                Id = 1,
                VehicleId = 1,
                UserId = 1,
                CategoryId = 1,
                Status = ReservationStatus.CONFIRMED,
                StartDate = new DateTime(2026, 4, 10),
                EndDate = new DateTime(2026, 4, 12)
            }
        });

        var repository = new InMemoryVehicleRepository(reservationRepository.Object);
        await repository.AddAsync(new Vehicle { LicensePlate = "AAA-0001", Model = "Car A", Year = 2025, CategoryId = 1, BranchId = 1, DailyRate = 100m, Status = VehicleStatus.AVAILABLE });
        await repository.AddAsync(new Vehicle { LicensePlate = "BBB-0002", Model = "Car B", Year = 2025, CategoryId = 1, BranchId = 1, DailyRate = 120m, Status = VehicleStatus.AVAILABLE });
        await repository.AddAsync(new Vehicle { LicensePlate = "CCC-0003", Model = "Car C", Year = 2025, CategoryId = 1, BranchId = 1, DailyRate = 140m, Status = VehicleStatus.MAINTENANCE });

        var result = await repository.SearchAsync(1, new DateTime(2026, 4, 11), new DateTime(2026, 4, 13));

        result.Should().ContainSingle();
        result.Single().LicensePlate.Should().Be("BBB-0002");
    }

    [Fact]
    public async Task UserRepository_ShouldLoadUsersAndSearchByEmailIgnoringCase()
    {
        await WithTempUsersJsonAsync(async _ =>
        {
            var repository = new InMemoryUserRepository(new ConfigurationBuilder().Build());

            var userById = await repository.GetByIdAsync(1);
            var userByEmail = await repository.GetByEmailAsync("USER@TEST.COM");
            var allUsers = await repository.GetAllAsync();

            userById.Should().NotBeNull();
            userByEmail.Should().NotBeNull();
            allUsers.Should().HaveCount(2);
        });
    }

    [Fact]
    public async Task UserRepository_ShouldAddUpdateAndDeletePersistingFile()
    {
        await WithTempUsersJsonAsync(async usersFilePath =>
        {
            var repository = new InMemoryUserRepository(new ConfigurationBuilder().Build());

            await repository.AddAsync(new User
            {
                Email = "new@test.com",
                PasswordHash = "hash",
                FirstName = "New",
                LastName = "User",
                Roles = new List<string> { "Customer" },
                CreatedAt = DateTime.UtcNow
            });

            var created = await repository.GetByEmailAsync("new@test.com");
            created.Should().NotBeNull();
            created!.Id.Should().Be(3);

            created.LastName = "Updated";
            await repository.UpdateAsync(created);
            (await repository.GetByIdAsync(3))!.LastName.Should().Be("Updated");

            await repository.DeleteAsync(3);
            (await repository.GetByIdAsync(3)).Should().BeNull();

            var persistedContent = await File.ReadAllTextAsync(usersFilePath);
            persistedContent.Should().NotContain("new@test.com");
        });
    }

    private static async Task WithTempUsersJsonAsync(Func<string, Task> action)
    {
        var originalDirectory = Directory.GetCurrentDirectory();
        var tempRoot = Path.Combine(Path.GetTempPath(), $"alugueldecarros-tests-{Guid.NewGuid():N}");
        var mockDataDirectory = Path.Combine(tempRoot, "Resources", "MockData");
        Directory.CreateDirectory(mockDataDirectory);

        var usersFilePath = Path.Combine(mockDataDirectory, "users.json");
        await File.WriteAllTextAsync(usersFilePath, """
[
  {
    "Id": 1,
    "Email": "user@test.com",
    "PasswordHash": "hash-1",
    "FirstName": "User",
    "LastName": "One",
    "CreatedAt": "2026-01-01T00:00:00Z",
    "Roles": ["Customer"]
  },
  {
    "Id": 2,
    "Email": "admin@test.com",
    "PasswordHash": "hash-2",
    "FirstName": "Admin",
    "LastName": "Two",
    "CreatedAt": "2026-01-02T00:00:00Z",
    "Roles": ["Admin"]
  }
]
""");

        Directory.SetCurrentDirectory(tempRoot);

        try
        {
            await action(usersFilePath);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, recursive: true);
            }
        }
    }
}