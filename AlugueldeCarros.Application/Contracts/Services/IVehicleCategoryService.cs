using AlugueldeCarros.Domain.Entities;

namespace AlugueldeCarros.Services;

public interface IVehicleCategoryService
{
    Task<IEnumerable<VehicleCategory>> GetAllAsync();
}
