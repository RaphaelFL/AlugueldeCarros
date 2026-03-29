using System.ComponentModel.DataAnnotations;

namespace AlugueldeCarros.DTOs.Auth;

public class RegisterRequest
{
    [Required]
    [EmailAddress]
    [StringLength(120)]
    public string Email { get; set; }

    [Required]
    [MinLength(6)]
    [StringLength(128)]
    public string Password { get; set; }

    [Required]
    [StringLength(60, MinimumLength = 2)]
    public string FirstName { get; set; }

    [Required]
    [StringLength(60, MinimumLength = 2)]
    public string LastName { get; set; }
}

public class LoginRequest
{
    [Required]
    [EmailAddress]
    [StringLength(120)]
    public string Email { get; set; }

    [Required]
    [MinLength(6)]
    [StringLength(128)]
    public string Password { get; set; }
}

public class AuthResponse
{
    public string Token { get; set; }
    public string Email { get; set; }
}