namespace AlugueldeCarros.Domain.Entities;

using AlugueldeCarros.Domain.Enums;

public class Payment
{
    public int Id { get; set; }
    public int ReservationId { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public Reservation Reservation { get; set; }
}