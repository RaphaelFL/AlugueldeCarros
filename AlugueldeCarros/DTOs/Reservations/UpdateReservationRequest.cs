using AlugueldeCarros.Domain.Enums;

namespace AlugueldeCarros.DTOs.Reservations;

public class UpdateReservationRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public ReservationStatus? Status { get; set; }
}
