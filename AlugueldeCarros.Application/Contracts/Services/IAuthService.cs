namespace AlugueldeCarros.Services;

public interface IAuthService
{
    Task<string> RegisterAsync(string email, string password, string firstName, string lastName);
    Task<string> LoginAsync(string email, string password);
    Task<string> RefreshAsync(string token);
}
