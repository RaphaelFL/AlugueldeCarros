using AlugueldeCarros.Domain.Entities;
using AlugueldeCarros.Repositories;
using AlugueldeCarros.Services;
using FluentAssertions;
using Moq;

namespace AlugueldeCarros.Tests.Unit.Services;

public class BranchServiceTests
{
    private readonly Mock<IBranchRepository> _branchRepositoryMock;
    private readonly BranchService _branchService;

    public BranchServiceTests()
    {
        _branchRepositoryMock = new Mock<IBranchRepository>();
        _branchService = new BranchService(_branchRepositoryMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_WithBranches_ReturnsRepositoryResult()
    {
        var branches = new List<Branch>
        {
            new() { Id = 1, Name = "Centro", Address = "Rua A" },
            new() { Id = 2, Name = "Aeroporto", Address = "Rua B" }
        };

        _branchRepositoryMock
            .Setup(repository => repository.GetAllAsync())
            .ReturnsAsync(branches);

        var result = await _branchService.GetAllAsync();

        result.Should().BeEquivalentTo(branches);
        _branchRepositoryMock.Verify(repository => repository.GetAllAsync(), Times.Once);
    }
}