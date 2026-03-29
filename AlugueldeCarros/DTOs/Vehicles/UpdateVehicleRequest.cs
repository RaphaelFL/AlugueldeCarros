using AlugueldeCarros.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace AlugueldeCarros.DTOs.Vehicles;

public class UpdateVehicleRequest
{
    [Required]
    [StringLength(10, MinimumLength = 3)]
    public string LicensePlate { get; set; }

    [Required]
    [StringLength(80, MinimumLength = 2)]
    public string Model { get; set; }

    [Range(2000, 2100)]
    public int Year { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    [Range(1, int.MaxValue)]
    public int BranchId { get; set; }

    [Range(typeof(decimal), "0.01", "1000000")]
    public decimal DailyRate { get; set; }
    public VehicleStatus Status { get; set; }
}
