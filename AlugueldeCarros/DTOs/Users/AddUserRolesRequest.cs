using System.ComponentModel.DataAnnotations;

namespace AlugueldeCarros.DTOs.Users;

public class AddUserRolesRequest
{
    [MinLength(1)]
    public List<string> Roles { get; set; } = new();
}