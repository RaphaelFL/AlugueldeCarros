using AlugueldeCarros.Domain.Entities;

namespace AlugueldeCarros.Repositories;

public interface IVehicleCategoryRepository
{
    Task<IEnumerable<VehicleCategory>> GetAllAsync();
    Task<VehicleCategory> GetByIdAsync(int id);
    Task AddAsync(VehicleCategory category);
    Task UpdateAsync(VehicleCategory category);
    Task DeleteAsync(int id);
}
