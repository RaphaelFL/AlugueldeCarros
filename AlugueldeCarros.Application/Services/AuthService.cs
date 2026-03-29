using AlugueldeCarros.Domain.Entities;
using AlugueldeCarros.Repositories;
using AlugueldeCarros.Security;

namespace AlugueldeCarros.Services;

public class AuthService
    : IAuthService
{
    private const string InvalidTokenMessage = "Invalid token";
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;

    public AuthService(IUserRepository userRepository, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
    }

    public async Task<string> RegisterAsync(string email, string password, string firstName, string lastName)
    {
        email = NormalizeEmail(email);
        firstName = firstName.Trim();
        lastName = lastName.Trim();

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

        return _tokenService.GenerateToken(user, user.Roles);
    }

    public async Task<string> LoginAsync(string email, string password)
    {
        email = NormalizeEmail(email);
        var user = await _userRepository.GetByEmailAsync(email);

        if (user == null || !IsPasswordValid(password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials");

        var roles = (user.Roles?.Any() ?? false) ? user.Roles : new List<string> { "Customer" };

        return _tokenService.GenerateToken(user, roles);
    }

    public async Task<string> RefreshAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new UnauthorizedAccessException(InvalidTokenMessage);

        int userId;
        try
        {
            userId = _tokenService.GetUserIdFromToken(token);
        }
        catch (ArgumentException)
        {
            throw new UnauthorizedAccessException(InvalidTokenMessage);
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) throw new UnauthorizedAccessException(InvalidTokenMessage);

        var roles = (user.Roles?.Any() ?? false) ? user.Roles : new List<string> { "Customer" };
        return _tokenService.GenerateToken(user, roles);
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
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