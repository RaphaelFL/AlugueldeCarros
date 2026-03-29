using AlugueldeCarros.Domain.Entities;

namespace AlugueldeCarros.Services;

public interface IBranchService
{
    Task<IEnumerable<Branch>> GetAllAsync();
}
