using AlugueldeCarros.Domain.Entities;

namespace AlugueldeCarros.Services;

public interface IUserService
{
    Task<User> GetUserByIdAsync(int id);
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task AssignRolesAsync(int userId, IEnumerable<string> roles);
}
