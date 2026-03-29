using System.Security.Claims;
using AlugueldeCarros.Controllers;
using AlugueldeCarros.Domain.Entities;
using AlugueldeCarros.Domain.Enums;
using AlugueldeCarros.DTOs.Payments;
using AlugueldeCarros.Repositories;
using AlugueldeCarros.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AlugueldeCarros.Tests.Unit.Controllers;

public class PaymentsControllerTests
{
    [Fact]
    public async Task Preauth_WhenReservationDoesNotBelongToUser_ShouldReturnForbid()
    {
        var paymentService = new Mock<IPaymentService>();
        var reservationRepository = new Mock<IReservationRepository>();
        reservationRepository.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(new Reservation { Id = 3, UserId = 99 });
        var controller = CreateController(paymentService, reservationRepository, userId: 1, isAdmin: false);

        var result = await controller.Preauth(new PreauthRequest { ReservationId = 3, Amount = 200m });

        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task Preauth_WhenReservationBelongsToUser_ShouldReturnCreatedAtAction()
    {
        var paymentService = new Mock<IPaymentService>();
        var reservationRepository = new Mock<IReservationRepository>();
        reservationRepository.Setup(r => r.GetByIdAsync(4)).ReturnsAsync(new Reservation { Id = 4, UserId = 1 });
        paymentService.Setup(s => s.PreauthorizePaymentAsync(4, 350m)).ReturnsAsync(new Payment
        {
            Id = 10,
            ReservationId = 4,
            Amount = 350m,
            Status = PaymentStatus.PENDING
        });

        var controller = CreateController(paymentService, reservationRepository, userId: 1, isAdmin: false);

        var result = await controller.Preauth(new PreauthRequest { ReservationId = 4, Amount = 350m });

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(PaymentsController.GetById));
        created.RouteValues!["id"].Should().Be(10);
    }

    [Fact]
    public async Task Capture_WhenAdminHasAccess_ShouldReturnOk()
    {
        var paymentService = new Mock<IPaymentService>();
        var reservationRepository = new Mock<IReservationRepository>();
        paymentService.Setup(s => s.GetByIdAsync(12)).ReturnsAsync(new Payment { Id = 12, ReservationId = 1 });
        paymentService.Setup(s => s.CapturePaymentAsync(12)).ReturnsAsync(new Payment
        {
            Id = 12,
            ReservationId = 1,
            Status = PaymentStatus.APPROVED
        });

        var controller = CreateController(paymentService, reservationRepository, userId: 1, isAdmin: true);

        var result = await controller.Capture(new CaptureRequest { PaymentId = 12 });

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_WhenPaymentDoesNotExist_ShouldReturnForbid()
    {
        var paymentService = new Mock<IPaymentService>();
        var reservationRepository = new Mock<IReservationRepository>();
        paymentService.Setup(s => s.GetByIdAsync(404)).ReturnsAsync((Payment?)null);
        var controller = CreateController(paymentService, reservationRepository, userId: 1, isAdmin: false);

        var result = await controller.GetById(404);

        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task GetById_WhenPaymentBelongsToUser_ShouldReturnOk()
    {
        var paymentService = new Mock<IPaymentService>();
        var reservationRepository = new Mock<IReservationRepository>();
        paymentService.Setup(s => s.GetByIdAsync(15)).ReturnsAsync(new Payment { Id = 15, ReservationId = 9, Status = PaymentStatus.APPROVED });
        reservationRepository.Setup(r => r.GetByIdAsync(9)).ReturnsAsync(new Reservation { Id = 9, UserId = 1 });
        var controller = CreateController(paymentService, reservationRepository, userId: 1, isAdmin: false);

        var result = await controller.GetById(15);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Refund_WhenPaymentBelongsToUser_ShouldReturnOk()
    {
        var paymentService = new Mock<IPaymentService>();
        var reservationRepository = new Mock<IReservationRepository>();
        paymentService.Setup(s => s.GetByIdAsync(21)).ReturnsAsync(new Payment { Id = 21, ReservationId = 5, Status = PaymentStatus.APPROVED });
        paymentService.Setup(s => s.RefundPaymentAsync(21)).ReturnsAsync(new Payment { Id = 21, ReservationId = 5, Status = PaymentStatus.REFUNDED });
        reservationRepository.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(new Reservation { Id = 5, UserId = 1 });
        var controller = CreateController(paymentService, reservationRepository, userId: 1, isAdmin: false);

        var result = await controller.Refund(new RefundRequest { PaymentId = 21 });

        result.Should().BeOfType<OkObjectResult>();
    }

    private static PaymentsController CreateController(
        Mock<IPaymentService> paymentService,
        Mock<IReservationRepository> reservationRepository,
        int userId,
        bool isAdmin)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString())
        };

        if (isAdmin)
        {
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));
        }

        var controller = new PaymentsController(paymentService.Object, reservationRepository.Object)
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