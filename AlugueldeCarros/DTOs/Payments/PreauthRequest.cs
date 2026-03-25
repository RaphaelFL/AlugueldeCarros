namespace AlugueldeCarros.DTOs.Payments;

public class PreauthRequest
{
    public int ReservationId { get; set; }
    public decimal Amount { get; set; }
}
