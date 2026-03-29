using AlugueldeCarros.Domain.Entities;

namespace AlugueldeCarros.Security;

public interface ITokenService
{
    string GenerateToken(User user, List<string> roles);
    int GetUserIdFromToken(string token);
}
