namespace AlugueldeCarros.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<UserRole> UserRoles { get; set; } = new();
    public List<string> Roles { get; set; } = new();
}