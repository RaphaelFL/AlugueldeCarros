using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using FluentAssertions;

namespace AlugueldeCarros.Tests.Integration.Controllers;

public class UserControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public UserControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetCurrentUser_WithoutToken_ReturnsUnauthorized()
    {
        using var client = _factory.CreateApiClient();

        var response = await client.GetAsync("/api/v1/users/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCurrentUser_WithValidToken_ReturnsCurrentUserProfile()
    {
        using var client = _factory.CreateApiClient();
        await _factory.AuthenticateAsync(client, "customer@example.com", "123456");

        var response = await client.GetAsync("/api/v1/users/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        document.RootElement.GetProperty("email").GetString().Should().Be("customer@example.com");
        document.RootElement.GetProperty("roles").EnumerateArray().Select(x => x.GetString()).Should().Contain("Customer");
    }

    [Fact]
    public async Task GetCurrentUser_WithExpiredToken_ReturnsUnauthorized()
    {
        using var client = _factory.CreateApiClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CustomWebApplicationFactory.CreateExpiredToken(1, "customer@example.com", "Customer"));

        var response = await client.GetAsync("/api/v1/users/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMyReservations_WithValidToken_ReturnsOnlyCurrentUserReservations()
    {
        using var client = _factory.CreateApiClient();
        await _factory.AuthenticateAsync(client, "customer@example.com", "123456");

        var response = await client.GetAsync("/api/v1/users/me/reservations");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var reservations = document.RootElement.EnumerateArray().ToList();
        reservations.Should().NotBeEmpty();
        reservations.Select(reservation => reservation.GetProperty("userId").GetInt32()).Should().OnlyContain(userId => userId == 1);
    }
}