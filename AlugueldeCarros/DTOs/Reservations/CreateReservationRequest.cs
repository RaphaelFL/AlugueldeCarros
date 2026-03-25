namespace AlugueldeCarros.DTOs.Reservations;

public class CreateReservationRequest
{
    public int CategoryId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
