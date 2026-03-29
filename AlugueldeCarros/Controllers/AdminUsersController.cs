using AlugueldeCarros.DTOs.Users;
using AlugueldeCarros.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlugueldeCarros.Controllers;

[ApiController]
[Route("api/v1/admin/users")]
[Authorize(Roles = "Admin")]
public class AdminUsersController : ControllerBase
{
    private readonly IUserService _userService;

    public AdminUsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users.Select(u => new
        {
            u.Id,
            u.Email,
            u.FirstName,
            u.LastName,
            u.CreatedAt,
            u.Roles
        }));
    }

    [HttpPost("{id}/roles")]
    public async Task<IActionResult> AddRolesToUser(int id, [FromBody] AddUserRolesRequest request)
    {
        if (!request.Roles.Any(role => !string.IsNullOrWhiteSpace(role)))
            return BadRequest(new { error = "At least one valid role is required" });

        await _userService.AssignRolesAsync(id, request.Roles);
        return NoContent();
    }
}