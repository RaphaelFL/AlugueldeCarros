using System.Security.Claims;
using AlugueldeCarros.Controllers;
using AlugueldeCarros.Domain.Entities;
using AlugueldeCarros.Domain.Enums;
using AlugueldeCarros.DTOs.Reservations;
using AlugueldeCarros.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AlugueldeCarros.Tests.Unit.Controllers;

public class ReservationsControllerTests
{
    [Fact]
    public async Task Create_WithoutUserClaim_ShouldReturnUnauthorized()
    {
        var service = new Mock<IReservationService>();
        var controller = CreateController(service, userId: null, isAdmin: false);

        var result = await controller.Create(new CreateReservationRequest
        {
            CategoryId = 1,
            StartDate = DateTime.UtcNow.AddDays(2),
            EndDate = DateTime.UtcNow.AddDays(4)
        });

        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task GetById_WhenReservationDoesNotExist_ShouldReturnNotFound()
    {
        var service = new Mock<IReservationService>();
        service.Setup(s => s.GetByIdAsync(33)).ReturnsAsync((Reservation?)null);
        var controller = CreateController(service, userId: 1, isAdmin: false);

        var result = await controller.GetById(33);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_WhenUserIsNotOwner_ShouldReturnForbid()
    {
        var service = new Mock<IReservationService>();
        service.Setup(s => s.GetByIdAsync(8)).ReturnsAsync(new Reservation { Id = 8, UserId = 44 });
        var controller = CreateController(service, userId: 1, isAdmin: false);

        var result = await controller.GetById(8);

        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task Update_WhenUserIsOwner_ShouldReturnNoContent()
    {
        var service = new Mock<IReservationService>();
        service.Setup(s => s.GetByIdAsync(8)).ReturnsAsync(new Reservation { Id = 8, UserId = 1 });
        var controller = CreateController(service, userId: 1, isAdmin: false);

        var result = await controller.Update(8, new UpdateReservationRequest
        {
            Status = ReservationStatus.CONFIRMED
        });

        result.Should().BeOfType<NoContentResult>();
        service.Verify(s => s.UpdateReservationAsync(8, null, null, ReservationStatus.CONFIRMED), Times.Once);
    }

    [Fact]
    public async Task Cancel_WhenUserIsAdmin_ShouldReturnNoContent()
    {
        var service = new Mock<IReservationService>();
        service.Setup(s => s.GetByIdAsync(7)).ReturnsAsync(new Reservation { Id = 7, UserId = 55 });
        var controller = CreateController(service, userId: 999, isAdmin: true);

        var result = await controller.Cancel(7);

        result.Should().BeOfType<NoContentResult>();
        service.Verify(s => s.CancelReservationAsync(7), Times.Once);
    }

    private static ReservationsController CreateController(Mock<IReservationService> service, int? userId, bool isAdmin)
    {
        var claims = new List<Claim>();
        if (userId.HasValue)
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()));
        }

        if (isAdmin)
        {
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));
        }

        var controller = new ReservationsController(service.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"))
                }
            }
        };

        return controller;
    }
}