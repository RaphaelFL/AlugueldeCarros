// Similar para outros DTOs, como VehicleDto, ReservationDto, etc. Exemplo para VehicleDto:

namespace AlugueldeCarros.DTOs.Vehicles;

public class VehicleDto
{
    public int Id { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
    public string CategoryName { get; set; }
    public decimal DailyRate { get; set; }
}