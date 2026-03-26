using AlugueldeCarros.Domain.Entities;
using AlugueldeCarros.Repositories;
using AlugueldeCarros.Services;
using FluentAssertions;
using Moq;

namespace AlugueldeCarros.Tests.Unit.Services;

public class PricingServiceTests
{
    private readonly Mock<IPricingRuleRepository> _pricingRuleRepositoryMock;
    private readonly PricingService _pricingService;

    public PricingServiceTests()
    {
        _pricingRuleRepositoryMock = new Mock<IPricingRuleRepository>();
        _pricingService = new PricingService(_pricingRuleRepositoryMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_WithRules_ReturnsRepositoryResult()
    {
        var rules = new List<PricingRule>
        {
            new() { Id = 1, CategoryId = 1, BaseDailyRate = 50m },
            new() { Id = 2, CategoryId = 2, BaseDailyRate = 120m }
        };

        _pricingRuleRepositoryMock
            .Setup(repository => repository.GetAllAsync())
            .ReturnsAsync(rules);

        var result = await _pricingService.GetAllAsync();

        result.Should().BeEquivalentTo(rules);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingRule_ReturnsRule()
    {
        var rule = new PricingRule { Id = 7, CategoryId = 1, BaseDailyRate = 70m };

        _pricingRuleRepositoryMock
            .Setup(repository => repository.GetByIdAsync(rule.Id))
            .ReturnsAsync(rule);

        var result = await _pricingService.GetByIdAsync(rule.Id);

        result.Should().Be(rule);
    }

    [Fact]
    public async Task CreateAsync_WithRule_PersistsAndReturnsSameInstance()
    {
        var rule = new PricingRule { CategoryId = 1, BaseDailyRate = 55m };

        var result = await _pricingService.CreateAsync(rule);

        result.Should().BeSameAs(rule);
        _pricingRuleRepositoryMock.Verify(repository => repository.AddAsync(rule), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithExistingRule_UpdatesAndReturnsRuleWithRequestedId()
    {
        var existing = new PricingRule { Id = 5, CategoryId = 1, BaseDailyRate = 60m };
        var updated = new PricingRule { CategoryId = 2, BaseDailyRate = 95m };

        _pricingRuleRepositoryMock
            .Setup(repository => repository.GetByIdAsync(existing.Id))
            .ReturnsAsync(existing);

        var result = await _pricingService.UpdateAsync(existing.Id, updated);

        result.Id.Should().Be(existing.Id);
        result.CategoryId.Should().Be(updated.CategoryId);
        result.BaseDailyRate.Should().Be(updated.BaseDailyRate);
        _pricingRuleRepositoryMock.Verify(repository => repository.UpdateAsync(updated), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithMissingRule_ThrowsKeyNotFoundException()
    {
        _pricingRuleRepositoryMock
            .Setup(repository => repository.GetByIdAsync(99))
            .ReturnsAsync((PricingRule)null!);

        var act = async () => await _pricingService.UpdateAsync(99, new PricingRule());

        await act.Should().ThrowAsync<KeyNotFoundException>();
        _pricingRuleRepositoryMock.Verify(repository => repository.UpdateAsync(It.IsAny<PricingRule>()), Times.Never);
    }
}