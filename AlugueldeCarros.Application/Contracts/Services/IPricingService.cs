using AlugueldeCarros.Domain.Entities;

namespace AlugueldeCarros.Services;

public interface IPricingService
{
    Task<IEnumerable<PricingRule>> GetAllAsync();
    Task<PricingRule> GetByIdAsync(int id);
    Task<PricingRule> CreateAsync(PricingRule rule);
    Task<PricingRule> UpdateAsync(int id, PricingRule rule);
}
