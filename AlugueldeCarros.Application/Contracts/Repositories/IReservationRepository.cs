using AlugueldeCarros.Domain.Entities;

namespace AlugueldeCarros.Repositories;

public interface IReservationRepository
{
    Task<Reservation> GetByIdAsync(int id);
    Task<IEnumerable<Reservation>> GetAllAsync();
    Task<IEnumerable<Reservation>> GetByUserIdAsync(int userId);
    Task AddAsync(Reservation reservation);
    Task UpdateAsync(Reservation reservation);
    Task DeleteAsync(int id);
}
