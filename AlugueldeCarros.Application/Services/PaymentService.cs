using AlugueldeCarros.Domain.Entities;
using AlugueldeCarros.Domain.Enums;
using AlugueldeCarros.Repositories;
using System.ComponentModel.DataAnnotations;

namespace AlugueldeCarros.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IVehicleRepository _vehicleRepository;

    public PaymentService(IPaymentRepository paymentRepository, IReservationRepository reservationRepository, IVehicleRepository vehicleRepository)
    {
        _paymentRepository = paymentRepository;
        _reservationRepository = reservationRepository;
        _vehicleRepository = vehicleRepository;
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

        // Mock determinístico: 90% APPROVED (paymentId % 10 < 9), 10% DECLINED (paymentId % 10 >= 9)
        var isApproved = (paymentId % 10) < 9;
        payment.Status = isApproved ? PaymentStatus.APPROVED : PaymentStatus.DECLINED;
        await _paymentRepository.UpdateAsync(payment);

        // Se aprovado, atualizar status da reserva para CONFIRMED
        if (isApproved)
        {
            var reservation = await _reservationRepository.GetByIdAsync(payment.ReservationId);
            if (reservation != null)
            {
                reservation.Status = ReservationStatus.CONFIRMED;
                await _reservationRepository.UpdateAsync(reservation);

                // Atualizar status do veículo para RESERVED
                if (reservation.VehicleId.HasValue)
                {
                    var vehicle = await _vehicleRepository.GetByIdAsync(reservation.VehicleId.Value);
                    if (vehicle != null)
                    {
                        vehicle.Status = VehicleStatus.RESERVED;
                        await _vehicleRepository.UpdateAsync(vehicle);
                    }
                }
            }
        }

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