using AlugueldeCarros.Domain.Entities;
using AlugueldeCarros.Repositories;
using AlugueldeCarros.Security;

namespace AlugueldeCarros.Services;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly JwtTokenService _jwtTokenService;

    public AuthService(IUserRepository userRepository, JwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<string> RegisterAsync(string email, string password, string firstName, string lastName)
    {
        var existingUser = await _userRepository.GetByEmailAsync(email);
        if (existingUser != null)
            throw new InvalidOperationException("User already exists");

        var user = new User
        {
            Email = email,
            PasswordHash = PasswordHasher.HashPassword(password),
            FirstName = firstName,
            LastName = lastName,
            CreatedAt = DateTime.UtcNow,
            Roles = new List<string> { "Customer" }
        };

        await _userRepository.AddAsync(user);

        return _jwtTokenService.GenerateToken(user, user.Roles);
    }

    public async Task<string> LoginAsync(string email, string password)
    {
        var user = await _userRepository.GetByEmailAsync(email);

        if (user == null || !IsPasswordValid(password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials");

        var roles = (user.Roles?.Any() ?? false) ? user.Roles : new List<string> { "Customer" };

        return _jwtTokenService.GenerateToken(user, roles);
    }

    public async Task<string> RefreshAsync(string token)
    {
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("Invalid token");

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) throw new UnauthorizedAccessException("User not found");

        var roles = (user.Roles?.Any() ?? false) ? user.Roles : new List<string> { "Customer" };
        return _jwtTokenService.GenerateToken(user, roles);
    }

    private static bool IsPasswordValid(string password, string storedPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(storedPasswordHash))
            return false;

        try
        {
            if (PasswordHasher.VerifyPassword(password, storedPasswordHash))
                return true;
        }
        catch
        {
        }

        return password == storedPasswordHash;
    }
}