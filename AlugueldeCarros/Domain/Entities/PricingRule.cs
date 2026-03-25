namespace AlugueldeCarros.Domain.Entities;

public class PricingRule
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public decimal BaseDailyRate { get; set; }
    public decimal WeekendMultiplier { get; set; }
    public decimal PeakSeasonMultiplier { get; set; }
    public VehicleCategory Category { get; set; }
}