using AlugueldeCarros.Domain.Entities;

namespace AlugueldeCarros.Repositories;

public interface IPaymentRepository
{
    Task<Payment> GetByIdAsync(int id);
    Task AddAsync(Payment payment);
    Task UpdateAsync(Payment payment);
}
