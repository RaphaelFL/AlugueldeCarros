using System.ComponentModel.DataAnnotations;

namespace AlugueldeCarros.DTOs.Reservations;

public class CreateReservationRequest
{
    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
