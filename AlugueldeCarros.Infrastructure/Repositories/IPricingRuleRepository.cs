using AlugueldeCarros.Domain.Entities;

namespace AlugueldeCarros.Repositories;

public class InMemoryPricingRuleRepository : IPricingRuleRepository
{
    private readonly List<PricingRule> _rules = new();

    public Task<IEnumerable<PricingRule>> GetAllAsync() => Task.FromResult(_rules.AsEnumerable());
    public Task<PricingRule> GetByIdAsync(int id) => Task.FromResult(_rules.FirstOrDefault(r => r.Id == id));
    public Task AddAsync(PricingRule pricingRule) { pricingRule.Id = _rules.Count + 1; _rules.Add(pricingRule); return Task.CompletedTask; }
    public Task UpdateAsync(PricingRule pricingRule) { var existing = _rules.FirstOrDefault(r => r.Id == pricingRule.Id); if (existing != null) { _rules.Remove(existing); _rules.Add(pricingRule); } return Task.CompletedTask; }
    public Task DeleteAsync(int id) { _rules.RemoveAll(r => r.Id == id); return Task.CompletedTask; }
}