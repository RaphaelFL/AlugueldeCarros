using AlugueldeCarros.Domain.Entities;

namespace AlugueldeCarros.Repositories;

public interface IReservationRepository
{
    Task<Reservation> GetByIdAsync(int id);
    Task<IEnumerable<Reservation>> GetByUserIdAsync(int userId);
    Task AddAsync(Reservation reservation);
    Task UpdateAsync(Reservation reservation);
    Task DeleteAsync(int id);
}

public class InMemoryReservationRepository : IReservationRepository
{
    private readonly List<Reservation> _reservations = new();

    public Task<Reservation> GetByIdAsync(int id) => Task.FromResult(_reservations.FirstOrDefault(r => r.Id == id));
    public Task<IEnumerable<Reservation>> GetByUserIdAsync(int userId) => Task.FromResult(_reservations.Where(r => r.UserId == userId).AsEnumerable());
    public Task AddAsync(Reservation reservation) { reservation.Id = _reservations.Count + 1; _reservations.Add(reservation); return Task.CompletedTask; }
    public Task UpdateAsync(Reservation reservation) { var existing = _reservations.FirstOrDefault(r => r.Id == reservation.Id); if (existing != null) { _reservations.Remove(existing); _reservations.Add(reservation); } return Task.CompletedTask; }
    public Task DeleteAsync(int id) { _reservations.RemoveAll(r => r.Id == id); return Task.CompletedTask; }
}