using AlugueldeCarros.Domain.Entities;
using AlugueldeCarros.Repositories;

namespace AlugueldeCarros.Services;

public class UserService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;

    public UserService(IUserRepository userRepository, IRoleRepository roleRepository)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
    }

    public async Task<User> GetUserByIdAsync(int id)
    {
        return await _userRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _userRepository.GetAllAsync();
    }

    public async Task AssignRolesAsync(int userId, IEnumerable<string> roles)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) throw new KeyNotFoundException("User not found");

        user.Roles = user.Roles ?? new List<string>();

        foreach (var roleName in roles.Where(r => !string.IsNullOrWhiteSpace(r)))
        {
            var existingRole = await _roleRepository.GetByNameAsync(roleName);
            if (existingRole == null)
                throw new InvalidOperationException($"Role '{roleName}' not found");

            if (!user.Roles.Contains(existingRole.Name, StringComparer.OrdinalIgnoreCase))
                user.Roles.Add(existingRole.Name);
        }

        await _userRepository.UpdateAsync(user);
    }
}