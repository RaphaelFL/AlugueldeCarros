using System.Collections.Generic;
using AlugueldeCarros.Domain.Entities;
using AlugueldeCarros.Domain.Enums;
using AlugueldeCarros.Repositories;
using AlugueldeCarros.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace AlugueldeCarros.Tests.Unit.Services;

public class VehicleServiceTests
{
    private readonly Mock<IVehicleRepository> _vehicleRepositoryMock;
    private readonly VehicleService _vehicleService;

    public VehicleServiceTests()
    {
        _vehicleRepositoryMock = new Mock<IVehicleRepository>();
        _vehicleService = new VehicleService(_vehicleRepositoryMock.Object);
    }

    [Fact]
    public async Task GetAllVehiclesAsync_WithAvailableVehicles_ReturnsListOfVehicles()
    {
        var vehicles = new List<Vehicle>
        {
            new Vehicle
            {
                Id = 1,
                Model = "Fiat Uno",
                Status = VehicleStatus.AVAILABLE,
                CategoryId = 1
            },
            new Vehicle
            {
                Id = 2,
                Model = "Onix",
                Status = VehicleStatus.AVAILABLE,
                CategoryId = 1
            }
        };

        _vehicleRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(vehicles);

        var result = await _vehicleService.GetAllVehiclesAsync();

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(v => v.Status.Should().Be(VehicleStatus.AVAILABLE));

        _vehicleRepositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetVehicleByIdAsync_WithValidId_ReturnsVehicle()
    {
        var vehicle = new Vehicle
        {
            Id = 1,
            Model = "Fiat Uno",
            Status = VehicleStatus.AVAILABLE,
            CategoryId = 1
        };

        _vehicleRepositoryMock
            .Setup(r => r.GetByIdAsync(vehicle.Id))
            .ReturnsAsync(vehicle);

        var result = await _vehicleService.GetVehicleByIdAsync(vehicle.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(vehicle.Id);
        result.Model.Should().Be(vehicle.Model);
        result.Status.Should().Be(VehicleStatus.AVAILABLE);

        _vehicleRepositoryMock.Verify(r => r.GetByIdAsync(vehicle.Id), Times.Once);
    }

    [Fact]
    public async Task GetVehicleByIdAsync_WithNonExistentId_ReturnsNull()
    {
        var vehicleId = 999;

        _vehicleRepositoryMock
            .Setup(r => r.GetByIdAsync(vehicleId))
            .ReturnsAsync((Vehicle?)null);

        var result = await _vehicleService.GetVehicleByIdAsync(vehicleId);

        result.Should().BeNull();

        _vehicleRepositoryMock.Verify(r => r.GetByIdAsync(vehicleId), Times.Once);
    }

    [Fact]
    public async Task CreateVehicleAsync_WithValidVehicle_CreatesAndReturnsVehicle()
    {
        var vehicle = new Vehicle
        {
            Id = 1,
            Model = "Fiat Uno",
            Status = VehicleStatus.AVAILABLE,
            CategoryId = 1
        };

        _vehicleRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Vehicle>()))
            .Returns(Task.CompletedTask);

        var result = await _vehicleService.CreateVehicleAsync(vehicle);

        result.Should().NotBeNull();
        result.Model.Should().Be(vehicle.Model);
        result.Status.Should().Be(vehicle.Status);
        result.CategoryId.Should().Be(vehicle.CategoryId);

        _vehicleRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Vehicle>()), Times.Once);
    }

    [Fact]
    public async Task SearchVehiclesAsync_WithCategoryFilter_ReturnsFilteredVehicles()
    {
        var categoryId = 1;
        var vehicle = new Vehicle
        {
            Id = 1,
            Model = "Fiat Uno",
            Status = VehicleStatus.AVAILABLE,
            CategoryId = categoryId
        };

        _vehicleRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Vehicle> { vehicle });

        _vehicleRepositoryMock
            .Setup(r => r.SearchAsync(categoryId, null, null))
            .ReturnsAsync(new List<Vehicle> { vehicle });

        var result = await _vehicleService.SearchVehiclesAsync(null, null, null, categoryId, null, null);

        result.Should().HaveCount(1);
        result.First().CategoryId.Should().Be(categoryId);
    }

    [Fact]
    public async Task SearchVehiclesAsync_WithNoMatches_ReturnsEmptyList()
    {
        var categoryId = 1;

        _vehicleRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Vehicle>());

        _vehicleRepositoryMock
            .Setup(r => r.SearchAsync(categoryId, null, null))
            .ReturnsAsync(new List<Vehicle>());

        var result = await _vehicleService.SearchVehiclesAsync(null, null, null, categoryId, null, null);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchVehiclesAsync_WithBranchAndPriceRange_AppliesLocalFilters()
    {
        var vehicles = new List<Vehicle>
        {
            new() { Id = 1, Model = "Fiat Uno", BranchId = 1, CategoryId = 1, DailyRate = 50m, Status = VehicleStatus.AVAILABLE },
            new() { Id = 2, Model = "SUV", BranchId = 2, CategoryId = 1, DailyRate = 150m, Status = VehicleStatus.AVAILABLE },
            new() { Id = 3, Model = "Sedan", BranchId = 1, CategoryId = 2, DailyRate = 90m, Status = VehicleStatus.AVAILABLE }
        };

        _vehicleRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(vehicles);

        var result = await _vehicleService.SearchVehiclesAsync(1, null, null, null, 40m, 100m);

        result.Should().HaveCount(2);
        result.Should().OnlyContain(vehicle => vehicle.BranchId == 1 && vehicle.DailyRate >= 40m && vehicle.DailyRate <= 100m);
    }

    [Fact]
    public async Task SearchVehiclesAsync_WithDateRange_UsesRepositoryAvailabilityResult()
    {
        var availableVehicle = new Vehicle { Id = 1, Model = "Fiat Uno", BranchId = 1, CategoryId = 1, DailyRate = 50m, Status = VehicleStatus.AVAILABLE };
        var unavailableVehicle = new Vehicle { Id = 2, Model = "SUV", BranchId = 1, CategoryId = 1, DailyRate = 70m, Status = VehicleStatus.AVAILABLE };
        var startDate = DateTime.UtcNow.AddDays(1);
        var endDate = DateTime.UtcNow.AddDays(3);

        _vehicleRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Vehicle> { availableVehicle, unavailableVehicle });

        _vehicleRepositoryMock
            .Setup(r => r.SearchAsync(1, startDate, endDate))
            .ReturnsAsync(new List<Vehicle> { availableVehicle });

        var result = await _vehicleService.SearchVehiclesAsync(null, startDate, endDate, 1, null, null);

        result.Should().ContainSingle(vehicle => vehicle.Id == availableVehicle.Id);
        _vehicleRepositoryMock.Verify(r => r.SearchAsync(1, startDate, endDate), Times.Once);
    }

    [Fact]
    public async Task UpdateVehicleAsync_WithValidId_UpdatesAndReturnsVehicle()
    {
        var vehicleId = 1;
        var existingVehicle = new Vehicle
        {
            Id = vehicleId,
            Model = "Fiat Uno",
            Status = VehicleStatus.AVAILABLE,
            CategoryId = 1
        };

        var updatedVehicle = new Vehicle
        {
            Model = "Toyota Corolla",
            Status = VehicleStatus.AVAILABLE,
            CategoryId = 2
        };

        _vehicleRepositoryMock
            .Setup(r => r.GetByIdAsync(vehicleId))
            .ReturnsAsync(existingVehicle);

        _vehicleRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Vehicle>()))
            .Returns(Task.CompletedTask);

        var result = await _vehicleService.UpdateVehicleAsync(vehicleId, updatedVehicle);

        result.Should().NotBeNull();
        result.Id.Should().Be(vehicleId);
        result.Model.Should().Be(updatedVehicle.Model);
        result.CategoryId.Should().Be(updatedVehicle.CategoryId);

        _vehicleRepositoryMock.Verify(r => r.GetByIdAsync(vehicleId), Times.Once);
        _vehicleRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Vehicle>()), Times.Once);
    }

    [Fact]
    public async Task UpdateVehicleAsync_WithNonExistentId_ThrowsKeyNotFoundException()
    {
        var vehicleId = 999;
        var vehicle = new Vehicle { Model = "Toyota Corolla" };

        _vehicleRepositoryMock
            .Setup(r => r.GetByIdAsync(vehicleId))
            .ReturnsAsync((Vehicle)null);

        var act = async () => await _vehicleService.UpdateVehicleAsync(vehicleId, vehicle);

        await act.Should().ThrowAsync<KeyNotFoundException>();

        _vehicleRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Vehicle>()), Times.Never);
    }
}