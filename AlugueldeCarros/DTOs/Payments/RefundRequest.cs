using System.ComponentModel.DataAnnotations;

namespace AlugueldeCarros.DTOs.Payments;

public class RefundRequest
{
    [Range(1, int.MaxValue)]
    public int PaymentId { get; set; }
}
