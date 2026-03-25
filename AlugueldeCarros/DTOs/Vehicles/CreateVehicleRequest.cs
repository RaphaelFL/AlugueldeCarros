using AlugueldeCarros.Domain.Enums;

namespace AlugueldeCarros.DTOs.Vehicles;

public class CreateVehicleRequest
{
    public string LicensePlate { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
    public int CategoryId { get; set; }
    public int BranchId { get; set; }
    public decimal DailyRate { get; set; }
    public VehicleStatus Status { get; set; }
}
