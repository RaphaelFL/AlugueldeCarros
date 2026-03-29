using AlugueldeCarros.Domain.Entities;

namespace AlugueldeCarros.Repositories;

public interface IBranchRepository
{
    Task<IEnumerable<Branch>> GetAllAsync();
    Task<Branch> GetByIdAsync(int id);
    Task AddAsync(Branch branch);
    Task UpdateAsync(Branch branch);
    Task DeleteAsync(int id);
}
