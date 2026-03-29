using AlugueldeCarros.Domain.Entities;
using AlugueldeCarros.Domain.Enums;

namespace AlugueldeCarros.Repositories;

public class InMemoryVehicleRepository : IVehicleRepository
{
    private readonly List<Vehicle> _vehicles = new();
    private readonly IReservationRepository _reservationRepository;

    public InMemoryVehicleRepository(IReservationRepository reservationRepository)
    {
        _reservationRepository = reservationRepository;
    }

    public Task<Vehicle> GetByIdAsync(int id) => Task.FromResult(_vehicles.FirstOrDefault(v => v.Id == id));
    public Task<IEnumerable<Vehicle>> GetAllAsync() => Task.FromResult(_vehicles.AsEnumerable());
    
    public async Task<IEnumerable<Vehicle>> SearchAsync(int? categoryId, DateTime? startDate, DateTime? endDate)
    {
        var query = _vehicles.AsQueryable();
        if (categoryId.HasValue) query = query.Where(v => v.CategoryId == categoryId);

        // Filtrar apenas veículos disponíveis (status AVAILABLE)
        query = query.Where(v => v.Status == VehicleStatus.AVAILABLE);

        // Se houver datas, validar sobreposição com reservas existentes
        if (startDate.HasValue && endDate.HasValue)
        {
            var allReservations = await _reservationRepository.GetAllAsync();
            var vehicles = query.ToList();
            var availableVehicles = new List<Vehicle>();

            foreach (var vehicle in vehicles)
            {
                var vehicleReservations = allReservations
                    .Where(r => r.VehicleId == vehicle.Id && 
                                (r.Status == ReservationStatus.CONFIRMED || r.Status == ReservationStatus.PENDING_PAYMENT))
                    .ToList();

                // Verificar se há sobreposição
                var hasOverlap = vehicleReservations.Any(r => 
                    !(endDate.Value <= r.StartDate || startDate.Value >= r.EndDate));

                if (!hasOverlap)
                    availableVehicles.Add(vehicle);
            }

            return availableVehicles.AsEnumerable();
        }

        return query.AsEnumerable();
    }
    
    public Task AddAsync(Vehicle vehicle) { vehicle.Id = _vehicles.Count + 1; _vehicles.Add(vehicle); return Task.CompletedTask; }
    public Task UpdateAsync(Vehicle vehicle) { var existing = _vehicles.FirstOrDefault(v => v.Id == vehicle.Id); if (existing != null) { _vehicles.Remove(existing); _vehicles.Add(vehicle); } return Task.CompletedTask; }
    public Task DeleteAsync(int id) { _vehicles.RemoveAll(v => v.Id == id); return Task.CompletedTask; }
}