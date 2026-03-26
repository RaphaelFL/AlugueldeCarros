using System.Net;
using System.Text.Json;
using FluentAssertions;

namespace AlugueldeCarros.Tests.Integration.Controllers;

public class PublicCatalogControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public PublicCatalogControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetBranches_ReturnsConfiguredBranches()
    {
        using var client = _factory.CreateApiClient();

        var response = await client.GetAsync("/api/v1/branches");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        document.RootElement.EnumerateArray().Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetCategories_ReturnsConfiguredCategories()
    {
        using var client = _factory.CreateApiClient();

        var response = await client.GetAsync("/api/v1/vehicles/categories");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        document.RootElement.EnumerateArray().Select(category => category.GetProperty("name").GetString()).Should().Contain(new[] { "Econômico", "SUV" });
    }

    [Fact]
    public async Task SearchVehicles_WithFilters_ReturnsMatchingVehiclesOnly()
    {
        using var client = _factory.CreateApiClient();

        var response = await client.GetAsync("/api/v1/vehicles/search?branchId=1&categoryId=1&priceMax=60");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var vehicles = document.RootElement.EnumerateArray().ToList();
        vehicles.Should().ContainSingle();
        vehicles[0].GetProperty("model").GetString().Should().Be("Fiat Uno");
    }

    [Fact]
    public async Task GetVehicleById_WithUnknownId_ReturnsNotFound()
    {
        using var client = _factory.CreateApiClient();

        var response = await client.GetAsync("/api/v1/vehicles/999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPricingRules_ReturnsConfiguredRules()
    {
        using var client = _factory.CreateApiClient();

        var response = await client.GetAsync("/api/v1/pricing/rules");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        document.RootElement.EnumerateArray().Should().NotBeEmpty();
    }
}