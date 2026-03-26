using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AlugueldeCarros.Domain.Entities;
using AlugueldeCarros.Domain.Enums;
using AlugueldeCarros.Repositories;
using AlugueldeCarros.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace AlugueldeCarros.Tests.Unit.Services;

public class ReservationServiceTests
{
    private readonly Mock<IReservationRepository> _reservationRepositoryMock;
    private readonly Mock<IVehicleRepository> _vehicleRepositoryMock;
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
    private readonly ReservationService _reservationService;

    public ReservationServiceTests()
    {
        _reservationRepositoryMock = new Mock<IReservationRepository>();
        _vehicleRepositoryMock = new Mock<IVehicleRepository>();
        _paymentRepositoryMock = new Mock<IPaymentRepository>();

        _reservationService = new ReservationService(
            _reservationRepositoryMock.Object,
            _vehicleRepositoryMock.Object,
            _paymentRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateReservationAsync_WithValidData_CreatesReservationInPendingPaymentStatus()
    {
        var userId = 1;
        var categoryId = 1;
        var startDate = DateTime.UtcNow.AddDays(1);
        var endDate = DateTime.UtcNow.AddDays(3);

        var vehicles = new List<Vehicle>
        {
            new Vehicle()
        };

        _reservationRepositoryMock
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(new List<Reservation>());

        _vehicleRepositoryMock
            .Setup(r => r.SearchAsync(categoryId, startDate, endDate))
            .ReturnsAsync(vehicles);

        _reservationRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Reservation>()))
            .Returns(Task.CompletedTask);

        var result = await _reservationService.CreateReservationAsync(userId, categoryId, startDate, endDate);

        result.Should().NotBeNull();
        result.Status.Should().Be(ReservationStatus.PENDING_PAYMENT);
        result.UserId.Should().Be(userId);
        result.CategoryId.Should().Be(categoryId);

        _reservationRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Reservation>()), Times.Once);
    }

    [Fact]
    public async Task CreateReservationAsync_WithNoAvailableVehicles_ThrowsValidationException()
    {
        var userId = 1;
        var categoryId = 1;
        var startDate = DateTime.UtcNow.AddDays(1);
        var endDate = DateTime.UtcNow.AddDays(3);

        _reservationRepositoryMock
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(new List<Reservation>());

        _vehicleRepositoryMock
            .Setup(r => r.SearchAsync(categoryId, startDate, endDate))
            .ReturnsAsync(new List<Vehicle>());

        var act = async () => await _reservationService.CreateReservationAsync(userId, categoryId, startDate, endDate);

        await act.Should().ThrowAsync<ValidationException>();

        _reservationRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Reservation>()), Times.Never);
    }

    [Fact]
    public async Task CreateReservationAsync_WithEndDateBeforeStartDate_ThrowsValidationException()
    {
        var userId = 1;
        var categoryId = 1;
        var startDate = DateTime.UtcNow.AddDays(5);
        var endDate = DateTime.UtcNow.AddDays(1);

        var act = async () => await _reservationService.CreateReservationAsync(userId, categoryId, startDate, endDate);

        await act.Should().ThrowAsync<ValidationException>();

        _reservationRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Reservation>()), Times.Never);
    }

    [Fact]
    public async Task CreateReservationAsync_WithUserHaving5ActiveReservations_ThrowsValidationException()
    {
        var userId = 1;
        var categoryId = 1;
        var startDate = DateTime.UtcNow.AddDays(1);
        var endDate = DateTime.UtcNow.AddDays(3);

        var activeReservations = new List<Reservation>
        {
            new Reservation { Id = 1, Status = ReservationStatus.CONFIRMED },
            new Reservation { Id = 2, Status = ReservationStatus.CONFIRMED },
            new Reservation { Id = 3, Status = ReservationStatus.CONFIRMED },
            new Reservation { Id = 4, Status = ReservationStatus.PENDING_PAYMENT },
            new Reservation { Id = 5, Status = ReservationStatus.CONFIRMED }
        };

        _reservationRepositoryMock
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(activeReservations);

        var act = async () => await _reservationService.CreateReservationAsync(userId, categoryId, startDate, endDate);

        await act.Should().ThrowAsync<ValidationException>();

        _reservationRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Reservation>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsReservation()
    {
        var reservationId = 1;
        var reservation = new Reservation
        {
            Id = reservationId,
            UserId = 1,
            CategoryId = 1,
            Status = ReservationStatus.PENDING_PAYMENT
        };

        _reservationRepositoryMock
            .Setup(r => r.GetByIdAsync(reservationId))
            .ReturnsAsync(reservation);

        var result = await _reservationService.GetByIdAsync(reservationId);

        result.Should().NotBeNull();
        result.Id.Should().Be(reservationId);
        result.Status.Should().Be(ReservationStatus.PENDING_PAYMENT);

        _reservationRepositoryMock.Verify(r => r.GetByIdAsync(reservationId), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ReturnsNull()
    {
        var reservationId = 999;

        _reservationRepositoryMock
            .Setup(r => r.GetByIdAsync(reservationId))
            .ReturnsAsync((Reservation)null);

        var result = await _reservationService.GetByIdAsync(reservationId);

        result.Should().BeNull();

        _reservationRepositoryMock.Verify(r => r.GetByIdAsync(reservationId), Times.Once);
    }

    [Fact]
    public async Task GetByUserIdAsync_WithValidUserId_ReturnsUserReservations()
    {
        var userId = 1;
        var reservations = new List<Reservation>
        {
            new Reservation { Id = 1, UserId = userId, Status = ReservationStatus.PENDING_PAYMENT },
            new Reservation { Id = 2, UserId = userId, Status = ReservationStatus.CONFIRMED }
        };

        _reservationRepositoryMock
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(reservations);

        var result = await _reservationService.GetByUserIdAsync(userId);

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(r => r.UserId.Should().Be(userId));

        _reservationRepositoryMock.Verify(r => r.GetByUserIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task UpdateReservationAsync_WithValidData_UpdatesReservation()
    {
        var reservationId = 1;
        var newStartDate = DateTime.UtcNow.AddDays(10);
        var newEndDate = DateTime.UtcNow.AddDays(12);
        var reservation = new Reservation { Id = reservationId, Status = ReservationStatus.PENDING_PAYMENT };

        _reservationRepositoryMock
            .Setup(r => r.GetByIdAsync(reservationId))
            .ReturnsAsync(reservation);

        _reservationRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Reservation>()))
            .Returns(Task.CompletedTask);

        await _reservationService.UpdateReservationAsync(reservationId, newStartDate, newEndDate, ReservationStatus.CONFIRMED);

        _reservationRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Reservation>()), Times.Once);
    }

    [Fact]
    public async Task UpdateReservationAsync_WithNonExistentReservation_ThrowsKeyNotFoundException()
    {
        var reservationId = 999;

        _reservationRepositoryMock
            .Setup(r => r.GetByIdAsync(reservationId))
            .ReturnsAsync((Reservation)null);

        var act = async () => await _reservationService.UpdateReservationAsync(reservationId, null, null, null);

        await act.Should().ThrowAsync<KeyNotFoundException>();

        _reservationRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Reservation>()), Times.Never);
    }

    [Fact]
    public async Task CancelReservationAsync_WithValidReservation_CancelsAndCreatesRefund()
    {
        var reservationId = 1;
        var futureDate = DateTime.UtcNow.AddDays(5);
        var reservation = new Reservation
        {
            Id = reservationId,
            UserId = 1,
            Status = ReservationStatus.CONFIRMED,
            StartDate = futureDate,
            EndDate = futureDate.AddDays(2),
            TotalAmount = 500m
        };

        _reservationRepositoryMock
            .Setup(r => r.GetByIdAsync(reservationId))
            .ReturnsAsync(reservation);

        _reservationRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Reservation>()))
            .Returns(Task.CompletedTask);

        _paymentRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Payment>()))
            .Returns(Task.CompletedTask);

        await _reservationService.CancelReservationAsync(reservationId);

        _reservationRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Reservation>()), Times.Once);
        _paymentRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Payment>()), Times.Once);
    }

    [Fact]
    public async Task CancelReservationAsync_WithinTwoHours_ThrowsValidationException()
    {
        var reservationId = 1;
        var soonDate = DateTime.UtcNow.AddHours(1);
        var reservation = new Reservation
        {
            Id = reservationId,
            UserId = 1,
            Status = ReservationStatus.CONFIRMED,
            StartDate = soonDate,
            EndDate = soonDate.AddDays(2),
            TotalAmount = 500m
        };

        _reservationRepositoryMock
            .Setup(r => r.GetByIdAsync(reservationId))
            .ReturnsAsync(reservation);

        var act = async () => await _reservationService.CancelReservationAsync(reservationId);

        await act.Should().ThrowAsync<ValidationException>();

        _reservationRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Reservation>()), Times.Never);
    }

    [Fact]
    public async Task CancelReservationAsync_WithNonExistentReservation_ThrowsKeyNotFoundException()
    {
        var reservationId = 999;

        _reservationRepositoryMock
            .Setup(r => r.GetByIdAsync(reservationId))
            .ReturnsAsync((Reservation)null);

        var act = async () => await _reservationService.CancelReservationAsync(reservationId);

        await act.Should().ThrowAsync<KeyNotFoundException>();

        _reservationRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Reservation>()), Times.Never);
    }

    [Fact]
    public async Task CancelReservationAsync_WithCheckInInFiveDays_CreatesEightyPercentRefund()
    {
        var reservation = new Reservation
        {
            Id = 12,
            UserId = 1,
            Status = ReservationStatus.CONFIRMED,
            StartDate = DateTime.UtcNow.AddDays(5),
            EndDate = DateTime.UtcNow.AddDays(7),
            TotalAmount = 1000m
        };
        Payment? capturedPayment = null;

        _reservationRepositoryMock
            .Setup(r => r.GetByIdAsync(reservation.Id))
            .ReturnsAsync(reservation);

        _reservationRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Reservation>()))
            .Returns(Task.CompletedTask);

        _paymentRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Payment>()))
            .Callback<Payment>(payment => capturedPayment = payment)
            .Returns(Task.CompletedTask);

        await _reservationService.CancelReservationAsync(reservation.Id);

        capturedPayment.Should().NotBeNull();
        capturedPayment!.Amount.Should().Be(800m);
        capturedPayment.Status.Should().Be(PaymentStatus.REFUNDED);
    }

    [Fact]
    public async Task CancelReservationAsync_WithCheckInInTwelveHours_CreatesZeroRefund()
    {
        var reservation = new Reservation
        {
            Id = 13,
            UserId = 1,
            Status = ReservationStatus.CONFIRMED,
            StartDate = DateTime.UtcNow.AddHours(12),
            EndDate = DateTime.UtcNow.AddDays(2),
            TotalAmount = 700m
        };
        Payment? capturedPayment = null;

        _reservationRepositoryMock
            .Setup(r => r.GetByIdAsync(reservation.Id))
            .ReturnsAsync(reservation);

        _reservationRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Reservation>()))
            .Returns(Task.CompletedTask);

        _paymentRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Payment>()))
            .Callback<Payment>(payment => capturedPayment = payment)
            .Returns(Task.CompletedTask);

        await _reservationService.CancelReservationAsync(reservation.Id);

        capturedPayment.Should().NotBeNull();
        capturedPayment!.Amount.Should().Be(0m);
    }
}