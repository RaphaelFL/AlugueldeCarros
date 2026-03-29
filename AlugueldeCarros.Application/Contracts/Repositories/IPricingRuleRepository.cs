using AlugueldeCarros.Domain.Entities;

namespace AlugueldeCarros.Repositories;

public interface IPricingRuleRepository
{
    Task<IEnumerable<PricingRule>> GetAllAsync();
    Task<PricingRule> GetByIdAsync(int id);
    Task AddAsync(PricingRule pricingRule);
    Task UpdateAsync(PricingRule pricingRule);
    Task DeleteAsync(int id);
}
