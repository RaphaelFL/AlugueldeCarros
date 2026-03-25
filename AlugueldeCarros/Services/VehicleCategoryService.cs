using AlugueldeCarros.Domain.Entities;
using AlugueldeCarros.Repositories;

namespace AlugueldeCarros.Services;

public class VehicleCategoryService
{
    private readonly IVehicleCategoryRepository _categoryRepository;

    public VehicleCategoryService(IVehicleCategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public Task<IEnumerable<VehicleCategory>> GetAllAsync() => _categoryRepository.GetAllAsync();
}
