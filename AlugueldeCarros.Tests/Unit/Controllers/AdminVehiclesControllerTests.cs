using System.Security.Claims;
using AlugueldeCarros.Controllers;
using AlugueldeCarros.Domain.Entities;
using AlugueldeCarros.Domain.Enums;
using AlugueldeCarros.DTOs.Vehicles;
using AlugueldeCarros.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AlugueldeCarros.Tests.Unit.Controllers;

public class AdminVehiclesControllerTests
{
    [Fact]
    public async Task Create_ShouldMapRequestAndReturnCreated()
    {
        var service = new Mock<IVehicleService>();
        var controller = CreateController(service);
        var request = new CreateVehicleRequest
        {
            LicensePlate = "TES-1000",
            Model = "Sedan",
            Year = 2026,
            CategoryId = 2,
            BranchId = 1,
            DailyRate = 150m,
            Status = VehicleStatus.AVAILABLE
        };

        service
            .Setup(s => s.CreateVehicleAsync(It.IsAny<Vehicle>()))
            .ReturnsAsync((Vehicle vehicle) =>
            {
                vehicle.Id = 99;
                return vehicle;
            });

        var result = await controller.Create(request);

        var created = result.Should().BeOfType<CreatedResult>().Subject;
        created.Location.Should().Be("/api/v1/vehicles/99");
        var body = created.Value.Should().BeOfType<Vehicle>().Subject;
        body.LicensePlate.Should().Be("TES-1000");
        body.Model.Should().Be("Sedan");
        body.Status.Should().Be(VehicleStatus.AVAILABLE);
    }

    [Fact]
    public async Task Update_ShouldMapRequestAndReturnOk()
    {
        var service = new Mock<IVehicleService>();
        var controller = CreateController(service);
        var request = new UpdateVehicleRequest
        {
            LicensePlate = "TES-2000",
            Model = "SUV",
            Year = 2027,
            CategoryId = 3,
            BranchId = 2,
            DailyRate = 210m,
            Status = VehicleStatus.RESERVED
        };

        service
            .Setup(s => s.UpdateVehicleAsync(10, It.IsAny<Vehicle>()))
            .ReturnsAsync((int _, Vehicle vehicle) => vehicle);

        var result = await controller.Update(10, request);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var body = ok.Value.Should().BeOfType<Vehicle>().Subject;
        body.Id.Should().Be(10);
        body.Model.Should().Be("SUV");
        body.BranchId.Should().Be(2);
        body.Status.Should().Be(VehicleStatus.RESERVED);
    }

    private static AdminVehiclesController CreateController(Mock<IVehicleService> service)
    {
        var controller = new AdminVehiclesController(service.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Role, "Admin")
                }, "TestAuth"))
            }
        };

        return controller;
    }
}