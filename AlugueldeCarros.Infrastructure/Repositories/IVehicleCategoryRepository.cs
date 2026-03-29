using AlugueldeCarros.Domain.Entities;

namespace AlugueldeCarros.Repositories;

public class InMemoryVehicleCategoryRepository : IVehicleCategoryRepository
{
    private readonly List<VehicleCategory> _categories = new();

    public Task<IEnumerable<VehicleCategory>> GetAllAsync() => Task.FromResult(_categories.AsEnumerable());
    public Task<VehicleCategory> GetByIdAsync(int id) => Task.FromResult(_categories.FirstOrDefault(c => c.Id == id));
    public Task AddAsync(VehicleCategory category) { category.Id = _categories.Count + 1; _categories.Add(category); return Task.CompletedTask; }
    public Task UpdateAsync(VehicleCategory category) { var existing = _categories.FirstOrDefault(c => c.Id == category.Id); if (existing != null) { _categories.Remove(existing); _categories.Add(category); } return Task.CompletedTask; }
    public Task DeleteAsync(int id) { _categories.RemoveAll(c => c.Id == id); return Task.CompletedTask; }
}