using System.ComponentModel.DataAnnotations;

namespace AlugueldeCarros.DTOs.Payments;

public class PreauthRequest
{
    [Range(1, int.MaxValue)]
    public int ReservationId { get; set; }

    [Range(typeof(decimal), "0.01", "1000000")]
    public decimal Amount { get; set; }
}
