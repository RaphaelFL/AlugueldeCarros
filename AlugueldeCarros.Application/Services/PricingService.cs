using AlugueldeCarros.Domain.Entities;
using AlugueldeCarros.Repositories;

namespace AlugueldeCarros.Services;

public class PricingService : IPricingService
{
    private readonly IPricingRuleRepository _pricingRuleRepository;

    public PricingService(IPricingRuleRepository pricingRuleRepository)
    {
        _pricingRuleRepository = pricingRuleRepository;
    }

    public Task<IEnumerable<PricingRule>> GetAllAsync() => _pricingRuleRepository.GetAllAsync();
    public Task<PricingRule> GetByIdAsync(int id) => _pricingRuleRepository.GetByIdAsync(id);
    public async Task<PricingRule> CreateAsync(PricingRule rule)
    {
        await _pricingRuleRepository.AddAsync(rule);
        return rule;
    }

    public async Task<PricingRule> UpdateAsync(int id, PricingRule rule)
    {
        var existing = await _pricingRuleRepository.GetByIdAsync(id);
        if (existing == null) throw new KeyNotFoundException("Pricing rule not found");

        rule.Id = id;
        await _pricingRuleRepository.UpdateAsync(rule);
        return rule;
    }
}