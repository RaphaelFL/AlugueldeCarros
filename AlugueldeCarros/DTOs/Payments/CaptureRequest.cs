using System.ComponentModel.DataAnnotations;

namespace AlugueldeCarros.DTOs.Payments;

public class CaptureRequest
{
    [Range(1, int.MaxValue)]
    public int PaymentId { get; set; }
}
