using AlugueldeCarros.Controllers;
using AlugueldeCarros.Domain.Entities;
using AlugueldeCarros.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AlugueldeCarros.Tests.Unit.Controllers;

public class PricingControllerTests
{
    [Fact]
    public async Task GetAll_ShouldReturnOkWithRules()
    {
        var service = new Mock<IPricingService>();
        service.Setup(s => s.GetAllAsync()).ReturnsAsync(new[]
        {
            new PricingRule { Id = 1, CategoryId = 1, BaseDailyRate = 120m, WeekendMultiplier = 1.25m, PeakSeasonMultiplier = 1.5m }
        });

        var controller = new PricingController(service.Object);

        var result = await controller.GetAll();

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeAssignableTo<IEnumerable<PricingRule>>();
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedAtAction()
    {
        var service = new Mock<IPricingService>();
        var rule = new PricingRule { Id = 7, CategoryId = 2, BaseDailyRate = 150m, WeekendMultiplier = 1.4m, PeakSeasonMultiplier = 1.8m };

        service.Setup(s => s.CreateAsync(rule)).ReturnsAsync(rule);

        var controller = new PricingController(service.Object);

        var result = await controller.Create(rule);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(PricingController.GetById));
        created.RouteValues!["id"].Should().Be(7);
    }

    [Fact]
    public async Task Update_ShouldReturnOkWithUpdatedRule()
    {
        var service = new Mock<IPricingService>();
        var rule = new PricingRule { CategoryId = 2, BaseDailyRate = 160m, WeekendMultiplier = 1.5m, PeakSeasonMultiplier = 1.9m };

        service.Setup(s => s.UpdateAsync(5, rule)).ReturnsAsync(new PricingRule
        {
            Id = 5,
            CategoryId = 2,
            BaseDailyRate = 160m,
            WeekendMultiplier = 1.5m,
            PeakSeasonMultiplier = 1.9m
        });

        var controller = new PricingController(service.Object);

        var result = await controller.Update(5, rule);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<PricingRule>().Subject.Id.Should().Be(5);
    }

    [Fact]
    public async Task GetById_WhenRuleDoesNotExist_ShouldReturnNotFound()
    {
        var service = new Mock<IPricingService>();
        service.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((PricingRule?)null);

        var controller = new PricingController(service.Object);

        var result = await controller.GetById(99);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_WhenRuleExists_ShouldReturnOk()
    {
        var service = new Mock<IPricingService>();
        service.Setup(s => s.GetByIdAsync(3)).ReturnsAsync(new PricingRule
        {
            Id = 3,
            CategoryId = 1,
            BaseDailyRate = 110m,
            WeekendMultiplier = 1.2m,
            PeakSeasonMultiplier = 1.7m
        });

        var controller = new PricingController(service.Object);

        var result = await controller.GetById(3);

        result.Should().BeOfType<OkObjectResult>();
    }
}