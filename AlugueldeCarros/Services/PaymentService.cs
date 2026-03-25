using AlugueldeCarros.Domain.Entities;
using AlugueldeCarros.Domain.Enums;
using AlugueldeCarros.Repositories;
using System.ComponentModel.DataAnnotations;

namespace AlugueldeCarros.Services;

public class PaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IReservationRepository _reservationRepository;

    public PaymentService(IPaymentRepository paymentRepository, IReservationRepository reservationRepository)
    {
        _paymentRepository = paymentRepository;
        _reservationRepository = reservationRepository;
    }

    public async Task<Payment> PreauthorizePaymentAsync(int reservationId, decimal amount)
    {
        var reservation = await _reservationRepository.GetByIdAsync(reservationId);
        if (reservation == null) throw new ValidationException("Reservation not found");

        var payment = new Payment
        {
            ReservationId = reservationId,
            Amount = amount,
            Status = PaymentStatus.PENDING,
            CreatedAt = DateTime.UtcNow
        };

        await _paymentRepository.AddAsync(payment);
        return payment;
    }

    public Task<Payment> GetByIdAsync(int id) => _paymentRepository.GetByIdAsync(id);

    public async Task<Payment> CapturePaymentAsync(int paymentId)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        if (payment == null) throw new KeyNotFoundException("Payment not found");

        if (payment.Status != PaymentStatus.PENDING)
            throw new ValidationException("Only pending payments can be captured");

        payment.Status = PaymentStatus.APPROVED;
        await _paymentRepository.UpdateAsync(payment);
        return payment;
    }

    public async Task<Payment> RefundPaymentAsync(int paymentId)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        if (payment == null) throw new KeyNotFoundException("Payment not found");

        if (payment.Status != PaymentStatus.APPROVED)
            throw new ValidationException("Only approved payments can be refunded");

        payment.Status = PaymentStatus.REFUNDED;
        await _paymentRepository.UpdateAsync(payment);
        return payment;
    }
}