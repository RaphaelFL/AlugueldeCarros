using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace AlugueldeCarros.Tests.Integration.Controllers;

public class AdminEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AdminEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAllUsers_WithoutToken_ReturnsUnauthorized()
    {
        using var client = _factory.CreateApiClient();

        var response = await client.GetAsync("/api/v1/admin/users");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllUsers_WithCustomerToken_ReturnsForbidden()
    {
        using var client = _factory.CreateApiClient();
        await _factory.AuthenticateAsync(client, "customer@example.com", "123456");

        var response = await client.GetAsync("/api/v1/admin/users");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAllUsers_WithAdminToken_ReturnsUsers()
    {
        using var client = _factory.CreateApiClient();
        await _factory.AuthenticateAsync(client, "admin@aluguel.com", "admin123");

        var response = await client.GetAsync("/api/v1/admin/users");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        document.RootElement.EnumerateArray().Select(user => user.GetProperty("email").GetString()).Should().Contain(new[]
        {
            "customer@example.com",
            "admin@aluguel.com"
        });
    }

    [Fact]
    public async Task AddRolesToUser_WithEmptyRoles_ReturnsBadRequest()
    {
        using var client = _factory.CreateApiClient();
        await _factory.AuthenticateAsync(client, "admin@aluguel.com", "admin123");

        var response = await client.PostAsJsonAsync("/api/v1/admin/users/1/roles", new
        {
            Roles = Array.Empty<string>()
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateVehicle_WithCustomerToken_ReturnsForbidden()
    {
        using var client = _factory.CreateApiClient();
        await _factory.AuthenticateAsync(client, "customer@example.com", "123456");

        var response = await client.PostAsJsonAsync("/api/v1/admin/vehicles", new
        {
            LicensePlate = "TES-0001",
            Model = "Test Car",
            Year = 2025,
            CategoryId = 1,
            BranchId = 1,
            DailyRate = 99.9m,
            Status = 0
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}