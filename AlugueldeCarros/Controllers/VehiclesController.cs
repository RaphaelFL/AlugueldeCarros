using AlugueldeCarros.Services;
using Microsoft.AspNetCore.Mvc;

namespace AlugueldeCarros.Controllers;

[ApiController]
[Route("api/v1/vehicles")]
public class VehiclesController : ControllerBase
{
    private readonly IVehicleService _vehicleService;
    private readonly IVehicleCategoryService _categoryService;

    public VehiclesController(IVehicleService vehicleService, IVehicleCategoryService categoryService)
    {
        _vehicleService = vehicleService;
        _categoryService = categoryService;
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _categoryService.GetAllAsync();
        return Ok(categories);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] int? branchId, [FromQuery(Name = "from")] DateTime? from, [FromQuery(Name = "to")] DateTime? to, [FromQuery] int? categoryId, [FromQuery] decimal? priceMin, [FromQuery] decimal? priceMax, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var s = startDate ?? from;
        var e = endDate ?? to;
        var results = await _vehicleService.SearchVehiclesAsync(branchId, s, e, categoryId, priceMin, priceMax);
        return Ok(results);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var v = await _vehicleService.GetVehicleByIdAsync(id);
        if (v == null) return NotFound();
        return Ok(v);
    }
}
