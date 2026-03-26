using AlugueldeCarros.Security;
using AlugueldeCarros.Configurations;
using AlugueldeCarros.Domain.Entities;
using FluentAssertions;
using System.IdentityModel.Tokens.Jwt;

namespace AlugueldeCarros.Tests.Unit.Security;

public class JwtTokenServiceTests
{
    private readonly JwtTokenService _jwtTokenService;

    public JwtTokenServiceTests()
    {
        var jwtSettings = new JwtSettings
        {
            SecretKey = "test-secret-key-min-32-chars-long-for-testing!!!!",
            ExpiryInMinutes = 60,
            Issuer = "AlugueldeCarros",
            Audience = "AlugueldeCarrosApi"
        };
        _jwtTokenService = new JwtTokenService(jwtSettings);
    }

    [Fact]
    public void GenerateToken_WithValidUser_ReturnsValidJwtToken()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "user@test.com",
            FirstName = "Test",
            LastName = "User"
        };
        var roles = new List<string> { "Customer" };

        // Act
        var token = _jwtTokenService.GenerateToken(user, roles);

        // Assert
        token.Should().NotBeNullOrEmpty();
        this.IsTokenValid(token).Should().BeTrue();
    }

    [Fact]
    public void GenerateToken_WithAdminRole_IncludesAdminClaimInToken()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "admin@test.com",
            FirstName = "Admin",
            LastName = "User"
        };
        var roles = new List<string> { "Admin" };

        // Act
        var token = _jwtTokenService.GenerateToken(user, roles);

        // Assert
        token.Should().NotBeNullOrEmpty();
        this.IsTokenValid(token).Should().BeTrue();
    }

    [Fact]
    public void GenerateToken_TokenHasCorrectFormat()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "user@test.com",
            FirstName = "Test",
            LastName = "User"
        };
        var roles = new List<string> { "Customer" };

        // Act
        var token = _jwtTokenService.GenerateToken(user, roles);

        // Assert
        var parts = token.Split('.');
        parts.Should().HaveCount(3);
    }

    [Fact]
    public void GenerateToken_WithMultipleRoles_IncludesAllRolesInToken()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "user@test.com",
            FirstName = "Test",
            LastName = "User"
        };
        var roles = new List<string> { "Customer", "Admin" };

        // Act
        var token = _jwtTokenService.GenerateToken(user, roles);

        // Assert
        token.Should().NotBeNullOrEmpty();
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var roleClaims = jwtToken.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role).ToList();
        roleClaims.Should().HaveCount(2);
    }

    private bool IsTokenValid(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            return jwtToken.ValidTo > DateTime.UtcNow;
        }
        catch
        {
            return false;
        }
    }
}
