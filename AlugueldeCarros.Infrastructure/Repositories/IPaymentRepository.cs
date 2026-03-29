using AlugueldeCarros.Domain.Entities;

namespace AlugueldeCarros.Repositories;

public class InMemoryPaymentRepository : IPaymentRepository
{
    private readonly List<Payment> _payments = new();

    public Task<Payment> GetByIdAsync(int id) => Task.FromResult(_payments.FirstOrDefault(p => p.Id == id));
    public Task AddAsync(Payment payment) { payment.Id = _payments.Count + 1; _payments.Add(payment); return Task.CompletedTask; }
    public Task UpdateAsync(Payment payment) { var existing = _payments.FirstOrDefault(p => p.Id == payment.Id); if (existing != null) { _payments.Remove(existing); _payments.Add(payment); } return Task.CompletedTask; }
}