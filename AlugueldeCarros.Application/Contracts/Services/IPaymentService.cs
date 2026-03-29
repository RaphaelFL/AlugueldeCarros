using AlugueldeCarros.Domain.Entities;

namespace AlugueldeCarros.Services;

public interface IPaymentService
{
    Task<Payment> PreauthorizePaymentAsync(int reservationId, decimal amount);
    Task<Payment> GetByIdAsync(int id);
    Task<Payment> CapturePaymentAsync(int paymentId);
    Task<Payment> RefundPaymentAsync(int paymentId);
}
