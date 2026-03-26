using AlugueldeCarros.Domain.Entities;
using AlugueldeCarros.Domain.Enums;
using AlugueldeCarros.DTOs.Auth;
using AlugueldeCarros.Security;

namespace AlugueldeCarros.Tests.Fixtures;

/// <summary>
/// Builder para construir dados de teste complexos passo a passo
/// </summary>
public class TestDataBuilder
{
    private static int _userIdCounter = 1;
    private static int _vehicleIdCounter = 1;
    private static int _categoryIdCounter = 1;
    private static int _branchIdCounter = 1;
    private static int _reservationIdCounter = 1;
    private static int _paymentIdCounter = 1;
    private static int _roleIdCounter = 1;

    private User _user;
    private Vehicle _vehicle;
    private VehicleCategory _vehicleCategory;
    private Branch _branch;
    private Reservation _reservation;
    private Payment _payment;
    private Role _role;

    public TestDataBuilder()
    {
        ResetCounters();
    }

    public TestDataBuilder Reset()
    {
        _user = null;
        _vehicle = null;
        _vehicleCategory = null;
        _branch = null;
        _reservation = null;
        _payment = null;
        _role = null;
        return this;
    }

    private static void ResetCounters()
    {
        _userIdCounter = 1;
        _vehicleIdCounter = 1;
        _categoryIdCounter = 1;
        _branchIdCounter = 1;
        _reservationIdCounter = 1;
        _paymentIdCounter = 1;
        _roleIdCounter = 1;
    }

    #region User

    public TestDataBuilder WithUser(string email = "user@test.com", string firstName = "Test", string lastName = "User")
    {
        _user = new User
        {
            Id = _userIdCounter++,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            PasswordHash = PasswordHasher.HashPassword("Test@123"),
            CreatedAt = DateTime.UtcNow,
            Roles = new List<string> { "Customer" }
        };
        return this;
    }

    public TestDataBuilder WithAdminUser()
    {
        return WithUser("admin@test.com", "Admin", "User");
    }

    public TestDataBuilder WithCustomerUser()
    {
        return WithUser("customer@test.com", "Customer", "User");
    }

    public User BuildUser() => _user ?? throw new InvalidOperationException("User not configured");

    #endregion

    #region Role

    public TestDataBuilder WithRole(string roleName = "Customer")
    {
        _role = new Role
        {
            Id = _roleIdCounter++,
            Name = roleName
        };
        return this;
    }

    public Role BuildRole() => _role ?? throw new InvalidOperationException("Role not configured");

    #endregion

    #region VehicleCategory

    public TestDataBuilder WithVehicleCategory(string name = "Econômico", string description = "Categoria Econômica")
    {
        _vehicleCategory = new VehicleCategory
        {
            Id = _categoryIdCounter++,
            Name = name,
            Description = description
        };
        return this;
    }

    public TestDataBuilder WithSuvCategory()
    {
        return WithVehicleCategory("SUV", "Categoria SUV");
    }

    public TestDataBuilder WithExecutiveCategory()
    {
        return WithVehicleCategory("Executivo", "Categoria Executiva");
    }

    public VehicleCategory BuildVehicleCategory() => _vehicleCategory ?? throw new InvalidOperationException("VehicleCategory not configured");

    #endregion

    #region Branch

    public TestDataBuilder WithBranch(string name = "Filial Centro", string city = "São Paulo", string address = "Rua Principal, 100")
    {
        _branch = new Branch
        {
            Id = _branchIdCounter++,
            Name = name,
            Address = address
        };
        return this;
    }

    public TestDataBuilder WithRioBranch()
    {
        return WithBranch("Filial Rio", "Rio de Janeiro", "Avenida Getúlio Vargas, 500");
    }

    public Branch BuildBranch() => _branch ?? throw new InvalidOperationException("Branch not configured");

    #endregion

    #region Vehicle

    public TestDataBuilder WithVehicle(string plate = "ABC-1234", string model = "Fiat Uno", int year = 2023)
    {
        var category = _vehicleCategory ?? (Reset().WithVehicleCategory()).BuildVehicleCategory();
        var branch = _branch ?? (new TestDataBuilder().WithBranch()).BuildBranch();

        _vehicle = new Vehicle
        {
            Id = _vehicleIdCounter++,
            LicensePlate = plate,
            Model = model,
            Year = year,
            CategoryId = category.Id,
            Category = category,
            BranchId = branch.Id,
            Branch = branch,
            Status = VehicleStatus.AVAILABLE,
        };
        return this;
    }

    public TestDataBuilder WithAvailableVehicle()
    {
        if (_vehicle == null)
            WithVehicle();
        _vehicle.Status = VehicleStatus.AVAILABLE;
        return this;
    }

    public TestDataBuilder WithReservedVehicle()
    {
        if (_vehicle == null)
            WithVehicle();
        _vehicle.Status = VehicleStatus.RESERVED;
        return this;
    }

    public Vehicle BuildVehicle() => _vehicle ?? throw new InvalidOperationException("Vehicle not configured");

    #endregion

    #region Reservation

    public TestDataBuilder WithReservation(DateTime? startDate = null, DateTime? endDate = null)
    {
        var user = _user ?? (new TestDataBuilder().WithUser()).BuildUser();
        var vehicle = _vehicle ?? (new TestDataBuilder().WithVehicle()).BuildVehicle();

        _reservation = new Reservation
        {
            Id = _reservationIdCounter++,
            UserId = user.Id,
            User = user,
            VehicleId = vehicle.Id,
            Vehicle = vehicle,
            CategoryId = vehicle.CategoryId,
            StartDate = startDate ?? DateTime.UtcNow.AddDays(1),
            EndDate = endDate ?? DateTime.UtcNow.AddDays(3),
            Status = ReservationStatus.PENDING_PAYMENT,
        };
        return this;
    }

    public TestDataBuilder WithConfirmedReservation()
    {
        if (_reservation == null)
            WithReservation();
        _reservation.Status = ReservationStatus.CONFIRMED;
        return this;
    }

    public TestDataBuilder WithCancelledReservation()
    {
        if (_reservation == null)
            WithReservation();
        _reservation.Status = ReservationStatus.CANCELLED;
        return this;
    }

    public Reservation BuildReservation() => _reservation ?? throw new InvalidOperationException("Reservation not configured");

    #endregion

    #region Payment

    public TestDataBuilder WithPayment(decimal amount = 500m)
    {
        var reservation = _reservation ?? (new TestDataBuilder().WithReservation()).BuildReservation();

        _payment = new Payment
        {
            Id = _paymentIdCounter++,
            ReservationId = reservation.Id,
            Reservation = reservation,
            Amount = amount,
            Status = PaymentStatus.PENDING,
            CreatedAt = DateTime.UtcNow
        };
        return this;
    }

    public TestDataBuilder WithApprovedPayment()
    {
        if (_payment == null)
            WithPayment();
        _payment.Status = PaymentStatus.APPROVED;
        return this;
    }

    public TestDataBuilder WithDeclinedPayment()
    {
        if (_payment == null)
            WithPayment();
        _payment.Status = PaymentStatus.DECLINED;
        return this;
    }

    public Payment BuildPayment() => _payment ?? throw new InvalidOperationException("Payment not configured");

    #endregion

    #region DTOs

    public LoginRequest BuildLoginRequest(string email = "user@test.com", string password = "Test@123")
    {
        return new LoginRequest
        {
            Email = email,
            Password = password
        };
    }

    #endregion
}
