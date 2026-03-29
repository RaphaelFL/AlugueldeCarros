using AlugueldeCarros.DTOs.Reservations;
using AlugueldeCarros.Domain.Entities;
using AlugueldeCarros.Domain.Enums;
using AlugueldeCarros.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlugueldeCarros.Controllers;

[ApiController]
[Route("api/v1/reservations")]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservationService;

    public ReservationsController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateReservationRequest request)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var reservation = await _reservationService.CreateReservationAsync(userId, request.CategoryId, request.StartDate, request.EndDate);
        return CreatedAtAction(nameof(GetById), new { id = reservation.Id }, reservation);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
    {
        var res = await _reservationService.GetByIdAsync(id);
        if (res == null) return NotFound();

        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var isAdmin = User.IsInRole("Admin");
        if (!isAdmin && (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId) || res.UserId != userId))
            return Forbid();

        return Ok(res);
    }

    [HttpPatch("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateReservationRequest request)
    {
        var res = await _reservationService.GetByIdAsync(id);
        if (res == null) return NotFound();

        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var isAdmin = User.IsInRole("Admin");
        if (!isAdmin && (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId) || res.UserId != userId))
            return Forbid();

        await _reservationService.UpdateReservationAsync(id, request.StartDate, request.EndDate, request.Status);
        return NoContent();
    }

    [HttpPost("{id}/cancel")]
    [Authorize]
    public async Task<IActionResult> Cancel(int id)
    {
        var res = await _reservationService.GetByIdAsync(id);
        if (res == null) return NotFound();

        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var isAdmin = User.IsInRole("Admin");
        if (!isAdmin && (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId) || res.UserId != userId))
            return Forbid();

        await _reservationService.CancelReservationAsync(id);
        return NoContent();
    }
}
