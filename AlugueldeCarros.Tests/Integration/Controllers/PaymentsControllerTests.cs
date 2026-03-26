using System.Net;
using System.Text.Json;
using FluentAssertions;

namespace AlugueldeCarros.Tests.Integration.Controllers;

public class PaymentsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public PaymentsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetById_WithoutToken_ReturnsUnauthorized()
    {
        using var client = _factory.CreateApiClient();

        var response = await client.GetAsync("/api/v1/payments/1");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetById_WithOwnerToken_ReturnsPayment()
    {
        using var client = _factory.CreateApiClient();
        await _factory.AuthenticateAsync(client, "customer@example.com", "123456");

        var response = await client.GetAsync("/api/v1/payments/1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        document.RootElement.GetProperty("id").GetInt32().Should().Be(1);
        document.RootElement.GetProperty("reservationId").GetInt32().Should().Be(1);
    }

    [Fact]
    public async Task GetById_WithDifferentCustomerToken_ReturnsForbidden()
    {
        using var client = _factory.CreateApiClient();
        await _factory.AuthenticateAsync(client, "customer@example.com", "123456");

        var response = await client.GetAsync("/api/v1/payments/2");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}