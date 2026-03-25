using AlugueldeCarros.Domain.Entities;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AlugueldeCarros.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetAllAsync();
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(int id);
}

public class InMemoryUserRepository : IUserRepository
{
    private readonly List<User> _users = new();
    private readonly string _mockFilePath;

    public InMemoryUserRepository(IConfiguration configuration)
    {
        var basePath = Directory.GetCurrentDirectory();

        _mockFilePath = Path.Combine(basePath, "Resources", "MockData", "users.json");

        // Cria pasta se não existir
        var mockDir = Path.GetDirectoryName(_mockFilePath);
        if (!string.IsNullOrEmpty(mockDir) && !Directory.Exists(mockDir))
            Directory.CreateDirectory(mockDir);

        // Se não existir no diretório de execução, busca arquivo em bin (fallback)
        if (!File.Exists(_mockFilePath))
        {
            var outputPath = Path.Combine(AppContext.BaseDirectory, "Resources", "MockData", "users.json");
            if (File.Exists(outputPath))
                _mockFilePath = outputPath;
        }

        LoadFromFile();
    }

    private void LoadFromFile()
    {
        if (!File.Exists(_mockFilePath))
            return;

        var json = File.ReadAllText(_mockFilePath);
        var users = JsonSerializer.Deserialize<List<User>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        });

        if (users != null)
            _users.AddRange(users);
    }

    private async Task PersistAsync()
    {
        var content = JsonSerializer.Serialize(_users, new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        });

        await File.WriteAllTextAsync(_mockFilePath, content);
    }

    public Task<User?> GetByIdAsync(int id) =>
        Task.FromResult(_users.FirstOrDefault(u => u.Id == id));

    public Task<User?> GetByEmailAsync(string email) =>
        Task.FromResult(_users.FirstOrDefault(u =>
            !string.IsNullOrWhiteSpace(u.Email) &&
            u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)));

    public Task<IEnumerable<User>> GetAllAsync() => Task.FromResult(_users.AsEnumerable());

    public async Task AddAsync(User user)
    {
        if (user.Id == 0)
            user.Id = _users.Count + 1;

        _users.Add(user);
        await PersistAsync();
    }

    public async Task UpdateAsync(User user)
    {
        var existing = _users.FirstOrDefault(u => u.Id == user.Id);
        if (existing != null)
        {
            _users.Remove(existing);
            _users.Add(user);
            await PersistAsync();
        }
    }

    public async Task DeleteAsync(int id)
    {
        _users.RemoveAll(u => u.Id == id);
        await PersistAsync();
    }
}