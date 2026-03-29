namespace AlugueldeCarros.Domain.Entities;

using AlugueldeCarros.Domain.Enums;

public class Vehicle
{
    public int Id { get; set; }
    public string LicensePlate { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
    public int CategoryId { get; set; }
    public int BranchId { get; set; }
    public VehicleStatus Status { get; set; }
    public decimal DailyRate { get; set; }
    public VehicleCategory Category { get; set; }
    public Branch Branch { get; set; }
}