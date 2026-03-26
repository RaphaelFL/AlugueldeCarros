using System;
using System.ComponentModel.DataAnnotations;
using AlugueldeCarros.Domain.Entities;
using AlugueldeCarros.Domain.Enums;
using AlugueldeCarros.Repositories;
using AlugueldeCarros.Services;
using AlugueldeCarros.Tests.Fixtures;
using FluentAssertions;
using Moq;
using Xunit;

namespace AlugueldeCarros.Tests.Unit.Services;

public class PaymentServiceTests
{
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
    private readonly Mock<IReservationRepository> _reservationRepositoryMock;
    private readonly Mock<IVehicleRepository> _vehicleRepositoryMock;
    private readonly PaymentService _paymentService;
    private readonly TestDataBuilder _testDataBuilder;

    public PaymentServiceTests()
    {
        _paymentRepositoryMock = new Mock<IPaymentRepository>();
        _reservationRepositoryMock = new Mock<IReservationRepository>();
        _vehicleRepositoryMock = new Mock<IVehicleRepository>();

        _paymentService = new PaymentService(
            _paymentRepositoryMock.Object,
            _reservationRepositoryMock.Object,
            _vehicleRepositoryMock.Object);

        _testDataBuilder = new TestDataBuilder();
    }

    [Fact]
    public async Task PreauthorizePaymentAsync_WithValidData_CreatesPaymentWithPendingStatus()
    {
        var reservationId = 1;
        var amount = 500m;
        var reservation = _testDataBuilder.WithReservation().BuildReservation();

        _reservationRepositoryMock
            .Setup(r => r.GetByIdAsync(reservationId))
            .ReturnsAsync(reservation);

        _paymentRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Payment>()))
            .Returns(Task.CompletedTask);

        var result = await _paymentService.PreauthorizePaymentAsync(reservationId, amount);

        result.Should().NotBeNull();
        result.Amount.Should().Be(amount);
        result.Status.Should().Be(PaymentStatus.PENDING);

        _paymentRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Payment>()), Times.Once);
    }

    [Fact]
    public async Task PreauthorizePaymentAsync_WithNonExistentReservation_ThrowsValidationException()
    {
        var reservationId = 999;
        var amount = 500m;

        _reservationRepositoryMock
            .Setup(r => r.GetByIdAsync(reservationId))
            .ReturnsAsync((Reservation)null);

        var act = async () => await _paymentService.PreauthorizePaymentAsync(reservationId, amount);

        await act.Should().ThrowAsync<ValidationException>();

        _paymentRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Payment>()), Times.Never);
    }

    [Fact]
    public async Task CapturePaymentAsync_WithValidPayment_UpdatesStatusToApprovedOrDeclined()
    {
        var paymentId = 1;
        var payment = _testDataBuilder.WithPayment().BuildPayment();
        payment.Id = paymentId;
        payment.Status = PaymentStatus.PENDING;

        _paymentRepositoryMock
            .Setup(r => r.GetByIdAsync(paymentId))
            .ReturnsAsync(payment);

        _paymentRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Payment>()))
            .Returns(Task.CompletedTask);

        var result = await _paymentService.CapturePaymentAsync(paymentId);

        result.Should().NotBeNull();
        result.Status.Should().BeOneOf(PaymentStatus.APPROVED, PaymentStatus.DECLINED);

        _paymentRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Payment>()), Times.Once);
    }

    [Fact]
    public async Task CapturePaymentAsync_WhenPaymentIsApproved_ConfirmsReservationAndReservesVehicle()
    {
        var reservation = _testDataBuilder.WithReservation().BuildReservation();
        reservation.Id = 3;
        reservation.VehicleId = 7;
        reservation.Status = ReservationStatus.PENDING_PAYMENT;

        var payment = _testDataBuilder.WithPayment().BuildPayment();
        payment.Id = 8;
        payment.ReservationId = reservation.Id;
        payment.Status = PaymentStatus.PENDING;

        var vehicle = _testDataBuilder.WithVehicle().BuildVehicle();
        vehicle.Id = reservation.VehicleId.Value;
        vehicle.Status = VehicleStatus.AVAILABLE;

        _paymentRepositoryMock
            .Setup(r => r.GetByIdAsync(payment.Id))
            .ReturnsAsync(payment);

        _paymentRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Payment>()))
            .Returns(Task.CompletedTask);

        _reservationRepositoryMock
            .Setup(r => r.GetByIdAsync(reservation.Id))
            .ReturnsAsync(reservation);

        _reservationRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Reservation>()))
            .Returns(Task.CompletedTask);

        _vehicleRepositoryMock
            .Setup(r => r.GetByIdAsync(vehicle.Id))
            .ReturnsAsync(vehicle);

        _vehicleRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Vehicle>()))
            .Returns(Task.CompletedTask);

        var result = await _paymentService.CapturePaymentAsync(payment.Id);

        result.Status.Should().Be(PaymentStatus.APPROVED);
        reservation.Status.Should().Be(ReservationStatus.CONFIRMED);
        vehicle.Status.Should().Be(VehicleStatus.RESERVED);
        _reservationRepositoryMock.Verify(r => r.UpdateAsync(reservation), Times.Once);
        _vehicleRepositoryMock.Verify(r => r.UpdateAsync(vehicle), Times.Once);
    }

    [Fact]
    public async Task CapturePaymentAsync_WhenPaymentIsDeclined_DoesNotChangeReservationOrVehicle()
    {
        var payment = _testDataBuilder.WithPayment().BuildPayment();
        payment.Id = 9;
        payment.ReservationId = 3;
        payment.Status = PaymentStatus.PENDING;

        _paymentRepositoryMock
            .Setup(r => r.GetByIdAsync(payment.Id))
            .ReturnsAsync(payment);

        _paymentRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Payment>()))
            .Returns(Task.CompletedTask);

        var result = await _paymentService.CapturePaymentAsync(payment.Id);

        result.Status.Should().Be(PaymentStatus.DECLINED);
        _reservationRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Reservation>()), Times.Never);
        _vehicleRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Vehicle>()), Times.Never);
    }

    [Fact]
    public async Task CapturePaymentAsync_WithNonPendingPayment_ThrowsValidationException()
    {
        var paymentId = 1;
        var payment = _testDataBuilder.WithApprovedPayment().BuildPayment();
        payment.Id = paymentId;

        _paymentRepositoryMock
            .Setup(r => r.GetByIdAsync(paymentId))
            .ReturnsAsync(payment);

        var act = async () => await _paymentService.CapturePaymentAsync(paymentId);

        await act.Should().ThrowAsync<ValidationException>();

        _paymentRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Payment>()), Times.Never);
    }

    [Fact]
    public async Task CapturePaymentAsync_WithNonExistentPayment_ThrowsKeyNotFoundException()
    {
        var paymentId = 999;

        _paymentRepositoryMock
            .Setup(r => r.GetByIdAsync(paymentId))
            .ReturnsAsync((Payment)null);

        var act = async () => await _paymentService.CapturePaymentAsync(paymentId);

        await act.Should().ThrowAsync<KeyNotFoundException>();

        _paymentRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Payment>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsPayment()
    {
        var paymentId = 1;
        var payment = _testDataBuilder.WithPayment().BuildPayment();
        payment.Id = paymentId;

        _paymentRepositoryMock
            .Setup(r => r.GetByIdAsync(paymentId))
            .ReturnsAsync(payment);

        var result = await _paymentService.GetByIdAsync(paymentId);

        result.Should().NotBeNull();
        result.Id.Should().Be(paymentId);
        result.Amount.Should().Be(payment.Amount);

        _paymentRepositoryMock.Verify(r => r.GetByIdAsync(paymentId), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ReturnsNull()
    {
        var paymentId = 999;

        _paymentRepositoryMock
            .Setup(r => r.GetByIdAsync(paymentId))
            .ReturnsAsync((Payment)null);

        var result = await _paymentService.GetByIdAsync(paymentId);

        result.Should().BeNull();

        _paymentRepositoryMock.Verify(r => r.GetByIdAsync(paymentId), Times.Once);
    }

    [Fact]
    public async Task RefundPaymentAsync_WithValidPayment_UpdatesStatusToRefunded()
    {
        var paymentId = 1;
        var payment = _testDataBuilder.WithApprovedPayment().BuildPayment();
        payment.Id = paymentId;

        _paymentRepositoryMock
            .Setup(r => r.GetByIdAsync(paymentId))
            .ReturnsAsync(payment);

        _paymentRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Payment>()))
            .Returns(Task.CompletedTask);

        var result = await _paymentService.RefundPaymentAsync(paymentId);

        result.Should().NotBeNull();
        result.Status.Should().Be(PaymentStatus.REFUNDED);

        _paymentRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Payment>()), Times.Once);
    }

    [Fact]
    public async Task RefundPaymentAsync_WithNonApprovedPayment_ThrowsValidationException()
    {
        var paymentId = 1;
        var payment = _testDataBuilder.WithPayment().BuildPayment();
        payment.Id = paymentId;
        payment.Status = PaymentStatus.PENDING;

        _paymentRepositoryMock
            .Setup(r => r.GetByIdAsync(paymentId))
            .ReturnsAsync(payment);

        var act = async () => await _paymentService.RefundPaymentAsync(paymentId);

        await act.Should().ThrowAsync<ValidationException>();

        _paymentRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Payment>()), Times.Never);
    }

    [Fact]
    public async Task RefundPaymentAsync_WithNonExistentPayment_ThrowsKeyNotFoundException()
    {
        var paymentId = 999;

        _paymentRepositoryMock
            .Setup(r => r.GetByIdAsync(paymentId))
            .ReturnsAsync((Payment)null);

        var act = async () => await _paymentService.RefundPaymentAsync(paymentId);

        await act.Should().ThrowAsync<KeyNotFoundException>();

        _paymentRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Payment>()), Times.Never);
    }
}