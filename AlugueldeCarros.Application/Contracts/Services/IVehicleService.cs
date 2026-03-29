using AlugueldeCarros.Domain.Entities;

namespace AlugueldeCarros.Services;

public interface IVehicleService
{
    Task<IEnumerable<Vehicle>> SearchVehiclesAsync(int? branchId, DateTime? startDate, DateTime? endDate, int? categoryId, decimal? priceMin, decimal? priceMax);
    Task<IEnumerable<Vehicle>> GetAllVehiclesAsync();
    Task<Vehicle> GetVehicleByIdAsync(int id);
    Task<Vehicle> CreateVehicleAsync(Vehicle vehicle);
    Task<Vehicle> UpdateVehicleAsync(int id, Vehicle vehicle);
}
