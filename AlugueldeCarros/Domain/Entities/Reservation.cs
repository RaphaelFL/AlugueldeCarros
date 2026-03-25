namespace AlugueldeCarros.Domain.Entities;

using AlugueldeCarros.Domain.Enums;

public class Reservation
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int CategoryId { get; set; }
    public int? VehicleId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public ReservationStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public User User { get; set; }
    public VehicleCategory Category { get; set; }
    public Vehicle? Vehicle { get; set; }
}