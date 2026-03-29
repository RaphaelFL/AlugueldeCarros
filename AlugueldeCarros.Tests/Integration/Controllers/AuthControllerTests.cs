using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace AlugueldeCarros.Tests.Integration.Controllers;

public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokenAndEmail()
    {
        using var client = _factory.CreateApiClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            Email = "customer@example.com",
            Password = "123456"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        document.RootElement.GetProperty("token").GetString().Should().NotBeNullOrWhiteSpace();
        document.RootElement.GetProperty("email").GetString().Should().Be("customer@example.com");
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        using var client = _factory.CreateApiClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            Email = "customer@example.com",
            Password = "wrong-password"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        document.RootElement.GetProperty("error").GetString().Should().Be("Invalid credentials");
    }

    [Fact]
    public async Task Login_WithInvalidPayload_ReturnsBadRequestWithValidationDetails()
    {
        using var client = _factory.CreateApiClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            Email = "invalid-email",
            Password = "123"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        document.RootElement.GetProperty("error").GetString().Should().Be("Validation failed");
        document.RootElement.GetProperty("details").TryGetProperty("Password", out _).Should().BeTrue();
    }

    [Fact]
    public async Task Refresh_WithValidToken_ReturnsNewToken()
    {
        using var client = _factory.CreateApiClient();
        var token = await _factory.LoginAndGetTokenAsync(client, "customer@example.com", "123456");

        var response = await client.PostAsJsonAsync("/api/v1/auth/refresh", token);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        document.RootElement.GetProperty("token").GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Refresh_WithInvalidToken_ReturnsUnauthorized()
    {
        using var client = _factory.CreateApiClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/refresh", "invalid.token.here");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        document.RootElement.GetProperty("error").GetString().Should().Be("Invalid token");
    }

    [Fact]
    public async Task Register_WithExistingEmail_ReturnsBadRequest()
    {
        using var client = _factory.CreateApiClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            Email = "customer@example.com",
            Password = "123456",
            FirstName = "Joao",
            LastName = "Silva"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        document.RootElement.GetProperty("error").GetString().Should().Be("User already exists");
    }
}