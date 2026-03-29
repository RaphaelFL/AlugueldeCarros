using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using AlugueldeCarros.DTOs.Auth;
using AlugueldeCarros.Services;

namespace AlugueldeCarros.Controllers;

[ApiController]
[Route("api/v1/auth")]
[EnableRateLimiting("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var token = await _authService.RegisterAsync(request.Email, request.Password, request.FirstName, request.LastName);
        return Ok(new AuthResponse { Token = token, Email = request.Email });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var token = await _authService.LoginAsync(request.Email, request.Password);
        return Ok(new AuthResponse { Token = token, Email = request.Email });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] string token)
    {
        var newToken = await _authService.RefreshAsync(token);
        return Ok(new AuthResponse { Token = newToken });
    }
}