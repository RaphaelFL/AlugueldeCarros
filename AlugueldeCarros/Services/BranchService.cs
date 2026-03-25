using AlugueldeCarros.Domain.Entities;
using AlugueldeCarros.Repositories;

namespace AlugueldeCarros.Services;

public class BranchService
{
    private readonly IBranchRepository _branchRepository;

    public BranchService(IBranchRepository branchRepository)
    {
        _branchRepository = branchRepository;
    }

    public Task<IEnumerable<Branch>> GetAllAsync() => _branchRepository.GetAllAsync();
}
