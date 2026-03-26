using AlugueldeCarros.Domain.Entities;
using AlugueldeCarros.Repositories;
using AlugueldeCarros.Services;
using FluentAssertions;
using Moq;

namespace AlugueldeCarros.Tests.Unit.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _userService = new UserService(_userRepositoryMock.Object, _roleRepositoryMock.Object);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithExistingUser_ReturnsUser()
    {
        var user = new User { Id = 1, Email = "customer@example.com", FirstName = "Joao", LastName = "Silva" };

        _userRepositoryMock
            .Setup(repository => repository.GetByIdAsync(user.Id))
            .ReturnsAsync(user);

        var result = await _userService.GetUserByIdAsync(user.Id);

        result.Should().Be(user);
    }

    [Fact]
    public async Task GetAllUsersAsync_WithUsers_ReturnsRepositoryResult()
    {
        var users = new List<User>
        {
            new() { Id = 1, Email = "customer@example.com", FirstName = "Joao", LastName = "Silva" },
            new() { Id = 2, Email = "admin@aluguel.com", FirstName = "Admin", LastName = "Sistema" }
        };

        _userRepositoryMock
            .Setup(repository => repository.GetAllAsync())
            .ReturnsAsync(users);

        var result = await _userService.GetAllUsersAsync();

        result.Should().BeEquivalentTo(users);
    }

    [Fact]
    public async Task AssignRolesAsync_WithUnknownUser_ThrowsKeyNotFoundException()
    {
        _userRepositoryMock
            .Setup(repository => repository.GetByIdAsync(77))
            .ReturnsAsync((User)null!);

        var act = async () => await _userService.AssignRolesAsync(77, new[] { "Admin" });

        await act.Should().ThrowAsync<KeyNotFoundException>();
        _userRepositoryMock.Verify(repository => repository.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task AssignRolesAsync_WithUnknownRole_ThrowsInvalidOperationException()
    {
        var user = new User { Id = 1, Email = "customer@example.com", FirstName = "Joao", LastName = "Silva", Roles = new List<string>() };

        _userRepositoryMock
            .Setup(repository => repository.GetByIdAsync(user.Id))
            .ReturnsAsync(user);

        _roleRepositoryMock
            .Setup(repository => repository.GetByNameAsync("Admin"))
            .ReturnsAsync((Role)null!);

        var act = async () => await _userService.AssignRolesAsync(user.Id, new[] { "Admin" });

        await act.Should().ThrowAsync<InvalidOperationException>();
        _userRepositoryMock.Verify(repository => repository.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task AssignRolesAsync_WithWhitespaceAndDuplicateRoles_AddsOnlyUniqueValidRoles()
    {
        var user = new User
        {
            Id = 1,
            Email = "customer@example.com",
            FirstName = "Joao",
            LastName = "Silva",
            Roles = new List<string> { "Customer" }
        };

        _userRepositoryMock
            .Setup(repository => repository.GetByIdAsync(user.Id))
            .ReturnsAsync(user);

        _roleRepositoryMock
            .Setup(repository => repository.GetByNameAsync(It.Is<string>(role => role.Equals("Admin", StringComparison.OrdinalIgnoreCase))))
            .ReturnsAsync(new Role { Id = 2, Name = "Admin" });

        _roleRepositoryMock
            .Setup(repository => repository.GetByNameAsync(It.Is<string>(role => role.Equals("Customer", StringComparison.OrdinalIgnoreCase))))
            .ReturnsAsync(new Role { Id = 1, Name = "Customer" });

        var roles = new[] { " ", "Admin", "admin", "Customer" };

        await _userService.AssignRolesAsync(user.Id, roles);

        user.Roles.Should().BeEquivalentTo(new[] { "Customer", "Admin" });
        _userRepositoryMock.Verify(repository => repository.UpdateAsync(user), Times.Once);
    }
}