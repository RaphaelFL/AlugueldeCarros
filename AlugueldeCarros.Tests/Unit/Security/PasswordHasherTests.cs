using AlugueldeCarros.Security;
using FluentAssertions;

namespace AlugueldeCarros.Tests.Unit.Security;

public class PasswordHasherTests
{
    [Fact]
    public void HashPassword_WithValidPassword_ReturnsHashedString()
    {
        // Arrange
        var password = "SecurePassword@123";

        // Act
        var hash = PasswordHasher.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Should().NotBe(password);
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ReturnsTrue()
    {
        // Arrange
        var password = "SecurePassword@123";
        var hash = PasswordHasher.HashPassword(password);

        // Act
        var isValid = PasswordHasher.VerifyPassword(password, hash);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ReturnsFalse()
    {
        // Arrange
        var correctPassword = "SecurePassword@123";
        var wrongPassword = "WrongPassword@123";
        var hash = PasswordHasher.HashPassword(correctPassword);

        // Act
        var isValid = PasswordHasher.VerifyPassword(wrongPassword, hash);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_WithEmptyPassword_ReturnsFalse()
    {
        // Arrange
        var password = "SecurePassword@123";
        var hash = PasswordHasher.HashPassword(password);

        // Act
        var isValid = PasswordHasher.VerifyPassword(string.Empty, hash);

        // Assert
        isValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("a")]
    [InlineData("short")]
    [InlineData("12345")]
    public void HashPassword_WithShortPassword_StillHashesSuccessfully(string shortPassword)
    {
        // Act
        var hash = PasswordHasher.HashPassword(shortPassword);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        var isValid = PasswordHasher.VerifyPassword(shortPassword, hash);
        isValid.Should().BeTrue();
    }

    [Fact]
    public void HashPassword_WithSpecialCharacters_HashesSuccessfully()
    {
        // Arrange
        var passwordWithSpecialChars = "P@$$wØrd#2024!";

        // Act
        var hash = PasswordHasher.HashPassword(passwordWithSpecialChars);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        PasswordHasher.VerifyPassword(passwordWithSpecialChars, hash).Should().BeTrue();
    }
}
