using AlugueldeCarros.Domain.Entities;
using AlugueldeCarros.DTOs.Vehicles;
using AlugueldeCarros.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlugueldeCarros.Controllers;

[ApiController]
[Route("api/v1/admin/vehicles")]
[Authorize(Roles = "Admin")]
public class AdminVehiclesController : ControllerBase
{
    private readonly VehicleService _vehicleService;

    public AdminVehiclesController(VehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVehicleRequest request)
    {
        var vehicle = new Vehicle
        {
            LicensePlate = request.LicensePlate,
            Model = request.Model,
            Year = request.Year,
            CategoryId = request.CategoryId,
            BranchId = request.BranchId,
            DailyRate = request.DailyRate,
            Status = request.Status
        };

        var created = await _vehicleService.CreateVehicleAsync(vehicle);
        return Created($"/api/v1/vehicles/{created.Id}", created);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateVehicleRequest request)
    {
        var vehicle = new Vehicle
        {
            Id = id,
            LicensePlate = request.LicensePlate,
            Model = request.Model,
            Year = request.Year,
            CategoryId = request.CategoryId,
            BranchId = request.BranchId,
            DailyRate = request.DailyRate,
            Status = request.Status
        };

        var updated = await _vehicleService.UpdateVehicleAsync(id, vehicle);
        return Ok(updated);
    }

}
