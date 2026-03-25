namespace AlugueldeCarros.Domain.Entities;

public class CustomerProfile
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }
    public User User { get; set; }
}