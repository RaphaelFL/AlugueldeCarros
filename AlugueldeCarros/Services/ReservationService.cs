using AlugueldeCarros.Domain.Entities;
using AlugueldeCarros.Domain.Enums;
using AlugueldeCarros.Repositories;
using System.ComponentModel.DataAnnotations;

namespace AlugueldeCarros.Services;

public class ReservationService
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IVehicleRepository _vehicleRepository;

    public ReservationService(IReservationRepository reservationRepository, IVehicleRepository vehicleRepository)
    {
        _reservationRepository = reservationRepository;
        _vehicleRepository = vehicleRepository;
    }

    public async Task<Reservation> CreateReservationAsync(int userId, int categoryId, DateTime startDate, DateTime endDate)
    {
        if (endDate <= startDate)
            throw new ValidationException("End date must be after start date");

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

        existing.Status = ReservationStatus.CANCELLED;
        await _reservationRepository.UpdateAsync(existing);
    }

    private decimal CalculateAmount(DateTime start, DateTime end, decimal dailyRate)
    {
        return Math.Max(1, (end - start).Days) * dailyRate;
    }
}