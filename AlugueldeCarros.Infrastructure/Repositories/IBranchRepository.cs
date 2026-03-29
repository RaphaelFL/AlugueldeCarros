using AlugueldeCarros.Domain.Entities;

namespace AlugueldeCarros.Repositories;

public class InMemoryBranchRepository : IBranchRepository
{
    private readonly List<Branch> _branches = new();

    public Task<IEnumerable<Branch>> GetAllAsync() => Task.FromResult(_branches.AsEnumerable());
    public Task<Branch> GetByIdAsync(int id) => Task.FromResult(_branches.FirstOrDefault(b => b.Id == id));
    public Task AddAsync(Branch branch) { branch.Id = _branches.Count + 1; _branches.Add(branch); return Task.CompletedTask; }
    public Task UpdateAsync(Branch branch) { var existing = _branches.FirstOrDefault(b => b.Id == branch.Id); if (existing != null) { _branches.Remove(existing); _branches.Add(branch); } return Task.CompletedTask; }
    public Task DeleteAsync(int id) { _branches.RemoveAll(b => b.Id == id); return Task.CompletedTask; }
}