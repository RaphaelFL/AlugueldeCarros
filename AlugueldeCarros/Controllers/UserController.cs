using AlugueldeCarros.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlugueldeCarros.Controllers;

[ApiController]
[Route("api/v1/users")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly UserService _userService;
    private readonly ReservationService _reservationService;

    public UserController(UserService userService, ReservationService reservationService)
    {
        _userService = userService;
        _reservationService = reservationService;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("Invalid user") ;
        return userId;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = GetCurrentUserId();
        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null) return NotFound();

        return Ok(new
        {
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.CreatedAt,
            Roles = user.Roles
        });
    }

    [HttpGet("me/reservations")]
    public async Task<IActionResult> GetMyReservations()
    {
        var userId = GetCurrentUserId();
        var reservations = await _reservationService.GetByUserIdAsync(userId);
        return Ok(reservations);
    }
}