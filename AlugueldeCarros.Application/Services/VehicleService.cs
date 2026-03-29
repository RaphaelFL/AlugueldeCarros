using AlugueldeCarros.Domain.Entities;
using AlugueldeCarros.Repositories;

namespace AlugueldeCarros.Services;

public class VehicleService : IVehicleService
{
    private readonly IVehicleRepository _vehicleRepository;

    public VehicleService(IVehicleRepository vehicleRepository)
    {
        _vehicleRepository = vehicleRepository;
    }

    public async Task<IEnumerable<Vehicle>> SearchVehiclesAsync(int? branchId, DateTime? startDate, DateTime? endDate, int? categoryId, decimal? priceMin, decimal? priceMax)
    {
        var all = await _vehicleRepository.GetAllAsync();

        var query = all.AsQueryable();
        if (branchId.HasValue) query = query.Where(v => v.BranchId == branchId.Value);
        if (categoryId.HasValue) query = query.Where(v => v.CategoryId == categoryId.Value);
        if (priceMin.HasValue) query = query.Where(v => v.DailyRate >= priceMin.Value);
        if (priceMax.HasValue) query = query.Where(v => v.DailyRate <= priceMax.Value);

        if (startDate.HasValue || endDate.HasValue)
        {
            var vehiclesInRange = await _vehicleRepository.SearchAsync(categoryId, startDate, endDate);
            query = query.Where(v => vehiclesInRange.Any(x => x.Id == v.Id));
        }

        return query.AsEnumerable();
    }

    public async Task<IEnumerable<Vehicle>> GetAllVehiclesAsync() => await _vehicleRepository.GetAllAsync();

    public async Task<Vehicle> GetVehicleByIdAsync(int id) => await _vehicleRepository.GetByIdAsync(id);

    public async Task<Vehicle> CreateVehicleAsync(Vehicle vehicle)
    {
        await _vehicleRepository.AddAsync(vehicle);
        return vehicle;
    }

    public async Task<Vehicle> UpdateVehicleAsync(int id, Vehicle vehicle)
    {
        var existing = await _vehicleRepository.GetByIdAsync(id);
        if (existing == null) throw new KeyNotFoundException("Vehicle not found");

        vehicle.Id = existing.Id;
        await _vehicleRepository.UpdateAsync(vehicle);
        return vehicle;
    }
}