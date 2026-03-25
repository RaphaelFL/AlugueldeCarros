using AlugueldeCarros.Domain.Entities;
using AlugueldeCarros.Domain.Enums;
using AlugueldeCarros.Repositories;
using System.ComponentModel.DataAnnotations;

namespace AlugueldeCarros.Services;

public class ReservationService
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IPaymentRepository _paymentRepository;

    public ReservationService(IReservationRepository reservationRepository, IVehicleRepository vehicleRepository, IPaymentRepository paymentRepository)
    {
        _reservationRepository = reservationRepository;
        _vehicleRepository = vehicleRepository;
        _paymentRepository = paymentRepository;
    }

    public async Task<Reservation> CreateReservationAsync(int userId, int categoryId, DateTime startDate, DateTime endDate)
    {
        if (endDate <= startDate)
            throw new ValidationException("End date must be after start date");

        // Validar máximo 5 reservas ativas (CONFIRMED ou PENDING_PAYMENT)
        var userReservations = await _reservationRepository.GetByUserIdAsync(userId);
        var activeReservations = userReservations.Count(r => r.Status == ReservationStatus.CONFIRMED || r.Status == ReservationStatus.PENDING_PAYMENT);
        if (activeReservations >= 5)
            throw new ValidationException("User has reached maximum of 5 active reservations");

        var availableVehicles = await _vehicleRepository.SearchAsync(categoryId, startDate, endDate);
        if (!availableVehicles.Any()) throw new ValidationException("No vehicles available");

        var selectedVehicle = availableVehicles.First();

        var reservation = new Reservation
        {
            UserId = userId,
            CategoryId = categoryId,
            VehicleId = selectedVehicle.Id,
            StartDate = startDate,
            EndDate = endDate,
            Status = ReservationStatus.PENDING_PAYMENT,
            TotalAmount = CalculateAmount(startDate, endDate, selectedVehicle.DailyRate)
        };

        await _reservationRepository.AddAsync(reservation);
        return reservation;
    }

    public Task<Reservation> GetByIdAsync(int id) => _reservationRepository.GetByIdAsync(id);

    public Task<IEnumerable<Reservation>> GetByUserIdAsync(int userId) => _reservationRepository.GetByUserIdAsync(userId);

    public async Task UpdateReservationAsync(int reservationId, DateTime? startDate, DateTime? endDate, ReservationStatus? status)
    {
        var existing = await _reservationRepository.GetByIdAsync(reservationId);
        if (existing == null) throw new KeyNotFoundException("Reservation not found");

        if (startDate.HasValue) existing.StartDate = startDate.Value;
        if (endDate.HasValue) existing.EndDate = endDate.Value;
        if (status.HasValue) existing.Status = status.Value;

        await _reservationRepository.UpdateAsync(existing);
    }

    public async Task CancelReservationAsync(int reservationId)
    {
        var existing = await _reservationRepository.GetByIdAsync(reservationId);
        if (existing == null) throw new KeyNotFoundException("Reservation not found");

        // Validar se faltam menos de 2 horas para check-in
        if (existing.StartDate <= DateTime.UtcNow.AddHours(2))
            throw new ValidationException("Cannot cancel reservation within 2 hours of check-in");

        existing.Status = ReservationStatus.CANCELLED;
        await _reservationRepository.UpdateAsync(existing);

        // Calcular refund e criar payment de refund
        var refundPercentage = CalculateRefundPercentage(existing.StartDate);
        var refundAmount = existing.TotalAmount * (refundPercentage / 100m);

        var refundPayment = new Payment
        {
            ReservationId = reservationId,
            Amount = refundAmount,
            Status = PaymentStatus.REFUNDED,
            CreatedAt = DateTime.UtcNow
        };
        await _paymentRepository.AddAsync(refundPayment);
    }

    private decimal CalculateAmount(DateTime start, DateTime end, decimal dailyRate)
    {
        return Math.Max(1, (end - start).Days) * dailyRate;
    }

    private decimal CalculateRefundPercentage(DateTime checkInDate)
    {
        var daysUntilCheckIn = (checkInDate - DateTime.UtcNow).TotalDays;
        
        if (daysUntilCheckIn > 7)
            return 100m;
        if (daysUntilCheckIn > 3)
            return 80m;
        if (daysUntilCheckIn > 1)
            return 50m;
        
        return 0m;
    }
}