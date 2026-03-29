using AlugueldeCarros.Domain.Entities;

namespace AlugueldeCarros.Repositories;

public interface IVehicleRepository
{
    Task<Vehicle> GetByIdAsync(int id);
    Task<IEnumerable<Vehicle>> GetAllAsync();
    Task<IEnumerable<Vehicle>> SearchAsync(int? categoryId, DateTime? startDate, DateTime? endDate);
    Task AddAsync(Vehicle vehicle);
    Task UpdateAsync(Vehicle vehicle);
    Task DeleteAsync(int id);
}
