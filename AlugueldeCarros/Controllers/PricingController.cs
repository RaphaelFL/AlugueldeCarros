using AlugueldeCarros.Domain.Entities;
using AlugueldeCarros.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlugueldeCarros.Controllers;

[ApiController]
[Route("api/v1/pricing/rules")]
public class PricingController : ControllerBase
{
    private readonly PricingService _pricingService;

    public PricingController(PricingService pricingService)
    {
        _pricingService = pricingService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var rules = await _pricingService.GetAllAsync();
        return Ok(rules);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] PricingRule rule)
    {
        var created = await _pricingService.CreateAsync(rule);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPatch("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] PricingRule rule)
    {
        var updated = await _pricingService.UpdateAsync(id, rule);
        return Ok(updated);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var rule = await _pricingService.GetByIdAsync(id);
        if (rule == null) return NotFound();
        return Ok(rule);
    }
}
