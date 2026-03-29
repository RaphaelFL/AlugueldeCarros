using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using AlugueldeCarros.Configurations;
using AlugueldeCarros.Domain.Entities;
using AlugueldeCarros.Repositories;
using AlugueldeCarros.Security;
using AlugueldeCarros.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace AlugueldeCarros.Tests.Unit.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly JwtTokenService _jwtTokenService;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _jwtTokenService = CreateJwtTokenService();
        _authService = new AuthService(_userRepositoryMock.Object, _jwtTokenService);
    }

    #region Login Tests

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsJwtToken()
    {
        var email = "user@test.com";
        var password = "ValidPassword@123";
        var user = CreateUser(email, password);

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(email))
            .ReturnsAsync(user);

        var result = await _authService.LoginAsync(email, password);

        result.Should().NotBeNullOrWhiteSpace();
        _userRepositoryMock.Verify(r => r.GetByEmailAsync(email), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentEmail_ThrowsUnauthorizedAccessException()
    {
        var email = "nonexistent@test.com";
        var password = "AnyPassword@123";

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(email))
            .ReturnsAsync((User)null);

        var act = async () => await _authService.LoginAsync(email, password);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        _userRepositoryMock.Verify(r => r.GetByEmailAsync(email), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ThrowsUnauthorizedAccessException()
    {
        var email = "user@test.com";
        var correctPassword = "ValidPassword@123";
        var wrongPassword = "WrongPassword@123";
        var user = CreateUser(email, correctPassword);

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(email))
            .ReturnsAsync(user);

        var act = async () => await _authService.LoginAsync(email, wrongPassword);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        _userRepositoryMock.Verify(r => r.GetByEmailAsync(email), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithPlainTextStoredPassword_ReturnsJwtToken()
    {
        var email = "legacy@test.com";
        var password = "legacy123";
        var user = new User
        {
            Id = 11,
            Email = email,
            FirstName = "Legacy",
            LastName = "User",
            PasswordHash = password,
            Roles = new List<string> { "Customer" }
        };

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(email))
            .ReturnsAsync(user);

        var result = await _authService.LoginAsync(email, password);

        result.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task LoginAsync_WithUppercaseAndSpacedEmail_NormalizesEmailBeforeLookup()
    {
        var email = "user@test.com";
        var password = "ValidPassword@123";
        var user = CreateUser(email, password);

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(email))
            .ReturnsAsync(user);

        var result = await _authService.LoginAsync("  USER@Test.com  ", password);

        result.Should().NotBeNullOrWhiteSpace();
        _userRepositoryMock.Verify(r => r.GetByEmailAsync(email), Times.Once);
    }

    #endregion

    #region Register Tests

    [Fact]
    public async Task RegisterAsync_WithValidData_CreatesUserAndReturnsJwtToken()
    {
        var email = "newuser@test.com";
        var password = "SecurePassword@123";
        var firstName = "John";
        var lastName = "Doe";

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(email))
            .ReturnsAsync((User)null);

        _userRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        var result = await _authService.RegisterAsync(email, password, firstName, lastName);

        result.Should().NotBeNullOrWhiteSpace();
        _userRepositoryMock.Verify(r => r.GetByEmailAsync(email), Times.Once);
        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateEmail_ThrowsInvalidOperationException()
    {
        var email = "existing@test.com";
        var password = "SecurePassword@123";
        var firstName = "John";
        var lastName = "Doe";
        var existingUser = CreateUser(email, "AnyPassword@123");

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(email))
            .ReturnsAsync(existingUser);

        var act = async () => await _authService.RegisterAsync(email, password, firstName, lastName);

        await act.Should().ThrowAsync<InvalidOperationException>();
        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WithValidData_UserCreatedWithCorrectProperties()
    {
        var email = "newuser@test.com";
        var password = "SecurePassword@123";
        var firstName = "John";
        var lastName = "Doe";
        User capturedUser = null;

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(email))
            .ReturnsAsync((User)null);

        _userRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<User>()))
            .Callback<User>(u => capturedUser = u)
            .Returns(Task.CompletedTask);

        await _authService.RegisterAsync(email, password, firstName, lastName);

        capturedUser.Should().NotBeNull();
        capturedUser!.Email.Should().Be(email);
        capturedUser.FirstName.Should().Be(firstName);
        capturedUser.LastName.Should().Be(lastName);
        capturedUser.Roles.Should().Contain("Customer");
    }

    [Fact]
    public async Task RegisterAsync_WithMixedCaseEmailAndSpacedNames_NormalizesValues()
    {
        User capturedUser = null;

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync("newuser@test.com"))
            .ReturnsAsync((User)null);

        _userRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<User>()))
            .Callback<User>(user => capturedUser = user)
            .Returns(Task.CompletedTask);

        await _authService.RegisterAsync("  NewUser@Test.com ", "SecurePassword@123", " John ", " Doe ");

        capturedUser.Should().NotBeNull();
        capturedUser!.Email.Should().Be("newuser@test.com");
        capturedUser.FirstName.Should().Be("John");
        capturedUser.LastName.Should().Be("Doe");
    }

    #endregion

    #region Refresh Tests

    [Fact]
    public async Task RefreshAsync_WithValidToken_ReturnsJwtToken()
    {
        var email = "user@test.com";
        var password = "ValidPassword@123";
        var user = CreateUser(email, password);

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(user.Id))
            .ReturnsAsync(user);

        var initialToken = _jwtTokenService.GenerateToken(user, user.Roles);
        var result = await _authService.RefreshAsync(initialToken);

        result.Should().NotBeNullOrWhiteSpace();
        _userRepositoryMock.Verify(r => r.GetByIdAsync(user.Id), Times.Once);
    }

    [Fact]
    public async Task RefreshAsync_WithInvalidToken_ThrowsUnauthorizedAccessException()
    {
        var invalidToken = "invalid.token.here";

        var act = async () => await _authService.RefreshAsync(invalidToken);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid token");
    }

    [Fact]
    public async Task RefreshAsync_WithNonExistentUser_ThrowsUnauthorizedAccessException()
    {
        var email = "nonexistent@test.com";
        var password = "Password@123";
        var user = CreateUser(email, password);
        var token = _jwtTokenService.GenerateToken(user, user.Roles);

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((User)null);

        var act = async () => await _authService.RefreshAsync(token);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task RefreshAsync_WithTokenMissingNameIdentifier_ThrowsUnauthorizedAccessException()
    {
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var token = handler.WriteToken(new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            claims: new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, "user@test.com") }));

        var act = async () => await _authService.RefreshAsync(token);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    #endregion

    private static User CreateUser(string email, string password, string firstName = "Test", string lastName = "User")
    {
        return new User
        {
            Id = 1,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            PasswordHash = PasswordHasher.HashPassword(password),
            Roles = new List<string> { "Customer" }
        };
    }

    private static JwtTokenService CreateJwtTokenService()
    {
        var settings = new JwtSettings();

        SetIfExists(settings, "SecretKey", "test-secret-key-with-at-least-32-characters!");
        SetIfExists(settings, "Key", "test-secret-key-with-at-least-32-characters!");
        SetIfExists(settings, "Issuer", "AlugueldeCarros.Tests");
        SetIfExists(settings, "Audience", "AlugueldeCarros.Tests");
        SetIfExists(settings, "ExpirationMinutes", 60);
        SetIfExists(settings, "ExpirationInMinutes", 60);
        SetIfExists(settings, "ExpiryInMinutes", 60);

        return new JwtTokenService(settings);
    }

    private static void SetIfExists<T>(object target, string propertyName, T value)
    {
        var property = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

        if (property == null || !property.CanWrite)
            return;

        property.SetValue(target, value);
    }
}