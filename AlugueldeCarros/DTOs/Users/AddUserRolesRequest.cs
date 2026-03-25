namespace AlugueldeCarros.DTOs.Users;

public class AddUserRolesRequest
{
    public List<string> Roles { get; set; } = new();
}