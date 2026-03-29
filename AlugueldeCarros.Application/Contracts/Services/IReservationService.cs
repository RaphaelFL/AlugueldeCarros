using AlugueldeCarros.Domain.Entities;
using AlugueldeCarros.Domain.Enums;

namespace AlugueldeCarros.Services;

public interface IReservationService
{
    Task<Reservation> CreateReservationAsync(int userId, int categoryId, DateTime startDate, DateTime endDate);
    Task<Reservation> GetByIdAsync(int id);
    Task<IEnumerable<Reservation>> GetByUserIdAsync(int userId);
    Task UpdateReservationAsync(int reservationId, DateTime? startDate, DateTime? endDate, ReservationStatus? status);
    Task CancelReservationAsync(int reservationId);
}
