using AlugueldeCarros.Domain.Entities;
using AlugueldeCarros.Repositories;
using AlugueldeCarros.Services;
using FluentAssertions;
using Moq;

namespace AlugueldeCarros.Tests.Unit.Services;

public class VehicleCategoryServiceTests
{
    private readonly Mock<IVehicleCategoryRepository> _categoryRepositoryMock;
    private readonly VehicleCategoryService _vehicleCategoryService;

    public VehicleCategoryServiceTests()
    {
        _categoryRepositoryMock = new Mock<IVehicleCategoryRepository>();
        _vehicleCategoryService = new VehicleCategoryService(_categoryRepositoryMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_WithCategories_ReturnsRepositoryResult()
    {
        var categories = new List<VehicleCategory>
        {
            new() { Id = 1, Name = "Economico", Description = "Compactos" },
            new() { Id = 2, Name = "SUV", Description = "Utilitarios" }
        };

        _categoryRepositoryMock
            .Setup(repository => repository.GetAllAsync())
            .ReturnsAsync(categories);

        var result = await _vehicleCategoryService.GetAllAsync();

        result.Should().BeEquivalentTo(categories);
        _categoryRepositoryMock.Verify(repository => repository.GetAllAsync(), Times.Once);
    }
}