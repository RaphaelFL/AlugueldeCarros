using AlugueldeCarros.Domain.Entities;

namespace AlugueldeCarros.Repositories;

public interface IRoleRepository
{
    Task<IEnumerable<Role>> GetAllAsync();
    Task<Role> GetByIdAsync(int id);
    Task<Role> GetByNameAsync(string name);
    Task AddAsync(Role role);
}

public class InMemoryRoleRepository : IRoleRepository
{
    private readonly List<Role> _roles = new();

    public Task<IEnumerable<Role>> GetAllAsync() => Task.FromResult(_roles.AsEnumerable());
    public Task<Role> GetByIdAsync(int id) => Task.FromResult(_roles.FirstOrDefault(r => r.Id == id));
    public Task<Role> GetByNameAsync(string name) => Task.FromResult(_roles.FirstOrDefault(r => r.Name.Equals(name, StringComparison.OrdinalIgnoreCase)));
    public Task AddAsync(Role role) { role.Id = _roles.Count + 1; _roles.Add(role); return Task.CompletedTask; }
}