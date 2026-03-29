using AlugueldeCarros.Domain.Entities;

namespace AlugueldeCarros.Repositories;

public interface IRoleRepository
{
    Task<IEnumerable<Role>> GetAllAsync();
    Task<Role> GetByIdAsync(int id);
    Task<Role> GetByNameAsync(string name);
    Task AddAsync(Role role);
}
